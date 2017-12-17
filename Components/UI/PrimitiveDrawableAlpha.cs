using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;

namespace Game.Shared.Components.UI
{
    public class PrimitiveDrawableAlpha : PrimitiveDrawable
    {

        public PrimitiveDrawableAlpha(Color col)
        {
            this.color = col;
            
        }
        public override void draw(Graphics graphics, float x, float y, float width, float height, Color color)
        {
            var col = this.color.HasValue ? this.color.Value : color;
            if (useFilledRect)
                graphics.batcher.drawRect(x, y, width, height, col);
            else
                graphics.batcher.drawHollowRect(x, y, width, height, col);
        }
    }
}