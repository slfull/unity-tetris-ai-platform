using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardToColdClear : MonoBehaviour
{
    public static BoardToColdClear instance;
    private Board board;
    private Dictionary<Tetromino, CCPiece> tetrominoMap;

    private void Initialize()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        board = GetComponent<Board>();

        tetrominoMap = new Dictionary<Tetromino, CCPiece>()
        {
            { Tetromino.I, CCPiece.CC_I },
            { Tetromino.J, CCPiece.CC_J },
            { Tetromino.L, CCPiece.CC_L },
            { Tetromino.O, CCPiece.CC_O },
            { Tetromino.S, CCPiece.CC_S },
            { Tetromino.Z, CCPiece.CC_Z },
            { Tetromino.T, CCPiece.CC_T }
        };
    }
    void Awake()
    {
        Initialize();
    }

    // 轉換棋盤為 coldclear 格式
    public bool[] GetFieldBoolArray()
    {
        int width = board.boardSize.x;
        int height = board.boardSize.y;
        bool[] field = new bool[10 * 40];
        bool[] untransformedField = board.GetField(true);
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 40; x++)
            {
                int index = y * width + x;
                if (width * height <= y * width + x)
                {
                    field[index] = false;
                    continue;
                }
                field[index] = untransformedField[index];
            }
        }

        // Debug: 印出 field 的每一行（從上到下）
        Debug.Log("=== ColdClear Field Debug ===");
        for (int y = 40 - 1; y >= 0; y--)
        {
            string row = $"y={y}: [";
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                row += field[index] ? " 1 " : " 0 ";
            }
            row += "]";
            Debug.Log(row);
        }
        Debug.Log($"field[0] (左下): {field[0]}, field[{width - 1}] (右下): {field[width - 1]}, field[{width * (height - 1)}] (左上): {field[width * (height - 1)]}, field[{width * height - 1}] (右上): {field[width * height - 1]}");

        return field;
    }

    // 取得 hold
    public CCPiece? GetHoldPiece()
    {
        if (board.savedPiece != null)
        {
            return TetrominoToCCPiece(board.savedPiece.data.tetromino);
        }
        return null;
    }

    // 取得 queue
    public CCPiece[] GetQueue()
    {
        List<CCPiece> queue = new List<CCPiece>
        {
            TetrominoToCCPiece(board.activePiece.data.tetromino),
            TetrominoToCCPiece(board.nextPiece.data.tetromino),
            TetrominoToCCPiece(board.nextPiece2.data.tetromino),
            TetrominoToCCPiece(board.nextPiece3.data.tetromino),
            TetrominoToCCPiece(board.nextPiece4.data.tetromino),
            TetrominoToCCPiece(board.nextPiece5.data.tetromino)
        };
        return queue.ToArray();
    }

    // 轉換 Tetromino
    public CCPiece TetrominoToCCPiece(Tetromino t)
    {
        return tetrominoMap[t];
    }

    public CCPiece GetNewestPiece()
    {
        return TetrominoToCCPiece(board.nextPiece5.data.tetromino);
    }
}
