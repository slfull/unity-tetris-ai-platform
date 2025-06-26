using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class TrashLine : MonoBehaviour
{
    public Board board { get; private set; }
    

    public void TempPrefabTSpinDouble()
    {
        //0是留空，剩下沒用到的會都是填入的
        List<int> trashPreset = new List<int> { 1, 1, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 1, 0, 1 };
        board.LineAddTrash(1, trashPreset);
    }

    public void TempPrefabTSpinTriple()
    {
        List<int> trashPreset = new List<int> { 1, 1, 0, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        board.LineAddTrash(1, trashPreset);
    }

    public void TempPrefabSSpinDouble()
    {
        List<int> trashPreset = new List<int> { 1, 1, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0 };
        board.LineAddTrash(1, trashPreset);
    }

    public void TempPrefabISpinSingle()
    {
        List<int> trashPreset = new List<int> { 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 0, 0, 0, 0 };
        board.LineAddTrash(1, trashPreset);
    }

    public void TempPrefabISpinTetris()
    {
        List<int> trashPreset = new List<int> { 1, 1, 0, 0, 0, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0, 0, 0, 0, 0, 0, 0, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        board.LineAddTrash(1, trashPreset);
        trashPreset = new List<int> { 1, 0 };
        board.LineAddTrash(1, trashPreset);
    }

    
}
