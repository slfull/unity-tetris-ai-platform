using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.Collections;

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
        activePiece.AgentExists();
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
        // Int Holes
        sensor.AddObservation(board.numberOfHoles);
        // Int EmptyTiles
        sensor.AddObservation(board.sumOfEmptyTiles);
        // Int UnfilledLines
        sensor.AddObservation(board.numberOfUnfilledLines);

        // Int Board
        int[,] state = board.GetBoardState();

        for (int x = 0; x < board.GetBoardSize(0); x++)
        {
            for (int y = 0; y < board.GetBoardSize(1); y++)
            {
                sensor.AddObservation(state[x, y]);
            }
        }

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

    public void Highlight()
    {

    }
    public override void OnActionReceived(ActionBuffers actions)
    {

        int PieceMove = actions.DiscreteActions[0]; // Get the action (1-5)
        //Debug.Log("action: " + PieceMove);
        switch (PieceMove)
        {
            case 0: activePiece.HandleUpdateMove((int)MovementInput.left); break;
            case 1: activePiece.HandleUpdateMove((int)MovementInput.right); break;
            case 2: activePiece.HandleUpdateMove((int)MovementInput.softdrop); break;
            case 3: activePiece.HandleUpdateMove((int)MovementInput.harddrop); break;
            case 4: activePiece.HandleUpdateMove((int)MovementInput.rotateclockwise); break;
            case 5: activePiece.HandleUpdateMove((int)MovementInput.rotatecounterclockwise); break;
            default: break;
        }

    }
    // Heuristic NOT WORKING NEED FIX
    public override void Heuristic(in ActionBuffers actions)
    {
        var PieceMove = actions.DiscreteActions;
        PieceMove[0] = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ? 0 : -1;
        PieceMove[0] = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ? 1 : -1;
        PieceMove[0] = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 2 : -1;
        PieceMove[0] = Input.GetKeyDown(KeyCode.Space) ? 3 : -1;
        PieceMove[0] = (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow)) ? 4 : -1;
        PieceMove[0] = (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.X)) ? 5 : -1;
    }
}
