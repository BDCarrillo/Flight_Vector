using Draygo.API;
using VRageMath;

namespace FlightVector
{
    public partial class Session
    {
        HudAPIv2.MenuRootCategory SettingsMenu;
        HudAPIv2.MenuSubCategory SymbolSize, LineSize, StopTimeOptions, MoveStopDisplay;

        HudAPIv2.MenuColorPickerInput ProgradeColor, RetrogradeColor, GravityColor;
        HudAPIv2.MenuItem Blank, DecreaseSymSize, IncreaseSymSize, JetPackMode, ShowLines, ShowSymbols, IncLineLen, DecLineLen, IncLineThick, DecLineThick, ShowStop, MoveLeft, MoveRight, MoveUp, MoveDown;
        HudAPIv2.MenuTextInput MinVel;


        private void InitMenu()//callback
        {
            SettingsMenu = new HudAPIv2.MenuRootCategory("Flight Vector", HudAPIv2.MenuRootCategory.MenuFlag.PlayerMenu, "Flight Vector Settings");
            ProgradeColor = new HudAPIv2.MenuColorPickerInput("Set prograde color >>", SettingsMenu, Settings.Instance.colorFwd, "Select color", ChangeProgradeColor);
            RetrogradeColor = new HudAPIv2.MenuColorPickerInput("Set retrograde color >>", SettingsMenu, Settings.Instance.colorRev, "Select color", ChangeRetrogradeColor);
            GravityColor = new HudAPIv2.MenuColorPickerInput("Set gravity color >>", SettingsMenu, Settings.Instance.colorGrav, "Select color", ChangeGravityColor);
            JetPackMode = new HudAPIv2.MenuItem("Enable for jetpack: " + Settings.Instance.jetpack, SettingsMenu, ChangeJetpack);
            MinVel = new HudAPIv2.MenuTextInput("Min velocity to draw lines/symbols " + Settings.Instance.minVelocity + "m/s", SettingsMenu, "Suppress showing lines/symbols if velocity is below this value", MinVelChange);
            Blank = new HudAPIv2.MenuItem("- - - - - - - - - - -", SettingsMenu, null);
            ShowLines = new HudAPIv2.MenuItem("Show lines: " + Settings.Instance.enableLines, SettingsMenu, ChangeLines);
            LineSize = new HudAPIv2.MenuSubCategory("Adjust Line Size >>", SettingsMenu, "Adjust Line Size");
                IncLineLen = new HudAPIv2.MenuItem("Increase line length", LineSize, IncLineLen_);
                DecLineLen = new HudAPIv2.MenuItem("Decrease line length", LineSize, DecLineLen_);
                IncLineThick = new HudAPIv2.MenuItem("Increase line thickness", LineSize, IncLineThick_);
                DecLineThick = new HudAPIv2.MenuItem("Decrease line thickness", LineSize, DecLineThick_);
            Blank = new HudAPIv2.MenuItem("- - - - - - - - - - -", SettingsMenu, null);
            ShowSymbols = new HudAPIv2.MenuItem("Show symbols: " + Settings.Instance.enableSymbols, SettingsMenu, ChangeSymbols);
            SymbolSize = new HudAPIv2.MenuSubCategory("Adjust Symbol Size >>", SettingsMenu, "Adjust Symbol Size");
                IncreaseSymSize = new HudAPIv2.MenuItem("Increase symbol size", SymbolSize, IncreaseSymbolSize);
                DecreaseSymSize = new HudAPIv2.MenuItem("Decrease symbol size", SymbolSize, DecreaseSymbolSize);
            Blank = new HudAPIv2.MenuItem("- - - - - - - - - - -", SettingsMenu, null);
            StopTimeOptions = new HudAPIv2.MenuSubCategory("Decel/Stop Time/Dist Options >>", SettingsMenu, "Adjust Stop Time Options");
                ShowStop = new HudAPIv2.MenuItem("Enable stop time/distance information: " + Settings.Instance.stopInfo, StopTimeOptions, ChangeStopTime);
                MoveStopDisplay = new HudAPIv2.MenuSubCategory("Move Stop Info Location >>", StopTimeOptions, "Stop Info Location");
                    MoveLeft = new HudAPIv2.MenuItem("Move Left", MoveStopDisplay, LeftMove);
                    MoveRight = new HudAPIv2.MenuItem("Move Right", MoveStopDisplay, RightMove);
                    MoveUp = new HudAPIv2.MenuItem("Move Up", MoveStopDisplay, UpMove);
                    MoveDown = new HudAPIv2.MenuItem("Move Down", MoveStopDisplay, DownMove);
            
        }

