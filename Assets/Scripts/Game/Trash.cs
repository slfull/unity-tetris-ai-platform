using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Trash
{
    private List<int> trashBuffer = new List<int>();

    //default board
    private RectInt Bounds { get; set; } = new RectInt(new Vector2Int(-5, -10), new Vector2Int(10, 20));

    public int GetTrashAmountRemove()
    {
        int trashAmount = trashBuffer[0];
        trashBuffer.RemoveAt(0);
        return trashAmount;
    }
    
    public List<int> GetTrashBuffer()
    {
        return trashBuffer;
    }

    public void TrashBufferAdd(int value)
    {
        trashBuffer.Add(value);
    }

    public void SetBounds(RectInt bounds)
    {
        Bounds = bounds;
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


}