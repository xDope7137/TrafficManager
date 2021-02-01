using CitizenFX.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class Node : ISerialisable
    {
        public int Id { get; set; }

        private Vector3 Position { get; set; }

        private bool Enabled { get; set; }

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

        public static void Create(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            Node node = reader.Read<Node>();
            Node existingNode = List.FirstOrDefault(n => n.Position == node.Position);
            if (existingNode != null)
            {
                List.Remove(existingNode);
                BaseScript.TriggerClientEvent("TrafficManager:RemoveNode", existingNode.Id);
                return;
            }

            node.Id = Utility.GetNextIdentifier();
            List.Add(node);

            stream.Position = 0;
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(node);

            BaseScript.TriggerClientEvent("TrafficManager:EditNode", stream.ToArray());
        }
    }
}
