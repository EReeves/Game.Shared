using System.Collections.Generic;
using System.Linq;
using Nez;
using Nez.UI;

namespace Game.Shared.Components.UI
{
    public class UIComponent : Component, IUpdatable
    {
        public const int UIRenderLayer = 25;
        private ScreenSpaceRenderer renderer;
        public UiCanvas Canvas;

        protected UIComponent(Skin skin = null)
        {
            Skin = skin ?? Skin.CreateDefaultSkin();
        }

        public Skin Skin { get; }
        public List<ISubUI> SubUI { get; set; } = new List<ISubUI>();

        //The parent element.
        public Table Table { get; set; }

        /// <summary>
        /// Update the SubUI's implementing IUpdateable.
        /// </summary>
        public void Update()
        {
            foreach (var subUI in SubUI)
                if (subUI is IUpdatable updatable)
                    updatable.Update();
        }

        public override void OnAddedToEntity()
        {
            SetUp();
        }

        /// <summary>
        /// Sets up the renderer, canvas, stage and puts in a table filling the whole screen.
        /// </summary>
        private void SetUp()
        {
            renderer = new ScreenSpaceRenderer(0, UIRenderLayer);
            Entity.Scene.AddRenderer(renderer);
            Canvas = new UiCanvas();
            Canvas.SetRenderLayer(UIRenderLayer);
            Entity.AddComponent(Canvas);
            Table = new Table();
            Canvas.Stage.AddElement(Table);
            Table.SetFillParent(true);
        }

        /// <summary>
        /// Gets the first instance of a SubUI of type T.
        /// e.g. IChatUI derives from ISubUI so a class implementing it could be found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetSubUI<T>() where T : ISubUI
        {
            return (T) SubUI.First(a => a is T);
        }

        public void RemoveFocus() => Canvas.Stage.UnfocusAll();
        
        
        
    }
}