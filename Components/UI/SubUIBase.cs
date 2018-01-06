using Nez.UI;

namespace Game.Shared.Components.UI
{
    /// <inheritdoc />
    /// <summary>
    /// Most UI should derive from this class.
    /// Is added to a UIComponent as a SubUI.
    /// </summary>
    public abstract class SubUIBase : ISubUI
    {
        public UIComponent UI { get; set; }
        public Element Element { get; set; }

        public virtual void RegisterSubUI(UIComponent ui, Element element = null)
        {
            UI = ui;
            Element = element;
            UI.SubUI.Add(this);
        }
    }
}