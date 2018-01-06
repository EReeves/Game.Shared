using Game.Shared.Utility;
using Nez;

namespace Game.Shared.Components.Map
{
    internal class IsometricMapComponent : Component, IUpdatable
    {
        private readonly IsometricMap map;

        public IsometricMapComponent(IsometricMap map)
        {
            this.map = map;
        }

        public void Update()
        {
            
        }

        public override void OnAddedToEntity()
        {
            //Add sub components.
            for (var i = 0; i < map.Layers.Count; i++)
            {
                //Set render layer before adding.
                var layer = Isometric.RenderLayerStart - i;
                if (layer > map.ObjectRenderLayer && layer < map.ObjectRenderLayerEnd)
                    map.Layers[i].SetRenderLayer(layer);
                else
                    map.Layers[i].SetAsObjectPositioningLayer();

                Entity.AddComponent(map.Layers[i]);
            }

            map.CalculateOverlapZones(Entity);
            
            //Check and set depth if specified.
            foreach (var layer in map.Layers)
            {
                layer.CheckForPropertyDepth();
            }
        }
    }
}