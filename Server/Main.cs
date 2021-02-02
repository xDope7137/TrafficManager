using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.IO;
using System.Linq;
using TrafficManager.Helpers;

namespace TrafficManager
{
    public class Main : BaseScript
    {
        private bool UsePermissions { get; }

        public Main()
        {
            EventHandlers["TrafficManager:CreateSpeedZone"] += new Action<Player, byte[]>(OnCreateSpeedZone);

            EventHandlers["TrafficManager:RemoveSpeedZone"] += new Action<Player, int>(OnRemoveSpeedZone);

            EventHandlers["TrafficManager:CreateSecureZone"] += new Action<Player, byte[]>(OnCreateSecureZone);

            EventHandlers["TrafficManager:RemoveSecureZone"] += new Action<Player, int>(OnRemoveSecureZone);

            EventHandlers["TrafficManager:EditNode"] += new Action<Player, byte[]>(OnEditNode);

            EventHandlers["TrafficManager:ToggleMenu"] += new Action<Player>(OnToggleMenu);

            EventHandlers["TrafficManager:PlayerConnected"] += new Action<Player>(OnPlayerConnected);

            string usePerms = GetResourceMetadata(GetCurrentResourceName(), "use_permissions", 0) ?? "false";
            UsePermissions = usePerms.ToLower() == "true";
        }

        private void OnCreateSpeedZone([FromSource] Player player, byte[] data)
        {
            if (!IsConfigurationEnabled("use_speed_zones")) return;

            if (!IsPlayerAllowed(player)) return;

            SpeedZone.Create(data);
        }

        private void OnRemoveSpeedZone([FromSource] Player player, int id)
        {
            if (!IsConfigurationEnabled("use_speed_zones")) return;

            if (!IsPlayerAllowed(player)) return;

            SpeedZone zone = SpeedZone.List.FirstOrDefault(z => z.Id == id);
            if (zone == null) return;

            SpeedZone.List.Remove(zone);
            TriggerClientEvent("TrafficManager:RemoveSpeedZone", id);
        }

        private void OnCreateSecureZone([FromSource] Player player, byte[] data)
        {
            if (!IsConfigurationEnabled("use_secure_zones")) return;

            if (!IsPlayerAllowed(player)) return;

            SecureZone.Create(data);
        }

        private void OnRemoveSecureZone([FromSource] Player player, int id)
        {
            if (!IsConfigurationEnabled("use_secure_zones")) return;

            if (!IsPlayerAllowed(player)) return;

            SecureZone zone = SecureZone.List.FirstOrDefault(z => z.Id == id);
            if (zone == null) return;

            SecureZone.List.Remove(zone);
            TriggerClientEvent("TrafficManager:RemoveSecureZone", id);
        }

        private void OnEditNode([FromSource] Player player, byte[] data)
        {
            if (!IsConfigurationEnabled("edit_traffic_nodes")) return;

            if (!IsPlayerAllowed(player)) return;

            Node.Create(data);
        }

        private void OnToggleMenu([FromSource] Player player)
        {
            if (!IsPlayerAllowed(player)) return;

            TriggerClientEvent(player, "TrafficManager:ToggleMenu");
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

        private bool IsPlayerAllowed(Player player)
        {
            if (!UsePermissions) return true;

            return IsPlayerAceAllowed(player.Handle, "TrafficManager.Access");
        }

        private bool IsConfigurationEnabled(string key)
        {
            string value = GetResourceMetadata(GetCurrentResourceName(), key, 0) ?? "false";
            return value.ToLower() == "true";
        }
    }
}
