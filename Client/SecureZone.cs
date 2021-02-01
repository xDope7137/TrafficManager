using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.IO;
using System.Collections.Generic;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class SecureZone : ISerialisable
    {
        public int Id { get; set; }

        public Vector3 Position { get; set; }

        private float Radius { get; set; }

        private int speedzone;

        private int blip;

        public void Enable()
        {
            speedzone = AddSpeedZoneForCoord(Position.X, Position.Y, Position.Z, Radius, 0f, false);
            blip = AddBlipForRadius(Position.X, Position.Y, Position.Z, Radius);
            SetBlipAlpha(blip, 120);
            SetBlipSprite(blip, 9);
            SetBlipColour(blip, 1);
        }

        public void Run(Dictionary<int, Vector3> kvPedPos)
        {
            foreach (var kv in kvPedPos)
            {
                float distance = Vector3.Distance(Position, kv.Value);
                if (distance < Radius)
                {
                    if (GetScriptTaskStatus(kv.Key, 4043842218) == 7)
                    {
                        TaskSmartFleeCoord(kv.Key, Position.X, Position.Y, Position.Z, 100f, 10000, false, false);
                    }
                }
            }
        }

        public void Disable()
        {
            RemoveSpeedZone(speedzone);
            RemoveBlip(ref blip);
        }

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

        public static void Create(int radius)
        {
            SecureZone zone = new SecureZone()
            {
                Position = GetEntityCoords(PlayerPedId(), true),
                Radius = radius
            };

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(zone);

            BaseScript.TriggerServerEvent("TrafficManager:CreateSecureZone", stream.ToArray());
        }
    }
}
