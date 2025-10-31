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
    // 0 -> rotate 1 -> move 2 -> drop move 3 -> rotate lock
    private Board board;
    private List<Movement> movements;
    [Range(0.01f, 1f)] // Allows setting the timescale in the Inspector
    public float timeSpeed = 1f;
    private float nextTime;

    [Header("Agent Reward")]
    [SerializeField] private float lineClearReward = 1f;
    [SerializeField] private float actionReward;
    [SerializeField] private float gameOverReward = -10f;
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
        if(epsodeUI != null && rewardUI != null)
        {
            epsodeUI.text = "Epsode: " + epsode;
            rewardUI.text = "Reward: " + reward;
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((int)board.activePiece.data.tetromino);
        sensor.AddObservation((int)board.nextPiece.data.tetromino);
        sensor.AddObservation((int)board.nextPiece2.data.tetromino);
        sensor.AddObservation((int)board.nextPiece3.data.tetromino);
        sensor.AddObservation((int)board.nextPiece4.data.tetromino);
        sensor.AddObservation((int)board.nextPiece5.data.tetromino);

        bool[] field = board.GetField(true);
        for (int y = 0; y < board.boardSize.y; y++)
        {
            for(int x = 0; x < board.boardSize.x; x++)
            {
                sensor.AddObservation(field[y * board.boardSize.x + x]);
            }
        }
    }
    public override void Initialize()
    {
        board = GetComponent<Board>();
        board.onGameOver += OnGameOver;
        board.onPieceLock += OnPieceLock;
        movements = new List<Movement>();
        nextTime = Time.time;
        actionReward = 10f / MaxStep;
        epsode = 0;
        reward = 0;
    }
    public override void OnEpisodeBegin()
    {
        reward = 0;
        step = 0;
        epsode++;
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
        movements.Add(Movement.DROP);
        MoveAgent(secondMove, secondDir);
        RotateAgent(secondRotate);
        movements.Add(Movement.HARDDROP);
    }
    private void MoveAgent(int step, bool dir)
    {
        for (int i = 0; i < step; i++)
        {
            if (dir)
            {
                movements.Add(Movement.RIGHT);
            }
            else
            {
                movements.Add(Movement.LEFT);
            }
        }
    }

    private void RotateAgent(int step)
    {
        if (step == -1)
        {
            movements.Add(Movement.CCW);
        }

        for (int i = 0; i < step; i++)
        {
            movements.Add(Movement.CW);
        }
    }

    private void AgentStep()
    {
        if(movements.Count > 0)
        {
            board.PieceMove(movements[0]);
            movements.RemoveAt(0);
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
        AgentReward(RewardType.LineClear, total);

        RequestDecision();
    }

    private void AgentReward(RewardType type, int line)
    {
        if (type == RewardType.Action)
        {
            AddReward(actionReward);
            reward += actionReward;
        }
        if (type == RewardType.LineClear)
        {
            AddReward(line * lineClearReward);
            reward += line * lineClearReward;
        }
        if(type == RewardType.GameOver)
        {
            AddReward(gameOverReward);
            reward += gameOverReward;
        }
    }
}
