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
using Configuration;
using static CarPictureSamples.Utils;

namespace CarPictureSamples
{

    public class CarPictureTaker : Script
    {
        Dictionary<String, String> cache = new Dictionary<string, string>();

        public Vehicle vehicle;
        private GTA.Camera mainCamera;
        public GTA.Camera MainCamera { get { return mainCamera; } set { mainCamera = value; } }
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
        }

        private void onKeyUp(object sender, KeyEventArgs e)
        {
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.O)
            {
                //foreach(int angle in angles)
                //{ 
                globalAngle += 45;
                if (globalAngle >= 360)
                {
                    globalAngle = 0;
                }

                //RotateCamera(vehicle.Heading + globalAngle);
                //Script.Wait(500);
                //}
            }

            if (e.KeyCode == Keys.NumPad1)
            {
                CreateCamera();
            }

            if (e.KeyCode == Keys.OemMinus)
            {
                StartPictureTaker();
            }

            if(e.KeyCode == Keys.B)
            {
                SavePosition();
            }

            if (e.KeyCode == Keys.NumPad0)
            {
                DestroyCamera();
            }

            if (e.KeyCode == Keys.L)
            {
                DestroyAllNearbyVehicles();
            }

            if (e.KeyCode == Keys.N)
            {
                CreateCar();
            }

