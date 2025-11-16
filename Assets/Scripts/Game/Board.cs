using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Events;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    [Header("Board")]
    public Tilemap tilemap { get; private set; }


    public Piece activePiece;
    public Piece nextPiece { get; private set; }
    public Piece nextPiece2 { get; private set; }
    public Piece nextPiece3 { get; private set; }
    public Piece nextPiece4 { get; private set; }
    public Piece nextPiece5 { get; private set; }
    public Piece savedPiece { get; private set; }

    public Tile tile;
    public Bag bag;
    public Trash trash;
    public TetrominoData[] tetrominoes;

    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int previewPosition = new Vector3Int(10, 8, 0);
    public Vector3Int previewPosition2 = new Vector3Int(10, 5, 0);
    public Vector3Int previewPosition3 = new Vector3Int(10, 2, 0);
    public Vector3Int previewPosition4 = new Vector3Int(10, -1, 0);
    public Vector3Int previewPosition5 = new Vector3Int(10, -3, 0);
    public Vector3Int holdPosition = new Vector3Int(-10, 8, 0);
    
    public int score = 0;
    public TextMeshProUGUI scoreText;

        

    //Event
    public event UnityAction onGameOver;
    public event UnityAction<int, int, bool, bool> onPieceLock; //(LineClear, combo, isLastMoveRotation, isB2B)
    public event UnityAction onSetNextPiece;
    public event UnityAction onLineAddTrash;
    public event UnityAction onTrashSpawner;

    [Header("State")]
    private int b2bCount = 0;
    public bool b2b { get; private set; } = false;
    public int combo { get; private set; } = 0;

    [SerializeField]
    private int boardSeed = 12345;

    [SerializeField]
    private bool clearBoard = false;
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        InitializeNextPiece();
        SpawnPiece();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync(0);
        }
    }
    public void Init()
    { 
        score = 0;
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponent<Piece>();

        nextPiece = gameObject.AddComponent<Piece>();
        nextPiece.enabled = false;

        nextPiece2 = gameObject.AddComponent<Piece>();
        nextPiece2.enabled = false;

        nextPiece3 = gameObject.AddComponent<Piece>();
        nextPiece3.enabled = false;

        nextPiece4 = gameObject.AddComponent<Piece>();
        nextPiece4.enabled = false;

        nextPiece5 = gameObject.AddComponent<Piece>();
        nextPiece5.enabled = false;

        savedPiece = gameObject.AddComponent<Piece>();
        savedPiece.enabled = false;

        bag = new Bag();
        bag.SetBoardSeed(boardSeed);

        trash = new Trash();
        trash.SetBounds(Bounds);
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    public void GameOver()
    {
        if(clearBoard)
        {
            tilemap.ClearAllTiles();
            GameReset();
        }
        score = 0;
        if(onGameOver != null)
        {
            onGameOver.Invoke();
        }
        
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int linesCleared = 0;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
                linesCleared++;
            }
            else
            {
                row++;
            }

        }

        //Score calculation
        //Default
        score += linesCleared;
        if (linesCleared > 0) { Debug.Log("LinesCleared!! :" + linesCleared); }
        //Tetris
        if (linesCleared == 4)
        {
            score += linesCleared;
        }
        //All-Spin
        if (activePiece.isLastMoveRotation)
        {
            score += linesCleared;
        }

        if (linesCleared != 0 && linesCleared != 4 && !activePiece.isLastMoveRotation)
        {
            b2bCount = 0;
        }
        else if(linesCleared != 0)
        {
            b2bCount++;
        }

        if (linesCleared > 0)
        {
            combo++;
        }
        if (linesCleared == 0)
        {
            combo = 0;
        }

        if (b2bCount > 1)
        {
            b2b = true;
        }
        else
        {
            b2b = false;
        }
        
        if(onPieceLock != null)
        {
            onPieceLock.Invoke(linesCleared, combo, activePiece.isLastMoveRotation, b2b);
        }
        


        ScoreTextUpdate();
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }


    private void SetNextPiece()
    {
        var nextPieces = new[] { nextPiece, nextPiece2, nextPiece3, nextPiece4, nextPiece5 };
        var nextpreviewPositions = new[] { previewPosition, previewPosition2, previewPosition3, previewPosition4, previewPosition5 };
        //Goes through all nextPieces, Display and Set Data
        for (int i = 0; i < nextPieces.Length; i++)
        {
            var piece = nextPieces[i];
            var piecenext = nextPieces[i + 1];

            // Clear the piece
            if (piece.cells != null)
            {
                Clear(piece);
            }

            // Check if last loop, break and BagGetPiece() for the last piece
            if (piecenext == nextPiece5)
            {
                piece.Initialize(this, nextpreviewPositions[i], piecenext.data);
                Set(piece);
                Clear(piecenext);
                nextPiece5.Initialize(this, previewPosition5, BagGetPiece());
                Set(nextPiece5);
                break;
            }

            piece.Initialize(this, nextpreviewPositions[i], piecenext.data);
            Set(piece);
        }

        if(onSetNextPiece != null)
        {
            onSetNextPiece.Invoke();
        }
    }

    private TetrominoData BagGetPiece()
    {
        TetrominoData piece = tetrominoes[bag.GetPiece()];
        return piece;
    }
    private void InitializeNextPiece()
    {
        // Clear all next pieces if they exist
        var nextPieces = new[] { nextPiece, nextPiece2, nextPiece3, nextPiece4, nextPiece5 };
        var nextpreviewPositions = new[] { previewPosition, previewPosition2, previewPosition3, previewPosition4, previewPosition5 };
        foreach (var piece in nextPieces)
        {
            if (piece.cells != null)
            {
                Clear(piece);
            }
        }
        // initailize bag
        bag.RefillBagNoSZ();

        //Set all next pieces on the board
        for (int i = 0; i < nextPieces.Length; i++)
        {
            nextPieces[i].Initialize(this, nextpreviewPositions[i], BagGetPiece());
            Set(nextPieces[i]);
        }

    }


    public void SpawnPiece()
    {
        TrashSpawner();
        // Initialize the active piece with the next piece data
        activePiece.Initialize(this, spawnPosition, nextPiece.data);

        // Only spawn the piece if valid position otherwise game over
        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }

        // Set the next random piece
        SetNextPiece();
    }

    public void SwapPiece()
    {
        if (activePiece == null)
        {
            return;
        }

        if(savedPiece == null)
        {
            savedPiece = gameObject.AddComponent<Piece>();
            savedPiece.enabled = false;
        }
        // Temporarily store the current saved data so we can swap
        TetrominoData savedData = savedPiece.data;

        // Clear the existing saved piece from the board
        if (savedPiece != null && savedData.cells != null)
        {
            Clear(savedPiece);
        }

        // Store the next piece as the new saved piece
        // Draw this piece at the "hold" position on the board
        savedPiece.Initialize(this, holdPosition, activePiece.data);

        Set(savedPiece);

        // Swap the saved piece to be the active piece
        if (savedData.cells != null)
        {
            // Clear the existing active piece before swapping
            Clear(activePiece);

            // Re-initialize the active piece with the saved data
            // Draw this piece at the "preview" position on the board
            activePiece.Initialize(this, spawnPosition, savedData);
            Set(activePiece);
        }
        if (savedData.cells == null)
        {
            Clear(activePiece);
            SpawnPiece();
        }


    }

    public void LineAddTrash(int trashNumberOfLines, List<int> trashPreset)
    {
        RectInt bounds = Bounds;
        for (int i = 0; i < trashNumberOfLines; i++)
        {
            int row = bounds.yMax;
            // Shift every row up one
            while (row > bounds.yMin)
            {
                for (int col = bounds.xMin; col < bounds.xMax; col++)
                {
                    Vector3Int position = new Vector3Int(col, row - 1, 0);
                    TileBase below = tilemap.GetTile(position);

                    position = new Vector3Int(col, row, 0);
                    tilemap.SetTile(position, below);
                }

                row--;
            }

            //Fill trash
            int cellnum = 0;
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int positiont = new Vector3Int(col, bounds.yMin, 0);
                tilemap.SetTile(positiont, tile);
                //Prevent null reference
                if (cellnum < trashPreset.Count)
                {
                    //Leave Empty
                    if (trashPreset[cellnum] == 0)
                    {
                        tilemap.SetTile(positiont, null);
                        cellnum++;
                        continue;
                    }
                }
                cellnum++;
            }
        }

        if(onLineAddTrash != null)
        {
            onLineAddTrash.Invoke();
        }
    }

    public void TrashSpawner()
    {
        while (trash.GetTrashBuffer().Count > 0 && onTrashSpawner == null)
        {
            LineAddTrash(trash.GetTrashAmountRemove(), trash.TrashPresetGenerate());
        }

        if(onTrashSpawner != null)
        {
            onTrashSpawner.Invoke();
        }
    }


    private void ScoreTextUpdate()
    {
        scoreText.text = "score:" + score;
    }

    public bool[] GetField(bool includeActivePiece)
    {
        int width = boardSize.x;
        int height = boardSize.y;
        bool[] field = new bool[width * height];
        RectInt bounds = Bounds;
        // run through board to mark tiles
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3Int position = new Vector3Int(x + bounds.xMin, y + bounds.yMin, 0);
                field[y * width + x] = tilemap.HasTile(position);
            }

        }
        if (includeActivePiece)
        {
            // clean board of activePiece
            if (activePiece != null & activePiece.cells != null)
            {
                for (int i = 0; i < activePiece.cells.Length; i++)
                {
                    Vector3Int cellPos = activePiece.cells[i] + activePiece.position;
                    int fx = cellPos.x - bounds.xMin;
                    int fy = cellPos.y - bounds.yMin;
                    if (fx >= 0 && fx < width && fy >= 0 && fy < height)
                    {
                        field[fy * width + fx] = false;
                    }
                }
            }
        }


        return field;
    }
    
    public int GetBoardSize(int axis)
    {
        if (axis == 0)
        {
            return boardSize.x;
        }
        return boardSize.y;
    }
    public bool CheckAllClear()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMax;
        // Shift every row up one
        while (row >= bounds.yMin)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row, 0);
                TileBase currTile = tilemap.GetTile(position);
                if (currTile != null)
                {
                    return false;
                }
            }
            row--;
        }
        return true;
    }

    public void PieceMove(Movement movement)
    {
        Clear(activePiece);
        if (movement == Movement.LEFT)
        {
            activePiece.Move(Vector2Int.left);
        }

        if (movement == Movement.RIGHT)
        {
            activePiece.Move(Vector2Int.right);
        }

        if (movement == Movement.DROP)
        {
            for (int i = 0; i < 20; i++)
            {
                activePiece.Move(Vector2Int.down);
            }
        }

        if (movement == Movement.HARDDROP)
        {
            activePiece.HardDrop();
        }

        if (movement == Movement.SOFTDROP)
        {
            activePiece.Move(Vector2Int.down);
        }

        if (movement == Movement.CW)
        {
            activePiece.Rotate(1);
        }

        if (movement == Movement.CCW)
        {
            activePiece.Rotate(-1);
        }

        if (movement == Movement.HOLD)
        {
            SwapPiece();
        }

        Set(activePiece);
    }
    
    public void GameReset()
    {
        bag = new Bag();
        bag.SetBoardSeed(boardSeed);
        savedPiece = null;
        score = 0;
        InitializeNextPiece();
        SpawnPiece();
    }

    

}