using Game.Shared.Network;

namespace Game.Shared.NetworkComponents
{
    public class NetworkComponent
    {
        protected NetworkSingleton Network;

        public NetworkComponent()
        {
            Network = NetworkSingleton.Instance;
        }
    }
}