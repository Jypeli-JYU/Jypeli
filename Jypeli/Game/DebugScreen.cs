using System;
using System.Text;
using Jypeli.Rendering;
using Matrix = System.Numerics.Matrix4x4;

namespace Jypeli
{
    public partial class Game
    {
        private string fpsText = "00";
        private int fpsSkipCounter;
        private Canvas debugCanvas;
        private Matrix canvasTransform = Matrix.Identity;
        private Camera camera { get => Game.Instance.Camera; }

        #region debugsettings
        /// <summary>
        /// Debug-näkymän (F12) piirtoasetuksia
        /// </summary>
        public struct DebugViewSettings
        {
            /// <summary>
            /// Piirretäänkö objektien ääriviivoja
            /// </summary>
            public static bool DrawOutlines = true;

            /// <summary>
            /// Piirretäänkö fysiikkamuotojen verteksien ääriviivat.
            /// Toimii ainoastaan Farseer-moottorilla.
            /// </summary>
            public static bool DrawPhysicsOutlines = true;

            /// <summary>
            /// Piirretäänkö ympyrän keskikohdasta viiva sen oikeaan reunaan.
            /// </summary>
            public static bool DrawCircleRotation = true;

            /// <summary>
            /// Piirretäänkö liitoksien kiinnityskohdat.
            /// </summary>
            public static bool DrawJointPoints = true;

            /// <summary>
            /// Millä värillä <c>GameObject</c>ien ääriviivat piirretään
            /// </summary>
            public static Color GameObjectColor = Color.LightGray;

            /// <summary>
            /// Millä värillä <c>GameObject</c>ien ääriviivat piirretään kun hiiri on niiden päällä
            /// </summary>
            public static Color GameObjectHoverColor = Color.LightGreen;

            /// <summary>
            /// Millä värillä <c>PhysicsObject</c>ien ääriviivat piirretään
            /// </summary>
            public static Color PhysicsObjectColor = Color.DarkRed;

            /// <summary>
            /// Millä värillä <c>PhysicsObject</c>ien ääriviivat piirretään kun hiiri on niiden päällä
            /// </summary>
            public static Color PhysicsObjectHoverColor = Color.Salmon;

            /// <summary>
            /// Millä värillä <c>PhysicsObject</c>ien fysiikkamuotojen verteksien ääriviivat piirretään
            /// </summary>
            public static Color PhysicsObjectVertexColor = Color.Gray;

            /// <summary>
            /// Millä värillä liitoksen liitoskohta piirretään
            /// </summary>
            public static Color JointPositionColor = Color.Red;
        }
        #endregion

        /// <summary>
        /// Debug-ruutukerros, joka näkyy kun painetaan F12.
        /// Voit lisätä olioita myös tälle kerrokselle.
        /// </summary>
        public Layer DebugLayer { get; private set; }

        /// <summary>
        /// Debug-ruutu F12-näppäimestä päällä / pois.
        /// </summary>
        public bool DebugKeyEnabled { get; set; }

        /// <summary>
        /// Debug-ruutu näkyvissä / pois.
        /// </summary>
        public bool DebugScreenVisible { get; set; }

        /// <summary>
        /// FPS-ikkuna.
        /// </summary>
        public Window FPSWindow { get; private set; }

        /// <summary>
        /// FPS-näyttö.
        /// </summary>
        public Label FPSDisplay { get; private set; }

        /// <summary>
        /// "Layers"-ikkuna. Huom. asettaa kokonsa automaattisesti.
        /// </summary>
        public Window LayerWindow { get; private set; }

        /// <summary>
        /// "Layers"-näyttö.
        /// </summary>
        public Label LayerDisplay { get; private set; }

