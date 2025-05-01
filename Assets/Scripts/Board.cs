using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro;
using Mirror;
using System;
using UnityEngine.UIElements;
using static UnityEngine.Networking.UnityWebRequest;
using System.Collections;

[DefaultExecutionOrder(-1)]
public class Board : NetworkBehaviour
{
    
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public Piece nextPiece { get; private set; }
    public Piece nextPiece2 { get; private set; }
    public Piece nextPiece3 { get; private set; }
    public Piece nextPiece4 { get; private set; }
    public Piece nextPiece5 { get; private set; }
    public Piece savedPiece { get; private set; }

    public Tile tile;

    public Ghost ghost;

    public TetrisNetworkManager tetrisNetworkManager;

    public TetrominoData[] tetrominoes;

    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int previewPosition = new Vector3Int(10, 8, 0);
    public Vector3Int previewPosition2 = new Vector3Int(10, 5, 0);
    public Vector3Int previewPosition3 = new Vector3Int(10, 2, 0);
    public Vector3Int previewPosition4 = new Vector3Int(10, -1, 0);
    public Vector3Int previewPosition5 = new Vector3Int(10, -3, 0);
    public Vector3Int holdPosition = new Vector3Int(-10, 8, 0);
    public readonly List<int> bagConst = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
    public List<int> bag = new List<int>();
    public List<int> trashBuffer = new List<int>();
    public List<float> trashBufferDelay = new List<float>();
    public int score = 0;
    private int comboCount = 0;
    private bool prevClearB2B = false;
    private float trashBufferDelayOffset = 5.0f;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI trashBufferDelayText;

    [SyncVar]
    public bool isGameStart = false;
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    void CopyBag(List<int> source, List<int> target)
    {
        target.Clear(); 
        target.AddRange(source);
    }

