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
    private Piece activePiece;
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;

    public enum MovementInput
    {
        left, right, softdrop, harddrop, rotateclockwise, rotatecounterclockwise, hold
    }
    public void FixedUpdate()
    {
        activePiece = board.activePiece;
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
        sensor.AddObservation(activePiece.position.x);
        sensor.AddObservation(activePiece.position.y);
        // Int Tetromino
        sensor.AddObservation((int)activePiece.data.tetromino);
        // Int rotationIndex
        sensor.AddObservation((int)activePiece.rotationIndex);

        //TODO: detect all tiles in tilemap, use forloop to scan through every tile with tilemap.HasTile(tilePosition) add 10x20(board bound xy) total of 200 tiles of observations, problem: we need to let the agent know what the active piece's tile is.
    }

    private void Start()
    {
        OnEpisodeBegin();

    }
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        //actionMask.SetActionEnabled(0, 3, false);
    }
    public override void OnEpisodeBegin()
    {
        board = GetComponent<Board>();
        activePiece = board.activePiece;
        activePiece.AgentExists();
    }
    public override void OnActionReceived(ActionBuffers actions)
    {

        int PieceMove = actions.DiscreteActions[0]; // Get the action (1-5)

        switch (PieceMove)
        {
            case 0: activePiece.HandleUpdateMove((int)MovementInput.left); break;
            case 1: activePiece.HandleUpdateMove((int)MovementInput.right); break;
            case 2: activePiece.HandleUpdateMove((int)MovementInput.softdrop); break;
            case 3: activePiece.HandleUpdateMove((int)MovementInput.harddrop); break;
            case 4: activePiece.HandleUpdateMove((int)MovementInput.rotateclockwise); break;
            case 5: activePiece.HandleUpdateMove((int)MovementInput.rotatecounterclockwise); break;
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
