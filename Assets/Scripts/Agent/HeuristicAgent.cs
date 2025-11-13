using System;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicAgent : MonoBehaviour
{
    private Board board;
    private Piece currPiece;

    private float aggregateHeightWeight = -0.510066f;
    private float completeLinesWeight = 0.760666f;
    private float holesWeight = -0.35663f;
    private float bumpinessWeight = -0.184483f;
    private Vector2Int[] cells;
    private Vector2Int position;
    private float currWeight;
    private int currMove;
    private List<Movement> movements;
    private float nextStepTime;
    public float stepInterval = 0.5f;
    private int hold = 0;

    private void Start()
    {
        board = GetComponent<Board>();
        movements = new List<Movement>();
        nextStepTime = Time.time + stepInterval;
        hold = 0;

        MoveDecision();
    }

    private void Update()
    {
        if (movements.Count > 0 && Time.time >= nextStepTime)
        {
            board.PieceMove(movements[0]);
            movements.RemoveAt(0);
            nextStepTime = Time.time + stepInterval;
        }
        else if(Time.time >= nextStepTime)
        {
            MoveDecision();
        }
    }

    private void MoveDecision()
    {
        bool[] baseFields = board.GetField(true);
        movements.Clear();
        currWeight = float.NegativeInfinity;
        currMove = 0;

        Tetromino t = board.activePiece.data.tetromino;
        Vector2Int[] originalCells = Data.Cells[t];
        currPiece = board.nextPiece;
        Vector2Int[] originalHoldCells = Data.Cells[board.nextPiece.data.tetromino];
        if(board.savedPiece != null)
        {
            originalHoldCells = Data.Cells[board.savedPiece.data.tetromino];
        }


        for (int i = 0; i < 88; i++)
        {
            int rotate = i / 22 - 1;
            int move = i / 2 % 11 - 5;
            hold = i % 2;
            if(hold == 1)
            {
                currPiece = board.savedPiece == null ? board.nextPiece : board.savedPiece;
                cells = (Vector2Int[])originalHoldCells.Clone();
            }
            else
            {
                currPiece = board.activePiece;
                cells = (Vector2Int[])originalCells.Clone();
            }
            position = new Vector2Int(board.activePiece.position.x, board.activePiece.position.y);
            bool[] fields = (bool[])baseFields.Clone();
            Rotate(rotate);
            if (!Move(move))
            {
                continue;
            }

            float weight = FinalWeight(fields);

            if (weight > currWeight)
            {
                currWeight = weight;
                currMove = i;
            }
        }
        //Debug.Log("[Heuristic Agent] Weight: " + currWeight + " Move: " + currMove);
        DecodeMovement();
    }
    private void DecodeMovement()
    {
        int rotate = currMove / 22 - 1;
        int move = currMove / 2 % 11 - 5;
        hold = currMove % 2;

        if(hold == 1)
        {
            movements.Add(Movement.HOLD);
        }

        for (int i = 0; i < Math.Abs(rotate); i++)
        {
            if (rotate > 0)
            {
                movements.Add(Movement.CW);
            }
            else
            {
                movements.Add(Movement.CCW);
            }
        }

        for (int i = 0; i < Math.Abs(move); i++)
        {
            if (move > 0)
            {
                movements.Add(Movement.RIGHT);
            }
            else
            {
                movements.Add(Movement.LEFT);
            }
        }
        movements.Add(Movement.HARDDROP);
    }
    private bool Move(int move)
    {
        Vector2Int newPosition = new Vector2Int(position.x + move, position.y);
        bool valid = IsValidPosition(newPosition);
        if (valid)
        {
            position = newPosition;
        }
        return valid;
    }

    private bool IsValidPosition(Vector2Int newPosition)
    {
        RectInt bounds = board.Bounds;

        bool[] field = board.GetField(true);
        int width = board.boardSize.x;

        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int tilePosition = cells[i] + newPosition;

            // An out of bounds tile is invalid
            if (!bounds.Contains(tilePosition))
            {
                return false;
            }

            int x = tilePosition.x - bounds.xMin;
            int y = tilePosition.y - bounds.yMin;

            // A tile already occupies the position, thus invalid
            if (field[y * width + x])
            {
                return false;
            }
        }
        return true;
    }

    private void Rotate(int dir)
    {
        int dirt = dir > 0 ? 1 : -1;
        for (int i = 0; i < Math.Abs(dir); i++)
        {
            ApplyRotationMatrix(dirt);
        }
    }

    private void ApplyRotationMatrix(int dir)
    {
        float[] matrix = Data.RotationMatrix;
        for (int i = 0; i < cells.Length; i++)
        {
            Vector2 cell = cells[i];

            int x, y;

            switch (currPiece.data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * dir) + (cell.y * matrix[1] * dir));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * dir) + (cell.y * matrix[3] * dir));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * dir) + (cell.y * matrix[1] * dir));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * dir) + (cell.y * matrix[3] * dir));
                    break;
            }

            cells[i] = new Vector2Int(x, y);
        }
    }

    private float FinalWeight(bool[] field)
    {
        int width = board.boardSize.x;
        int height = board.boardSize.y;
        RectInt bounds = board.Bounds;
        while (IsValidPosition(position + Vector2Int.down))
        {
            position += Vector2Int.down;
        }


        for (int i = 0; i < cells.Length; i++)
        {
            Vector2Int cell = cells[i] + position;
            int x = cell.x - bounds.xMin;
            int y = cell.y - bounds.yMin;
            field[y * width + x] = true;
        }
        var (columnHeight, aggregateHeight, completedLines, holes, bumpiness) = Observations.GetCalculatedObservations(field, width, height);


        float weight = aggregateHeight * aggregateHeightWeight + completedLines * completeLinesWeight
            + holes * holesWeight + bumpiness * bumpinessWeight;

        //Debug.Log("[Heuristic Agent] Weight: " + weight);
        return weight;
    }


    
    
}
