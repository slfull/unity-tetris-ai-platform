using System;
using System.Collections.Generic;
using System.Linq;

public class BoardObservationOld
{
    /**
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
    
    public int GetBoardSize(int axis)
    {
        if (axis == 0)
        {
            return boardSize.x;
        }
        return boardSize.y;
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
                if (fieldNOActivepiece[index] == true)
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
                        if (fieldNOActivepiece[positionabove] == false) { continue; }
                        numberOfOverHangs++;
                    }
                    if (x - 1 >= 0)
                    {
                        positionleft = y * width + x - 1;
                        if (fieldNOActivepiece[positionleft] == false) { continue; }
                    }
                    if (x + 1 < width)
                    {
                        positionright = y * width + x + 1;
                        if (fieldNOActivepiece[positionright] == false) { continue; }
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
        numberOfHolesLast = numberOfHoles;
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
    **/
}