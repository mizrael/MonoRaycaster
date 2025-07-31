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

    protected readonly Map _map;

    protected readonly int _frameWidth;
    protected readonly int _frameHeight;

    public Raycaster(Map map, int frameWidth, int frameHeight)
    {
        _frameWidth = frameWidth;
        _frameHeight = frameHeight;

        _map = map;

        FrameBuffer = new Color[frameWidth * frameHeight];
    }

    public void Update(Camera camera)
    {
        var span = FrameBuffer.AsSpan();
        span.Clear();

        for (int y = 0; y < _frameHeight; y++)
        {
            //calculate ray position and direction
            float cameraY = 2 * y / (float)_frameHeight - 1; //y-coordinate in camera space
            float rayDirX = camera.DirX + camera.PlaneX * cameraY;
            float rayDirY = camera.DirY + camera.PlaneY * cameraY;

            //which box of the map we're in
            int mapX = (int)camera.PosX;
            int mapY = (int)camera.PosY;

            float deltaDistX = (rayDirX == 0) ? 1e30f : Math.Abs(1 / rayDirX);
            float deltaDistY = (rayDirY == 0) ? 1e30f : Math.Abs(1 / rayDirY);

            int stepX;
            int stepY;
            float sideDistX;
            float sideDistY;

            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (camera.PosX - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0f - camera.PosX) * deltaDistX;
            }

            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (camera.PosY - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0f - camera.PosY) * deltaDistY;
            }

            //DDA
            bool hit = false;
            int side = 0;
            while (!hit)
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

                if (_map.Cells[mapY][mapX] > 0)
                    hit = true;
            }

            float perpWallDist = side == 0
                ? (sideDistX - deltaDistX)
                : (sideDistY - deltaDistY);

            int lineWidth = (int)(_frameWidth / perpWallDist);

            int drawStart = (-lineWidth + _frameWidth) / 2;
            if (drawStart < 0)
                drawStart = 0;

            int drawEnd = (lineWidth + _frameWidth) / 2;
            if (drawEnd >= _frameWidth)
                drawEnd = _frameWidth - 1;

            int length = drawEnd - drawStart + 1;
            if (length > 0)
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
        float perpWallDist,
        float rayDirX,
        float rayDirY,
        int lineWidth)
    {
        var cell = _map.Cells[mapY][mapX];
        var color = _map.CellColors[cell];
        if (side == 1)
            color = color * .5f;

        int length = drawEnd - drawStart + 1;
        int row = y * _frameWidth;
        span.Slice(row + drawStart, length).Fill(color);
    }
}
