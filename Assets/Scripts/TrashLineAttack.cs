using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrashLineAttack : MonoBehaviour
{
    [Header("Board")]
    public Board playerBoard;
    public TrashLineAttack enemyBoard;

    [Header("TrashLineVal")]
    public List<float> trashlineBufferTimer;

    [Header("otherVal")]
    public int trashCount = 0;
    [SerializeField] private int combo = 0;
    [SerializeField] private bool prevClearB2B = false;
    [SerializeField] private float delayTime = 5.0f;
    [SerializeField] private Vector3 trashLineCountPosition = new Vector3(-6, -10, 0);
    [SerializeField] private GameObject trashLineCountUI;

    //Unity Function

    void Awake()
    {
        playerBoard = GetComponent<Board>();
        combo = 0;
        prevClearB2B = false;
    }

    void Update()
    {
        UpdateTrashLineCount();
    }

    //custom function

    public void HandleTrashLine(int lines, bool isLastMoveRotation)
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
            totalLine += ComboBonus();
        }

        if (isLastMoveRotation || lines == 4)
        {
            if (prevClearB2B && lines < 4)
            {
                totalLine += lines;
            }
            else if (prevClearB2B)
            {
                totalLine += 2;
            }
            prevClearB2B = true;
        }
        else
        {
            prevClearB2B = false;
        }

        if (lines > 0)
        {
            combo++;
        }
        else
        {
            combo = 0;
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
            if (lines >= playerBoard.trashBuffer[0])
            {
                lines -= playerBoard.trashBuffer[0];
                trashCount -= playerBoard.trashBuffer[0];
                playerBoard.trashBuffer.RemoveAt(0);
                trashlineBufferTimer.RemoveAt(0);
            }
            else
            {
                playerBoard.trashBuffer[0] -= lines;
                trashCount -= lines;
                lines = 0;
            }
        }
    }

    private void TrashSend(int lines)
    {
        enemyBoard.playerBoard.trashBuffer.Add(lines);
        enemyBoard.trashlineBufferTimer.Add(Time.time + delayTime);
        enemyBoard.trashCount += lines;
    }

    private void UpdateTrashLineCount()
    {
        enemyBoard.trashLineCountUI.transform.position = trashLineCountPosition + new Vector3(0, 0.5f * enemyBoard.trashCount, 0);
        enemyBoard.trashLineCountUI.transform.localScale = new Vector3(1, enemyBoard.trashCount, 1);
    }

    private int ComboBonus()
    {
        return combo > 4 ? 4 : combo;
    }

    private bool IsAllClear()
    {
        return playerBoard.CheckAllClear();
    }

}
