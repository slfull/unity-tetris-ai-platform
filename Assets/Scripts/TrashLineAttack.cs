using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashLineAttack : MonoBehaviour
{
    [Header("Board")]
    public Board board;
    public TrashLineAttack enemyBoard;

    [Header("TrashLineVal")]
    public List<float> trashlineBufferTimer;

    [Header("otherVal")]
    public int trashCount = 0;
    [SerializeField] private float delayTime = 5.0f;
    [SerializeField] private Vector3 trashLineCountPosition = new Vector3(-6, -10, 0);
    [SerializeField] private GameObject trashLineCountUI;

    //Unity Function

    void Awake()
    {
        board = GetComponent<Board>();
        board.onPieceLock += OnPieceLock;
        board.onTrashSpawner += OnTrashSpawner;
    }

    void Update()
    {
        UpdateTrashLineCount();
    }

    //custom function

    public void HandleTrashLine(int lines, int combo, bool isLastMoveRotation, bool b2b)
    {
        int totalLine = lines > 1 ? lines - 1 : 0;
        if (lines == 4)
        {
            totalLine++;
        }

        if (isLastMoveRotation)
        {
            totalLine += lines;
            if (lines == 1)
            {
                totalLine++;
            }
        }

        if (lines > 0)
        {
            totalLine += ComboBonus(combo);
        }

        if (b2b && lines == 4)
        {
            totalLine += 2;
        }
        
        if(b2b && isLastMoveRotation)
        {
            totalLine += lines;
        }

        if (IsAllClear())
        {
            totalLine += 10;
        }

        TrashSend(totalLine);
        CounteringGarbage(totalLine);
    }

    private void CounteringGarbage(int lines)
    {
        while (trashlineBufferTimer.Count > 0 && lines > 0)
        {
            if (lines >= board.trashBuffer[0])
            {
                lines -= board.trashBuffer[0];
                trashCount -= board.trashBuffer[0];
                board.trashBuffer.RemoveAt(0);
                trashlineBufferTimer.RemoveAt(0);
            }
            else
            {
                board.trashBuffer[0] -= lines;
                trashCount -= lines;
                lines = 0;
            }
        }
    }

    private void TrashSend(int lines)
    {
        enemyBoard.board.trashBuffer.Add(lines);
        enemyBoard.trashlineBufferTimer.Add(Time.time + delayTime);
        enemyBoard.trashCount += lines;
    }

    private void UpdateTrashLineCount()
    {
        enemyBoard.trashLineCountUI.transform.position = trashLineCountPosition + new Vector3(0, 0.5f * enemyBoard.trashCount, 0);
        enemyBoard.trashLineCountUI.transform.localScale = new Vector3(1, enemyBoard.trashCount, 1);
    }

    private int ComboBonus(int combo)
    {
        return combo > 4 ? 4 : combo;
    }

    private bool IsAllClear()
    {
        return board.CheckAllClear();
    }

    public void OnPieceLock(int line, int combo, bool isRotation, bool b2b)
    {
        HandleTrashLine(line, combo, isRotation, b2b);
    }
    public void OnTrashSpawner()
    {
        while (board.trashBuffer.Count > 0 && trashlineBufferTimer[0] <= Time.time)
        {
            int trashAmount = board.trashBuffer[0];
            board.LineAddTrash(trashAmount, board.TrashPresetGenerate());
            trashCount -= board.trashBuffer[0];
            trashlineBufferTimer.RemoveAt(0);
            board.trashBuffer.RemoveAt(0);
        }
    }
}
