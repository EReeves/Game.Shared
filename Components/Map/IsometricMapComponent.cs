using Game.Shared.Utility;
using Nez;

namespace Game.Shared.Components.Map
{
    internal class IsometricMapComponent : Component, IUpdatable
    {
        private readonly IsometricMap map;

        public IsometricMapComponent(IsometricMap _map)
        {
            map = _map;
        }

        public void update()
        {
        }

        public override void onAddedToEntity()
        {
            //Add sub components.
            for (var i = 0; i < map.Layers.Count; i++)
            {
                //Set render layer before adding.
                var layer = Isometric.RENDER_LAYER_START - i;
                if (layer > map.ObjectRenderLayer && layer < map.ObjectRenderLayerEnd)
                    map.Layers[i].setRenderLayer(layer);
                else
                    map.Layers[i].SetAsObjectPositioningLayer();

                entity.addComponent(map.Layers[i]);
            }

            map.CalculateOverlapZones(entity);
        }
    }
}