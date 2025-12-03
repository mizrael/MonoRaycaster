using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace MonoRaycaster;

public unsafe class TexturedRaycasterUnsafe : Raycaster, IDisposable
{
    private readonly Color[][] _texturesData;
    private readonly int _texWidth;
    private readonly int _texHeight;
    private readonly int _mask;
    private readonly GCHandle[] _textureHandles;
    private readonly uint*[] _texturePointers;

    public TexturedRaycasterUnsafe(
        Map map,
        int screenWidth,
        int screenHeight,
        Texture2D[] textures
        ) : base(map, screenWidth, screenHeight)
    {
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

    protected override void UpdateRow(
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
        int texNum = _map.Cells[mapY][mapX] - 1;
        uint* texturePtr = _texturePointers[texNum];

        float wallY = (side == 0) ?
            camera.Position.Y + perpWallDist * rayDirY :
            camera.Position.X + perpWallDist * rayDirX;

        wallY -= MathF.Floor(wallY);

        int texY = (int)(wallY * _texWidth);

        int flipMask = ((side == 0 && rayDirX > 0) || (side == 1 && rayDirY < 0)) ? _mask : 0;
        texY = texY ^ flipMask;

        float step = 1.0f * _texWidth / lineWidth;
        float texPos = (drawStart - _frameWidth * .5f + lineWidth * .5f) * step;

        uint* sourcePtr = texturePtr + (_texHeight * texY);
        int drawLen = drawEnd - drawStart + 1;

        fixed (Color* destColorPtr = span)
        {
            uint* destPtr = (uint*)(destColorPtr + y * _frameWidth + drawStart);

            // Use simplified rendering when wall is extremely close
            if (step < 0.15f)
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
                    {
                        destPtr[j] = color;
                    }

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