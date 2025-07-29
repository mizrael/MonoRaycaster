using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoRaycaster;

public class MiniMap
{
    private readonly int[][] _map;
    private readonly Camera _camera;
    private readonly int _cols;
    private readonly int _rows;
    private readonly int _cellWidth;
    private readonly int _cellHeight;
    private readonly Color[] _colors;
    private readonly Texture2D _texture;

    public MiniMap(
        int[][] map, 
        int frameWidth, 
        int frameHeight,
        GraphicsDevice graphicsDevice,
        Camera camera)
    {
        _map = map;
        _camera = camera;
        _rows = _map.Length;
        _cols = _map[0].Length;

        _cellWidth = frameWidth / _cols; 
        _cellHeight = frameHeight / _rows;

        var colorsCount = 0;
        for (int row = 0; row != _rows; row++)
            for (int col = 0; col != _cols; col++)
                colorsCount = MathHelper.Max(colorsCount, _map[row][col]);
        colorsCount++;

        _colors = new Color[colorsCount];
        _colors[0] = Color.DarkSlateGray;
        for(int c = 1; c != colorsCount; c++)
        {
            _colors[c] = new Color(
                (byte)Random.Shared.Next(100, 220),
                (byte)Random.Shared.Next(100, 220),
                (byte)Random.Shared.Next(100, 220),
                (byte)255);
        }

        _texture = new Texture2D(graphicsDevice, 1, 1);
        _texture.SetData([Color.White]);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for(int row=0; row!=_rows; row++) 
            for(int col=0; col!=_cols; col++)
            {
                var cell = _map[row][col];
                var color = _colors[cell];

                var dest = new Rectangle(
                    col * _cellWidth,
                    row * _cellHeight,
                    _cellWidth,
                    _cellHeight);
                spriteBatch.Draw(_texture, dest, color);
            }

        var cameraRect = new Rectangle(
            (int)_camera.PosX * _cellWidth,
            (int)_camera.PosY * _cellHeight,
            _cellWidth / 2,
            _cellHeight / 2);
        spriteBatch.Draw(_texture, cameraRect, Color.Black);
    }
}