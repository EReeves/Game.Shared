using Nez.UI;

namespace Game.Shared.Components.UI
{
    /// <summary>
    /// Used to add UI to a UIComponent
    /// </summary>
    public interface ISubUI
    {
        UIComponent UI { get; set; }
        Element Element { get; set; }

        /// <summary>
        /// Called to link the UI to the parent UIComponent with an optional element passed for miscelaneous use.
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="element"></param>
        void RegisterSubUI(UIComponent ui, Element element = null);
    }
}