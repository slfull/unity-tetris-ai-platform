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
    private Piece nextPiece;
    private Piece nextPiece2;
    private Piece nextPiece3;
    private Piece nextPiece4;
    private Piece nextPiece5;
    
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    public enum MovementInput
    {
        left, right, softdrop, harddrop, rotateclockwise, rotatecounterclockwise, hold
    }
    /**
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

    **/



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
        // Int numberOfHoles
        sensor.AddObservation(board.numberOfHoles);
        // Int numberOfOverHangs
        sensor.AddObservation(board.numberOfOverHangs);
        // Int aggregateHeight
        sensor.AddObservation(board.aggregateHeight);
        // Int bumpiness
        sensor.AddObservation(board.bumpiness);
        // Int density
        sensor.AddObservation(board.density);
        // Int nextPiece
        sensor.AddObservation((int)nextPiece.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)nextPiece2.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)nextPiece3.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)nextPiece4.data.tetromino);
        // Int nextPiece
        sensor.AddObservation((int)nextPiece5.data.tetromino);

        // Int Board
        bool[] field = board.GetField();

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
        base.Initialize();
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
        nextPiece = board.nextPiece;
        nextPiece2 = board.nextPiece2;
        nextPiece3 = board.nextPiece3;
        nextPiece4 = board.nextPiece4;
        nextPiece5 = board.nextPiece5;
        
    }

    public void Highlight()
    {

    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        activePiece = board.activePiece;
        activePiece.AgentExists();
        int PieceMove = actions.DiscreteActions[0]; // Get the action (1-5)
        //Debug.Log("action: " + PieceMove);
        switch (PieceMove)
        {
            case 0: activePiece.HandleUpdateMove((int)MovementInput.left); AddReward(-0.01f); break;
            case 1: activePiece.HandleUpdateMove((int)MovementInput.right); AddReward(-0.01f); break;
            case 2: activePiece.HandleUpdateMove((int)MovementInput.softdrop); AddReward(0.01f); break;
            case 3: activePiece.HandleUpdateMove((int)MovementInput.harddrop); break;
            case 4: activePiece.HandleUpdateMove((int)MovementInput.rotateclockwise); AddReward(-0.02f); break;
            case 5: activePiece.HandleUpdateMove((int)MovementInput.rotatecounterclockwise); AddReward(-0.02f); break;
            default: break;
        }

    }

    public override void Heuristic(in ActionBuffers actions)
    {
        var PieceMove = actions.DiscreteActions;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) { PieceMove[0] = 0; }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) { PieceMove[0] = 1; }
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) { PieceMove[0] = 2; }
        else if (Input.GetKey(KeyCode.Space)) { PieceMove[0] = 3; }
        else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) { PieceMove[0] = 4; }
        else if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.X)) { PieceMove[0] = 5; }
        else { PieceMove[0] = -1; }
    }
}
