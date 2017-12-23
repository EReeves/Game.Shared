using Nez;
using Nez.UI;

namespace Game.Shared.Components.UI
{
    public interface ISubUI
    {
        UIComponent parent { get; set; }
        Element element { get; set; }
        
        void RegisterSubUI(UIComponent _parent, Element _element = null);
    }
}