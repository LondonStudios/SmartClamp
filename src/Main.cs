using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace SmartClamp
{
    public class Main : BaseScript
    {
        public static Dictionary<int, int> Clamps { get; set; }
        public static Dictionary<int, int> Stickers { get; set; }
        public static int Object { get; set; }

        public Main()
        {
            RequestLoad(GetHashKey("prop_cs_protest_sign_03"));
            RequestLoad(GetHashKey("p_car_keys_01"));

            Clamps = new Dictionary<int, int> { };
            Stickers = new Dictionary<int, int> { };
            TriggerEvent("chat:addSuggestion", "/clamp", "Open the vehicle seize menu.");

            EventHandlers["Client:ChangeCarState"] += new Action<int, bool>((netid, disable) =>
            {
                if (disable)
                {
                    SetVehicleEngineOn(NetToVeh(netid), false, false, true);
                }
                else
                {
                    SetVehicleEngineOn(NetToVeh(netid), true, false, false);
                }
            });
        }

        [Command("clamp")]
        private void ClampCommand()
        {
            Menu.Toggle();
        }

        private static int Raycast()
        {
            var location = GetEntityCoords(PlayerPedId(), true);
            var offSet = GetOffsetFromEntityInWorldCoords(PlayerPedId(), 0.0f, 2.0f, 0.0f);
            var shapeTest = StartShapeTestCapsule(location.X, location.Y, location.Z, offSet.X, offSet.Y, offSet.Z, 5.0f, 10, PlayerPedId(), 7);
            bool hit = false;
            Vector3 endCoords = new Vector3(0f, 0f, 0f);
            Vector3 surfaceNormal = new Vector3(0f, 0f, 0f);
            int entityHit = 0;
            var result = GetShapeTestResult(shapeTest, ref hit, ref endCoords, ref surfaceNormal, ref entityHit);
            return entityHit;
        }

        private static void ShowNotification(string text)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentString(text);
            EndTextCommandThefeedPostTicker(false, false);
        }

        private static void SpawnModel(int type, int vehicle)
        {
            var coords = GetEntityCoords(PlayerPedId(), false);
            SetNetworkIdExistsOnAllMachines(VehToNet(vehicle), true);
            if (type == 0)
            {
                RequestModel((uint)GetHashKey("p_car_keys_01"));
                Object = CreateObject(GetHashKey("p_car_keys_01"), coords.X, coords.Y, coords.Z, true, true, true);
                var boneIndex = GetEntityBoneIndexByName(vehicle, "wheel_lf");
                SetEntityHeading(Object, 0f);
                SetEntityRotation(Object, 60f, 20f, 10f, 1, true);
                AttachEntityToEntity(Object, vehicle, boneIndex, -0.10f, 0.15f, -0.30f, 180f, 200f, 90f, true, true, false, false, 2, true);
                SetEntityRotation(Object, 60f, 20f, 10f, 1, true);
                SetEntityAsMissionEntity(Object, true, true);
                FreezeEntityPosition(Object, true);
                TriggerServerEvent("Server:ChangeCarState", VehToNet(vehicle), true);
                Clamps.Add(Object, vehicle);
            }
            else if (type == 1)
            {
                RequestModel((uint)GetHashKey("prop_cs_protest_sign_03"));
                Object = CreateObject(GetHashKey("prop_cs_protest_sign_03"), coords.X, coords.Y, coords.Z, true, true, true);
                var boneIndex = GetEntityBoneIndexByName(vehicle, "interiorlight");
                var model = GetEntityModel(vehicle);
                var vector1 = new Vector3(0f, 0f, 0f);
                var vector2 = new Vector3(0f, 0f, 0f);
                GetModelDimensions((uint)model, ref vector1, ref vector2);
                FreezeEntityPosition(Object, true);
                SetEntityAsMissionEntity(Object, true, true);
                //AttachEntityToEntity(Object, vehicle, boneIndex, 0f, ((vector2.Y - vector1.Y) / 2), ((vector2.Z - vector1.Z) / 2) - ((vector2.Z - vector1.Z) / 5f), 0f, 0f, 0f, true, true, false, false, 1, true);
                AttachEntityToEntity(Object, vehicle, boneIndex, 0f, -((vector2.Y - vector1.Y) / 2) + 5f, -0.12f, 0f, 0f, 0f, true, true, false, false, 1, true);
                Stickers.Add(Object, vehicle);
            }
        }

        public static void UpdateObject()
        {
            var vehicle = Raycast();
            if (IsEntityAVehicle(vehicle))
            {
                foreach (KeyValuePair<int, int> kvp in Clamps)
                {
                    if (vehicle == kvp.Value)
                    {
                        var entity = kvp.Key;
                        DeleteEntity(ref entity);
                        TriggerServerEvent("Server:ChangeCarState", VehToNet(vehicle), false);
                        break;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in Stickers)
                {
                    if (vehicle == kvp.Value)
                    {
                        var entity = kvp.Key;
                        DeleteEntity(ref entity);
                        break;
                    }
                }
            }
            else
            {
                Main.ShowNotification("No ~b~nearby ~w~vehicle found.");
            }
        }

        public static void ClampHandler()
        {
            var vehicle = Raycast();
            var coords = GetEntityCoords(PlayerPedId(), true);
            if (IsEntityAVehicle(vehicle))
            {
                SpawnModel(0, vehicle);
            }
            else
            {
                Main.ShowNotification("No ~b~nearby ~w~vehicle found.");
            }
        }
        public static void StickerHandler()
        {
            var vehicle = Raycast();
            if (IsEntityAVehicle(vehicle))
            {
                SpawnModel(1, vehicle);
            }
            else
            {
                Main.ShowNotification("No ~b~nearby ~w~vehicle found.");
            }
        }

        private async void RequestLoad(int model)
        {
            while (!HasModelLoaded((uint)model))
            {
                await Delay(0);
            }
            await Delay(100);
        }


    }
}
