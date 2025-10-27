using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Collections;
using Unity.MLAgents.Integrations.Match3;


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
    public TetrominoData[] tetrominoes;

    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int previewPosition = new Vector3Int(10, 8, 0);
    public Vector3Int previewPosition2 = new Vector3Int(10, 5, 0);
    public Vector3Int previewPosition3 = new Vector3Int(10, 2, 0);
    public Vector3Int previewPosition4 = new Vector3Int(10, -1, 0);
    public Vector3Int previewPosition5 = new Vector3Int(10, -3, 0);
    public Vector3Int holdPosition = new Vector3Int(-10, 8, 0);
    public List<int> trashBuffer = new List<int>();
    public int score = 0;

    public TextMeshProUGUI scoreText;
    private TetrisAgent agent;
    private TrashLineAttack attacker;
    private ColdClearAgent coldClearAgent;

    [Header("AgentObservations")]
    public int distanceFromBottom = 0;
    public int distanceFromBottomLast;
    public int numberOfHoles = 0;
        public int numberOfHolesLast = 0;
    public int numberOfOverHangs = 0;
    public int completedLines = 0;
    public float aggregateHeight = 0;
    public float bumpiness = 0;
    public float density = 0;
    public int[] columnheight;
    public int[] rowheight;



    //Add more RewardType if needed
    public enum RewardType
    {
        GameOver, LineClear
    }
    public bool agentExists { get; private set; } = false;
    public bool attackerExists { get; private set; } = false;
    public bool ccExists { get; private set; } = false;
    private bool isInit = true;
    private int b2bCount = 0;
    public bool b2b { get; private set; } = false;
    public int combo { get; private set; } = 0;

    [SerializeField]
    private int seed = 12345;
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
        isInit = true;

        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

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

        
        

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }

        coldClearAgent = GetComponent<ColdClearAgent>();
        if (coldClearAgent != null)
        {
            ccExists = true;
        }
    }

    private void Start()
    {
        Init();
        InitializeNextPiece();
        SpawnPiece();
        isInit = false;
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.C)) && !activePiece.isPlayerTwo)
        {
            SwapPiece();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            TempAddTrashFunction();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadSceneAsync(0);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            PrintObservations();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            PrintField();
        }
        CalculateObservations();
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        score = 0;
        if (agentExists)
        {
            AgentReward((int)RewardType.GameOver, 1);
            agent.EndEpisode();
        }
        Start();
        Debug.Log("gameover");


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
            if (IsLineFull(row) == 0)
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

            if (agentExists) { AgentReward((int)RewardType.LineClear, linesCleared); }
        }
        //All-Spin
        if (activePiece.isLastMoveRotation)
        {
            score += linesCleared;
            if (agentExists) { AgentReward((int)RewardType.LineClear, linesCleared); }
        }
        if (agentExists)
        {
            AgentReward((int)RewardType.LineClear, linesCleared);
        }

        if (attackerExists)
        {
            attacker.HandleTrashLine(linesCleared, activePiece.isLastMoveRotation);
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

        if (b2b == true)
        {
            //Debug.Log("B2B: " + b2b);
        }
        if (combo > 1)
        {
            //Debug.Log("combo: " + combo);
        }
        ScoreTextUpdate();
    }

    public int IsLineFull(int row)
    {
        RectInt bounds = Bounds;
        int lineEmptyAmout = 0;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position))
            {
                lineEmptyAmout++;
            }
        }

        if (lineEmptyAmout > 0)
        {
            return lineEmptyAmout;
        }
        return 0;
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

        if (ccExists && !isInit)
        {
            coldClearAgent.pieceCounter++;
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
        // Temporarily store the current saved data so we can swap
        TetrominoData savedData = savedPiece.data;

        // Clear the existing saved piece from the board
        if (savedData.cells != null)
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

        if (ccExists)
        {
            coldClearAgent.needReset = true;
        }


    }
    public List<int> TrashPresetGenerate()
    {
        RectInt bounds = Bounds;
        int length = bounds.xMax - bounds.xMin;
        List<int> trashPreset = new List<int>(new int[length]);

        for (int i = 0; i < length; i++)
        {
            trashPreset[i] = 1;
        }


        //0是留空，剩下沒用到的會都是填入的
        int index = UnityEngine.Random.Range(0, length); // Random index for 0
        trashPreset[index] = 0;


        return trashPreset;
    }

    public void TrashSpawner()
    {
        while (trashBuffer.Count > 0 && !attackerExists)
        {
            int trashAmount = trashBuffer[0];
            LineAddTrash(trashAmount, TrashPresetGenerate());
            trashBuffer.RemoveAt(0);
        }

        while (attackerExists && trashBuffer.Count > 0 && attacker.trashlineBufferTimer[0] <= Time.time)
        {
            int trashAmount = trashBuffer[0];
            LineAddTrash(trashAmount, TrashPresetGenerate());
            attacker.trashCount -= trashBuffer[0];
            attacker.trashlineBufferTimer.RemoveAt(0);
            trashBuffer.RemoveAt(0);
        }
    }

    //暫定function，之後移除/更改位置
    public void TempAddTrashFunction()
    {
        //這裡的1是垃圾行數
        trashBuffer.Add(1);
    }

    private void ScoreTextUpdate()
    {
        scoreText.text = "score:" + score;
    }

    public void Init()
    {
        bag = new Bag();
        agent = GetComponent<TetrisAgent>();
        if (agent != null)
        {
            agentExists = true;
            agent.OnEpisodeBegin();
        }
        else
        {
            agentExists = false;
        }
        attacker = GetComponent<TrashLineAttack>();
        if (attacker != null)
        {
            attackerExists = true;
        }
        else
        {
            attackerExists = false;
        }
        score = 0;
        distanceFromBottom = 0;
        numberOfHoles = 0;
        density = 0;
        bag.SetBoardSeed(seed);
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
        distanceFromBottom = 0;


        return field;
    }
    public void CalculateObservations()
    {
        bool[] fieldWithActivepiece = GetField(false);
        bool[] fieldNOActivepiece = GetField(true);
        int width = boardSize.x;
        int height = boardSize.y;
        columnheight = new int[width];
        rowheight = new int[height];
        
        
        numberOfHoles = 0;
        numberOfOverHangs = 0;
        aggregateHeight = 0;
        bumpiness = 0;
        density = 0;
        completedLines = 0;
        // run through cleaned board again for observations
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                // columnheight, rowheight
                if (fieldWithActivepiece[index] == true)
                {
                    columnheight[x] = y + 1;
                    rowheight[y]++;
                }
                //numberOfHoles = check block above, left and right, if all is filled then numberOfHoles++

                else
                {
                    int positionabove = 0;
                    int positionleft = 0;
                    int positionright = 0;
                    if (y + 1 < height)
                    {
                        positionabove = (y + 1) * width + x;
                        if (fieldWithActivepiece[positionabove] == false) { continue; }
                        numberOfOverHangs++;
                    }
                    if (x - 1 >= 0)
                    {
                        positionleft = y * width + x - 1;
                        if (fieldWithActivepiece[positionleft] == false) { continue; }
                    }
                    if (x + 1 < width)
                    {
                        positionright = y * width + x + 1;
                        if (fieldWithActivepiece[positionright] == false) { continue; }
                    }
                    numberOfHoles++;
                }
            }

        }


        //aggregateHeight = sum of the height of each column
        for (int i = 0; i < columnheight.Length; i++)
        {
            aggregateHeight += columnheight[i];
        }

        //bumpiness = summing up the absolute differences between all two adjacent columns. (for well{tetris-clear setup} generalization)
        for (int i = 0; i < columnheight.Length; i++)
        {
            if (i < columnheight.Length - 1)
            {
                bumpiness += Mathf.Abs(columnheight[i] - columnheight[i + 1]);
            }
            else if (i == columnheight.Length - 1)
            {
                break;
            }

        }

        //density = sum of filled tiles / numberOfUnfilledLines(non-empty lines)
        int numberOfTiles = 0;
        int numberOfUnfilledLines = 0;
        for (int i = 0; i < rowheight.Length; i++)
        {
            numberOfTiles += rowheight[i];
            if (rowheight[i] != 0 && rowheight[i] != width) { numberOfUnfilledLines++; }
            if (rowheight[i] == width) { completedLines++; }
        }
        if (numberOfUnfilledLines != 0) { density = numberOfTiles / numberOfUnfilledLines; }

        //distanceFromBottom = 0;

        if (numberOfHoles < numberOfHolesLast) { AgentReward(4, 1); }
        if (numberOfHoles > numberOfHolesLast) { AgentReward(4, -1); }
        numberOfHolesLast = numberOfHoles;


    }

    public int GetBoardSize(int axis)
    {
        if (axis == 0)
        {
            return boardSize.x;
        }
        return boardSize.y;
    }

    public void AgentReward(int rewardType, int rewardMultiplier)
    {
        if (!agentExists)
        {
            return;
        }

        // 0 = GameOver, 1 = LineClear
            float lineClearReward = 0.5f;
        float LockPieceReward = 0.01f;
        float HoleReward = 0.01f;
        lineClearReward *= rewardMultiplier;
        HoleReward *= rewardMultiplier;
        switch (rewardType)
        {
            case 0: agent.AddReward(-2f); break;
            case 1: agent.AddReward(lineClearReward); break;
            case 2: agent.AddReward(LockPieceReward); break;
            case 3: agent.AddReward(-0.02f); break;
            case 4: agent.AddReward(HoleReward); break;
        }
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

    public void PrintObservations()
    {
        CalculateObservations();
        Debug.Log("activePiece.position.x:" + activePiece.position.x);
        Debug.Log("activePiece.position.y:" + activePiece.position.y);
        Debug.Log("activePiece.data.tetromino:" + activePiece.data.tetromino);
        Debug.Log("activePiece.rotationIndex:" + activePiece.rotationIndex);
        Debug.Log("distanceFromBottom:" + distanceFromBottom);
        Debug.Log("numberOfHoles:" + numberOfHoles);
        Debug.Log("numberOfOverHangs:" + numberOfOverHangs);
        Debug.Log("aggregateHeight:" + aggregateHeight);
        Debug.Log("bumpiness:" + bumpiness);
        Debug.Log("density:" + density);
        string columnheightprint = "columnheight:";
        for (int i = 0; i < columnheight.Length; i++)
        {
            columnheightprint += "" + columnheight[i];
        }
        Debug.Log(columnheightprint);
        string rowheightprint = "rowheight:";
        for (int i = 0; i < rowheight.Length; i++)
        {
            rowheightprint += "" + rowheight[i];
        }
        Debug.Log(rowheightprint);
        PrintField();

    }

    //NOTE: prints from top to down(reverse)
    void PrintField()
    {
        bool[] fields = GetField(false);
        int width = boardSize.x;
        int height = boardSize.y;
        for (int y = height - 1; y >= 0; y--)
        {
            string row = $"y={y}: [";
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                row += fields[index] ? " 1 " : " 0 ";
            }
            row += "]";
            Debug.Log(row);
        }
    }

}