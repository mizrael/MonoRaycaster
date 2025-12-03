using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace MonoRaycaster;

public unsafe class Raycaster : IDisposable
{
    private readonly Color[][] _texturesData;
    private readonly int _texWidth;
    private readonly int _texHeight;
    private readonly int _mask;
    private readonly GCHandle[] _textureHandles;
    private readonly uint*[] _texturePointers;

    private const uint ceilingColor = 0xFF383838; // Dark gray ceiling
    private const uint floorColor = 0xFF707070;   // Lighter gray floor
    private const float stepTreshold = 0.15f;

    public readonly Color[] FrameBuffer;

    private readonly Map _map;

    private readonly int _frameWidth;
    private readonly int _frameHeight;

    public Raycaster(
        Map map,
        int screenWidth,
        int screenHeight,
        Texture2D[] textures
        )
    {
        _frameWidth = screenWidth;
        _frameHeight = screenHeight;

        _map = map;

        FrameBuffer = new Color[screenWidth * screenHeight];

        _texWidth = textures[0].Width;
        _texHeight = textures[0].Height;
        _mask = _texWidth - 1;

        _texturesData = new Color[textures.Length][];
        _textureHandles = new GCHandle[textures.Length];
        _texturePointers = new uint*[textures.Length];

        for (int i = 0; i != textures.Length; i++)
        {
            _texturesData[i] = new Color[_texWidth * _texHeight];
            textures[i].GetData(_texturesData[i]);

            // Pin the array and get a pointer
            _textureHandles[i] = GCHandle.Alloc(_texturesData[i], GCHandleType.Pinned);
            _texturePointers[i] = (uint*)_textureHandles[i].AddrOfPinnedObject();
        }
    }

    public void Update(Camera camera)
    {
        var span = FrameBuffer.AsSpan();
        span.Clear();

        for (int y = 0; y < _frameHeight; y++)
        {
            //calculate ray position and direction
            float cameraY = 2 * y / (float)_frameHeight - 1; //y-coordinate in camera space
            float rayDirX = camera.Direction.X + camera.Plane.X * cameraY;
            float rayDirY = camera.Direction.Y + camera.Plane.Y * cameraY;

            //which box of the map we're in
            int mapX = (int)camera.Position.X;
            int mapY = (int)camera.Position.Y;

            float deltaDistX = (rayDirX == 0) ? 1e30f : Math.Abs(1 / rayDirX);
            float deltaDistY = (rayDirY == 0) ? 1e30f : Math.Abs(1 / rayDirY);

            int stepX;
            int stepY;
            float sideDistX;
            float sideDistY;

            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (camera.Position.X - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0f - camera.Position.X) * deltaDistX;
            }

            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (camera.Position.Y - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0f - camera.Position.Y) * deltaDistY;
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

                var cell = _map.Cells[mapY][mapX];
                if(cell == TileTypes.Floor)
                    continue;

                if (cell == TileTypes.Door)
                {
                    var door = _map.GetDoor(mapX, mapY);
                    if (door!.IsBlocking)
                        hit = true;
                }
                else 
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

    private void UpdateRow(
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
        fixed (Color* destColorPtr = span)
        {
            uint* columnPtr = (uint*)(destColorPtr + y * _frameWidth);

            // Render ceiling (from top of screen to wall start)
            for (int i = 0; i < drawStart; i++)
                columnPtr[i] = ceilingColor;

            float wallY = (side == 0) ?
                camera.Position.Y + perpWallDist * rayDirY :
                camera.Position.X + perpWallDist * rayDirX;
            wallY -= MathF.Floor(wallY);

            int tileType = _map.Cells[mapY][mapX];
            if (tileType == TileTypes.Door)
            {
                var door = _map.GetDoor(mapX, mapY);
                if (door!.IsBlocking)
                {
                    float doorOffset = door!.IsVertical ?
                        (side == 0 ? door.OpenAmount : 0) :
                        (side == 1 ? door.OpenAmount : 0);

                    wallY -= doorOffset;
                    if (wallY < 0 || wallY > 1)
                    {
                        for (int i = drawStart; i <= drawEnd; i++)
                            columnPtr[i] = ceilingColor;
                    }
                    else
                        UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: 0);
                }
            }
            else if (tileType != TileTypes.Floor)
                UpdateRow(side, drawStart, drawEnd, rayDirX, rayDirY, lineWidth, columnPtr, wallY, texNum: tileType - 1);

            // Render floor (from wall end to bottom of screen)
            for (int i = drawEnd + 1; i < _frameWidth; i++)
                columnPtr[i] = floorColor;
        }
    }

    private void UpdateRow(int side, int drawStart, int drawEnd, float rayDirX, float rayDirY, int lineWidth, uint* columnPtr, float wallY, int texNum)
    {
        uint* texturePtr = _texturePointers[texNum];

        int texY = (int)(wallY * _texWidth);

        int flipMask = ((side == 0 && rayDirX > 0) || (side == 1 && rayDirY < 0)) ? _mask : 0;
        texY = texY ^ flipMask;

        float step = 1.0f * _texWidth / lineWidth;
        float texPos = (drawStart - _frameWidth * .5f + lineWidth * .5f) * step;

        uint* sourcePtr = texturePtr + (_texHeight * texY);
        int drawLen = drawEnd - drawStart + 1;

        uint* destPtr = columnPtr + drawStart;

        // Use simplified rendering when wall is extremely close
        if (step < stepTreshold)
        {
            int i = 0;

            // Process larger chunks when sampling the same texel multiple times
            while (i < drawLen)
            {
                int currentTexX = ((int)texPos) & _mask;
                uint color = sourcePtr[currentTexX];

                // Calculate how many pixels will use this same texel
                int nextTexX = ((int)(texPos + step)) & _mask;
                int pixelsToFill = (nextTexX != currentTexX) ?
                    Math.Min((int)(1.0f / step), drawLen - i) : 1;

                // Fill multiple pixels with the same color
                int fillEnd = Math.Min(i + pixelsToFill, drawLen);
                for (int j = i; j < fillEnd; j++)
                    destPtr[j] = color;

                i = fillEnd;
                texPos += step * pixelsToFill;
            }
        }
        else
        {
            // rounds down to the nearest multiple of 4
            int unrollCount = drawLen & ~3;
            int i = 0;
            for (; i < unrollCount; i += 4)
            {
                destPtr[i] = sourcePtr[((int)texPos) & _mask];
                texPos += step;
                destPtr[i + 1] = sourcePtr[((int)texPos) & _mask];
                texPos += step;
                destPtr[i + 2] = sourcePtr[((int)texPos) & _mask];
                texPos += step;
                destPtr[i + 3] = sourcePtr[((int)texPos) & _mask];
                texPos += step;
            }

            // Handle remaining pixels
            for (; i < drawLen; i++)
            {
                destPtr[i] = sourcePtr[((int)texPos) & _mask];
                texPos += step;
            }
        }
    }

    public void Dispose()
    {
        foreach (var handle in _textureHandles)
        {
            if (handle.IsAllocated)
                handle.Free();
        }
    }
}