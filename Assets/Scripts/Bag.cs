using System.Collections.Generic;

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

    public int GetPiece()
    {
        RefillBag();

        int piece = (int)bag[0];
        bag.RemoveAt(0);

        return piece;
    }

    public int[] DumpBagInfo()
    {
        RefillBag();
        int[] bagInfo = new bagInfo[bag.length];
        for (i = 0; i < bag.length; i++)
        {
            bagInfo[i] = bag[i];
        }
        return bagInfo;
    }

    private void RefillBag()
    {
        if (bag.IsNullOrEmpty())
        {
            bag = new List<Tetromino>(pieces);
            bag.Shuffle();
        }
    }
    // call this when starting game, to prevent S and Z piece from the start
    public void RefillBagNoSZ()
    {

        if (bag.IsNullOrEmpty())
        {
            bag = new List<Tetromino>(pieces);
            bag.Shuffle();
        }
        int piece = (int)bag[0];
        while (piece == 4 || piece == 6)
        {
            bag = new List<Tetromino>(pieces);
            bag.Shuffle();
            piece = (int)bag[0];
        }
    }
    
    
}