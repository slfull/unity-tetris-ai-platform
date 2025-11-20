using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using TMPro;
using Unity.MLAgents.Integrations.Match3;
public class TetrisAgent : Agent
{
    private Board board;
    private int ep;
    private float reward;
    private Queue<Movement> movements;
    private const int NUM_TETROMINO_TYPES = 7;
    private float gameOverReward = -1f;
    private float lineReward = 0.1f;
    [SerializeField] private TextMeshProUGUI epUI;
    [SerializeField] private TextMeshProUGUI rewardUI;

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
        movements = new Queue<Movement>();
        ep = 0;
        reward = 0f;
    }

    public override void OnEpisodeBegin()
    {
        ep++;
        reward = 0f;
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
        sensor.AddOneHotObservation((int)board.activePiece.data.tetromino, NUM_TETROMINO_TYPES);
        sensor.AddObservation(GetStates());
    }

    private float[] GetStates()
    {
        int width = board.Bounds.width;
        int height = board.Bounds.height;
        float[] states = new float[width * height + width + 3];
        bool[] fields = board.GetField(true);

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                states[x + y * width] = fields[x + y * width]? 1f : 0f;
            }
        }

        var (columnHeight, aggregateHeight, completedLines, holes, bumpiness) 
            = Observations.GetCalculatedObservations(fields, width, height);
        
        for(int i = 0; i < width; i++)
        {
            states[height * width + i] = (float)columnHeight[i] / height;
        }

        states[height * width + width] = (float)aggregateHeight / (height * width);
        states[height * width + width + 1] = (float)holes / (height * width);
        states[height * width + width + 2] = (float)bumpiness / (height * width);

        return states;
    }

    public void OnGameOver()
    {
        AgentReward(RewardType.GameOver, 0);
        //Debug.Log("GameOver" + ep);
        EndEpisode();
    }

    public void OnPieceLock(int line, int combo, bool isLastMoveRotation, bool isB2B)
    {
        if(line > 0)
        {
            AgentReward(RewardType.LineClear, line);
        }
    }

    public void AgentReward(RewardType type, int line)
    {
        if(type == RewardType.GameOver)
        {
            AddReward(gameOverReward);
            reward += gameOverReward;
        }
        if(type == RewardType.LineClear)
        {
            AddReward(line * lineReward);
            reward += line * lineReward;
        }
    }
}
