using System;

namespace Game.Shared.Components.UI
{
    public interface IChatUI : ISubUI
    {
        void SetChatText(string text);
        event EventHandler<string> OnChatSubmitted;
    }
}