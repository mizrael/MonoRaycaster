using Microsoft.Xna.Framework;
using System;

namespace MonoRaycaster;

/// <summary>
/// simple 2D raycaster implementation, based on 
/// https://lodev.org/cgtutor/raycasting.html
/// the code has been updated to render the map to a framebuffer, rotated 90 degrees
/// </summary>
public class Raycaster
{
    public readonly Color[] FrameBuffer;

    protected readonly int[][] _map;

    protected readonly int _screenWidth;
    protected readonly int _screenHeight;

    public Raycaster(int[][] map, int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _map = map;

        FrameBuffer = new Color[screenWidth * screenHeight];
    }

    public void Update(Camera camera)
    {
        var span = FrameBuffer.AsSpan();
        span.Fill(Color.Transparent);

        for (int y = 0; y < _screenHeight; y++)
        {
            //calculate ray position and direction
            double cameraY = 2 * y / (double)_screenHeight - 1; //y-coordinate in camera space
            double rayDirX = camera.DirX + camera.PlaneX * cameraY;
            double rayDirY = camera.DirY + camera.PlaneY * cameraY;
            //which box of the map we're in
            int mapX = (int)camera.PosX;
            int mapY = (int)camera.PosY;

            double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
            double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);

            int stepX;
            int stepY;
            double sideDistX;
            double sideDistY;

            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (camera.PosX - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0 - camera.PosX) * deltaDistX;
            }
            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (camera.PosY - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0 - camera.PosY) * deltaDistY;
            }

            //DDA
            int hit = 0;
            int side = 0;
            while (hit == 0)
            {
                if (sideDistX < sideDistY)
                {
                    sideDistX += deltaDistX;
                    mapX += stepX;
                    side = 0;
                }
                else
                {
                    sideDistY += deltaDistY;
                    mapY += stepY;
                    side = 1;
                }

                if (_map[mapX][mapY] > 0)
                    hit = 1;
            }

            double perpWallDist = side == 0
                ? (sideDistX - deltaDistX)
                : (sideDistY - deltaDistY);

            int lineWidth = (int)(_screenWidth / perpWallDist);

            int drawStart = (-lineWidth + _screenWidth) / 2;
            if (drawStart < 0)
                drawStart = 0;

            int drawEnd = (lineWidth + _screenWidth) / 2;
            if (drawEnd >= _screenWidth)
                drawEnd = _screenWidth - 1;

            int length = drawEnd - drawStart + 1;
            if (length != 0)
                UpdateRow(span, camera, y, mapX, mapY, side, drawStart, drawEnd, perpWallDist, rayDirX, rayDirY, lineWidth);
        }
    }

    protected virtual void UpdateRow(
        Span<Color> span,
        Camera camera,
        int y,
        int mapX,
        int mapY,
        int side,
        int drawStart,
        int drawEnd,
        double perpWallDist,
        double rayDirX,
        double rayDirY,
        int lineWidth)
    {
        var color = Color.Yellow;
        color = _map[mapX][mapY] switch
        {
            1 => Color.Red,
            2 => Color.Green,
            3 => Color.Blue,
            4 => Color.White,
            _ => Color.Yellow,
        };
        if (side == 1)
            color = color * .5f;

        int length = drawEnd - drawStart + 1;
        int row = y * _screenWidth;
        span.Slice(row + drawStart, length).Fill(color);
    }
}
