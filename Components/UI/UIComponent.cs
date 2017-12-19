using Nez;
using Nez.UI;

namespace Game.Shared.Components.UI
{
    public class UIComponent : Component
    {
        public const int UI_RENDER_LAYER = 25;
        protected UICanvas canvas;
        protected Scene currentScene;
        private ScreenSpaceRenderer renderer;
        public Skin Skin { get; private set; }
        //The parent element.
        public Table table { get; set; }

        public UIComponent(Skin _skin = null)
        {
            if (_skin == null) Skin = Skin.createDefaultSkin();
        }

        public override void onAddedToEntity()
        {
            currentScene = entity.scene;
            SetUp();
        }

        public void SetUp()
        {
            renderer = new ScreenSpaceRenderer(0, UI_RENDER_LAYER);
            currentScene.addRenderer(renderer);
            canvas = new UICanvas();
            canvas.setRenderLayer(UI_RENDER_LAYER);
            entity.addComponent(canvas);
            table = new Table();
            canvas.stage.addElement(table);
            table.setFillParent(true);
        }
    }
}