    public static void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1); // Get a random index
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static void ShuffleWithConstraints<T>(List<T> list)
    {
        Shuffle(list);

        // Ensure that 5 or 7 is not at the 0th position
        while (list[0].Equals(6) || list[0].Equals(4))
        {
            // If 5 or 7 is at the 0th position, swap it with a random position (other than 0)
            System.Random rng = new System.Random();
            int randomIndex = rng.Next(1, list.Count); // Ensure the index is not 0
            T temp = list[0];
            list[0] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    private void Awake()
    {
        tetrisNetworkManager = GameObject.Find("NetworkManager").GetComponent<TetrisNetworkManager>();
        ghost = GetComponentInChildren<Ghost>();

        prevClearB2B = false;
        comboCount = 0;

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
        CopyBag(bagConst, bag);
    }

    private void StartGame()
    {
        Debug.Log($"[Board] isServer: {isServer}, isClient: {isClient}, isOwned: {isOwned}");
        if(!isOwned)
        {
            return;
        }
        InitializeNextPiece();
        //TempPrefabTetris();
        SpawnPiece();
    }
    
    [TargetRpc]
    public void TargetStartGame(NetworkConnection target)
    {
        StartGame();
    }

    [Server]
    public void StartGameOnServer()
    {
        if(connectionToClient != null)
        {
            TargetStartGame(connectionToClient);
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
            var piecenext = nextPieces[i+1];

            // Clear the piece
            if (piece.cells != null)
            {
                Clear(piece);
            }

            // Check if last loop, break and BagTakeNextPiece() for the last piece
            if (piecenext == nextPiece5)
            {
                piece.Initialize(this, nextpreviewPositions[i], piecenext.data);
                Set(piece);
                Clear(piecenext);
                nextPiece5.Initialize(this, previewPosition5, BagTakeNextPiece());
                Set(nextPiece5);
                break;
            }

            piece.Initialize(this, nextpreviewPositions[i], piecenext.data);
            Set(piece);


        }
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

        //Initialize Bag
        CopyBag(bagConst, bag);
        //Ensure that Z and S is not the first piece
        ShuffleWithConstraints(bag);

        //Set all next pieces on the board
        for (int i = 0; i < nextPieces.Length; i++)
        {
            nextPieces[i].Initialize(this, nextpreviewPositions[i], BagTakeNextPiece());
            Set(nextPieces[i]);
        }
        
    }

    private TetrominoData BagTakeNextPiece()
    {
        // Pick a random tetromino to use
        int random = bag[0];
        TetrominoData data = tetrominoes[random];
        bag.RemoveAt(0);
        if (bag.Count == 0)
        {
            CopyBag(bagConst, bag);
            Shuffle(bag);
        }
        return data;
    }

    public void SpawnPiece()
    {
        TrashSpawner();
        // Initialize the active piece with the next piece data
        activePiece.enabled = true;
        ghost.PieceAwake();
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

    private void Update()
    {
        if(isOwned && isGameStart)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || Input.GetKeyDown(KeyCode.C))
            {
                SwapPiece();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                TempAddTrashFunction();
            }
            TrashLineCountDownTextUpdate();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        score = 0;
        // TODO
    }
    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
            if(!isServer)
            {
                CmdSetTile(tilePosition, piece.data.tetromino);
            }
            else
            {
                RpcSet(tilePosition, piece.data.tetromino);
            }
            
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
            if(!isServer)
            {
                CmdClearTile(tilePosition);
            }
            else
            {
                RpcClear(tilePosition);
            }
            
        }
        if(isServer)
        {
            //RpcClear(positions);
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
        SendTrashLine(linesCleared);

        //Score calculation
        //Default
        score += linesCleared;
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
        ScoreTextUpdate();
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        if(!isServer)
        {
            CmdLineClear(row);
        }
        else
        {
            RpcLineClear(row);
        }
        LocalLineClear(row);
    }

    public void LineAddTrash(int trashNumberOfLines, List<int> trashPreset)
    {
        LocalLineAddTrash(trashNumberOfLines, trashPreset);
        if(!isServer)
        {
            CmdLineAddTrash(trashNumberOfLines, trashPreset);
        }
        else
        {
            RpcLineAddTrash(trashNumberOfLines, trashPreset);
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
        while (trashBuffer.Count > 0)
        {
            if(Time.time < trashBufferDelay[0])
            {
                break;
            }
            int trashAmount = trashBuffer[0];
            LineAddTrash(trashAmount, TrashPresetGenerate());
            trashBuffer.RemoveAt(0);
            trashBufferDelay.RemoveAt(0);
        }
    }
    private void SendTrashToOppoent(int trashAmount)
    {
        foreach(var board in FindObjectsByType<Board>(FindObjectsSortMode.None))
        {
            if(board != this)
            {
                if(isServer)
                {
                    board.RpcSendTrashLine(trashAmount); 
                }
                else
                {
                    board.CmdSendTrashLine(trashAmount); 
                }
            }
        }
    }
    

    //暫定function，之後移除/更改位置
    public void TempPrefabTSpinDouble()
    {
        //0是留空，剩下沒用到的會都是填入的
        List<int> trashPreset = new List<int> { 1, 1, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 1, 0, 1 };
        LineAddTrash(1, trashPreset);
    }

    public void TempPrefabTSpinTriple()
    {
        List<int> trashPreset = new List<int> { 1, 1, 0, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        LineAddTrash(1, trashPreset);
    }

    public void TempPrefabSSpinDouble()
    {
        List<int> trashPreset = new List<int> { 1, 1, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0 };
        LineAddTrash(1, trashPreset);
    }

    public void TempPrefabISpinSingle()
    {
        List<int> trashPreset = new List<int> { 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 0, 0, 0, 0 };
        LineAddTrash(1, trashPreset);
    }

    public void TempPrefabISpinTetris()
    {
        List<int> trashPreset = new List<int> { 1, 1, 0, 0, 0, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int>{ 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        LineAddTrash(1, trashPreset);
    }
    public void TempPrefabTetris()
    {
        List<int> trashPreset = new List<int> { 1, 0 };
        for(int i = 0; i < 4; i++)
        {
            LineAddTrash(1, trashPreset);
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
    private void TrashLineCountDownTextUpdate()
    {
        if(trashBufferDelay.Count == 0)
        {
            trashBufferDelayText.text = "";
            return;
        }
        int currCountDown = Mathf.RoundToInt(trashBufferDelay[0] - Time.time);
        if(currCountDown <= 0)
        {
            trashBufferDelayText.text = "TrashLineCountDown: 0";
        }
        else
        {
            trashBufferDelayText.text = "TrashLineContDown: " + currCountDown;
        }
    }
    private int ComboToTrashLine(int totalLine)
    {
        if(totalLine > 0)
        {
            comboCount++;
        }
        else
        {
            comboCount = 0;
        }
        return (comboCount < 0) ? 0 : ((comboCount - 1 < 4) ? comboCount - 1 : 4); //if(comboCount < 0) 0 else min(comboCount - 1, 4);
    }
    private bool B2BCheck(int totalLine)
    {
        if(totalLine == 4 || (activePiece.isLastMoveRotation && activePiece.data.tetromino == Tetromino.T && totalLine > 0)) //tetris or T-spin
        {
            if(prevClearB2B) // check prev line clear B2B
            {
                return true;
            }
            prevClearB2B = true;
        }
        else if(totalLine > 0) // normal line clear
        {
            prevClearB2B = false;
        }
        return false;
    }
    private bool CheckAllClear()
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
                if(currTile != null)
                {
                    return false;
                }
            }
            row--;
        }
        return true;
    }
    private void SendTrashLine(int totalLine)
    {
        int totalTrash = 0;
        int comboTrash = ComboToTrashLine(totalLine);
        bool isB2B = B2BCheck(totalLine);
        bool isAllClear = CheckAllClear();
        if(activePiece.isLastMoveRotation)
        {
            totalTrash = totalLine * 2;
            if(isB2B)
            {
                totalTrash += totalLine;
            }
        }
        else if (totalLine > 0)
        {
            if(totalLine == 4)
            {
                totalTrash = 4;
                if(isB2B)
                {
                    totalTrash += 2;
                }
            }
            else
            {
                totalTrash = totalLine - 1;
            }
        }
        if(comboTrash > 0)
        {
            totalTrash += comboTrash;
        }
        if(isAllClear)
        {
            totalTrash += 10;
        }
        Debug.Log("totalTrash: " + totalTrash + " comboTrash: " + comboTrash + " isB2B: " + isB2B + " isAllClear: " + isAllClear);
        SendTrashToOppoent(totalTrash);
        TrashLineOffset(totalTrash);
    }
    private void GetAttack(int lines)
    {
        trashBuffer.Add(lines);
        trashBufferDelay.Add(Time.time + trashBufferDelayOffset);
    }
    private void TrashLineOffset(int lines)
    {
        if(trashBuffer.Count == 0)
        {
            return;
        }
        trashBuffer[0] -= lines;
        if(trashBuffer[0] <= 0)
        {
            trashBuffer.RemoveAt(0);
            trashBufferDelay.RemoveAt(0);
        }
    }

    private Tile GetTileFromType(Tetromino type)
    {
        foreach(var data in tetrominoes)
        {
            if(data.tetromino == type)
            {
                return data.tile;
            }
        }
        return null;
    }
    private void LocalLineClear(int row)
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
    private void LocalLineAddTrash(int trashNumberOfLines, List<int> trashPreset)
    {
        RectInt bounds = Bounds;
        for (int i = 0; i< trashNumberOfLines; i++)
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
    }

    [Command]
    private void CmdSetTile(Vector3Int position, Tetromino type)
    {
        tilemap.SetTile(position, GetTileFromType(type));
    }
    [Command]
    private void CmdClearTile(Vector3Int position)
    {
        tilemap.SetTile(position, null);
    }
    [Command]
    private void CmdLineClear(int row)
    {
        LocalLineClear(row);
    }
    [Command]
    private void CmdLineAddTrash(int trashNumberOfLines, List<int> trashPreset)
    {
        LocalLineAddTrash(trashNumberOfLines, trashPreset);
    }
    [Command(requiresAuthority = false)]
    public void CmdSendTrashLine(int lines)
    {
        if(lines > 0)
        {
            GetAttack(lines);
        }
        Debug.Log("get trash: " + lines);
    }

    [ClientRpc]
    public void RpcSet(Vector3Int position, Tetromino type)
    {
        if(isServer)
        {
            return;
        }
        tilemap.SetTile(position, GetTileFromType(type));
    }
    
    [ClientRpc]
    public void RpcClear(Vector3Int position)
    {
        if(isServer)
        {
            return;
        }
        tilemap.SetTile(position, null);
    }
    
    [ClientRpc]
    public void RpcLineClear(int row)
    {
        if(isServer)
        {
            return;
        }
        Debug.Log("RpcLineClear");
        LocalLineClear(row);
    }

    [ClientRpc]
    public void RpcLineAddTrash(int trashNumberOfLines, List<int> trashPreset)
    {
        if(isServer)
        {
            return;
        }
        LocalLineAddTrash(trashNumberOfLines, trashPreset);
    }
    
    [ClientRpc]
    public void RpcSendTrashLine(int lines)
    {
        if(isServer)
        {
            return;
        }
        if(lines > 0)
        {
            GetAttack(lines);
        }
        Debug.Log("get trash: " + lines);
    }

    /**
     * debug list print
     *  string r = "list:";
     for(int i = 0; i < list.Count; i++)
        {
            r += " " + list[i];
        }
        Debug.Log(r);
     **/
}