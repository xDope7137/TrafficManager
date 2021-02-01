using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using System;
using System.Collections.Generic;
using System.IO;
using TrafficManager.Helpers;
using System.Linq;
using System.Threading.Tasks;
using CitizenFX.Core.UI;

namespace TrafficManager
{
    public class Main : BaseScript
    {
        public Main()
        {
            EventHandlers["TrafficManager:CreateSpeedZone"] += new Action<byte[]>((data) =>
            {
                using MemoryStream stream = new MemoryStream(data);
                using BinaryReader reader = new BinaryReader(stream);

                SpeedZone zone = reader.Read<SpeedZone>();
                zone.Enable();
                SpeedZone.List.Add(zone);
            });

            EventHandlers["TrafficManager:RemoveSpeedZone"] += new Action<int>((id) =>
            {
                SpeedZone zone = SpeedZone.List.FirstOrDefault(z => z.Id == id);
                if (zone == null) return;

                zone.Disable();
                SpeedZone.List.Remove(zone);
            });

            EventHandlers["TrafficManager:CreateSecureZone"] += new Action<byte[]>((data) =>
            {
                using MemoryStream stream = new MemoryStream(data);
                using BinaryReader reader = new BinaryReader(stream);

                SecureZone zone = reader.Read<SecureZone>();
                zone.Enable();
                SecureZone.List.Add(zone);

                Tick += SecureZoneThread;
            });

            EventHandlers["TrafficManager:RemoveSecureZone"] += new Action<int>((id) =>
            {
                SecureZone zone = SecureZone.List.FirstOrDefault(z => z.Id == id);
                if (zone == null) return;

                zone.Disable();
                SecureZone.List.Remove(zone);

                if (SecureZone.List.Count == 0)
                {
                    Tick -= SecureZoneThread;
                }
            });

            EventHandlers["TrafficManager:EditNode"] += new Action<byte[]>((data) =>
            {
                using MemoryStream stream = new MemoryStream(data);
                using BinaryReader reader = new BinaryReader(stream);

                Node node = reader.Read<Node>();
                node.Enable();
                Node.List.Add(node);
            });

            EventHandlers["TrafficManager:RemoveNode"] += new Action<int>((id) =>
            {
                Node node = Node.List.FirstOrDefault(n => n.Id == id);
                if (node == null) return;

                node.Disable();
                Node.List.Remove(node);
            });

            EventHandlers["TrafficManager:FullSync"] += new Action<byte[]>((data) =>
            {
                using MemoryStream stream = new MemoryStream(data);
                using BinaryReader reader = new BinaryReader(stream);

                List<SpeedZone> speedZones = reader.ReadList<SpeedZone>();
                speedZones.ForEach(s => s.Enable());
                SpeedZone.List.AddRange(speedZones);

                List<SecureZone> secureZones = reader.ReadList<SecureZone>();
                secureZones.ForEach(s => s.Enable());
                SecureZone.List.AddRange(secureZones);

                if (SecureZone.List.Count > 0)
                {
                    Tick += SecureZoneThread;
                }

                List<Node> nodes = reader.ReadList<Node>();
                nodes.ForEach(n => n.Enable());
                Node.List.AddRange(nodes);
            });

            RegisterCommand("trafficmenu", new Action<int, List<object>, string>((source, args, raw) =>
            {
                Menu.Instance.Toggle();
            }), false);
            RegisterKeyMapping("trafficmenu", "Traffic", "keyboard", "F5");

            Menu.ForceLoad();

            TriggerServerEvent("TrafficManager:PlayerConnected");
        }

        private async Task SecureZoneThread()
        {
            Dictionary<int, Vector3> kvPedPos = new Dictionary<int, Vector3>();
            foreach (int ped in GetGamePool("CPed"))
            {
                if (NetworkHasControlOfEntity(ped) && GetPedType(ped) != 28 && !IsPedAPlayer(ped))
                {
                    kvPedPos.Add(ped, GetEntityCoords(ped, true));
                }
            }

            foreach (SecureZone zone in SecureZone.List)
            {
                zone.Run(kvPedPos);
            }

            await Delay(2500);
        }
    }
}