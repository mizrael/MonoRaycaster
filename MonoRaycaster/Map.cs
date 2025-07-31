using Microsoft.Xna.Framework;
using System;

namespace MonoRaycaster;

public class Map
{
    public int[][] Cells = [
      [ 8,8,8,8,8,8,8,8,8,8,8,4,4,6,4,4,6,4,6,4,4,4,6,4],
      [ 8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,0,0,0,0,0,0,4],
      [ 8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,6],
      [ 8,0,0,3,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,6],
      [ 8,0,3,3,0,0,0,0,0,8,8,4,0,0,0,0,0,0,0,0,0,0,0,4],
      [ 8,0,0,0,0,0,0,0,0,0,8,4,0,0,0,0,0,6,6,6,0,6,4,6],
      [ 8,8,8,8,0,8,8,8,8,8,8,4,4,4,4,4,4,6,0,0,0,0,0,6],
      [ 7,7,7,7,0,7,7,7,7,0,8,0,8,0,8,0,8,4,0,4,0,6,0,6],
      [ 7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,0,0,0,0,0,6],
      [ 7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,0,0,0,0,4],
      [ 7,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,8,6,0,6,0,6,0,6],
      [ 7,7,0,0,0,0,0,0,7,8,0,8,0,8,0,8,8,6,4,6,0,6,6,6],
      [ 7,7,7,7,0,7,7,7,7,8,8,4,0,6,8,4,8,3,3,3,0,3,3,3],
      [ 2,2,2,2,0,2,2,2,2,4,6,4,0,0,6,0,6,3,0,0,0,0,0,3],
      [ 2,2,0,0,0,0,0,2,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3],
      [ 2,0,0,0,0,0,0,0,2,4,0,0,0,0,0,0,4,3,0,0,0,0,0,3],
      [ 1,0,0,0,0,0,0,0,1,4,4,4,4,4,6,0,6,3,3,0,0,0,3,3],
      [ 2,0,0,0,0,0,0,0,2,2,2,1,2,2,2,6,6,0,0,5,0,5,0,5],
      [ 2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5],
      [ 2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5],
      [ 1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,5],
      [ 2,0,0,0,0,0,0,0,2,0,0,0,0,0,2,5,0,5,0,5,0,5,0,5],
      [ 2,2,0,0,0,0,0,2,2,2,0,0,0,2,2,0,5,0,5,0,0,0,5,5],
      [ 2,2,2,2,1,2,2,2,2,2,2,1,2,2,2,5,5,5,5,5,5,5,5,5]
    ];

    public readonly int Cols;
    public readonly int Rows;

    public readonly Color[] CellColors;

    public Map()
    {
        Rows = Cells.Length;
        Cols = Cells.Length;

        var colorsCount = 0;
        for (int row = 0; row != Rows; row++)
            for (int col = 0; col != Cols; col++)
                colorsCount = MathHelper.Max(colorsCount, Cells[row][col]);
        colorsCount++;

        CellColors = new Color[colorsCount];
        CellColors[0] = Color.DarkSlateGray;
        for (int c = 1; c != colorsCount; c++)
        {
            CellColors[c] = new Color(
                (byte)Random.Shared.Next(100, 220),
                (byte)Random.Shared.Next(100, 220),
                (byte)Random.Shared.Next(100, 220),
                (byte)255);
        }
    }

    public Vector2 FindInterceptionPoint(Camera camera, Vector2 rayDir)
    {
        int mapX = (int)camera.PosX;
        int mapY = (int)camera.PosY;

        float deltaDistX = (rayDir.X == 0) ? 1e30f : Math.Abs(1 / rayDir.X);
        float deltaDistY = (rayDir.Y == 0) ? 1e30f : Math.Abs(1 / rayDir.Y);

        int stepX;
        int stepY;
        float sideDistX;
        float sideDistY;

        if (rayDir.X < 0)
        {
            stepX = -1;
            sideDistX = (camera.PosX - mapX) * deltaDistX;
        }
        else
        {
            stepX = 1;
            sideDistX = (mapX + 1.0f - camera.PosX) * deltaDistX;
        }

        if (rayDir.Y < 0)
        {
            stepY = -1;
            sideDistY = (camera.PosY - mapY) * deltaDistY;
        }
        else
        {
            stepY = 1;
            sideDistY = (mapY + 1.0f - camera.PosY) * deltaDistY;
        }

        bool hit = false;
        bool isHorizontalWall = false;
        while (!hit)
        {
            if (sideDistX < sideDistY)
            {
                sideDistX += deltaDistX;
                mapX += stepX;
                isHorizontalWall = false;
            }
            else
            {
                sideDistY += deltaDistY;
                mapY += stepY;
                isHorizontalWall = true;
            }

            if (this.Cells[mapY][mapX] > 0)
                hit = true;
        }

        float perpWallDist = isHorizontalWall == false
            ? (sideDistX - deltaDistX)
            : (sideDistY - deltaDistY);

        Vector2 pointOnWall;
        if (!isHorizontalWall)
        {
            pointOnWall.X = mapX + (stepX < 0 ? 1.0f : 0.0f);
            pointOnWall.Y = camera.PosY + perpWallDist * rayDir.Y;
        }
        else
        {
            pointOnWall.X = camera.PosX + perpWallDist * rayDir.X;
            pointOnWall.Y = mapY + (stepY < 0 ? 1.0f : 0.0f);
        }

        return pointOnWall;
    }
}
