using System;
using System.IO;
using System.Security.Cryptography;
using Lidgren.Network;
using ProtoBuf;

namespace Game.Shared.Network
{
    public static class NetworkEvents
    {
        public enum Event : uint
        {
            BlankNetworkEntity = 0,
            Name = 1,
            ChatMessage = 2
        }

        /// <summary>
        ///     Runs a callback over a NetIncomingMessage if the event matches.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="ev"></param>
        /// <param name="receiverPeerType"></param>
        /// <param name="callback"></param>
        public static void RunEvent(this NetIncomingMessage message, Event ev, NetworkSingleton.PeerType receiverPeerType,
            NetworkSingleton.IncomingMessageDelegate callback)
        {
            if ((Event) message.ReadUInt32() == ev)
                callback.Invoke(receiverPeerType, message);
        }

        /// <summary>
        ///     Serualizes an event with an object of attribute ProtoContract.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ev"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize<T>(this Event ev, T obj) //Where T has attribute ProtoContract
        {
            var ms = new MemoryStream();
            var eventBytes = BitConverter.GetBytes((uint) ev);
            ms.Write(eventBytes, 0, eventBytes.Length);
            Serializer.Serialize(ms, obj);
            return ms.ToArray();
        }

        /// <summary>
        ///     Deserializes a byte array in to an event and object of attribute ProtoContract.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static T DeserializeEventData<T>(this byte[] bytes) //Where T has attribute ProtoContract
        {
            var stream = new MemoryStream(bytes);
            var ne = Serializer.Deserialize<T>(stream);
            return ne;
        }

        public static byte[] ReadDataBytes(this NetIncomingMessage msg)
        {
            const int size = sizeof(uint);
            msg.Position = size * 8; //Set buffer position in bits.
            return msg.ReadBytes(msg.LengthBytes - size);
        }
    }
}