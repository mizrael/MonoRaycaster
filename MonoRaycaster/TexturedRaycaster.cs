using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoRaycaster;

public class TexturedRaycaster : Raycaster
{
    private readonly Color[][] _texturesData;
    private readonly int _texWidth;
    private readonly int _texHeight;
    private readonly int _mask;

    public TexturedRaycaster(
        int[][] map,
        int screenWidth,
        int screenHeight,
        Texture2D[] textures
        ) : base(map, screenWidth, screenHeight)
    {       
        _texWidth = textures[0].Width;
        _texHeight = textures[0].Height;
        _mask = _texWidth - 1;

        _texturesData = new Color[textures.Length][];
        for (int i = 0; i != textures.Length; i++)
        {
            _texturesData[i] = new Color[_texWidth * _texHeight];
            textures[i].GetData(_texturesData[i]);
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
        double perpWallDist,
        double rayDirX,
        double rayDirY,
        int lineWidth)
    {
        int texNum = _map[mapX][mapY] - 1;
        var textureData = _texturesData[texNum];

        double wallY = (side == 0) ?
            camera.PosY + perpWallDist * rayDirY :
            camera.PosX + perpWallDist * rayDirX;

        wallY -= Math.Floor(wallY);

        int texY = (int)(wallY * _texWidth);
        if (side == 0 && rayDirX > 0) texY = _mask - texY;
        else if (side == 1 && rayDirY < 0) texY = _mask - texY;

        double step = 1.0 * _texWidth / lineWidth;
        double texPos = (drawStart - _screenWidth * .5 + lineWidth * .5) * step;
        
        var sourceStart = _texHeight * texY;
        var sourceData = textureData.AsSpan(sourceStart, textureData.Length - sourceStart);

        int drawLen = drawEnd - drawStart + 1;
        int destDataStartIndex = y * _screenWidth + drawStart;
        var destRowData = span.Slice(destDataStartIndex, drawLen);

        for (int x = 0; x != drawLen; x++)
        {
            int texX = ((int)texPos) & _mask;
            texPos += step;
            destRowData[x] = sourceData[texX];
        }
    }
}