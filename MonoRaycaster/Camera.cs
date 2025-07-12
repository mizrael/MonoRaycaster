using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoRaycaster;

public class Camera
{
    private double _posX = 22; 
    private double _posY = 12; 
    private double _dirX = 0; 
    private double _dirY = -1; 
    private double _planeX = .66; 
    private double _planeY = 0;

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
        double moveSpeed = gameTime.ElapsedGameTime.TotalMilliseconds * .015f;
        double rotSpeed = gameTime.ElapsedGameTime.TotalMilliseconds * .005f;

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
            double oldDirX = _dirX;
            var cos = Math.Cos(-rotSpeed);
            var sin = Math.Sin(-rotSpeed);

            _dirX = _dirX * cos - _dirY * sin;
            _dirY = oldDirX * sin + _dirY * cos;

            double oldPlaneX = _planeX;
            _planeX = _planeX * cos - _planeY * sin;
            _planeY = oldPlaneX * sin + _planeY * cos;
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            double oldDirX = _dirX;
            var cos = Math.Cos(rotSpeed);
            var sin = Math.Sin(rotSpeed);

            _dirX = _dirX * cos - _dirY * sin;
            _dirY = oldDirX * sin + _dirY * cos;

            double oldPlaneX = _planeX;
            _planeX = _planeX * cos - _planeY * sin;
            _planeY = oldPlaneX * sin + _planeY * cos;
        }
    }

    public double PosX => _posX;
    public double PosY => _posY;
    public double DirX => _dirX;
    public double DirY => _dirY;
    public double PlaneX => _planeX;
    public double PlaneY => _planeY;
}
