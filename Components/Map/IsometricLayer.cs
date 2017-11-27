using Game.Shared.Utility;

namespace Game.Shared.Components.Map
{
    public class IsometricLayer
    {
        public string Name { get; set; }
        public DenseArray<int> indices { get; set; }

        public IsometricLayer(int sizex, int sizey)
        {
            indices = new DenseArray<int>(sizey, sizex);
        }
    }
}