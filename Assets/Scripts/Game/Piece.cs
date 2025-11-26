using Unity.Profiling;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public Board board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public Vector3Int[] cellsPosition { get; private set; }
    public int rotationIndex;
    public bool isLastMoveRotation { get; private set; }

    public Spin spin = Spin.None;
    public float stepDelay = 1f;
    public float moveDelay = 0.03f;
    public float lockDelay = 0.5f;
    public float holdmoveDelay = 0.1f;

    public float maxlockTime = 2f;
    private float stepTime;
    private float moveTime;
    private float lockTime;
    private float holdmoveTime;
    private bool agentExists = false;
    public bool isPlayerTwo = false;

    public void Initialize(Board board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;
        holdmoveTime = Time.time + holdmoveDelay;

        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }

        if (cellsPosition == null)
        {
            cellsPosition = new Vector3Int[data.cells.Length];
        }
        for (int i = 0; i < cells.Length; i++)
        {
            cellsPosition[i] = cells[i] + position;
        }
    }

    private void Update()
    {
        if (agentExists == false) { HandleUpdateMove(-1); }
        for (int i = 0; i < cells.Length; i++)
        {
            cellsPosition[i] = cells[i] + position;
        }
        //if (agentExists == true) { agent.RequestDecision(); }
    }

    public void AgentExists()
    {
        agentExists = true;
    }

    public void PlayerTwoExists()
    {
        isPlayerTwo = true;
    }

    public void HandleUpdateMove(int movementInput)
    {
        board.Clear(this);

        // We use a timer to allow the player to make adjustments to the piece
        // before it locks in place
        lockTime += Time.deltaTime;
        if (Time.time > stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    private void Step()
    {
        stepTime = Time.time + stepDelay;

        // Step down to the next row
        Move(Vector2Int.down);

        // Once the piece has been inactive for too long it becomes locked
        if (lockTime >= lockDelay)
        {
            Lock();
        }

    }

    public void HardDrop()
    {
        NormalDrop();
        Lock();
    }

    public void NormalDrop()
    {
        while (Move(Vector2Int.down))
        {
            continue;
        }
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
        Debug.Log(spin);
    }

    public bool Move(Vector2Int translation)
    {
        Vector3Int newPosition = position;
        newPosition.x += translation.x;
        newPosition.y += translation.y;

        bool valid = board.IsValidPosition(this, newPosition);

        // Only save the movement if the new position is valid
        if (valid)
        {
            position = newPosition;
            moveTime = Time.time + moveDelay;
            lockTime = 0f; // reset
            isLastMoveRotation = translation.x == 0 ? isLastMoveRotation : false;
        }

        return valid;
    }

    public void Rotate(int direction)
    {
        // Store the current rotation in case the rotation fails
        // and we need to revert
        int originalRotation = rotationIndex;

        // Rotate all of the cells using a rotation matrix
        rotationIndex = Wrap(rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction);

        // Revert the rotation if the wall kick tests fail
        if (!TestWallKicks(rotationIndex, direction))
        {
            rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction);
            spin = Spin.None;
        }

        if (rotationIndex != originalRotation)
        {
            isLastMoveRotation = true;
        }
    }

    private void ApplyRotationMatrix(int direction)
    {
        float[] matrix = Data.RotationMatrix;

        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3 cell = cells[i];

            int x, y;

            switch (data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector3Int(x, y, 0);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection);
        if (wallKickIndex % 2 == 0)
        {
            wallKickIndex = Wrap(wallKickIndex - 2, 0, data.wallKicks.GetLength(0));
        }
        else
        {
            wallKickIndex = Wrap(wallKickIndex + 2, 0, data.wallKicks.GetLength(0));
        }

        for (int i = 0; i < data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = data.wallKicks[wallKickIndex, i];

            if (Move(translation))
            {
                spin = CheckTSpin(rotationIndex, i, position);
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
    private Spin CheckTSpin(int rotationIndex, int kickIndex, Vector3Int currentPos)
    {
        if (data.tetromino != Tetromino.T)
        {
            return Spin.None;
        }

        Vector2Int[] corners = new Vector2Int[] {
        new Vector2Int(-1, -1),
        new Vector2Int( 1, -1),
        new Vector2Int(-1,  1),
        new Vector2Int( 1,  1)
        };

        int occupiedCorners = 0;

        foreach (var offset in corners)
        {
            Vector3Int cornerPos = currentPos + (Vector3Int)offset;

            if (!board.IsValidTile(cornerPos))
            {
                occupiedCorners++;
            }
        }

        if (occupiedCorners < 3)
        {
            return Spin.None;
        }

        int occupiedMiniCorners = 0;

        for (int i = 0; i < data.miniCornerOffsets.GetLength(1); i++)
        {
            Vector2Int offset = data.miniCornerOffsets[rotationIndex, i];
            Vector3Int cornerPos = currentPos + (Vector3Int)offset;

            if (!board.IsValidTile(cornerPos))
            {
                occupiedMiniCorners++;
            }
        }

        if (kickIndex == 4 || occupiedMiniCorners == 2)
        {
            return Spin.Full;
        }
        else
        {
            return Spin.Mini;
        }
    }


}