            if(e.KeyCode == Keys.OemPeriod)
            {
                UI.ShowSubtitle("A");
            }
        }

        private void SavePosition()
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

        private void LoadCache()
        {
            String[] lines = System.IO.File.ReadAllLines(@"C:\Users\ale\Desktop\dataset\index.csv");
            cache.Clear();
            foreach (String line in lines)
            {
                String hash = line.Split(';')[20];
                if (cache.ContainsKey(hash))
                {
                    continue;
                }

                cache.Add(hash, hash);
            }
        }

        private void StartPictureTaker()
        {
            DateTime startProcess = DateTime.Now;
            Log($"Start process: {startProcess}");
            LoadCache();

            DestroyAllNearbyVehicles();

            if(MainCamera != null)
            {
                DestroyCamera();
            }

            String json = System.IO.File.ReadAllText(@"C:\Users\ale\Desktop\dataset\scenes.json");
            var scene = Scene.FromJson(json);

            var cartesianProduct = from time in scene.Time
                                   from weather in scene.Weather
                                   from position in scene.Position
                                   from vehicle in scene.Vehicles
                                   from color in scene.Colors
                                   from distance in scene.Camera.Distance
                                   from pitch in scene.Camera.Pitch
                                   from angle in scene.Camera.Rotation
                                   select new { time, weather, position, vehicle, color, distance, pitch, angle };

            DisplayRadar(false);
            DisplayPlayer(false);

            TimeSpan currentTime = World.CurrentDayTime;
            Weather currentWeather = World.Weather;
            VehicleHash currentVehicleHash = VehicleHash.Adder;
            VehicleColor currentColor = VehicleColor.Blue;
            Vector3 currentVehiclePosition = Vector3.Zero;
            GTA.Vehicle currentVehicle = vehicle;
            float currentAngle = 0;
            float currentDistance = 0;
            float currentPitch = 0;

            foreach (var Index in cartesianProduct)
            {
                Vector3 vehiclePosition = new Vector3(Index.position.Location.X, Index.position.Location.Y, Index.position.Location.Z);
                Vector3 vehicleRotation = new Vector3(Index.position.Rotation.X, Index.position.Rotation.Y, Index.position.Rotation.Z);

                String hash = CreatePictureHash(Index.time.TimeOfDay, Index.weather, Index.vehicle, Index.color,
                    vehiclePosition, Index.distance, Index.pitch, Index.angle);

                if(cache.ContainsKey(hash))
                {
                    continue;
                }

                if(!Index.position.Enabled)
                {
                    continue;
                }

                //DateTime startPictureTime = DateTime.Now;
                // ------------------------
                //      CREATE SCENE
                // ------------------------
                if (currentVehiclePosition != vehiclePosition)
                {
                    DestroyCamera();
                    DestroyAllNearbyVehicles(); // Clean the scene.
                    CreateCamera();
                    Teleport(vehiclePosition, new Vector3(4, 0, 0));
                    currentVehiclePosition = vehiclePosition;
                    Script.Wait(1000); // Wait to display the vehicle
                }

                if (currentTime != Index.time.TimeOfDay)
                {
                    TimeSpan time = Index.time.TimeOfDay;
                    currentTime = time;
                    World.CurrentDayTime = currentTime;
                }

                if (currentWeather != Index.weather)
                {
                    Weather weather = Index.weather;
                    currentWeather = weather;
                    World.Weather = currentWeather;
                }

                // ------------------------
                //      CREATE OBJECT
                // ------------------------
                if (currentVehicleHash != Index.vehicle)
                {
                    DestroyAllNearbyVehicles(); // Clean the scene.
                    Script.Wait(10); // Wait to wipe the car
                    currentVehicle = CreateCar(Index.vehicle, Index.color, vehiclePosition, vehicleRotation);
                    vehicle = currentVehicle;
                    currentVehicleHash = Index.vehicle;
                }

                if (currentVehicle.Model.Hash == Index.vehicle.GetHashCode())
                {
                    currentVehicle.PrimaryColor = Index.color;
                    currentColor = Index.color;
                }

                // ------------------------
                //      SET CAMERA
                // ------------------------
                if (currentDistance != Index.distance)
                {
                    currentDistance = Index.distance;
                }

                if (currentPitch != Index.pitch)
                {
                    currentPitch = Index.pitch;
                }

                float refAngle = currentVehicle.Heading + Index.angle;
                if (refAngle <= 0)
                {
                    refAngle = 0;
                }
                currentAngle = Index.angle;
                CameraPosition(currentDistance, currentPitch, refAngle);
                Script.Wait(250);

                String fileName = $"{hash}.png";

                String data =
                    $"{currentTime};{currentWeather};{(uint)vehicle.Model.Hash};{currentColor};" +
                    $"{vehiclePosition.X};{vehiclePosition.Y};{vehiclePosition.Z};" +
                    $"{vehicleRotation.X};{vehicleRotation.Y};{vehicleRotation.Z};" +
                    $"{vehicle.ClassType};" +
                    $"{MainCamera.Position.X};{MainCamera.Position.Y};{MainCamera.Position.Z};" +
                    $"{MainCamera.Rotation.X};{MainCamera.Rotation.Y};{MainCamera.Rotation.Z};" +
                    //$"{vehiclePosition.DistanceTo(MainCamera.Position)};" +
                    $"{currentDistance};{currentPitch};{currentAngle};{hash}" +
                    $"\n";

                System.IO.File.AppendAllText(@"C:\Users\ale\Desktop\dataset\index.csv", data);

                SavePicture(fileName);
                //TimeSpan pictureTime = DateTime.Now.Subtract(startPictureTime);
                //Log($"Picture Took: {pictureTime}");
            }

            DisplayRadar(true);
            DisplayPlayer(true);
            DestroyCamera();
            DestroyAllNearbyVehicles();
            TimeSpan endTime = DateTime.Now.Subtract(startProcess);
            Log($"End process. Total time: {endTime} ");
            UI.ShowSubtitle("The process has finish.");
        }

        private static String CreatePictureHash(TimeSpan time, Weather weather, VehicleHash vehicleHash, VehicleColor vehiclePrimaryColor, 
            Vector3 position, float distance, float pitch, float angle)
        {
            return Utils.Sha1Hash(
                $"{time};{weather};{vehicleHash};{vehiclePrimaryColor};" +
                $"{position.X};{position.Y};{position.Z};{distance};{pitch};{angle}");
        }

        static int IMAGE_WIDTH  = 299; // 800 - 400 - 299
        static int IMAGE_HEIGHT = 165; // 450 - 225 - 165

        private void SavePicture(String fileName)
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                Bitmap bmpLowestResolution = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT);
                Graphics gLowestResolution = Graphics.FromImage(bmpLowestResolution);
                gLowestResolution.DrawImage(bitmap, 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT);
                bmpLowestResolution.Save(@"C:\Users\ale\Desktop\dataset\pictures\" + fileName, ImageFormat.Png);
            }
        }

        private Vehicle CreateCar()
        {
            return CreateCar(VehicleHash.Blista, VehicleColor.MetaillicVDarkBlue, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 3.0f, Game.Player.Character.Rotation);
        }

        private Vehicle CreateCar(VehicleHash hash, VehicleColor primaryColor, Vector3 position, Vector3 rotation)
        {
            Vehicle vehicle = World.CreateVehicle(hash, position, 0);
            if (rotation != null)
            {
                vehicle.Rotation = rotation;
            }
            vehicle.NumberPlate = "iunigo";
            vehicle.PrimaryColor = primaryColor;
            vehicle.Model.GetDimensions(out Vector3 min, out Vector3 max);
            radius = max.DistanceTo(min) / 2;

            return vehicle;
        }

        private void DestroyAllNearbyVehicles()
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

        public void CreateCamera()
        {
            CreateCamera(GameplayCamera.Position, GameplayCamera.Rotation, vehicle);
        }

        public void CreateCamera(Vector3 position, Vector3 rotation, GTA.Vehicle pointAtEntity)
        {
            // Destroy the camera if exists.
            DestroyCamera();
            MainCamera = World.CreateCamera(position, rotation, GameplayCamera.FieldOfView);
            if(pointAtEntity != null)
            {
                MainCamera.PointAt(pointAtEntity);
            }
            World.RenderingCamera = MainCamera;
        }

        private void CameraPosition(float distance, float pitch, float angle)
        {
            double angleInRadians = DegreeToRadians(angle);
            float offsetX = (radius + distance) * (float) Math.Cos(angleInRadians);
            float offsetY = (radius + distance) * (float) Math.Sin(angleInRadians);
            Vector3 offset = new Vector3(offsetX, offsetY, pitch);

            MainCamera.StopPointing();
            MainCamera.Position = new Vector3(vehicle.Position.X + offset.X, vehicle.Position.Y + offset.Y, vehicle.Position.Z + offset.Z);
            MainCamera.PointAt(vehicle);
        }

        private void DestroyCamera()
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
    }
}

