using Nez;

namespace Game.Shared.Components.UI
{
    /// <summary>
    /// Clean up the scene class a bit.
    /// Has slots for all the UI elements and related elements.
    /// </summary>
    public class UIContainer
    {
        public Entity Entity { get; set; }
        public UIComponent Component { get; set; }
        public IChatUI Chat { get; set; }
    }
}