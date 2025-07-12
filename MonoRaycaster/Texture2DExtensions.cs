﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRaycaster;

public static class Texture2DExtensions
{
    public static Texture2D[] Split(this Texture2D source, int destWidth, int destHeight)
    {
        int cols = source.Width / destWidth,
            rows = source.Height / destHeight;

        var textures = new Texture2D[cols * rows];
        for (int i = 0; i != textures.Length; i++)
            textures[i] = new Texture2D(source.GraphicsDevice, destWidth, destHeight);

        var buffer = new Color[destWidth * destHeight];
        var textI = 0;
        for(int col = 0; col!=cols; col++)
            for(int row = 0; row!=rows; row++)
            {
                var rect = new Rectangle(col * destWidth, row * destHeight, destWidth, destHeight);
                source.GetData(0, rect, buffer, 0, buffer.Length);
                textures[textI++].SetData(buffer);
            }
        return textures;
    }

    public static Texture2D Rotate90(this Texture2D source, RotationDirection direction = RotationDirection.Clockwise)
    {
        Texture2D rotated = new Texture2D(
            source.GraphicsDevice,
            source.Height,
            source.Width);

        var originalData = new Color[source.Width * source.Height];
        source.GetData(originalData);

        var rotatedData = new Color[source.Width * source.Height];

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                int newX, newY;

                if (direction == RotationDirection.Clockwise)
                {
                    newX = source.Height - 1 - y;
                    newY = x;
                }
                else
                {
                    newX = y;
                    newY = source.Width - 1 - x;
                }

                int originalIndex = y * source.Width + x;
                int rotatedIndex = newY * source.Height + newX;

                rotatedData[rotatedIndex] = originalData[originalIndex];
            }
        }

        rotated.SetData(rotatedData);

        return rotated;
    }
}

public enum RotationDirection
{
    Clockwise,
    CounterClockwise
}