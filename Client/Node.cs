using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.Collections.Generic;
using System.IO;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class Node : ISerialisable
    {
        public int Id { get; set; }

        private Vector3 Position { get; set; }

        private bool Enabled { get; set; }

        public void Enable()
        {
            SetRoadsInArea(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f, Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f, Enabled, false);
            SetIgnoreSecondaryRouteNodes(true);
        }

        public void Disable()
        {
            SetRoadsBackToOriginal(Position.X + 0.5f, Position.Y + 0.5f, Position.Z + 0.5f, Position.X - 0.5f, Position.Y - 0.5f, Position.Z - 0.5f);
        }

        public void Serialise(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Position);
            writer.Write(Enabled);
        }

        public void Deserialise(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Position = reader.ReadVector3();
            Enabled = reader.ReadBoolean();
        }

        public static List<Node> List { get; } = new List<Node>();

        public static void Create(Vector3 position, bool enabled)
        {
            Node node = new Node()
            {
                Position = position,
                Enabled = enabled
            };

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(node);

            BaseScript.TriggerServerEvent("TrafficManager:EditNode", stream.ToArray());
        }
    }
}
