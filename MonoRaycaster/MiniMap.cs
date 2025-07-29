using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoRaycaster;

public class MiniMap
{
    private readonly Map _map;
    private readonly Camera _camera;
    
    private readonly int _cellWidth;
    private readonly int _cellHeight;
    private readonly Texture2D _texture;
    private readonly Vector2 _cellCenter;

    public MiniMap(
        Map map, 
        int frameWidth, 
        int frameHeight,
        GraphicsDevice graphicsDevice,
        Camera camera)
    {
        _map = map;
        _camera = camera;

        _cellWidth = frameWidth / _map.Cols; 
        _cellHeight = frameHeight / _map.Rows;

        _cellCenter = new Vector2(_cellWidth, _cellHeight) * .25f;

        _texture = new Texture2D(graphicsDevice, 1, 1);
        _texture.SetData([Color.White]);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for (int row = 0; row != _map.Rows; row++)
            for (int col = 0; col != _map.Cols; col++)
            {
                var cell = _map.Cells[row][col];
                var color = _map.CellColors[cell];

                var dest = new Rectangle(
                    col * _cellWidth,
                    row * _cellHeight,
                    _cellWidth,
                    _cellHeight);
                spriteBatch.Draw(_texture, dest, color);
            }

        var cameraPos = new Vector2(_camera.PosX * _cellWidth, _camera.PosY * _cellHeight) - _cellCenter;
        spriteBatch.Draw(
            _texture,
            cameraPos,
            sourceRectangle: null,
            color: Color.Black,
            rotation: 0f,
            origin: Vector2.Zero,
            scale: _cellCenter * 2f,
            effects: SpriteEffects.None,
            layerDepth: 0);

        var thickness = 4f;
        var startPos = cameraPos + _cellCenter; 
        var endPos = cameraPos + new Vector2(_camera.DirX * _cellWidth, _camera.DirY * _cellHeight) * 4f;
        DrawLine(spriteBatch, endPos, startPos, thickness);

        endPos = cameraPos + new Vector2((_camera.DirX - _camera.PlaneX) * _cellWidth, (_camera.DirY - _camera.PlaneY) * _cellHeight) * 4f;
        DrawLine(spriteBatch, endPos, startPos, thickness);

        endPos = cameraPos + new Vector2((_camera.DirX + _camera.PlaneX) * _cellWidth, (_camera.DirY + _camera.PlaneY) * _cellHeight) * 4f;
        DrawLine(spriteBatch, endPos, startPos, thickness);
    }

    private void DrawLine(SpriteBatch spriteBatch, Vector2 endPos, Vector2 startPos, float thickness)
    {
        var rotation = (float)Math.Atan2(endPos.Y - startPos.Y, endPos.X - startPos.X);
        var length = Vector2.Distance(endPos, startPos);
        var scale = new Vector2(length, thickness);

        spriteBatch.Draw(
            _texture,
            startPos,
            sourceRectangle: null,
            color: Color.Red,
            rotation: rotation,
            origin: Vector2.Zero,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0);
    }
}