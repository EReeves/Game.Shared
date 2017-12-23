using Nez.UI;

namespace Game.Shared.Components.UI
{
    public abstract class SubUIBase : ISubUI
    {
        public UIComponent parent { get; set; }
        public Element element { get; set; }
        
        public virtual void RegisterSubUI(UIComponent _parent, Element _element = null)
        {
            parent = _parent;
            element = _element;
            parent.SubUI.Add(this);
        }
    }
}