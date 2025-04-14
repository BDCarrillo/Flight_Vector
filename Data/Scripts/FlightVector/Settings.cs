using ProtoBuf;
using Sandbox.ModAPI;
using System.IO;
using VRageMath;

namespace FlightVector
{
    [ProtoContract]
    public class Settings
    {
        public static Settings Instance;
        public static readonly Settings Default = new Settings()
        {

            colorFwd = new Vector4(0, 0, 1, 0.5f),
            colorRev = new Vector4(1, 0, 0, 0.5f),
            colorGrav = new Vector4(0, 1, 0, 0.5f),
            colorGravInverse = new Vector4(0.5f, 0, 1, 0.5f),
            enableLines = false,
            enableSymbols = true,
            lineLength = 80,
            lineThickness = 1,
            jetpack = true,
            stopInfo = false,
            symbolWidth = 0.06f,
            minVelocity = 0.1f,
            stopTimeDrawCoords = new Vector2D(-0.71, -0.72),

    };

        [ProtoMember(1)]
        public Vector4 colorFwd { get; set; }
        [ProtoMember(2)]
        public Vector4 colorRev { get; set; }
        [ProtoMember(3)]
        public Vector4 colorGrav { get; set; }
        [ProtoMember(4)]
        public bool enableLines { get; set; } = false;
        [ProtoMember(5)]
        public bool enableSymbols { get; set; } = true;
        [ProtoMember(6)]
        public float symbolThickness { get; set; } //Deprecated
        [ProtoMember(7)]
        public float symbolHeight { get; set; } //Deprecated
        [ProtoMember(8)]
        public float symbolDrawDistance { get; set; } //Deprecated
        [ProtoMember(9)]
        public float lineLength { get; set; }
        [ProtoMember(10)]
        public float lineThickness { get; set; }
        [ProtoMember(11)]
        public bool bold { get; set; } //Deprecated
        [ProtoMember(12)]
        public bool jetpack { get; set; }
        [ProtoMember(13)]
        public bool stopInfo { get; set; }
        [ProtoMember(14)]
        public float symbolWidth { get; set; } = 0.06f;
        [ProtoMember(15)]
        public float minVelocity { get; set; } = 0.1f;
        [ProtoMember(16)]
        public Vector2D stopTimeDrawCoords { get; set; } = new Vector2D(-0.71, -0.72);
        [ProtoMember(17)]
        public Vector4 colorGravInverse { get; set; } = new Vector4(1, 1, 0, 0.5f);

    }
    public partial class Session
    {
        private void InitConfig()
        {
            Settings s = Settings.Default;
            var Filename = "FlightVectorConfig.cfg";
            try
            {
                if (MyAPIGateway.Utilities.FileExistsInLocalStorage(Filename, typeof(Settings)))
                {
                    TextReader reader = MyAPIGateway.Utilities.ReadFileInLocalStorage(Filename, typeof(Settings));
                    string text = reader.ReadToEnd();
                    reader.Close();

                    s = MyAPIGateway.Utilities.SerializeFromXML<Settings>(text);
                    Validate(ref s);
                    Save(s);
                }
                else
                {
                    s = Settings.Default;
                    Save(s);
                }
            }
            catch
            {
                Settings.Instance = Settings.Default;
                s = Settings.Default;
                Save(s);
                MyAPIGateway.Utilities.ShowNotification("Flight Vector: Error with config file, overwriting with default.");
            }
        }

        public static void Validate(ref Settings s)
        {
            s.colorFwd = new Vector4(MathHelper.Clamp(s.colorFwd.X, 0, 255), MathHelper.Clamp(s.colorFwd.Y, 0, 255), MathHelper.Clamp(s.colorFwd.Z, 0, 255), MathHelper.Clamp(s.colorFwd.W, 0, 255));
            s.colorRev = new Vector4(MathHelper.Clamp(s.colorRev.X, 0, 255), MathHelper.Clamp(s.colorRev.Y, 0, 255), MathHelper.Clamp(s.colorRev.Z, 0, 255), MathHelper.Clamp(s.colorRev.W, 0, 255));
            s.colorGrav = new Vector4(MathHelper.Clamp(s.colorGrav.X, 0, 255), MathHelper.Clamp(s.colorGrav.Y, 0, 255), MathHelper.Clamp(s.colorGrav.Z, 0, 255), MathHelper.Clamp(s.colorGrav.W, 0, 255));
            s.colorGravInverse = new Vector4(MathHelper.Clamp(s.colorGravInverse.X, 0, 255), MathHelper.Clamp(s.colorGravInverse.Y, 0, 255), MathHelper.Clamp(s.colorGravInverse.Z, 0, 255), MathHelper.Clamp(s.colorGravInverse.W, 0, 255));
        }
        public static void Save(Settings settings)
        {
            var Filename = "FlightVectorConfig.cfg";
            try
            {
                TextWriter writer;
                writer = MyAPIGateway.Utilities.WriteFileInLocalStorage(Filename, typeof(Settings));
                writer.Write(MyAPIGateway.Utilities.SerializeToXML(settings));
                writer.Close();
                Settings.Instance = settings;
            }
            catch
            { }
        }
    }
}

