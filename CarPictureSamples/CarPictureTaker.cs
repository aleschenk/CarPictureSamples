using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using System.Windows.Forms;
using System.Drawing;
using GTA.Native;
// using DeveloperConsole;

namespace CarPictureSamples
{
    public class CarPictureTaker : Script
    {
        UIText line1 = new UIText("", new Point(0, 15 * 0), 0.3f);
        UIText line2 = new UIText("", new Point(0, 15 * 1), 0.3f);
        UIText line3 = new UIText("", new Point(0, 15 * 2), 0.3f);
        UIText line4 = new UIText("", new Point(0, 15 * 3), 0.3f);
        UIText line5 = new UIText("", new Point(0, 15 * 4), 0.3f);
        UIText line6 = new UIText("", new Point(0, 15 * 5), 0.3f);
        UIText line7 = new UIText("", new Point(0, 15 * 6), 0.3f);
        UIText line8 = new UIText("", new Point(0, 15 * 7), 0.3f);
        UIText line9 = new UIText("", new Point(0, 15 * 8), 0.3f);
        UIText line10 = new UIText("", new Point(0, 15 * 9), 0.3f);
        UIText line11 = new UIText("", new Point(0, 15 * 10), 0.3f);

        public Vehicle vehicle;
        private Camera mainCamera;
        public Camera MainCamera { get { return mainCamera; } set { mainCamera = value; } }
        Vector3 min = new Vector3();
        Vector3 max = new Vector3();
        Vector3 vehicleDimensions;
        public Line3D line;
        public float radius = 4f;
        public float angleFactor = 0.0174532924f;
        public float angle = 0f;
        public float offsetX = 0;
        public float offsetY = 0;

        public CarPictureTaker()
        {
            this.Tick += onTick;
            this.KeyUp += onKeyUp;
            this.KeyDown += onKeyDown;
            line1.Color = Color.Black;
        }

