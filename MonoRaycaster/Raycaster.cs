using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoRaycaster;

/// <summary>
/// simple 2D raycaster implementation, based on 
/// https://lodev.org/cgtutor/raycasting.html
/// the code has been updated to render the map to a framebuffer, rotated 90 degrees
/// </summary>
public class Raycaster
{
    private double posX = 22; // player position X
    private double posY = 12; // player position Y
    private double dirX = 0; // initial direction vector X (rotated 90 degrees)
    private double dirY = -1; // initial direction vector Y (rotated 90 degrees)
    private double planeX = .66; // the 2D raycaster version of camera plane X (rotated 90 degrees)
    private double planeY = 0; // the 2D raycaster version of camera plane Y (rotated 90 degrees)

    public readonly Color[] FrameBuffer;

    private readonly int[][] _map;

    private readonly int _screenWidth;
    private readonly int _screenHeight;

    private readonly int _mapWidth;
    private readonly int _mapHeight;

    public Raycaster(int[][] map, int screenWidth, int screenHeight)
    {
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _map = map;
        _mapWidth = map.Length;
        _mapHeight = map[0].Length;

        FrameBuffer = new Color[screenWidth * screenHeight];
    }

    public void Update(GameTime gameTime)
    {
        UpdateCamera(gameTime);

        UpdateFrameBuffer();
    }

    private void UpdateCamera(GameTime gameTime)
    {
        double moveSpeed = gameTime.ElapsedGameTime.TotalMilliseconds * .015f;
        double rotSpeed = gameTime.ElapsedGameTime.TotalMilliseconds * .005f;

        var keyboardState = Keyboard.GetState();

        var nextPosX = posX;
        var nextPosY = posY;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            nextPosX = posX + dirX * moveSpeed;
            nextPosY = posY + dirY * moveSpeed;
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            nextPosX = posX - dirX * moveSpeed;
            nextPosY = posY - dirY * moveSpeed;
        }

        if (nextPosX >= 0 && nextPosX < _mapWidth &&
            nextPosY >= 0 && nextPosY < _mapHeight &&
            _map[(int)nextPosX][(int)nextPosY] == 0)
        {
            posX = nextPosX;
            posY = nextPosY;
        }

        if (keyboardState.IsKeyDown(Keys.D))
        {
            double oldDirX = dirX;
            var cos = Math.Cos(-rotSpeed);
            var sin = Math.Sin(-rotSpeed);

            dirX = dirX * cos - dirY * sin;
            dirY = oldDirX * sin + dirY * cos;

            double oldPlaneX = planeX;
            planeX = planeX * cos - planeY * sin;
            planeY = oldPlaneX * sin + planeY * cos;
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            double oldDirX = dirX;
            var cos = Math.Cos(rotSpeed);
            var sin = Math.Sin(rotSpeed);

            dirX = dirX * cos - dirY * sin;
            dirY = oldDirX * sin + dirY * cos;

            double oldPlaneX = planeX;
            planeX = planeX * cos - planeY * sin;
            planeY = oldPlaneX * sin + planeY * cos;
        }
    }

    private void UpdateFrameBuffer()
    {
        var span = FrameBuffer.AsSpan();
        span.Fill(Color.Transparent);

        for (int y = 0; y < _screenHeight; y++)
        {
            //calculate ray position and direction
            double cameraY = 2 * y / (double)_screenHeight - 1; //y-coordinate in camera space
            double rayDirX = dirX + planeX * cameraY;
            double rayDirY = dirY + planeY * cameraY;
            //which box of the map we're in
            int mapX = (int)posX;
            int mapY = (int)posY;

            double deltaDistX = (rayDirX == 0) ? 1e30 : Math.Abs(1 / rayDirX);
            double deltaDistY = (rayDirY == 0) ? 1e30 : Math.Abs(1 / rayDirY);

            int stepX;
            int stepY;
            double sideDistX;
            double sideDistY;

            if (rayDirX < 0)
            {
                stepX = -1;
                sideDistX = (posX - mapX) * deltaDistX;
            }
            else
            {
                stepX = 1;
                sideDistX = (mapX + 1.0 - posX) * deltaDistX;
            }
            if (rayDirY < 0)
            {
                stepY = -1;
                sideDistY = (posY - mapY) * deltaDistY;
            }
            else
            {
                stepY = 1;
                sideDistY = (mapY + 1.0 - posY) * deltaDistY;
            }

            //DDA
            int hit = 0;
            int side = 0;
            while (hit == 0)
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

                if (_map[mapX][mapY] > 0)
                    hit = 1;
            }

            double perpWallDist = side == 0
                ? (sideDistX - deltaDistX)
                : (sideDistY - deltaDistY);

            int lineWidth = (int)(_screenWidth / perpWallDist);

            int drawStart = (-lineWidth + _screenWidth) / 2;
            if (drawStart < 0)
                drawStart = 0;

            int drawEnd = (lineWidth + _screenWidth) / 2;
            if (drawEnd >= _screenWidth)
                drawEnd = _screenWidth - 1;

            var color = Color.Yellow;
            color = _map[mapX][mapY] switch
            {
                1 => Color.Red,
                2 => Color.Green,
                3 => Color.Blue,
                4 => Color.White,
                _ => Color.Yellow,
            };
            if (side == 1)
                color = color * .5f;

            int length = drawEnd - drawStart + 1;
            if (length > 0)
            {
                int row = y * _screenWidth;
                span.Slice(row + drawStart, length).Fill(color);
            }
        }
    }
}