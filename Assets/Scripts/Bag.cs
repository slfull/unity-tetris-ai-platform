using System;
using System.Collections.Generic;
using System.Linq;

public class Bag
{
    private List<Tetromino> pieces = new List<Tetromino>
    {
        Tetromino.I,
        Tetromino.J,
        Tetromino.L,
        Tetromino.O,
        Tetromino.S,
        Tetromino.T,
        Tetromino.Z
    };
    private List<Tetromino> bag = new List<Tetromino>();
    private int boardSeed = new int();
    private static Random rng;
    public int GetPiece()
    {
        RefillBag();

        int piece = (int)bag[0];
        bag.RemoveAt(0);

        return piece;
    }


    
    public void SetRNGSeed()
    {
        rng = new Random(boardSeed);
        boardSeed += 1;
        IListExtension.SetRandomSeed(rng.Next(0,10000));
    }
    public void SetBoardSeed(int seedInput)
    {
        boardSeed = seedInput;
    }

    public int[] DumpBagInfo()
    {
        RefillBag();
        int[] bagInfo = { bag.Count };
        for (int i = 0; i < bag.Count; i++)
        {
            bagInfo[i] = (int)bag[i];
        }
        return bagInfo;
    }

    private void RefillBag()
    {
        if (bag.IsNullOrEmpty())
        {
            bag = new List<Tetromino>(pieces);
            SetRNGSeed();
            bag.Shuffle();
        }
    }
    // call this when starting game, to prevent S and Z piece from the start
    public void RefillBagNoSZ()
    {

        if (bag.IsNullOrEmpty())
        {
            bag = new List<Tetromino>(pieces);
            SetRNGSeed();
            bag.Shuffle();
        }
        int piece = (int)bag[0];
        while (piece == 4 || piece == 6)
        {
            bag = new List<Tetromino>(pieces);
            SetRNGSeed();
            piece = (int)bag[0];
        }
    }

    public List<Tetromino> GetBag()
    {
        return bag;
    }
}