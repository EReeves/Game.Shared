using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Game.Shared.Components.UI
{
    public class PrimitiveDrawableAlpha : PrimitiveDrawable
    {

        public PrimitiveDrawableAlpha(Color col)
        {
            this.Color = col;
            
        }
        public override void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            var col = this.Color.HasValue ? this.Color.Value : color;
            if (UseFilledRect)
                graphics.Batcher.DrawRect(x, y, width, height, col);
            else
                graphics.Batcher.DrawHollowRect(x, y, width, height, col);
        }
    }
}