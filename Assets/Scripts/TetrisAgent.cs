using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TetrisAgent : Agent
{
    private Board board;
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    public void FixedUpdate()
    {
        WaitTimeInference();
    }

    void WaitTimeInference()
    {

        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                m_TimeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                m_TimeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        // Piece data I = 0, J = 1, L = 2, O = 3, S = 4, T = 5, Z = 6
        // Vector3Int position 
        sensor.AddObservation(board.activePiece.position.x);
        sensor.AddObservation(board.activePiece.position.y);
        // Int Tetromino
        sensor.AddObservation((int)board.activePiece.data.tetromino);
        // Int rotationIndex
        sensor.AddObservation((int)board.activePiece.rotationIndex);
    }

    private void Start()
    {
        OnEpisodeBegin();
    }
    public override void OnEpisodeBegin()
    {
        board = GetComponent<Board>();
    }
    public override void OnActionReceived(ActionBuffers actions)
    {

        int PieceMove = actions.DiscreteActions[0]; // Get the action (1-5)

       switch (PieceMove)
        {
            case 0: board.activePiece.RotateAgent(1); break;
            case 1: board.activePiece.RotateAgent(-1); break;
            case 2: board.activePiece.MoveAgent(Vector2Int.left); break;
            case 3: board.activePiece.MoveAgent(Vector2Int.down); break;
            case 4: board.activePiece.MoveAgent(Vector2Int.right); break;
            default: throw new ArgumentException("Invalid action value");
        }
 /**
switch (PieceMove)
        {
            case 0: Debug.Log("1"); break;
            case 1: Debug.Log("2"); break;
            case 2: Debug.Log("3"); break;
            case 3: Debug.Log("4"); break;
            case 4: Debug.Log("5"); break;
            default: throw new ArgumentException("Invalid action value");
        }
        **/
    }
}
