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
        var rayDir = new Vector2(_camera.DirX, _camera.DirY);
        var endPos = FindInterceptionPoint(rayDir);
        DrawLine(spriteBatch, endPos, startPos, thickness);

        rayDir = new Vector2((_camera.DirX - _camera.PlaneX), (_camera.DirY - _camera.PlaneY));
        endPos = FindInterceptionPoint(rayDir);
        DrawLine(spriteBatch, endPos, startPos, thickness);

        rayDir = new Vector2((_camera.DirX + _camera.PlaneX), (_camera.DirY + _camera.PlaneY));
        endPos = FindInterceptionPoint(rayDir);
        DrawLine(spriteBatch, endPos, startPos, thickness);
    }

    private Vector2 FindInterceptionPoint(Vector2 rayDir)
    {
        int mapX = (int)_camera.PosX;
        int mapY = (int)_camera.PosY;

        float deltaDistX = (rayDir.X == 0) ? 1e30f : Math.Abs(1 / rayDir.X);
        float deltaDistY = (rayDir.Y == 0) ? 1e30f : Math.Abs(1 / rayDir.Y);

        int stepX;
        int stepY;
        float sideDistX;
        float sideDistY;

        if (rayDir.X < 0)
        {
            stepX = -1;
            sideDistX = (_camera.PosX - mapX) * deltaDistX;
        }
        else
        {
            stepX = 1;
            sideDistX = (mapX + 1.0f - _camera.PosX) * deltaDistX;
        }

        if (rayDir.Y < 0)
        {
            stepY = -1;
            sideDistY = (_camera.PosY - mapY) * deltaDistY;
        }
        else
        {
            stepY = 1;
            sideDistY = (mapY + 1.0f - _camera.PosY) * deltaDistY;
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

            if (_map.Cells[mapY][mapX] > 0)
                hit = true;
        }

        float perpWallDist = isHorizontalWall == false
            ? (sideDistX - deltaDistX)
            : (sideDistY - deltaDistY);

        float wallX, wallY;
        if (!isHorizontalWall) 
        {
            wallX = mapX + (stepX < 0 ? 1.0f : 0.0f);
            wallY = _camera.PosY + perpWallDist * rayDir.Y;
        }
        else
        {
            wallX = _camera.PosX + perpWallDist * rayDir.X;
            wallY = mapY + (stepY < 0 ? 1.0f : 0.0f);
        }

        return new Vector2(wallX * _cellWidth, wallY * _cellHeight);
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