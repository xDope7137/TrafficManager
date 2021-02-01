using CitizenFX.Core;
using System.Collections.Generic;
using System.IO;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class SecureZone : ISerialisable
    {
        public int Id { get; set; }

        private Vector3 Position { get; set; }

        private float Radius { get; set; }

        public void Serialise(BinaryWriter writer)
        {
            writer.Write(Id);
            writer.Write(Position);
            writer.Write(Radius);
        }

        public void Deserialise(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Position = reader.ReadVector3();
            Radius = reader.ReadSingle();
        }

        public static List<SecureZone> List { get; } = new List<SecureZone>();

        public static void Create(byte[] data)
        {
            using MemoryStream stream = new MemoryStream(data);
            using BinaryReader reader = new BinaryReader(stream);

            SecureZone zone = reader.Read<SecureZone>();
            zone.Id = Utility.GetNextIdentifier();
            List.Add(zone);

            stream.Position = 0;
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(zone);

            BaseScript.TriggerClientEvent("TrafficManager:CreateSecureZone", stream.ToArray());
        }
    }
}
