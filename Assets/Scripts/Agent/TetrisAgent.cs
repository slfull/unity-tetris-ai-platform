using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;
public class TetrisAgent : Agent
{
    private Board board;
    private int ep;
    private float reward;
    private Queue<Movement> movements;
    private const int NUM_TETROMINO_TYPES = 7;
    private const float DELTA_FITNESS_NORMALIZE = 46.86f;
    private const float MAX_SCORE = 16;
    private float gameOverReward = -1f;
    private float lineReward = 0.1f;
    private float prevFitness;
    private int lines;
    private int[] lineClear = new int[4];
    private int pieceCount;
    private float averageDeltaFitness;
    private float heuristicWeight = 0.9f;
    [SerializeField] private TextMeshProUGUI epUI;
    [SerializeField] private TextMeshProUGUI rewardUI;
    [SerializeField] private TextMeshProUGUI[] lineClearUI = new TextMeshProUGUI[4];
    [SerializeField] private TextMeshProUGUI pieceCountUI;
    [SerializeField] private TextMeshProUGUI averageDeltaFitnessUI;

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        if(board != null)
        {
            AgentStep();            
        }

        if(epUI != null && rewardUI != null)
        {
            epUI.text = $"episode: {ep}";
            rewardUI.text = $"reward: {reward}";
        }
    }

    private void Init()
    {
        board = GetComponent<Board>();
        board.onGameOver += OnGameOver;
        board.onPieceLock += OnPieceLock;
        board.onSetNextPiece += OnSetNextPiece;
        movements = new Queue<Movement>();
        ep = 0;
        reward = 0f;
        prevFitness = 0f;
        lineClear = new int[4];
    }

    public override void OnEpisodeBegin()
    {
        ep++;
        reward = 0f;
        prevFitness = 0f;
        lines = 0;
        for(int i = 0; i < 4; i++)
        {
            lineClear[i] = 0;
        }
        averageDeltaFitness = 0f;
        pieceCount = 0;
        GameDataUIUpdate();
        heuristicWeight = Academy.Instance.EnvironmentParameters.GetWithDefault("heuristic_weight", 0.9f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int rotate = actions.DiscreteActions[0] - 1;
        int move = actions.DiscreteActions[1] - 5;
        
        MoveAgent(move);
        RotateAgent(rotate);
        movements.Enqueue(Movement.HARDDROP);
    }

    private void MoveAgent(int step)
    {
        bool dir = step > 0;

        for(int i = 0; i < Math.Abs(step); i++)
        {
            if(dir)
            {
                movements.Enqueue(Movement.RIGHT);
            }
            else
            {
                movements.Enqueue(Movement.LEFT);
            }
        }
    }

    private void RotateAgent(int step)
    {
        if(step == -1)
        {
            movements.Enqueue(Movement.CCW);
        }

        for(int i = 0; i < step; i++)
        {
            movements.Enqueue(Movement.CW);
        }
    }

    private void AgentStep()
    {
        if(movements.Count > 0)
        {
            board.PieceMove(movements.Dequeue());
        }
        else
        {
            RequestDecision();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddOneHotObservation((int)board.activePiece.data.tetromino, NUM_TETROMINO_TYPES); // 7
        sensor.AddObservation(GetStates()); // 213
    }

    private float[] GetStates()
    {
        int width = board.Bounds.width;
        int height = board.Bounds.height;
        float[] states = new float[213];
        bool[] fields = board.GetField(true);

        for(int x = 0; x < 10; x++)
        {
            for(int y = 0; y < 20; y++)
            {
                states[x + y * 10] = fields[x + y * 10] ? 1f : 0f;
            }
        }

        var (columnHeight, aggregateHeight, completedLines, holes, bumpiness) 
            = Observations.GetCalculatedObservations(fields, width, height);
        
        for(int i = 0; i < 10; i++)
        {
            states[200 + i] = columnHeight[i] / 20f;
        }

        states[210] = aggregateHeight / 200f;
        states[211] = holes / 200f;
        states[212] = bumpiness / 200f;

        return states;
    }

    public void OnGameOver()
    {
        AgentReward(RewardType.SCORE, -4);
        //Debug.Log("GameOver" + ep);
        EndEpisode();
    }

    public void OnPieceLock(int line, int combo, bool isLastMoveRotation, bool isB2B)
    {
        lines = line;
        pieceCount++;
        if(line > 1)
        {
            lineClear[line - 1]++;
        }
    }

    private void GameDataUIUpdate()
    {
        if(pieceCountUI == null)
        {
            return;
        }
        pieceCountUI.text = $"Piece Count:{pieceCount}";
        for(int line = 0; line < lineClearUI.Length; line++)
        {
            lineClearUI[line].text = $"Line({line + 1}):{lineClear[line]}";
        }
    }
    public void OnSetNextPiece()
    {
        AgentReward(RewardType.HEURISTIC, lines);
        AgentReward(RewardType.SCORE, lines);
        GameDataUIUpdate();
    }

    public void AgentReward(RewardType type, int line)
    {
        if(type == RewardType.HEURISTIC)
        {
            float reward = DeltaFitness(line);
            averageDeltaFitness = (reward + averageDeltaFitness * (pieceCount - 1)) / pieceCount;
            reward /= DELTA_FITNESS_NORMALIZE;
            reward *= heuristicWeight;
            AddReward(reward);
            this.reward += reward;
        }

        if(type == RewardType.SCORE)
        {
            float reward = line * Math.Abs(line);
            reward /= MAX_SCORE;
            reward *= 1f - heuristicWeight;
            AddReward(reward);
            this.reward += reward;
        }
    }

    private float DeltaFitness(int line)
    {
        float fitness = Fitness(line);
        float res = fitness - prevFitness;
        prevFitness = fitness;
        return res;
    }

    private float Fitness(int line)
    {
        bool[] fields = board.GetField(true);
        int width = board.Bounds.width;
        int height = board.Bounds.height;
        var (columnHeight, aggregateHeight, completedLines, holes, bumpiness) 
            = Observations.GetCalculatedObservations(fields, width, height);
        
        float fitness = -0.51f * aggregateHeight + 0.76f * line - 0.36f * holes - 0.18f * bumpiness;
        return fitness;
    }
}
