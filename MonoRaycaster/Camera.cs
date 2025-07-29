using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoRaycaster;

public class Camera
{
    private float _posX = 18; 
    private float _posY = 3; 
    private float _dirX = -1; 
    private float _dirY = 0;
    private float _planeX = 0;
    private float _planeY = .45f;

    private readonly Map _map;

    public Camera(Map map)
    {
        _map = map;
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

        if (nextPosX >= 0 && nextPosX < _map.Cols &&
            nextPosY >= 0 && nextPosY < _map.Rows &&
            _map.Cells[(int)nextPosY][(int)nextPosX] == 0)
        {
            _posX = nextPosX;
            _posY = nextPosY;
        }

        if (keyboardState.IsKeyDown(Keys.A))
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
        else if (keyboardState.IsKeyDown(Keys.D))
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
