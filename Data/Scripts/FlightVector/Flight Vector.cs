using Sandbox.ModAPI;
using VRage.Game.Components;
using VRageMath;
using VRage.Game;
using VRage.Utils;
using System;
using VRage.Game.ModAPI;
using Draygo.API;
using System.Text;

namespace FlightVector
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public partial class Session : MySessionComponentBase
    {
        HudAPIv2 hudAPI;
        internal bool client;
        internal int tick;
        internal MyStringId particle = MyStringId.GetOrCompute("SciFiEngineThrustMiddle"); //Square  GizmoDrawLine  particle_laser ReflectorConeNarrow
        internal MyStringId progradeSymbol = MyStringId.GetOrCompute("Prograde");
        internal MyStringId retrogradeSymbol = MyStringId.GetOrCompute("Retrograde");
        internal MyStringId gravSymbol = MyStringId.GetOrCompute("Gravity");
        internal IMyCubeGrid controlledGrid;
        internal Vector3 gravDir;
        internal int decelTick = 0;
        internal int viewDist = 10000; //This needs to be fixed for the screencoords.z tomfoolery
        internal float symbolHeight = 0f; //Leave this as zero, dynamically altered later
        internal float aspectRatio = 0f; //Leave this as zero, monitor aspect ratio is figured in later
        internal bool clientActionRegistered = false;
        internal HudAPIv2.HUDMessage stopInfo = null;
        internal string modName = "[Flight Vector]";

        public override void BeforeStart()
        {
            client = !MyAPIGateway.Utilities.IsDedicated;
            if (client)
            {
                InitConfig();
                hudAPI = new HudAPIv2(InitMenu);
                MyAPIGateway.Utilities.MessageEnteredSender += OnMessageEnteredSender;
                viewDist = (int)Math.Min(10000, Math.Min(Session.SessionSettings.SyncDistance, Session.SessionSettings.ViewDistance) * 0.95f);
            }
        }

        private void SeamlessServerLoaded()
        {
            clientActionRegistered = false;
        }

        private void SeamlessServerUnloaded()
        {
            if (clientActionRegistered)
            {
                Session.Player.Controller.ControlledEntityChanged -= GridChange;
                clientActionRegistered = false;
                MyLog.Default.WriteLine(modName + " De-registered client GridChange action");
            }
        }

        private void OnMessageEnteredSender(ulong sender, string messageText, ref bool sendToOthers)
        {
            var msg = messageText.ToLower();
            var s = Settings.Instance;

            if (msg.Contains("/vector"))
            {
                switch (msg)
                {
                    case "/vector":
                        MyAPIGateway.Utilities.ShowMessage("Flight Vector", "Options: \n/vector symbols \n/vector lines \n/vector jetpack \n/vector stopinfo \n/vector reset");
                        sendToOthers = false;
                        break;

                    case "/vector symbols":
                        s.enableSymbols = !s.enableSymbols;
                        MyAPIGateway.Utilities.ShowNotification("Flight Vector symbols " + (s.enableSymbols == true ? "on" : "off"));
                        Save(Settings.Instance);
                        sendToOthers = false;
                        break;

                    case "/vector lines":
                        s.enableLines = !s.enableLines;
                        MyAPIGateway.Utilities.ShowNotification("Flight Vector lines " + (s.enableLines == true ? "on" : "off"));
                        Save(Settings.Instance);
                        sendToOthers = false;
                        break;

                    case "/vector jetpack":
                        s.jetpack = !s.jetpack;
                        MyAPIGateway.Utilities.ShowNotification("Flight Vector show for jetpack " + (s.jetpack == true ? "on" : "off"));
                        Save(Settings.Instance);
                        sendToOthers = false;
                        break;

                    case "/vector stopinfo":
                        s.stopInfo = !s.stopInfo;
                        MyAPIGateway.Utilities.ShowNotification("Flight Vector show stop time/distance " + (s.stopInfo == true ? "on" : "off"));
                        decelTick = 0;
                        Save(Settings.Instance);
                        sendToOthers = false;
                        break;

                    case "/vector reset":
                        Settings.Instance = Settings.Default;
                        MyAPIGateway.Utilities.ShowNotification("Flight Vector settings changed to default");
                        Save(Settings.Instance);
                        symbolHeight = Settings.Instance.symbolWidth * aspectRatio;
                        LabelUpdate();
                        sendToOthers = false;
                        break;

                    default:
                        break;
                }
            }
            return;
        }

        public override void Draw()
        {
            if (client && (Settings.Instance.enableLines || Settings.Instance.enableSymbols))
            {
                //Update grav vector
                if (tick % 59 == 0)
                {
                    if (controlledGrid != null)
                        gravDir = controlledGrid.Physics.Enabled ? Vector3.Normalize(controlledGrid.Physics.Gravity) : Vector3.Zero;
                    else if (Settings.Instance.jetpack && Session?.Player?.Character != null)
                        gravDir = Vector3.Normalize(Session.Player.Character.Physics.Gravity);
                    else
                        gravDir = Vector3.Zero;
                }
                DrawMarkers();

                //Register client action of changing entity
                if (client && !clientActionRegistered && Session?.Player?.Controller != null)
                {
                    clientActionRegistered = true;
                    Session.Player.Controller.ControlledEntityChanged += GridChange;
                    MyLog.Default.WriteLine(modName + " Registered client GridChange action");
                    GridChange(null, Session.Player.Controller.ControlledEntity);
                }
                if (hudAPI.Heartbeat && stopInfo == null)
                {
                    stopInfo = new HudAPIv2.HUDMessage(null, Settings.Instance.stopTimeDrawCoords, null, -1, 1, true, true);
                    stopInfo.Visible = false;
                }
                //Calc aspect ratio and symbol height
                if (symbolHeight == 0)
                {
                    aspectRatio = Session.Camera.ViewportSize.X / Session.Camera.ViewportSize.Y;
                    symbolHeight = Settings.Instance.symbolWidth * aspectRatio;
                }
                tick++;
            }
        }
        private void DrawMarkers()
        {
            var s = Settings.Instance;
            var validGrid = controlledGrid != null && !controlledGrid.MarkedForClose && controlledGrid.Physics.Enabled;
            var jetpackMode = !validGrid && s.jetpack && Session?.Player?.Character != null && (Session?.ControlledObject != null && Session.ControlledObject.EnabledThrusts);
            if ((validGrid || jetpackMode) && hudAPI.Heartbeat && MyAPIGateway.Session.Config.HudState != 0)
            {
                var gridDir = jetpackMode ? Session.Player.Character.Physics.LinearVelocity : controlledGrid.Physics.LinearVelocity;
                var linearVel = gridDir.Normalize();


                //Decel info                
                if (s.stopInfo && tick % 15 == 0 && stopInfo != null)
                {
                    if (validGrid && MyAPIGateway.Multiplayer.MultiplayerActive) controlledGrid.Physics?.UpdateAccelerations(); //This is super dumb for MP
                    var accDir = jetpackMode ? Session.Player.Character.Physics.LinearAcceleration : controlledGrid.Physics.LinearAcceleration;
                    var accLen = accDir.Normalize();
                    if (linearVel >= 5 && accLen >= 2 && Vector3D.Dot(gridDir, accDir) < -0.75)
                    {
                        var timeToStop = linearVel / accLen;
                        var timeMin = (int)timeToStop / 60;
                        var timeToStopStr = timeMin.ToString("00") + ":" + (timeToStop - timeMin * 60).ToString("00") + "  ";
                        var distToStop = timeToStop > 0 ? timeToStop * linearVel * 0.5 : 0;
                        var dstToStopStr = distToStop > 1000 ? (distToStop / 1000).ToString("0.0") + "km" : (int)distToStop + "m";
                        var info = new StringBuilder(timeToStopStr + dstToStopStr);
                        stopInfo.Message = info;
                        stopInfo.Visible = true;
                    }
                    else if (stopInfo.Visible)
                        stopInfo.Visible = false;
                }

                //Draw vars
                var gridCtr = jetpackMode ? Session.Player.Character.PositionComp.WorldVolume.Center : controlledGrid.PositionComp.WorldVolume.Center;
                var gridSizeHalf = jetpackMode ? 5 : controlledGrid.PositionComp.LocalVolume.Radius * 0.5f;
                var viewProjectionMat = Session.Camera.ViewMatrix * Session.Camera.ProjectionMatrix;

                //Gravity draws
                if (gravDir != Vector3.Zero)
                {
                    if (s.enableLines)
                    {
                        var colorGrav = s.colorGrav;
                        MySimpleObjectDraw.DrawLine(gridCtr + gravDir * gridSizeHalf, gridCtr + gravDir * s.lineLength, particle, ref colorGrav, s.lineThickness);
                    }
                    if (s.enableSymbols)
                    {
                        var screenCoords = Vector3D.Transform(gridCtr + gravDir * viewDist, viewProjectionMat);
                        var offScreen = screenCoords.X > 1.1 || screenCoords.X < -1.1 || screenCoords.Y > 1.1 || screenCoords.Y < -1.1 || screenCoords.Z > 1;
                        var symbolObj = new HudAPIv2.BillBoardHUDMessage(gravSymbol, new Vector2D(screenCoords.X, screenCoords.Y), offScreen ? s.colorGravInverse : s.colorGrav, Width: s.symbolWidth, Height: symbolHeight, TimeToLive: 2, HideHud: true, Shadowing: true, Rotation: offScreen ? -1.5708f : 1.5708f);
                    }
                }
                    
                //Skip prograde/retrograde lines and symbols if velo is below threshold
                if (linearVel <= s.minVelocity)
                    return;

                //Prograde/retrograde draws
                if (s.enableLines)
                {
                    var colorFwd = s.colorFwd;
                    var colorRev = s.colorRev;
                    var offsetStartFwd = gridCtr + gridDir * gridSizeHalf;
                    var offsetStartRev = gridCtr - gridDir * gridSizeHalf;
                    MySimpleObjectDraw.DrawLine(offsetStartFwd, gridCtr + gridDir * s.lineLength, particle, ref colorFwd, s.lineThickness);
                    MySimpleObjectDraw.DrawLine(offsetStartRev, gridCtr - gridDir * s.lineLength, particle, ref colorRev, s.lineThickness);
                }
                if (s.enableSymbols)
                {
                    var screenCoords = Vector3D.Transform(gridCtr + gridDir * viewDist, viewProjectionMat);
                    var offScreen = screenCoords.X > 1.1 || screenCoords.X < -1.1 || screenCoords.Y > 1.1 || screenCoords.Y < -1.1 || screenCoords.Z > 1;
                    var symbolObj = new HudAPIv2.BillBoardHUDMessage(!offScreen ? progradeSymbol : retrogradeSymbol, new Vector2D(screenCoords.X, screenCoords.Y), !offScreen ? s.colorFwd : s.colorRev, Width: s.symbolWidth, Height: symbolHeight, TimeToLive: 2, HideHud: true, Shadowing: true, Rotation: 1.5708f);
                }
            }
        }
        private void GridChange(VRage.Game.ModAPI.Interfaces.IMyControllableEntity previousEnt, VRage.Game.ModAPI.Interfaces.IMyControllableEntity newEnt)
        {
            if (newEnt is IMyCharacter || newEnt == null)
            {
                controlledGrid = null;
            }
            else if (newEnt is IMyCubeBlock)
            {
                var block = newEnt as IMyCubeBlock;
                controlledGrid = block.CubeGrid;
            }
        }
        protected override void UnloadData()
        {
            if (client)
            {
                hudAPI?.Unload();
                if (clientActionRegistered)
                {
                    Session.Player.Controller.ControlledEntityChanged -= GridChange;
                    clientActionRegistered = false;
                    MyLog.Default.WriteLine(modName + " De-registered client GridChange action");
                }
                Save(Settings.Instance);
                controlledGrid = null;
                MyAPIGateway.Utilities.MessageEnteredSender -= OnMessageEnteredSender;
            }
        }
    }
}

