using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using GTA;
using GTA.Math;
using GTA.Native;
using Font = GTA.Font;

namespace CarPictureSamples
{
    class Utils
    {
        public static void DisplayRadar(bool enabled)
        {
            Function.Call(Hash.DISPLAY_RADAR, enabled);
        }

        public static void DisplayPlayer(bool enabled)
        {
            Game.Player.Character.IsInvincible = !enabled;
            Game.Player.Character.IsVisible = enabled;
        }

        public static VehicleHash ToVehicleHash(int value)
        {
            VehicleHash vehicleHash = (VehicleHash)Enum.Parse(typeof(VehicleHash), ((uint)value).ToString());
            return vehicleHash;
        }

        public static Weather ToWeather(String weather)
        {
            return (Weather)Enum.Parse(typeof(Weather), weather);
        }

        public static double RadianToDegree(float radians)
        {
            return radians * (180.0 / Math.PI);
        }

        public static double DegreeToRadians(float degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        public static float GetAngleBetwee2dVectors(float x1, float y1, float x2, float y2)
        {
            return Function.Call<float>(GTA.Native.Hash.GET_ANGLE_BETWEEN_2D_VECTORS, x1, y1, x2, y2);
        }

        public static string Sha1Hash(string input)
        {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        public static void Log(String data)
        {
            System.IO.File.AppendAllText(@"C:\Users\ale\Desktop\dataset\log.txt", $"{DateTime.Now.ToString()}: " + data + "\r\n");
        }

        public static Vector3 RotateVector(float radius, float angle)
        {
            double angleInRadians = DegreeToRadians(angle);
            float offsetX = radius * (float)Math.Cos(angleInRadians);
            float offsetY = radius * (float)Math.Sin(angleInRadians);
            return new Vector3(offsetX, offsetY, 0);
        }

        public static void Teleport(Vector3 newPosition, Vector3 offest)
        {
            Game.Player.Character.Position = newPosition + offest;
            // Wait render scene
            Script.Wait(2500);
        }

        public static void SetTime(String time)
        {
            DateTime dt = DateTime.ParseExact(time, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            World.CurrentDayTime = dt.TimeOfDay;
        }

        //private void setNearbyVehicle()
        //{
        //    Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 4);

        //    if (nearbyVehicles.Length > 0)
        //    {
        //        NearbyVehicle = nearbyVehicles.ElementAt(0);
        //        NearbyVehicle.Model.GetDimensions(out min, out max);
        //        vehicleDimensions = NearbyVehicle.Model.GetDimensions();
        //        radius = max.X - min.X;
        //        //radius = (vehicleDimensions.X / 2);
        //        UI.ShowSubtitle("Set Vehicle: " + NearbyVehicle);
        //    }
        //}

        // UI.ShowSubtitle("Is Invicible " + Game.Player.Character.IsInvincible);
        //UI.ShowSubtitle("Is Visible" + Game.Player.Character.IsVisible);
        //if (Game.Player.Character.IsVisible)
        //{
        //    Game.Player.Character.IsInvincible = true;
        //    Game.Player.Character.IsVisible = false;
        //} else
        //{
        //    Game.Player.Character.IsInvincible = false;
        //    Game.Player.Character.IsVisible = true;
        //}


        // Print("DD");
        //vehicle = World.CreateVehicle(VehicleHash.Cheetah2, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3.0f, Game.Player.Character.Heading + 90);
        //vehicle.CanTiresBurst = false;
        //vehicle.CustomPrimaryColor = Color.FromArgb(38, 38, 38);
        //vehicle.CustomSecondaryColor = Color.DarkOrange;
        //vehicle.PlaceOnGround();
        //vehicle.NumberPlate = "SHVDN";


        //public bool UsePlayerView
        //{
        //    get
        //    {
        //        return _usePlayerView;
        //    }
        //    set
        //    {
        //        if (value)
        //        {
        //            _startPos = Game.Player.Character.Position;
        //            Game.Player.Character.IsInvincible = true;
        //            Game.Player.Character.IsVisible = false;
        //        }
        //        else
        //        {
        //            if (_startPos != null)
        //                Game.Player.Character.Position = _startPos;
        //            Game.Player.Character.IsInvincible = false;
        //            Game.Player.Character.IsVisible = true;
        //        }

        //        this._usePlayerView = value;
        //    }
        //}


    }
}
