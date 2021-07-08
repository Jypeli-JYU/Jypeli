using System.Text;
using Microsoft.Xna.Framework;

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
        private void UpdateFps(GameTime gameTime)
        {
            fpsText = (10000000.0 / gameTime.ElapsedGameTime.Ticks).ToString("F2");
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

            debugCanvas.Begin(ref canvasTransform, Screen);
            if (DebugViewSettings.DrawOutlines)
                PaintDebugScreen(debugCanvas);
            debugCanvas.End();

            DebugLayer.Draw(Camera);
        }

        private void PaintShapeOutlines(Canvas canvas, IGameObject obj, Color color)
        {
            var vertexes = obj.Shape.Cache.OutlineVertices;
            double wmul = obj.Shape.IsUnitSize ? obj.Width : 1;
            double hmul = obj.Shape.IsUnitSize ? obj.Height : 1;

            canvas.BrushColor = color;

            Matrix transform =
                     Matrix.CreateRotationZ((float)obj.Angle.Radians)
                    * Matrix.CreateTranslation((float)obj.Position.X, (float)obj.Position.Y, 0f)
                    * Matrix.CreateTranslation(-(float)camera.Position.X, -(float)camera.Position.Y, 0f)
                    * Matrix.CreateScale((float)(camera.ZoomFactor), (float)(camera.ZoomFactor), 1f);

            for (int j = 0; j < vertexes.Length - 1; j++)
            {
                double x1 = wmul * vertexes[j].X;
                double y1 = hmul * vertexes[j].Y;
                double x2 = wmul * vertexes[j + 1].X;
                double y2 = hmul * vertexes[j + 1].Y;

                var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);
            }

            if (vertexes.Length > 2)
            {
                double x1 = wmul * vertexes[vertexes.Length - 1].X;
                double y1 = hmul * vertexes[vertexes.Length - 1].Y;
                double x2 = wmul * vertexes[0].X;
                double y2 = hmul * vertexes[0].Y;

                var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);
            }
            if (obj.Shape == Shape.Circle && DebugViewSettings.DrawCircleRotation)
            {
                double x1 = 0;
                double y1 = 0;
                double x2 = obj.Width / 2;
                double y2 = 0;

                var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);
            }
        }

        private void PaintPhysicsOutlines(Canvas canvas, PhysicsObject obj, Color color)
        {
            if (obj.Body == null || obj.Body.Shape == null || obj.Body.Shape.Cache == null)
                return;

            var vertices = obj.Body.Vertices;
            double wmul = 100; // Farseerin SimToDisplay-kerroin. Tämä pitäiis varmaan yhdistää jotenkin siihen.
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

                    var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                    var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                    canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);
                }

                if (vertexes.Count > 2)
                {
                    double x1 = wmul * vertexes[vertexes.Count - 1].X;
                    double y1 = hmul * vertexes[vertexes.Count - 1].Y;
                    double x2 = wmul * vertexes[0].X;
                    double y2 = hmul * vertexes[0].Y;

                    var t1 = Vector2.Transform(new Vector2((float)x1, (float)y1), transform);
                    var t2 = Vector2.Transform(new Vector2((float)x2, (float)y2), transform);

                    canvas.DrawLine(t1.X, t1.Y, t2.X, t2.Y);
                }
            }
        }

        private void PaintDebugScreen(Canvas canvas)
        {
            for (int i = Layers.FirstIndex; i <= Layers.LastIndex; i++)
            {
                foreach (var obj in Layers[i].Objects)
                {
                    if (obj == null || obj.Shape == null || obj.Shape.Cache == null || obj.Layer == null || obj is Widget) // Toistaiseksi ei piirretä widgettien ääriviivoja (halutaanko edes?)
                        continue;

                    if (obj is PhysicsObject)
                    {
                        PaintShapeOutlines(canvas, obj, Mouse.IsCursorOn((GameObject)obj) ? DebugViewSettings.PhysicsObjectHoverColor : DebugViewSettings.PhysicsObjectColor);
                        if (DebugViewSettings.DrawPhysicsOutlines)
                            PaintPhysicsOutlines(canvas, (PhysicsObject)obj, DebugViewSettings.PhysicsObjectVertexColor);
                    }
                    else
                    {
                        if (obj is PhysicsStructure)
                        {
                            PhysicsStructure ps = obj as PhysicsStructure;
                            foreach (var o in ps.Objects)
                            {
                                PaintShapeOutlines(canvas, o, Mouse.IsCursorOn(o) ? DebugViewSettings.GameObjectHoverColor : DebugViewSettings.GameObjectColor);
                            }
                        }
                        else
                            PaintShapeOutlines(canvas, obj, Mouse.IsCursorOn((GameObject)obj) ? DebugViewSettings.GameObjectHoverColor : DebugViewSettings.GameObjectColor);
                    }
                }
            }
        }
    }
}
