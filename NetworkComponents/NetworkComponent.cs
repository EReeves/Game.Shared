using Game.Shared.Network;

namespace Game.Shared.NetworkComponents
{
    public class NetworkComponent
    {
        protected NetworkSingleton network;

        public NetworkComponent()
        {
            network = NetworkSingleton.Instance;
        }
    }
}