using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Collections;
using TMPro;
using Mirror.Examples.MultipleAdditiveScenes;

public class TetrisAgent : Agent
{
    public const int NUM_TETROMINO_TYPES = (int)Tetromino.Z + 1;
    // 0 -> rotate 1 -> move 2 -> drop move 3 -> rotate lock
    private Board board;
    private Queue<Movement> movements;
    [Range(0.01f, 1f)] // Allows setting the timescale in the Inspector
    public float timeSpeed = 1f;
    private float nextTime;

    [Header("Agent Reward")]
    [SerializeField] private float lineClearReward = 0.1f;
    [SerializeField] private float actionReward;
    [SerializeField] private float gameOverReward = -1f;
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI epsodeUI;
    [SerializeField] private TextMeshProUGUI rewardUI;
    private int epsode;
    private float reward;
    private int step;

    void Update()
    {
        if (Time.time >= nextTime)
        {
            nextTime = Time.time + timeSpeed;
            AgentStep();
        }
        if (epsodeUI != null && rewardUI != null)
        {
            epsodeUI.text = "Epsode: " + epsode;
            rewardUI.text = "Reward: " + reward;
        }
        
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddOneHotObservation((int)board.activePiece.data.tetromino, NUM_TETROMINO_TYPES);
        sensor.AddOneHotObservation((int)board.nextPiece.data.tetromino, NUM_TETROMINO_TYPES);
        sensor.AddOneHotObservation((int)board.nextPiece2.data.tetromino, NUM_TETROMINO_TYPES);
        sensor.AddOneHotObservation((int)board.nextPiece3.data.tetromino, NUM_TETROMINO_TYPES);
        sensor.AddOneHotObservation((int)board.nextPiece4.data.tetromino, NUM_TETROMINO_TYPES);
        sensor.AddOneHotObservation((int)board.nextPiece5.data.tetromino, NUM_TETROMINO_TYPES);

        sensor.AddObservation(board.aggregateHeight);
        sensor.AddObservation(board.numberOfHoles);
        sensor.AddObservation(board.bumpiness);
    }
    public override void Initialize()
    {
        board = GetComponent<Board>();
        board.onGameOver += OnGameOver;
        board.onPieceLock += OnPieceLock;
        movements = new Queue<Movement>();
        nextTime = Time.time;
        actionReward = 1f / MaxStep;
        epsode = 0;
        reward = 0;

        if(board != null && board.activePiece != null)
        {
            board.activePiece.AgentExists();
        }
    }
    public override void OnEpisodeBegin()
    {
        reward = 0;
        step = 0;
        epsode++;
        RequestDecision();
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        AgentReward(RewardType.Action, 0);
        step++;
        int firstRotate = actions.DiscreteActions[0] - 1;
        int firstMove = actions.DiscreteActions[1] - 5;
        int secondMove = actions.DiscreteActions[2] - 5;
        int secondRotate = actions.DiscreteActions[3] - 1;
        bool firstDir = firstMove > 0;
        bool secondDir = secondMove > 0;
        firstMove = Math.Abs(firstMove);
        secondMove = Math.Abs(secondMove);

        //Debug.Log($"[ML Agent] First Move:{firstMove} First Rotate:{firstRotate} Second Move:{secondMove} Second Rotate:{secondRotate}");

        RotateAgent(firstRotate);
        MoveAgent(firstMove, firstDir);
        movements.Enqueue(Movement.DROP);
        MoveAgent(secondMove, secondDir);
        RotateAgent(secondRotate);
        movements.Enqueue(Movement.HARDDROP);
    }
    private void MoveAgent(int step, bool dir)
    {
        for (int i = 0; i < step; i++)
        {
            if (dir)
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
        if (step == -1)
        {
            movements.Enqueue(Movement.CCW);
        }

        for (int i = 0; i < step; i++)
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

        if(step == MaxStep)
        {
            board.GameReset();
        }
    }
    public void OnGameOver()
    {
        AgentReward(RewardType.GameOver, 0);
        EndEpisode();
    }

    public void OnPieceLock(int line, int combo, bool isRotation, bool b2b)
    {
        int total = line;
        if (isRotation)
        {
            total += line;
            if (b2b)
            {
                total += line;
            }
        }

        if (b2b && line == 4)
        {
            total += 2;
        }

        total += Math.Min(combo, 4);
        if (line > 0)
        {
            AgentReward(RewardType.LineClear, total);
        }
        RequestDecision();
    }

    private void AgentReward(RewardType type, int line)
    {
        if (type == RewardType.Action)
        {
            float temp = board.aggregateHeight * (-0.510066f) + board.numberOfHoles * (-0.35663f) + board.bumpiness * (-0.184483f);
            AddReward(temp * 0.01f);
            reward += temp * 0.01f;
        }
        if (type == RewardType.LineClear)
        {
            AddReward(line * 0.760666f);
            reward += line * 0.760666f;
        }
        if(type == RewardType.GameOver)
        {
            AddReward(gameOverReward);
            reward += gameOverReward;
        }
    }
}
