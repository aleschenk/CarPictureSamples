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
using NativeUI;
using System.Globalization;
using System.Drawing.Imaging;
using System.Security.Cryptography;

namespace CarPictureSamples
{

    public class CarPictureTaker : Script
    {

        VehicleHash[] vehicles = {
            //VehicleHash.Asea,
            //VehicleHash.Asterope,
            //VehicleHash.Baller,
            //VehicleHash.Bison,
            VehicleHash.Blista,
            VehicleHash.BobcatXL,
            //VehicleHash.Cavalcade,
            //VehicleHash.FQ2,
            //VehicleHash.Futo,
            //VehicleHash.Granger,
            //VehicleHash.Gresley,
            //VehicleHash.Landstalker,
            //VehicleHash.Manana,
            //VehicleHash.Mesa,
            //VehicleHash.Oracle,
            //VehicleHash.Regina,
            //VehicleHash.Rocoto,
            //VehicleHash.Sadler,
            //VehicleHash.Sandking,
            //VehicleHash.Stanier,
            //VehicleHash.Taxi,
            //VehicleHash.Tornado,
            //VehicleHash.Burrito,
            //VehicleHash.Camper,
            //VehicleHash.Speedo,
            //VehicleHash.Surfer
        };

        VehicleColor[] colors =
        {
            VehicleColor.MetallicBlack,
            //VehicleColor.MetallicBlackSteel,
            //VehicleColor.MetallicSilver,
            //VehicleColor.MetallicRed,
            //VehicleColor.MetallicSunriseOrange,
            //VehicleColor.MetallicOliveGreen,
            //VehicleColor.MetallicMidnightBlue,
            //VehicleColor.MetallicFrostWhite
        };

        private int[] angles = { 0, 45, 90, 135, 180, 225, 270, 315 };

        public Vehicle vehicle;
        private Camera mainCamera;
        public Camera MainCamera { get { return mainCamera; } set { mainCamera = value; } }
        public Line3D line;
        public float radius = 4f;
        public float angleFactor = 0.0174532924f;
        public float globalAngle = 0f;
        public float offsetX = 0;
        public float offsetY = 0;
        private MenuPool _menuPool;
        private bool isProcessActive = false;

        public CarPictureTaker()
        {
            _menuPool = new MenuPool();
            var mainMenu = new UIMenu("Native UI", "~b~CarPictureTaker");
            _menuPool.Add(mainMenu);
            //AddMenuKetchup(mainMenu);
            //AddMenuFoods(mainMenu);
            //AddMenuCook(mainMenu);
            //AddMenuAnotherMenu(mainMenu);
            //_menuPool.RefreshIndex();

            this.KeyUp += onKeyUp;
            this.KeyDown += onKeyDown;
            this.Tick += onTick;
            //Tick += (o, e) => _menuPool.ProcessMenus();
            //KeyDown += (o, e) =>
            //{
            //    if (e.KeyCode == Keys.F5 && !_menuPool.IsAnyMenuOpen()) // Our menu on/off switch
            //        mainMenu.Visible = !mainMenu.Visible;
            //};
        }

        private void onTick(object sender, EventArgs e)
        {
            // showCameraPosition();
            drawCarLines();
        }

        private void drawCarLines()
        {
            if (vehicle == null)
            {
                return;
            }

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

        private void onKeyUp(object sender, KeyEventArgs e)
        {
        }

        private Vector3 rotateVector(float angle)
        {
            if(vehicle == null)
            {
                return Vector3.Zero;
            }

            double angleInRadians = DegreeToRadians(angle);
            offsetX = radius * (float) Math.Cos(angleInRadians);
            offsetY = radius * (float) Math.Sin(angleInRadians);

            line = new Line3D(
                new Vector3(vehicle.Position.X, vehicle.Position.Y, vehicle.Position.Z),
                new Vector3(vehicle.Position.X + offsetX, vehicle.Position.Y + offsetY, vehicle.Position.Z));

            // UI.ShowSubtitle("Car Heading: " + vehicle.Heading);
            // UI.ShowSubtitle("Angle: " + angle);

            return new Vector3(offsetX, offsetY, 1);
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.O)
            {
                foreach(int angle in angles)
                { 
                    //globalAngle++;
                    //if (globalAngle > 360)
                    //{
                    //    globalAngle = 0;
                    //}
                    rotateCamera(angle);
                    Script.Wait(500);
                }
            }

            if (e.KeyCode == Keys.NumPad1)
            {
                createCamera();
            }

            if (e.KeyCode == Keys.OemMinus)
            {
                startPictureTaker();
            }

            if(e.KeyCode == Keys.B)
            {
                savePosition();
            }

            if (e.KeyCode == Keys.NumPad0)
            {
                destroyCamera();
            }

            if (e.KeyCode == Keys.L)
            {
                destroyAllNearbyVehicles();
            }

            if (e.KeyCode == Keys.N)
            {
                createCar();
            }
        }

