using System;
using System.Collections.Generic;
using UnityEngine;
public static class Observations
{
    public static (int[] columnHeight, int aggregateHeight, int completedLines, int holes, int bumpiness) GetCalculatedObservations(bool[] field, int width, int height)
    {
        int[] columnHeight = ColumnHeight(field, width, height);
        int aggregateHeight = AggregateHeight(columnHeight);
        int completedLines = CompleteLines(field, width, height);
        int holes = Holes(field, columnHeight, width);
        int bumpiness = Bumpiness(columnHeight);
        return (columnHeight, aggregateHeight, completedLines, holes, bumpiness);

    }
    private static int[] ColumnHeight(bool[] field, int width, int height)
    {
        int[] columnheight = new int[width];
        for (int x = 0; x < width; x++)
        {
            columnheight[x] = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                if (field[y * width + x])
                {
                    columnheight[x] = y + 1;
                    break;
                }
            }
        }
        return columnheight;
    }

    private static int AggregateHeight(int[] columnHeight)
    {
        int height = 0;
        for (int i = 0; i < columnHeight.Length; i++)
        {
            height += columnHeight[i];
        }
        return height;
    }

    private static int CompleteLines(bool[] field, int width, int height)
    {
        int lines = 0;

        for (int y = 0; y < height; y++)
        {
            bool flag = true;
            for (int x = 0; x < width; x++)
            {
                if (!field[y * width + x])
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                lines++;
            }
        }
        return lines;
    }

    private static int Holes(bool[] field, int[] columnHeight, int width)
    {
        int holes = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = columnHeight[x] - 2; y >= 0; y--)
            {
                if (!field[y * width + x])
                {
                    holes++;
                }
            }
        }
        return holes;
    }

    private static int Bumpiness(int[] columnHeight)
    {
        int bumpiness = 0;
        int width = columnHeight.Length;
        for (int x = 1; x < width; x++)
        {
            bumpiness += Math.Abs(columnHeight[x - 1] - columnHeight[x]);
        }
        return bumpiness;
    }

}