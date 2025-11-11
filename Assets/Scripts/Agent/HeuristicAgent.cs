using System;
using System.Collections.Generic;
using UnityEngine;

public class HeuristicAgent : MonoBehaviour
{
    private Board board;

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

    private void Start()
    {
        board = GetComponent<Board>();
        movements = new List<Movement>();
        nextStepTime = Time.time + stepInterval;

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


        for (int i = 0; i < 44; i++)
        {
            int rotate = i / 11 - 1;
            int move = i % 11 - 5;
            cells = (Vector2Int[])originalCells.Clone();
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
        int rotate = currMove / 11 - 1;
        int move = currMove % 11 - 5;

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

            switch (board.activePiece.data.tetromino)
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

        int[] columnHeight = ColumnHeight(field);

        int aggregateHeight = AggregateHeight(columnHeight);
        int completedLines = CompleteLines(field);
        int holes = Holes(field, columnHeight);
        int bumpiness = Bumpiness(columnHeight);

        float weight = aggregateHeight * aggregateHeightWeight + completedLines * completeLinesWeight
            + holes * holesWeight + bumpiness * bumpinessWeight;

        //Debug.Log("[Heuristic Agent] Weight: " + weight);
        return weight;
    }

    private int[] ColumnHeight(bool[] field)
    {
        int width = board.boardSize.x;
        int height = board.boardSize.y;
        int[] columnheight = new int[width];
        for (int x = 0; x < width; x++)
        {
            columnheight[x] = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                if (field[y * width + x])
                {
                    columnheight[x] = y + 1;
                    break;
                }
            }
        }
        return columnheight;
    }

    private int AggregateHeight(int[] columnHeight)
    {
        int height = 0;
        for (int i = 0; i < columnHeight.Length; i++)
        {
            height += columnHeight[i];
        }
        return height;
    }

    private int CompleteLines(bool[] field)
    {
        int lines = 0;
        int width = board.boardSize.x;
        int height = board.boardSize.y;

        for (int y = 0; y < height; y++)
        {
            bool flag = true;
            for (int x = 0; x < width; x++)
            {
                if (!field[y * width + x])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                lines++;
            }
        }
        return lines;
    }

    private int Holes(bool[] field, int[] columnHeight)
    {
        int holes = 0;
        int width = board.boardSize.x;
        int height = board.boardSize.y;
        for (int x = 0; x < width; x++)
        {
            for (int y = columnHeight[x] - 2; y >= 0; y--)
            {
                if (!field[y * width + x])
                {
                    holes++;
                }
            }
        }
        return holes;
    }

    private int Bumpiness(int[] columnHeight)
    {
        int bumpiness = 0;
        int width = columnHeight.Length;
        for (int x = 1; x < width; x++)
        {
            bumpiness += Math.Abs(columnHeight[x - 1] - columnHeight[x]);
        }
        return bumpiness;
    }
    
    
}
