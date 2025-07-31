using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoRaycaster;

public class MiniMap
{
    private readonly Map _map;
    private readonly Camera _camera;

    private readonly int _cellWidth;
    private readonly int _cellHeight;
    private readonly Texture2D _texture;
    private readonly Vector2 _cellCenter;
    private readonly int _rayCount;

    public MiniMap(
        Map map,
        int frameWidth,
        int frameHeight,
        GraphicsDevice graphicsDevice,
        Camera camera,
        int rayCount = 100)
    {
        _map = map;
        _camera = camera;
        _rayCount = rayCount;

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

        var cameraPos = new Vector2(_camera.PosX * _cellWidth, _camera.PosY * _cellHeight);
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

        RenderFieldOfViewCone(spriteBatch, cameraPos + _cellCenter);
    }

    private void RenderFieldOfViewCone(SpriteBatch spriteBatch, Vector2 startPos)
    {
        for (int i = 0; i < _rayCount; i++)
        {
            float offset = 2 * i / (float)(_rayCount - 1) - 1;

            var rayDir = new Vector2(
                _camera.DirX + _camera.PlaneX * offset,
                _camera.DirY + _camera.PlaneY * offset);

            var interceptionPoint = _map.FindInterceptionPoint(_camera, rayDir);
            interceptionPoint.X *= _cellWidth;
            interceptionPoint.Y *= _cellHeight;
            spriteBatch.DrawLine(_texture, startPos, interceptionPoint, 2f, Color.Red);
        }
    }
}
