using CitizenFX.Core;
using System;
using System.IO;
using System.Linq;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class Main : BaseScript
    {
        public Main()
        {
            EventHandlers["TrafficManager:CreateSpeedZone"] += new Action<byte[]>((data) =>
            {
                SpeedZone.Create(data);
            });

            EventHandlers["TrafficManager:RemoveSpeedZone"] += new Action<int>((id) =>
            {
                SpeedZone zone = SpeedZone.List.FirstOrDefault(z => z.Id == id);
                if (zone == null) return;

                SpeedZone.List.Remove(zone);
                TriggerClientEvent("TrafficManager:RemoveSpeedZone", id);
            });

            EventHandlers["TrafficManager:CreateSecureZone"] += new Action<byte[]>((data) =>
            {
                SecureZone.Create(data);
            });

            EventHandlers["TrafficManager:RemoveSecureZone"] += new Action<int>((id) =>
            {
                SecureZone zone = SecureZone.List.FirstOrDefault(z => z.Id == id);
                if (zone == null) return;

                SecureZone.List.Remove(zone);
                TriggerClientEvent("TrafficManager:RemoveSecureZone", id);
            });

            EventHandlers["TrafficManager:EditNode"] += new Action<byte[]>((data) =>
            {
                Node.Create(data);
            });

            EventHandlers["TrafficManager:PlayerConnected"] += new Action<Player>(OnPlayerConnected);
        }

        private void OnPlayerConnected([FromSource] Player player)
        {
            using MemoryStream stream = new MemoryStream();
            using BinaryWriter writer = new BinaryWriter(stream);

            writer.Write(SpeedZone.List);
            writer.Write(SecureZone.List);
            writer.Write(Node.List);

            TriggerClientEvent(player, "TrafficManager:FullSync", stream.ToArray());
        }
    }
}