        private void InitDebugScreen()
        {
            debugCanvas = new Canvas();

            DebugLayer = Layer.CreateStaticLayer();
            DebugLayer.Objects.ItemAdded += OnObjectAdded;
            DebugLayer.Objects.ItemRemoved += OnObjectRemoved;

            FPSWindow = new Window();
            FPSWindow.IsModal = false;
            FPSWindow.Color = new Color(Color.HotPink, 100);
            DebugLayer.Add(FPSWindow);

            FPSDisplay = new Label("00");
            FPSDisplay.Color = Color.HotPink;
            FPSDisplay.Position = FPSWindow.Position;
            FPSWindow.Size = 1.5 * FPSDisplay.Size;
            FPSWindow.Add(FPSDisplay);

            LayerWindow = new Window();
            LayerWindow.IsModal = false;
            LayerWindow.Color = new Color(Color.Blue, 100);
            DebugLayer.Add(LayerWindow);

            LayerDisplay = new Label("Layers: no data");
            LayerDisplay.TextColor = Color.White;
            LayerDisplay.Position = LayerWindow.Position;
            LayerWindow.Add(LayerDisplay);
            LayerWindow.Size = LayerDisplay.Size;

            DebugKeyEnabled = true;
            DebugScreenVisible = false;
        }

        private StringBuilder layerTextBuilder = new StringBuilder();
        private const string layerTextTitle = "Layers:\n";

        /// <summary>
        /// Jypelin aika pitää sisällään tiedon edellisestä pelin päivityksestä,
        /// MonoGamen aika edellisestä ruudunpäivityksestä.
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateFps(Time gameTime)
        {
            fpsText = (10000000.0 / gameTime.SinceLastUpdate.Ticks).ToString("F2");
            if (fpsSkipCounter++ > 10)
            {
                fpsSkipCounter = 0;
            }
        }

        private void UpdateDebugScreen(Time time)
        {
            if (DebugKeyEnabled && Keyboard.GetKeyState(Key.F12) == ButtonState.Pressed)
            {
                DebugScreenVisible = !DebugScreenVisible;

                // Päivitetään elementtien sijainti ruudulle oikeaan kohtaan
                UpdateLayerWindow(); // Ensimmäistä aukaisua varten pitää täydentää elementtien sisällöt
                FPSDisplay.Text = fpsText;

                LayerWindow.Left = Screen.Left;
                LayerWindow.Top = Screen.Top;

                FPSWindow.Right = Screen.Right;
                FPSWindow.Top = Screen.Top;
            }

            DebugLayer.Update(time);

            if (!DebugScreenVisible)
                return;

            FPSDisplay.Text = fpsText;
            UpdateLayerWindow();
        }

        private void UpdateLayerWindow()
        {
            layerTextBuilder.Clear();
            layerTextBuilder.Append(layerTextTitle);

            for (int i = Layers.FirstIndex; i <= Layers.LastIndex; i++)
            {
                layerTextBuilder.Append("[");
                layerTextBuilder.Append(i);
                layerTextBuilder.Append("]: ");
                layerTextBuilder.Append(Layers[i].Objects.Count);
                layerTextBuilder.AppendLine();
            }

            LayerDisplay.Text = layerTextBuilder.ToString();
            LayerWindow.Size = LayerDisplay.Size;
        }

        private void DrawDebugScreen()
        {
            if (!DebugScreenVisible)
                return;

            if (DebugViewSettings.DrawOutlines)
                PaintDebugScreen();

            DebugLayer.Draw(Camera);
        }

