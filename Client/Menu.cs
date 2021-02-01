using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;
using System.Collections.Generic;

namespace TrafficManager
{
    public class Menu
    {
        private readonly MenuPool menuPool;

        private readonly UIMenu mainMenu;

        private UIMenu speedZoneMenu;

        public Menu()
        {
            menuPool = new MenuPool();
            mainMenu = new UIMenu("Traffic Manager", "Main Menu")
            {
                MouseControlsEnabled = false
            };
            menuPool.Add(mainMenu);

            AddSpeedZoneMenu();
            AddSecureZoneMenu();
            EditNodeMenu();
        }

        private void AddSpeedZoneMenu()
        {
            UIMenu submenu = menuPool.AddSubMenu(mainMenu, "Speed Zones");
            speedZoneMenu = submenu; // Used in ForceLoad, serves no other purpose.
            submenu.MouseControlsEnabled = false;

            List<dynamic> radiusList = new List<dynamic>() { 5, 10, 15, 20, 25, 30, 40, 50, 75, 100 };
            UIMenuListItem radius = new UIMenuListItem("Radius", radiusList, 0);
            submenu.AddItem(radius);

            List<dynamic> speedList = new List<dynamic>() { 0, 5, 10, 15, 20, 25, 30, 40, 50, 60, 70 };
            UIMenuListItem speed = new UIMenuListItem("Speed (MPH)", speedList, 0);
            submenu.AddItem(speed);

            UIMenuItem create = new UIMenuItem("~b~Create");
            submenu.AddItem(create);

            UIMenuItem remove = new UIMenuItem("~r~Remove Closest");
            submenu.AddItem(remove);

            submenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == create)
                {
                    string speedType = GetResourceMetadata(GetCurrentResourceName(), "speed_type", 0) ?? "MPH";
                    float speedDiv = speedType.ToLower() == "mph" ? 2.237f : 3.59f;

                    SpeedZone.Create(radiusList[radius.Index], speedList[speed.Index] / speedDiv);
                }
                else if (item == remove)
                {
                    Vector3 myPos = GetEntityCoords(PlayerPedId(), true);

                    SpeedZone closest = null;
                    float closestDistance = 100f;
                    foreach (SpeedZone zone in SpeedZone.List)
                    {
                        float distance = Vector3.Distance(myPos, zone.Position);
                        if (distance < closestDistance)
                        {
                            closest = zone;
                            closestDistance = distance;
                        }
                    }

                    if (closest != null)
                    {
                        BaseScript.TriggerServerEvent("TrafficManager:RemoveSpeedZone", closest.Id);
                    }
                }
            };
        }

        private void AddSecureZoneMenu()
        {
            UIMenu submenu = menuPool.AddSubMenu(mainMenu, "Secure Zones");
            submenu.MouseControlsEnabled = false;

            List<dynamic> radiusList = new List<dynamic>() { 5, 10, 15, 20, 25, 30, 40, 50, 75, 100 };
            UIMenuListItem radius = new UIMenuListItem("Radius", radiusList, 0);
            submenu.AddItem(radius);

            UIMenuItem create = new UIMenuItem("~b~Create");
            submenu.AddItem(create);

            UIMenuItem remove = new UIMenuItem("~r~Remove Closest");
            submenu.AddItem(remove);

            submenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == create)
                {
                    SecureZone.Create(radiusList[radius.Index]);
                }
                else if (item == remove)
                {
                    Vector3 myPos = GetEntityCoords(PlayerPedId(), true);

                    SecureZone closest = null;
                    float closestDistance = 100f;
                    foreach (SecureZone zone in SecureZone.List)
                    {
                        float distance = Vector3.Distance(myPos, zone.Position);
                        if (distance < closestDistance)
                        {
                            closest = zone;
                            closestDistance = distance;
                        }
                    }

                    if (closest != null)
                    {
                        BaseScript.TriggerServerEvent("TrafficManager:RemoveSecureZone", closest.Id);
                    }
                }
            };
        }

        private void EditNodeMenu()
        {
            UIMenu submenu = menuPool.AddSubMenu(mainMenu, "Traffic Nodes");
            submenu.MouseControlsEnabled = false;

            UIMenuItem editor = new UIMenuItem("Enter Editor");
            submenu.AddItem(editor);

            UIMenuItem skyEditor = new UIMenuItem("Enter Sky Editor");
            submenu.AddItem(skyEditor);

            UIMenuItem close = new UIMenuItem("Close Editor");
            submenu.AddItem(close);

            submenu.OnItemSelect += (sender, item, index) =>
            {
                if (item == editor)
                {
                    menuPool.CloseAllMenus();
                    NodeEditor.Instance.Enable(false);
                }
                else if (item == skyEditor)
                {
                    menuPool.CloseAllMenus();
                    NodeEditor.Instance.Enable(true);
                }
                else if (item == close)
                {
                    NodeEditor.Instance.Disable();
                }
            };
        }

        public async void Toggle()
        {
            if (menuPool.IsAnyMenuOpen())
            {
                menuPool.CloseAllMenus();
            }
            else
            {
                mainMenu.Visible = true;
                while (menuPool.IsAnyMenuOpen())
                {
                    menuPool.ProcessMenus();
                    menuPool.ProcessMouse();
                    await BaseScript.Delay(0);
                }
            }
        }

        public static Menu Instance { get; }

        static Menu()
        {
            Instance = new Menu();
        }

        public static void ForceLoad()
        {
            // If I don't pre-call a menu, the first time you open a menu with a dynamic list
            // The entire thread blocks for a second. So as a "workaroud".
            Instance.speedZoneMenu.Visible = true;
            Instance.menuPool.ProcessMenus();
            Instance.speedZoneMenu.Visible = false;
        }
    }
}