        private void savePosition()
        {
            if (vehicle == null)
            {
                UI.ShowSubtitle("No vehicle");
                return;
            }
            String data =
                $"{World.CurrentDayTime};{World.Weather};" +
                $"{vehicle.Position.X};{vehicle.Position.Y};{vehicle.Position.Z};" +
                $"{vehicle.Rotation.X};{vehicle.Rotation.Y};{vehicle.Rotation.Z}" +
                $"\n";

            System.IO.File.AppendAllText(@"C:\Users\ale\Desktop\dataset\locations.csv", data);
        }

        private void startPictureTaker()
        {
            log("Start process");
            if (vehicle != null)
            {
                destroyAllNearbyVehicles();
                return;
            }

            if(MainCamera != null)
            {
                destroyCamera();
            }

            String[] lines = System.IO.File.ReadAllLines(@"C:\Users\ale\Desktop\dataset\locations.csv");

            displayRadar(false);
            displayPlayer(false);
            foreach (String line in lines)
            {
                if(line.StartsWith("#"))
                    continue;

                String[] location = line.Split(';');
                foreach (VehicleHash vehicleHash in vehicles)
                {
                    foreach(VehicleColor vehicoleColor in colors)
                    {
                        log($"Create new Scene: Vehicle: {vehicleHash} Color: {vehicoleColor}");
                        createScene(location, vehicleHash, vehicoleColor);
                        createCamera();
                        takePictures();
                        destroyAllNearbyVehicles();
                        destroyCamera();
                    }
                }
            }
            displayRadar(true);
            displayPlayer(true);
            log("End process");
        }

        private void log(String data)
        {
            System.IO.File.AppendAllText(@"C:\Users\ale\Desktop\dataset\log.txt", $"{DateTime.Now.ToString()}: " + data + "\r\n");
        }

        private void savePicture(String fileName)
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                //Bitmap bmp_800_600 = new Bitmap(800, 600);
                //Graphics g_800_600 = Graphics.FromImage(bmp_800_600);
                //g_800_600.DrawImage(bitmap, 0, 0, 800, 600);
                //bmp_800_600.Save(@"C:\Users\ale\Desktop\dataset\pictures\" + fileName, ImageFormat.Png);
                bitmap.Save(@"C:\Users\ale\Desktop\dataset\pictures\" + fileName, ImageFormat.Png);
            }
        }

        private void displayRadar(bool enabled)
        {
            Function.Call(Hash.DISPLAY_RADAR, enabled);
        }

        private void displayPlayer(bool enabled)
        {
            Game.Player.Character.IsInvincible = !enabled;
            Game.Player.Character.IsVisible = enabled;
        }

        private void takePictures()
        {
            if (vehicle == null)
            {
                UI.ShowSubtitle("Can't take picture. The vehicle doesn't exists.");
                return;
            }

            if (MainCamera == null)
            {
                UI.ShowSubtitle("Can't take picture. The MainCamera doesn't exists.");
            }

            foreach(int angle in angles)
            {
                float refAngle = vehicle.Heading + angle;
                if (refAngle <= 0)
                {
                    refAngle = 0;
                }
                rotateCamera(refAngle);

                String str =
                    $"12:00:00;{World.Weather};{(uint)vehicle.Model.Hash};{vehicle.PrimaryColor};" + 
                    $"{vehicle.Position.X};{vehicle.Position.Y};{vehicle.Position.Z};{angle}";

                String fileName = Sha1Hash(str) + ".png";

                String data =
                    $"12:00:00;{World.Weather};{(uint)vehicle.Model.Hash};{vehicle.PrimaryColor};" +
                    $"{vehicle.Position.X};{vehicle.Position.Y};{vehicle.Position.Z};" +
                    $"{vehicle.Rotation.X};{vehicle.Rotation.Y};{vehicle.Rotation.Z};" +
                    $"{vehicle.ClassType};" +
                    $"{MainCamera.Position.X};{MainCamera.Position.Y};{MainCamera.Position.Z};" +
                    $"{MainCamera.Rotation.X};{MainCamera.Rotation.Y};{MainCamera.Rotation.Z};" +
                    $"{vehicle.Position.DistanceTo(MainCamera.Position)};" +
                    $"{angle};{fileName}" +
                    $"\n";

                System.IO.File.AppendAllText(@"C:\Users\ale\Desktop\dataset\index.csv", data);

                savePicture(fileName);

                Script.Wait(500);
            }

            rotateCamera(0);
        }

