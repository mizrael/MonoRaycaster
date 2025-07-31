using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace MonoRaycaster;

public static class SpriteBatchExtensions
{
    public static void DrawLine(
        this SpriteBatch spriteBatch, 
        Texture2D texture,
        Vector2 start, 
        Vector2 end, 
        float thickness, 
        Color color)
    {
        var rotation = (float)Math.Atan2(end.Y - start.Y, end.X - start.X);
        var length = Vector2.Distance(end, start);
        var scale = new Vector2(length, thickness);

        spriteBatch.Draw(
            texture,
            start,
            sourceRectangle: null,
            color: color,
            rotation: rotation,
            origin: Vector2.Zero,
            scale: scale,
            effects: SpriteEffects.None,
            layerDepth: 0);
    }
}