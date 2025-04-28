using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class TetrisAgent : Agent
{
    public Board board;
    
    [SerializeField] private float decisionDelay = 1f; // Delay in seconds between decisions
    private float lastDecisionTime;

    void Update()
    {
        // Request a decision only after the delay has passed
        if (Time.time - lastDecisionTime >= decisionDelay)
        {
            RequestDecision();
            lastDecisionTime = Time.time;
            
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

    public override void OnActionReceived(ActionBuffers actions)
    {
        
        int PieceMove = actions.DiscreteActions[0]; // Get the action (1-3)
        switch (PieceMove)
        {
            case 0: board.activePiece.MoveAgent(Vector2Int.left);  break;
            case 1: board.activePiece.MoveAgent(Vector2Int.down);  break;
            case 2: board.activePiece.MoveAgent(Vector2Int.right);  break;
            default: break;
        }

        /**
        int PieceRotate = actions.DiscreteActions[1]; // Get the action (1-2)

        switch (PieceRotate)
        {
            case 0: board.activePiece.RotateAgent(1); break;
            case 1: board.activePiece.RotateAgent(-1);  break;
            default: break;
        }
        **/
    }
}
