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

public class TetrisAgent : Agent
{
    private Board board;
    [Range(0.01f, 1f)] // Allows setting the timescale in the Inspector
    public float timeSpeed = 1f;

    [Header("Agent Reward")]
    private float gameOverReward = -50f;
    private float lineClearReward = 5f;
    private float lockPieceReward = 0.1f;
    private float holeReward = 0.01f;
    private float actionReward;
    private float currReward = 0f;
    private int currEpsode = 0;
    [Header("TMPro")]
    [SerializeField] private TextMeshProUGUI currEpsodeUI;
    [SerializeField] private TextMeshProUGUI currRewardUI;
    bool[] field;

    void Update()
    {
        if (currEpsodeUI != null && currRewardUI != null)
        {
            currEpsodeUI.text = "Epsode: " + currEpsode;
            currRewardUI.text = "Reward: " + currReward;
        }
        field = board.GetField(true);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Piece data I = 0, J = 1, L = 2, O = 3, S = 4, T = 5, Z = 6
        // Vector3Int position 
        for(int i = 0; i < board.activePiece.cellsPosition.Length; i++)
        {
            sensor.AddObservation(board.activePiece.cellsPosition[i].x);
            sensor.AddObservation(board.activePiece.cellsPosition[i].y);
        }
        // Int Tetromino
        sensor.AddObservation((int)board.activePiece.data.tetromino);
        // Int rotationIndex
        sensor.AddObservation(board.activePiece.rotationIndex);
        // Int nextPiece
        sensor.AddObservation((int)board.nextPiece.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)board.nextPiece2.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)board.nextPiece3.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)board.nextPiece4.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)board.nextPiece5.data.tetromino);

        // Int Board
        field = board.GetField(true);

        for (int y = 0; y < board.GetBoardSize(1); y++)
        {
            for (int x = 0; x < board.GetBoardSize(0); x++)
            {
                int index = y * board.GetBoardSize(0) + x;
                sensor.AddObservation(field[index]);
            }
        }
    }
    public override void Initialize()
    {
        board = GetComponent<Board>();
        board.onGameOver += OnGameOver;
        board.onPieceLock += OnPieceLock;
        actionReward = -75f / MaxStep;
    }
    public override void OnEpisodeBegin()
    {
        currEpsode++;
        currReward = 0f;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> pieceMove = actions.DiscreteActions;
        // int PieceHardDrop = actions.DiscreteActions[1];
        //Debug.Log("action: " + PieceMove);
        MoveAgent(pieceMove);
    }
    private void MoveAgent(ActionSegment<int> acts)
    {
        foreach(int act in acts)
        {
            Movement movement = (Movement)act;
            board.PieceMove(movement);
            AgentReward(RewardType.Action, 0);
        }
    }
    public void OnGameOver()
    {
        AgentReward(RewardType.GameOver, 0);
        EndEpisode();
    }

    public void OnPieceLock(int line, int combo, bool isRotation, bool b2b)
    {
        int reward = line + Math.Min(combo, 4);
        if (isRotation)
        {
            reward += line;
            if (b2b)
            {
                reward += line - 2;
            }
        }
        
        if(b2b)
        {
            reward += 2;
        }
        AgentReward(RewardType.LineClear, reward);
        AgentReward(RewardType.LockPiece, 0);
        actionReward = -75f / MaxStep;
    }

    public void AgentReward(RewardType rewardType, int rewardMultiplier)
    {
        lineClearReward *= rewardMultiplier;
        holeReward *= rewardMultiplier;

        if (rewardType == RewardType.GameOver)
        {
            GameOverReward();
        }

        if (rewardType == RewardType.LineClear)
        {
            LineClearReward(rewardMultiplier);
        }

        if (rewardType == RewardType.LockPiece)
        {
            LockPieceReward();
        }

        if (rewardType == RewardType.Distance)
        {
            DistanceFromBottom();
        }

        if (rewardType == RewardType.Hole)
        {
            NumberOfHoleReward();
        }
        
        if (rewardType == RewardType.Action)
        {
            ActionReward();
        }
    }

    private void GameOverReward()
    {
        AddReward(gameOverReward);
        currReward += gameOverReward;
    }

    private void LineClearReward(int line)
    {
        AddReward(line * lineClearReward);
        currReward += line * lineClearReward;
    }
    private void LockPieceReward()
    {
        AddReward(lockPieceReward);
        currReward += lockPieceReward;
    }
    private void DistanceFromBottom()
    {
        AddReward(-0.02f);
        currReward += -0.02f;
    }
    private void NumberOfHoleReward()
    {
        AddReward(holeReward);
        currReward += holeReward;
    }
    private void ActionReward()
    {
        AddReward(actionReward);
        currReward += actionReward;
        actionReward += -25f / MaxStep;
    }
}
