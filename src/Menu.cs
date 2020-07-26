using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using NativeUI;

namespace SmartClamp
{
    public static class Menu
    {
        private static MenuPool menuPool;
        private static UIMenu mainMenu;

        static Menu()
        {
            menuPool = new MenuPool()
            {
                MouseEdgeEnabled = false,
                ControlDisablingEnabled = false
            };
            mainMenu = new UIMenu("Vehicle Clamp", "Actions")
            {
                MouseControlsEnabled = false
            };
            menuPool.Add(mainMenu);
            VehicleControlMenu(mainMenu);
            menuPool.RefreshIndex();
        }

        internal static async void Toggle()
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
                    await BaseScript.Delay(0);
                }
            }
        }

        private static void VehicleControlMenu(UIMenu menu)
        {
            var clampItem = new UIMenuItem("Clamp Vehicle", "Clamp the nearest vehicle");
            clampItem.SetRightBadge(UIMenuItem.BadgeStyle.Car);
            menu.AddItem(clampItem);

            var stickerItem = new UIMenuItem("Place Sticker", "Place a seized sticker on the nearest vehicle");
            stickerItem.SetRightBadge(UIMenuItem.BadgeStyle.Car);
            menu.AddItem(stickerItem);

            var removeClamp = new UIMenuItem("Remove Object", "Removes a nearby clamp or sticker");
            menu.AddItem(removeClamp);
            menu.OnItemSelect += (sender, item, index) =>
            {
                if (item == stickerItem)
                {
                    Main.StickerHandler();
                }
                else if (item == clampItem)
                {
                    Main.ClampHandler();
                }
                else if (item == removeClamp)
                {
                    Main.UpdateObject();
                }
                Toggle();
            };
        }
    }
}
