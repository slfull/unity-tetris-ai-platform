using UnityEngine;
using Mirror;

public class PieceMultiplayer : NetworkBehaviour
{
    public BoardMultiplayer board { get; private set; }
    public TetrominoData data { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }
    public int rotationIndex;
    public bool isLastMoveRotation { get; private set; }
    public float stepDelay = 1f;
    public float moveDelay = 0.05f;
    public float lockDelay = 0.5f;
    public float holdmoveDelay = 0.25f;
   
    private float stepTime;
    private float moveTime;
    private float lockTime;
    private float holdmoveTime;
    private bool agentExists = false;

    public void Initialize(BoardMultiplayer board, Vector3Int position, TetrominoData data)
    {
        this.data = data;
        this.board = board;
        this.position = position;

        rotationIndex = 0;
        stepTime = Time.time + stepDelay;
        moveTime = Time.time + moveDelay;
        lockTime = 0f;

        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }

        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
    }

    private void Awake()
    {
        this.enabled = false;
    }


    private void Update()
    {
        if(!isOwned)
        {
            return;
        }
        if(!board.isGameStart)
        {
            return;
        }
        if (agentExists == false) { HandleUpdateMove(-1); }
    }
    
    public void HandleUpdateMove(int movementInput)
    {
        board.Clear(this);

        // We use a timer to allow the player to make adjustments to the piece
        // before it locks in place
        lockTime += Time.deltaTime;

        // Handle rotation
        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Rotate(-1);
            isLastMoveRotation = true;
        }
        else if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.X))
        {
            Rotate(1);
            isLastMoveRotation = true;
        }

        // Handle hard drop
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }

        // Allow the player to hold movement keys but only after a move delay
        // so it does not move too fast
        if (Time.time > moveTime)
        {
            HandleMoveInputs();
        }

        switch (movementInput)
        {
            case -1: break;
            case 0: Move(Vector2Int.left); isLastMoveRotation = false; break;
            case 1: Move(Vector2Int.right); isLastMoveRotation = false; break;
            case 2:
                if (Move(Vector2Int.down))
                {
                    // Update the step time to prevent double movement
                    stepTime = Time.time + stepDelay;
                }
                break;
            case 3: HardDrop(); break;
            case 4: Rotate(-1); isLastMoveRotation = true; break;
            case 5: Rotate(1); isLastMoveRotation = true; break;
            default: break;
        }

        // Advance the piece to the next row every x seconds
        if (Time.time > stepTime)
        {
            Step();
        }

        board.Set(this);
    }

    private void HandleMoveInputs()
    {
        // Soft drop movement
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            if (Move(Vector2Int.down))
            {
                // Update the step time to prevent double movement
                stepTime = Time.time + stepDelay;
            }
            isLastMoveRotation = false;
        }

        // Left/right movement
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            holdmoveTime = Time.time + holdmoveDelay;
            Move(Vector2Int.left);
            isLastMoveRotation = false;
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            holdmoveTime = Time.time + holdmoveDelay;
            Move(Vector2Int.right);
            isLastMoveRotation = false;
        }

        // Left/right hold movement
        if ((Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) && (Time.time >= holdmoveTime))
        {
            Move(Vector2Int.left);
            isLastMoveRotation = false;
        }
        else if ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) && (Time.time >= holdmoveTime))
        {
            Move(Vector2Int.right);
            isLastMoveRotation = false;
        }
    }
    
    public void AgentExists()
    {
        agentExists = true;
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

    private void HardDrop()
    {
        while (Move(Vector2Int.down)) {
            continue;
        }

        Lock();
    }

    private void Lock()
    {
        board.Set(this);
        board.ClearLines();
        board.SpawnPiece();
    }

    private bool Move(Vector2Int translation)
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
        }

        return valid;
    }

    private void Rotate(int direction)
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
        if(wallKickIndex % 2 == 0)
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

            if (Move(translation)) {
                //Debug.Log(wallKickIndex);
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0) {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min) {
            return max - (min - input) % (max - min);
        } else {
            return min + (input - min) % (max - min);
        }
    }

}