        // TODO: Mikä olisi järkevin tapa toteuttaa näiden piirto?
        private void PaintPhysicsOutlines(Canvas canvas, PhysicsObject obj, Color color)
        {
            if (obj.Body == null || obj.Body.Shape == null || obj.Body.Shape.Cache == null)
                return;

            var vertices = obj.Body.Vertices;
            double wmul = 100; // Farseerin SimToDisplay-kerroin. Tämä pitäisi varmaan yhdistää jotenkin siihen.
            double hmul = 100;

            canvas.BrushColor = color;

            Matrix transform =
                     Matrix.CreateRotationZ((float)obj.Angle.Radians)
                    * Matrix.CreateTranslation((float)obj.Position.X, (float)obj.Position.Y, 0f)
                    * Matrix.CreateTranslation(-(float)camera.Position.X, -(float)camera.Position.Y, 0f)
                    * Matrix.CreateScale((float)(camera.ZoomFactor), (float)(camera.ZoomFactor), 1f);

            foreach (var vertexes in vertices)
            {
                for (int j = 0; j < vertexes.Count - 1; j++)
                {
                    double x1 = wmul * vertexes[j].X;
                    double y1 = hmul * vertexes[j].Y;
                    double x2 = wmul * vertexes[j + 1].X;
                    double y2 = hmul * vertexes[j + 1].Y;
                    /*
                    var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                    var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                    canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);*/
                }

                if (vertexes.Count > 2)
                {
                    double x1 = wmul * vertexes[vertexes.Count - 1].X;
                    double y1 = hmul * vertexes[vertexes.Count - 1].Y;
                    double x2 = wmul * vertexes[0].X;
                    double y2 = hmul * vertexes[0].Y;
                    /*
                    var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                    var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                    canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);*/
                }
            }
        }

        private void DrawJoints(Canvas canvas, IAxleJoint joint)
        {
            PhysicsObject obj1 = joint.Object1;
            PhysicsObject obj2 = joint.Object2;
            Vector ap = joint.AxlePoint;

            Matrix transform =
                     Matrix.CreateRotationZ((float)obj1.Angle.Radians)
                    * Matrix.CreateTranslation((float)obj1.Position.X, (float)obj1.Position.Y, 0f)
                    * Matrix.CreateTranslation(-(float)camera.Position.X, -(float)camera.Position.Y, 0f)
                    * Matrix.CreateScale((float)(camera.ZoomFactor), (float)(camera.ZoomFactor), 1f);

            ap = ap.Transform(transform);

            Vector p1 = ap + Vector.Diagonal * 5;
            Vector p2 = ap + Vector.UnitX * 5 - Vector.UnitY * 5;

            canvas.BrushColor = DebugViewSettings.JointPositionColor;

            canvas.DrawLine(p1, p1 - Vector.Diagonal * 10);
            canvas.DrawLine(p2, p2 - Vector.UnitX * 10 + Vector.UnitY * 10);

        }

        private void PaintDebugScreen()
        {
            Layers.ForEach(l => l.DrawOutlines(Camera, DebugViewSettings.GameObjectColor, typeof(GameObject)));
            Layers.ForEach(l => l.DrawOutlines(Camera, DebugViewSettings.PhysicsObjectColor, typeof(PhysicsObject)));

            var mouseOverObjects = GetObjects(Mouse.IsCursorOn);

            var zoomMatrix = Matrix.CreateScale((float)(camera.ZoomFactor), (float)(camera.ZoomFactor), 1f);
            var worldMatrix =
                Matrix.CreateTranslation((float)(-camera.Position.X), (float)(-camera.Position.Y), 0)
                * zoomMatrix;

            Graphics.ShapeBatch.Begin(ref worldMatrix, PrimitiveType.OpenGLLines);

            foreach (GameObject obj in mouseOverObjects)
            {
                if (obj is PhysicsObject)
                    Graphics.ShapeBatch.DrawOutlines(obj.Shape.Cache, DebugViewSettings.PhysicsObjectHoverColor, obj.Position, obj.Size, (float)obj.Angle.Radians);
                else
                    Graphics.ShapeBatch.DrawOutlines(obj.Shape.Cache, DebugViewSettings.GameObjectHoverColor, obj.Position, obj.Size, (float)obj.Angle.Radians);
            }

            Graphics.ShapeBatch.End();

            if (PhysicsGameBase.Instance is null)
                return;

            foreach (var joint in PhysicsGameBase.Instance.Joints)
            {
                DrawJoints(debugCanvas, joint);

            }
        }
    }
}
