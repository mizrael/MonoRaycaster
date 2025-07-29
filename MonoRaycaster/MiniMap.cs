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

        _texture = new Texture2D(graphicsDevice, 1, 1);
        _texture.SetData([Color.White]);
    }

    public void Render(SpriteBatch spriteBatch)
    {
        for(int row=0; row!=_map.Rows; row++) 
            for(int col=0; col!=_map.Cols; col++)
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
            scale: new Vector2(_cellWidth, _cellHeight) * .5f,
            effects: SpriteEffects.None,
            layerDepth: 0);
    }
}