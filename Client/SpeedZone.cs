using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System.IO;
using System.Collections.Generic;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class SpeedZone : ISerialisable
    {
        public int Id { get; set; }

        public Vector3 Position { get; set; }

        private float Radius { get; set; }

        private float Speed { get; set; }

        private int speedzone;

        private int blip;

        public void Enable()
        {
            speedzone = AddSpeedZoneForCoord(Position.X, Position.Y, Position.Z, Radius, Speed, false);
            blip = AddBlipForRadius(Position.X, Position.Y, Position.Z, Radius);
            SetBlipAlpha(blip, 120);
            SetBlipSprite(blip, 9);
            SetBlipColour(blip, 62);
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
            writer.Write(Speed);
        }

        public void Deserialise(BinaryReader reader)
        {
            Id = reader.ReadInt32();
            Position = reader.ReadVector3();
            Radius = reader.ReadSingle();
            Speed = reader.ReadSingle();
        }

        public static List<SpeedZone> List { get; } = new List<SpeedZone>();

        public static void Create(int radius, float speed)
        {
            SpeedZone zone = new SpeedZone()
            {
                Position = GetEntityCoords(PlayerPedId(), true),
                Radius = radius,
                Speed = speed
            };

            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(zone);

            BaseScript.TriggerServerEvent("TrafficManager:CreateSpeedZone", stream.ToArray());
        }
    }
}
