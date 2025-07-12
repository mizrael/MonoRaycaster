using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoRaycaster;

public class Camera
{
    private float _posX = 22; 
    private float _posY = 12; 
    private float _dirX = 0; 
    private float _dirY = -1; 
    private float _planeX = .66f; 
    private float _planeY = 0;

    private readonly int[][] _map;

    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public Camera(int[][] map)
    {
        _map = map;
        _mapWidth = map.Length;
        _mapHeight = map[0].Length;
    }

    public void Update(GameTime gameTime)
    {
        float ms = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        float moveSpeed = ms * .015f;
        float rotSpeed = ms * .005f;

        var keyboardState = Keyboard.GetState();

        var nextPosX = _posX;
        var nextPosY = _posY;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            nextPosX = _posX + _dirX * moveSpeed;
            nextPosY = _posY + _dirY * moveSpeed;
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            nextPosX = _posX - _dirX * moveSpeed;
            nextPosY = _posY - _dirY * moveSpeed;
        }

        if (nextPosX >= 0 && nextPosX < _mapWidth &&
            nextPosY >= 0 && nextPosY < _mapHeight &&
            _map[(int)nextPosX][(int)nextPosY] == 0)
        {
            _posX = nextPosX;
            _posY = nextPosY;
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            float oldDirX = _dirX;
            var cos = MathF.Cos(-rotSpeed);
            var sin = MathF.Sin(-rotSpeed);

            _dirX = _dirX * cos - _dirY * sin;
            _dirY = oldDirX * sin + _dirY * cos;

            float oldPlaneX = _planeX;
            _planeX = _planeX * cos - _planeY * sin;
            _planeY = oldPlaneX * sin + _planeY * cos;
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            float oldDirX = _dirX;
            var cos = MathF.Cos(rotSpeed);
            var sin = MathF.Sin(rotSpeed);

            _dirX = _dirX * cos - _dirY * sin;
            _dirY = oldDirX * sin + _dirY * cos;

            float oldPlaneX = _planeX;
            _planeX = _planeX * cos - _planeY * sin;
            _planeY = oldPlaneX * sin + _planeY * cos;
        }
    }

    public float PosX => _posX;
    public float PosY => _posY;
    public float DirX => _dirX;
    public float DirY => _dirY;
    public float PlaneX => _planeX;
    public float PlaneY => _planeY;
}