        private void onTick(object sender, EventArgs e)
        {
            //line1.Caption = "GamplayCamera Position: " + GameplayCamera.Position;
            //line2.Caption = "GamplayCamera Rotation: " + GameplayCamera.Rotation;
            //line3.Caption = "GamplayCamera FOV: " + GameplayCamera.FieldOfView;
            //line1.Draw(); line2.Draw(); line3.Draw();

            //if (vehicle != null) {
                //line4.Caption = "Vehicle Position: " + vehicle.Position;
                //line5.Caption = "Vehicle Dimension Min: " + min;
                //line6.Caption = "Vehicle Dimension Max: " + max;
                //line7.Caption = "Vehicle Dimensions: " + vehicleDimensions;
                //line4.Draw(); line5.Draw(); line6.Draw(); line7.Draw();
                //new Rectangle3D(vehicle.Position, min, max).Rotate(vehicle.Quaternion).DrawWireFrame(Color.Black, true);
            //}

            //if (MainCamera != null)
            //{
            //    line8.Caption = "MainCamera Position: " + MainCamera.Position;
            //    line9.Caption = "MainCamera Rotation: " + MainCamera.Rotation;
            //    line10.Caption = "MainCamera FOV: " + MainCamera.FieldOfView;
            //    line8.Draw(); line9.Draw(); line10.Draw();
            //}

            if (vehicle != null)
            {

                if (line != null)
                {
                    line.Draw(Color.Blue);
                }

                new Line3D(
                    vehicle.Position,
                    vehicle.Position + vehicle.ForwardVector * 3.0f)
                    .Draw(Color.Red);

                new Line3D(
                    vehicle.Position,
                    vehicle.Position + vehicle.RightVector * 3.0f)
                    .Draw(Color.Blue);

            }

        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void rotateVector()
        {
            if (vehicle == null)
            {
                UI.ShowSubtitle("Vehicle not found. Press N to create a vehicle.");
                return;
            }

            angle += angleFactor; //* 0.0174532924f;
            offsetX = radius * (float)Math.Cos(angle);
            offsetY = radius * (float)Math.Sin(angle);

            line = new Line3D(
                new Vector3(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z),
                new Vector3(vehicle.Position.X + offsetX, vehicle.Position.Y + offsetY, vehicle.Position.Z));

            

            rotateCamera();
            // UI.ShowSubtitle("OffsetX(" + offsetX + ") = Radius(" + radius + ") * cos(" + Math.Cos(angle) + "). Angle: " + angle);
            UI.ShowSubtitle("Angle: " + RadianToDegree(angle));
        }

        private double RadianToDegree(float angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {

            if(e.KeyCode == Keys.O)
            {
                // rotateCamera();
                rotateVector();
                //vehicle.Heading = vehicle.Heading + 90;
            }

            if (e.KeyCode == Keys.NumPad1)
            {
                createCamera();
            }

            if(e.KeyCode == Keys.B)
            {
                // Game.Player.Character.Position = new Vector3(1690.27f, 3246.72f, 41);
                // float distance = World.GetDistance(vehicle.Position, Game.Player.Character.Position);
                if (MainCamera != null) {
                    float distance = World.GetDistance(vehicle.Position, MainCamera.Position);
                    UI.ShowSubtitle("distance: " + distance);
                }
            }


            if (e.KeyCode == Keys.NumPad0)
            {
                destroyCamera();
            }

            if(e.KeyCode == Keys.L)
            {
                // Reset all
                line = null;
                vehicle.Detach();
                vehicle.Delete();
                vehicle = null;
            }

            if(e.KeyCode == Keys.N)
            {
                if (vehicle != null)
                {
                    UI.ShowSubtitle("Vehicle already exists. Press L to reset all.");
                    return;
                }
                destroyAllNearbyVehicles();
                vehicle = World.CreateVehicle(VehicleHash.Blista, new Vector3(1684.27f, 3246.72f, 41));
                vehicle.Heading = 0;
                vehicle.Model.GetDimensions(out min, out max);
                vehicleDimensions = vehicle.Model.GetDimensions();
                // radius = Math.Abs((max.X - min.X)) / 2;
            }
        }

        private void destroyAllNearbyVehicles()
        {
            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 100);
            if (nearbyVehicles.Length > 0)
            {
                foreach(Vehicle vehicle in nearbyVehicles) {
                    vehicle.Delete();
                }
            }
        }

        public void createCamera()
        {
            if (MainCamera == null)
            {
                this.MainCamera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
                UI.ShowSubtitle("New Camera Created.");
            }

            if (vehicle != null)
            {
                // MainCamera.Position = vehicle.Position;
                //MainCamera.Position = new Vector3(this.vehicle.Position.X + radius, this.vehicle.Position.Y, this.vehicle.Position.Z);
                MainCamera.Position = vehicle.Position + vehicle.ForwardVector * 3.0f;
                MainCamera.PointAt(vehicle);
                //  MainCamera.StopPointing();
                World.RenderingCamera = MainCamera;
            }
        }

        public void destroyCamera()
        {
            if (MainCamera != null)
            {
                MainCamera.StopPointing();
                MainCamera.IsActive = false;
            }

            World.RenderingCamera = null;
            World.DestroyAllCameras();
            MainCamera = null;
            UI.ShowSubtitle("Camera destroyed");
        }

        private void rotateCamera()
        {
            if (MainCamera == null || vehicle == null)
            {
                // UI.ShowSubtitle("No posible to rotate. Camera doesn't exists or vehicle doesn't exists.");
                return;
            }

            MainCamera.StopPointing();
            // MainCamera.Position = new Vector3(this.MainCamera.Position.X + offsetX, this.MainCamera.Position.Y + offsetY, this.MainCamera.Position.Z);
            MainCamera.Position = new Vector3(vehicle.Position.X + offsetX, vehicle.Position.Y + offsetY, vehicle.Position.Z + 1);
            // getAngleBetwee2dVectors();
            MainCamera.PointAt(vehicle);
        }

        float getAngleBetwee2dVectors(float x1, float y1, float x2, float y2)
        {
            return Function.Call<float>(GTA.Native.Hash.GET_ANGLE_BETWEEN_2D_VECTORS, x1, y1, x2, y2);
        }

        //Vector3 Rot = GameplayCamera.Rotation;

        ////normalize to directional vector
        //Vector3 direction = RotationToDirection(Rot);
        ////directional force vector
        //direction.X = force * direction.X;
        //direction.Y = force * direction.Y;
        //direction.Z = force * direction.Z;

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



}

/*
if (e.KeyCode == Keys.NumPad0)
{
    if(vehicle == null)
    {
        return;
    }
    vehicle.Rotation = new Vector3(vehicle.Rotation.X, vehicle.Rotation.Y, vehicle.Rotation.Z + 2);
}
*/



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


// Calling native C++ Hash functions
// Function.Call(Hash.SET_VEHICLE_RADIO_ENABLED, Game.Player.Character.CurrentVehicle, false)

// Return value from a hash function
//var test = Function.Call<int>(GTA.Native.Hash.CREATE_SYNCHRONIZED_SCENE, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z - 1, 0, 0, 180, 2);


// If a hash does not exist under the Hash enum
// Function.Call((Hash)0x1268615ACE24D504, true);

// Camera camera = World.CreateCamera(new Vector3(1, 2, 3), new Vector3(1, 2, 3), 3.3f);
// UI.ShowSubtitle("Camera: " + Game.);
// World.RenderingCamera.Rotation.X


//var car = World.CreateVehicle(VehicleHash.Blista3, Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 5, 0)));
//var npc = World.CreatePed(PedHash.AviSchwartzman, Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 5, 0)));
//npc.Weapons.Give(WeaponHash.Crowbar, 1, true, true);
//npc.Task.FightAgainst(Game.Player.Character);


//void Print(string text, int time = 2500)
//{
//    GTA.Native.Function.Call(Hash._0xB87A37EEB7FAA67D, "STRING");
//    GTA.Native.Function.Call(Hash._ADD_TEXT_COMPONENT_STRING, text);
//    GTA.Native.Function.Call(Hash._0x9D77056A530643F6, time, 1);
//}

//void hellow()
//{
//    Function.Call(Hash.SET_TEXT_FONT, 0);
//    Function.Call(Hash.SET_TEXT_SCALE, 0.0, 0.5f);
//    Function.Call(Hash.SET_TEXT_COLOUR, 255/*r*/, 255/*g*/, 255/*b*/, 255 /*a*/);
//    Function.Call(Hash.SET_TEXT_CENTRE, false);
//    Function.Call(Hash.SET_TEXT_OUTLINE);
//    Function.Call(Hash._SET_TEXT_ENTRY, String.Format("%s", "STRING"));
//    Function.Call(Hash.ADD_TEXT_COMPONENT_SUBSTRING_TIME, String.Format("%s", "Test"));
//    Function.Call(Hash._DRAW_TEXT, 3, 4);
//}

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
