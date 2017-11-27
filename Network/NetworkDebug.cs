using System;
using System.Diagnostics;
using Nez.Console;
using Debug = Nez.Debug;

namespace Game.Shared.Network
{
    public class NetworkDebug
    {
        private static NetworkDebug instance;
        private static NetworkSingleton network;

        public NetworkDebug()
        {
            network = NetworkSingleton.Instance;
        }

        public static NetworkDebug Instance
        {
            get
            {
                instance = instance ?? new NetworkDebug();
                return instance;
            }
        }

        [Command("server", "starts the server on 1777 or specified port")]
        private static void StartServer(int port = 1777)
        {
            network = NetworkSingleton.Instance;
            network.InitNetwork(NetworkSingleton.Type.Both, port);
        }

        [Command("connect", "connect to server bu default 127.0.0.1:1777")]
        private static void Connect(string ipPort = "127.0.0.1:1777")
        {
            var split = ipPort.Split(':');
            var ip = split[0];
            var port = Convert.ToInt32(split[1]);
            network.Connect(ip, port);
        }

        [Conditional("DEBUG")]
        public static void log(NetworkSingleton.Type t, string fmt, params object[] args)
        {
            var s = string.Format("{0}: {1}", Enum.GetName(typeof(NetworkSingleton.Type), t), string.Format(fmt, args));
            Debug.log(s);
            DebugConsole.instance.log(s);
        }
    }
}