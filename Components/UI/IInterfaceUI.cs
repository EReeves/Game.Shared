using System;

namespace Game.Shared.Components.UI
{
    public interface IInventoryUI
    {
        event EventHandler<object> OnItemAdded;
        event EventHandler<object> OnItemRemoved;
    }
}