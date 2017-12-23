using System.Collections.Generic;
using System.Linq;
using Nez;
using Nez.UI;

namespace Game.Shared.Components.UI
{
    public class UIComponent : Component
    {
        public const int UI_RENDER_LAYER = 25;
        protected UiCanvas canvas;
        protected Scene currentScene;
        private ScreenSpaceRenderer renderer;
        public Skin Skin { get; private set; }
        public List<ISubUI> SubUI { get; set; } = new List<ISubUI>();
        //The parent element.
        public Table table { get; set; }

        public UIComponent(Skin _skin = null)
        {
            if (_skin == null) Skin = Skin.CreateDefaultSkin();
        }

        public override void OnAddedToEntity()
        {
            currentScene = Entity.Scene;
            SetUp();
        }

        public void SetUp()
        {
            renderer = new ScreenSpaceRenderer(0, UI_RENDER_LAYER);
            currentScene.AddRenderer(renderer);
            canvas = new UiCanvas();
            canvas.SetRenderLayer(UI_RENDER_LAYER);
            Entity.AddComponent(canvas);
            table = new Table();
            canvas.Stage.AddElement(table);
            table.SetFillParent(true);
        }

        public T GetSubUI<T>() where T : ISubUI
        {
            return (T) SubUI.First(a => a is T);
        }
    }
}