        private void createScene(String[] param, VehicleHash vehicleHash, VehicleColor vehiclePrimaryColor)
        {
            //String time = param[0];
            //String weather = param[1];
            //VehicleHash vehicleHash = (VehicleHash)Enum.Parse(typeof(VehicleHash), param[2]);
            //VehicleColor vehiclePrimaryColor = (VehicleColor) Enum.Parse(typeof(VehicleColor), param[3]);
            Vector3 vehiclePosition = new Vector3(float.Parse(param[2]), float.Parse(param[3]), float.Parse(param[4]));
            Vector3 vehicleRotation = new Vector3(float.Parse(param[5]), float.Parse(param[6]), float.Parse(param[7]));

            Game.Player.Character.Position = vehiclePosition + new Vector3(5,0,0);

            // Wait render scene
            Script.Wait(2500);
            destroyAllNearbyVehicles();
            setTime("12:00:00");
            setWeather("ExtraSunny");
            createCar(vehicleHash, vehiclePrimaryColor, vehiclePosition, vehicleRotation);
            // Wait to display the vehicle
            Script.Wait(500);
        }

        private void setTime(String time)
        {
            DateTime dt = DateTime.ParseExact(time, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            World.CurrentDayTime = dt.TimeOfDay;
        }

        private void setWeather(String weather)
        {
            World.Weather = (Weather)Enum.Parse(typeof(Weather), weather);
        }

        private void createCar()
        {
            createCar(VehicleHash.Blista, VehicleColor.MetaillicVDarkBlue, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3.0f, Game.Player.Character.Rotation);
        }

        private void createCar(VehicleHash hash, VehicleColor primaryColor, Vector3 position, Vector3 rotation)
        {
            if (vehicle != null)
            {
                vehicle.Delete();
                vehicle = null;
            }
            vehicle = World.CreateVehicle(hash, position, 0);
            if (rotation != null)
            {
                vehicle.Rotation = rotation;
            }
            vehicle.NumberPlate = "iunigo";
            vehicle.PrimaryColor = primaryColor;
            Vector3 min, max;
            vehicle.Model.GetDimensions(out min, out max);
            radius = (max.DistanceTo(min) / 2) + 1;
        }

        private void destroyAllNearbyVehicles()
        {
            if (vehicle != null)
            {
                vehicle.Detach();
                vehicle.Delete();
                vehicle = null;
            }

            Vehicle[] nearbyVehicles = World.GetNearbyVehicles(Game.Player.Character.Position, 10);
            if (nearbyVehicles.Length > 0)
            {
                foreach(Vehicle vehicle in nearbyVehicles) {
                    vehicle.Delete();
                }
            }
        }

        public void createCamera()
        {
            if(vehicle == null)
            {
                UI.ShowSubtitle("Unable to create the camera. First you need to create a Car. Press N to create a Car.");
                return ;
            }

            // Destroy the camera if exists.
            destroyCamera();

            MainCamera = World.CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, GameplayCamera.FieldOfView);
            rotateCamera(0);
            World.RenderingCamera = MainCamera;
        }

        public void destroyCamera()
        {
            if(MainCamera == null)
            {
                World.RenderingCamera = null;
                return;
            }
            else
            {
                MainCamera.StopPointing();
                MainCamera.IsActive = false;
            }

            World.RenderingCamera = null;
            World.DestroyAllCameras();
            MainCamera = null;
        }

        private void rotateCamera(float angle)
        {
            if (MainCamera == null || vehicle == null)
            {
                return;
            }

            Vector3 offset = rotateVector(angle);
            MainCamera.StopPointing();
            MainCamera.Position = new Vector3(vehicle.Position.X + offset.X, vehicle.Position.Y + offset.Y, vehicle.Position.Z + offset.Z);
            MainCamera.PointAt(vehicle);
        }

        private double RadianToDegree(float radians)
        {
            return radians * (180.0 / Math.PI);
        }

        private double DegreeToRadians(float degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        private float getAngleBetwee2dVectors(float x1, float y1, float x2, float y2)
        {
            return Function.Call<float>(GTA.Native.Hash.GET_ANGLE_BETWEEN_2D_VECTORS, x1, y1, x2, y2);
        }

        static string Sha1Hash(string input)
        {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

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
