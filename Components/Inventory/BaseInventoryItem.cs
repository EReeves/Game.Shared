using Game.Shared.Components.Inventory;

namespace Game.Shared.Components.Inventory
{
    public class BaseInventoryItem : IInventoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}