        private void ChangeProgradeColor(Color obj)
        {
            Settings.Instance.colorFwd = obj;
            ProgradeColor.InitialColor = obj;
        }
        private void ChangeRetrogradeColor(Color obj)
        {
            Settings.Instance.colorRev = obj;
            RetrogradeColor.InitialColor = obj;
        }
        private void ChangeGravityColor(Color obj)
        {
            Settings.Instance.colorGrav = obj;
            GravityColor.InitialColor = obj;
        }
        private void ChangeJetpack()
        {
            Settings.Instance.jetpack = !Settings.Instance.jetpack;
            LabelUpdate();
        }
        private void ChangeLines()
        {
            Settings.Instance.enableLines = !Settings.Instance.enableLines;
            LabelUpdate();
        }
        private void ChangeSymbols()
        {
            Settings.Instance.enableSymbols = !Settings.Instance.enableSymbols;
            LabelUpdate();
        }
        private void DecreaseSymbolSize()
        {
            Settings.Instance.symbolWidth -= 0.0025f;
            symbolHeight = Settings.Instance.symbolWidth * aspectRatio;
        }
        private void IncreaseSymbolSize()
        {
            Settings.Instance.symbolWidth += 0.0025f;
            symbolHeight = Settings.Instance.symbolWidth * aspectRatio;            
        }
        private void LabelUpdate()
        {
            JetPackMode.Text = "Enable for jetpack: " + Settings.Instance.jetpack;
            ShowLines.Text = "Show lines: " + Settings.Instance.enableLines;
            ShowSymbols.Text = "Show symbols: " + Settings.Instance.enableSymbols;
            MinVel.Text = "Min velocity to draw lines/symbols " + Settings.Instance.minVelocity + "m/s";
            ShowStop.Text = "Enable stop time/distance information: " + Settings.Instance.stopInfo;
        }
        private void IncLineLen_()
        {
            Settings.Instance.lineLength += 5;
        }
        private void DecLineLen_()
        {
            Settings.Instance.lineLength -= 5;
        }
        private void IncLineThick_()
        {
            Settings.Instance.lineThickness += 0.05f;
        }
        private void DecLineThick_()
        {
            Settings.Instance.lineThickness -= 0.05f;
        }
        private void MinVelChange(string obj)
        {
            float getter;
            if (!float.TryParse(obj, out getter))
                return;
            Settings.Instance.minVelocity = getter;
            LabelUpdate();
        }
        private void ChangeStopTime()
        {
            Settings.Instance.stopInfo = !Settings.Instance.stopInfo;
            stopInfo.Visible = false;
            LabelUpdate();
        }
        private void LeftMove()
        {
            Settings.Instance.stopTimeDrawCoords += new Vector2D(-0.01, 0);
            UpdateStopInfoCoords();
        }
        private void RightMove()
        {
            Settings.Instance.stopTimeDrawCoords += new Vector2D(0.01, 0);
            UpdateStopInfoCoords();
        }
        private void UpMove()
        {
            Settings.Instance.stopTimeDrawCoords += new Vector2D(0, 0.01);
            UpdateStopInfoCoords();
        }
        private void DownMove()
        {
            Settings.Instance.stopTimeDrawCoords += new Vector2D(0, -0.01);
            UpdateStopInfoCoords();
        }

        private void UpdateStopInfoCoords()
        {
            stopInfo.Origin = Settings.Instance.stopTimeDrawCoords;
        }
    }
}

