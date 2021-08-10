using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Jypeli.Effects;

using Matrix = System.Numerics.Matrix4x4;


namespace Jypeli
{
    /// <summary>
    /// Piirtojärjestys.
    /// </summary>
    public enum DrawOrder
    {
        /// <summary>
        /// Piirtojärjestyksellä ei ole väliä. Oliot piirretään mahdollisimman tehokkaassa järjestyksessä.
        /// Tämä on oletus.
        /// </summary>
        Irrelevant,

        /// <summary>
        /// Oliot piirretään siinä järjestyksessä missä ne on lisätty peliin.
        /// </summary>
        FirstToLast,
    }


    /// <summary>
    /// Kerros. Vastaa olioiden piirtämisestä.
    /// </summary>
    public class Layer : Updatable
    {
        /// <summary>
        /// Vertices for drawing a filled square.
        /// </summary>
        static readonly Vector[] squareVertices =
        {
            new Vector( -0.5, -0.5 ),
            new Vector( -0.5, 0.5 ),
            new Vector( 0.5, -0.5 ),
            new Vector( 0.5, 0.5 )
        };

        /// <summary>
        /// Indices for the vertex array of the square.
        /// </summary>
        static readonly Int16[] squareIndices =
        {
            0, 1, 2,
            2, 1, 3
        };

        /// <summary>
        /// Vertices for drawing a filled triangle.
        /// </summary>
        static readonly Vector[] triangleVertices =
        {
            new Vector( 0f, 0.5f ),
            new Vector( 0.5f, -0.5f ),
            new Vector( -0.5f, -0.5f ),
        };

        /// <summary>
        /// Indices for the vertex array of the triangle.
        /// </summary>
        static readonly Int16[] triangleIndices = { 0, 1, 2 };

        static readonly TextureCoordinates defaultCoords = new TextureCoordinates()
        {
            TopLeft = new Vector(0.0f, 0.0f),
            TopRight = new Vector(1.0f, 0.0f),
            BottomLeft = new Vector(0.0f, 1.0f),
            BottomRight = new Vector(1.0f, 1.0f),
        };

        /// <summary>
        /// Kerroksen oliot
        /// </summary>
        public SynchronousList<IGameObject> Objects = new SynchronousList<IGameObject>();

        /// <summary>
        /// Kerroksen efektit
        /// </summary>
        public SynchronousList<ParticleSystem> Effects = new SynchronousList<ParticleSystem>();

        private List<IGameObject> objectsWithImage = new List<IGameObject>();
        private List<IGameObject> objectsWithoutImage = new List<IGameObject>();
        private List<IGameObject> objectsWithDrawMethod = new List<IGameObject>();

        private Vector _relativeTransition = new Vector(1, 1);

        /// <summary>
        /// Ajetaanko kerrokselle päivitystä
        /// (Aina kyllä)
        /// </summary>
        public bool IsUpdated
        {
            get { return true; }
        }

        /// <summary>
        /// Olioiden piirtojärjestys.
        /// </summary>
        public DrawOrder DrawOrder { get; set; }

        /// <summary>
        /// Kuinka paljon tämän kerroksen olioiden paikka muuttuu kameran siirtyessä suhteessa muihin kerroksiin.
        /// Esimerkiksi arvo 0.5 aiheuttaa siirtymän joka on puolet normaalista.
        /// </summary>
        public Vector RelativeTransition
        {
            get { return _relativeTransition; }
            set { _relativeTransition = value; }
        }

        /// <summary>
        /// Jättää kameran zoomin huomiotta jos asetettu.
        /// </summary>
        public bool IgnoresZoom { get; set; }

        /// <summary>
        /// Ruudukko. Ruudukko piirretään asettamalla tähän <c>Grid</c>-olio.
        /// Jos <c>null</c>, ruudukkoa ei piirretä.
        /// </summary>
        public Grid Grid { get; set; }

        /// <summary>
        /// Muodostaa uuden kerroksen
        /// </summary>
        public Layer()
        {
            DrawOrder = DrawOrder.Irrelevant;
            Objects.ItemAdded += ObjectAdded;
            Objects.ItemRemoved += ObjectRemoved;
        }

        /// <summary>
        /// Luo staattisen kerroksen (ei liiku kameran mukana)
        /// </summary>
        /// <returns></returns>
        public static Layer CreateStaticLayer()
        {
            return new Layer() { RelativeTransition = Vector.Zero, IgnoresZoom = true };
        }

        private void ObjectAdded(IGameObject obj)
        {
            if (obj is ParticleSystem)
            {
                Effects.Add((ParticleSystem)obj);
            }
            else
            if (obj is CustomDrawable)
            {
                objectsWithDrawMethod.Add(obj);
            }
            else if (obj.Image != null)
            {
                objectsWithImage.Add(obj);
            }
            else
            {
                objectsWithoutImage.Add(obj);
            }

            ((IGameObjectInternal)obj).Layer = this;
        }

        private void ObjectRemoved(IGameObject obj)
        {
            if (obj is ParticleSystem)
            {
                Effects.Remove((ParticleSystem)obj);
            }
            else
            {
                objectsWithDrawMethod.Remove(obj);
                objectsWithImage.Remove(obj);
                objectsWithoutImage.Remove(obj);
            }

            ((IGameObjectInternal)obj).Layer = null;
        }

        internal void Add(IGameObject o)
        {
            Objects.Add(o);
        }

        internal void Remove(IGameObject o)
        {
            Objects.Remove(o);
        }

        /// <summary>
        /// Tyhjentää kerroksen olioista
        /// </summary>
        public void Clear()
        {
            Effects.Clear();
            Objects.Clear();
            objectsWithImage.Clear();
            objectsWithoutImage.Clear();
            objectsWithDrawMethod.Clear();
        }

        /// <summary>
        /// Instantly applies changes made to the layer's objects.
        /// </summary>
        public void ApplyChanges()
        {
            Effects.UpdateChanges();
            Objects.UpdateChanges();
        }

        /// <summary>
        /// Ajaa päivityksen kerroksen olioille ja efekteille
        /// </summary>
        /// <param name="time"></param>
        public void Update(Time time)
        {
            Objects.Update(time);
            Effects.Update(time);
        }

        internal void Draw(Camera camera)
        {
            var zoomMatrix = IgnoresZoom ? Matrix.Identity : Matrix.CreateScale((float)(camera.ZoomFactor), (float)(camera.ZoomFactor), 1f);
            var worldMatrix =
                Matrix.CreateTranslation((float)(-camera.Position.X * RelativeTransition.X), (float)(-camera.Position.Y * RelativeTransition.Y), 0)
                * zoomMatrix;

            SortObjects();

            switch (DrawOrder)
            {
                case DrawOrder.Irrelevant:
                    DrawEfficientlyInNoParticularOrder(ref worldMatrix);
                    break;
                case DrawOrder.FirstToLast:
                    DrawInOrderFromFirstToLast(ref worldMatrix); // TODO: Halutaanko tätä vaihtoehtoa enää säilyttää, jos piirtojärjestykseen halutaan vaikuttaa, olisi layerit oikea vaihtoehto.
                    break;
                default:
                    break;
            }

            Effects.ForEach(e => e.Draw(worldMatrix));

            if (Grid != null)
            {
                DrawGrid(ref worldMatrix);
            }
        }
        
        /// <summary>
        /// Järjestelee kappaleet uudestaan oikeisiin listoihin, jos niille on lisätty tai poistettu kuva.
        /// TODO: Tätä ei oikeasti tarvitsisi ajaa kuin kuvaa muutettaessa.
        /// </summary>
        private void SortObjects()
        {
            for(int i = 0; i < objectsWithImage.Count; i++)
            {
                var obj = objectsWithImage[i];
                if (obj.Image is null)
                {
                    objectsWithoutImage.Add(obj);
                    objectsWithImage.Remove(obj);
                    i--;
                }
            }

            for (int i = 0; i < objectsWithoutImage.Count; i++)
            {
                var obj = objectsWithoutImage[i];
                if (obj.Image is not null)
                {
                    objectsWithImage.Add(obj);
                    objectsWithoutImage.Remove(obj);
                    i--;
                }
            }
        }

        private void DrawGrid(ref Matrix matrix)
        {
            Graphics.LineBatch.Begin(ref matrix);

            var camera = Game.Instance.Camera;
            var screen = Game.Screen;
            Vector topLeft = camera.ScreenToWorld(new Vector(screen.Left, screen.Top));
            Vector topRight = camera.ScreenToWorld(new Vector(screen.Right, screen.Top));
            Vector bottomLeft = camera.ScreenToWorld(new Vector(screen.Left, screen.Bottom));
            Vector bottomRight = camera.ScreenToWorld(new Vector(screen.Right, screen.Bottom));

            int horizontalCount = (int)Math.Ceiling((topRight.X - topLeft.X) / Grid.CellSize.X);
            int leftmostLine = (int)Math.Ceiling(topLeft.X / Grid.CellSize.X);
            double leftmostX = leftmostLine * Grid.CellSize.X;

            for (int i = 0; i < horizontalCount; i++)
            {
                double x = leftmostX + i * Grid.CellSize.X;
                Vector startPoint = new Vector(x, topLeft.Y);
                Vector endPoint = new Vector(x, bottomLeft.Y);
                Graphics.LineBatch.Draw(startPoint, endPoint, Grid.Color);
            }

            int verticalCount = (int)Math.Ceiling((topRight.Y - bottomRight.Y) / Grid.CellSize.Y);
            int bottommostLine = (int)Math.Ceiling(bottomRight.Y / Grid.CellSize.Y);
            double bottommostY = bottommostLine * Grid.CellSize.Y;

            for (int i = 0; i < verticalCount; i++)
            {
                double y = bottommostY + i * Grid.CellSize.Y;

                // Doesn't draw the line when y is 0! Wonder why...
                if (y == 0.0)
                    y += 0.1;

                Vector startPoint = new Vector(bottomLeft.X, y);
                Vector endPoint = new Vector(bottomRight.X, y);
                Graphics.LineBatch.Draw(startPoint, endPoint, Grid.Color);
            }

            Graphics.LineBatch.End();
        }

        private void DrawInOrderFromFirstToLast(ref Matrix worldMatrix)
        {
            Renderer.LightingEnabled = true;

            foreach (var o in Objects)
            {
                if (o is CustomDrawable)
                {
                    if (o.IsVisible)
                        ((CustomDrawable)o).Draw(worldMatrix);
                }
                else
                {
                    Renderer.LightingEnabled = !o.IgnoresLighting;
                    Draw(o, ref worldMatrix);
                }
            }

            DrawChildObjects(worldMatrix);

            Renderer.LightingEnabled = false;
        }

        int CompareByImageReference(IGameObject o1, IGameObject o2)
        {
            if (o1.Image == null || o2.Image == null)
                return 0;

            int hash1 = RuntimeHelpers.GetHashCode(o1.Image);
            int hash2 = RuntimeHelpers.GetHashCode(o2.Image);

            if (hash1 == hash2)
            {
                // Because the sorting algorithm of the List class is not stable,
                // objects with similar images on top of each other flicker rather
                // annoyingly. Therefore, compare object references in order to keep
                // the sorting stable.
                return RuntimeHelpers.GetHashCode(o1) - RuntimeHelpers.GetHashCode(o2);
            }
            return hash1 - hash2;
        }

        private void DrawEfficientlyInNoParticularOrder(ref Matrix worldMatrix)
        {
            Renderer.LightingEnabled = true;
            DrawSimpleObjectsWithoutImages(worldMatrix);
            DrawObjectsWithImages(worldMatrix);
            DrawCustomDrawables(worldMatrix);
            DrawChildObjects(worldMatrix);
            Renderer.LightingEnabled = false;
        }

        private void DrawSimpleObjectsWithoutImages(Matrix worldMatrix)
        {
            Graphics.ShapeBatch.Begin(ref worldMatrix);
            foreach (var o in objectsWithoutImage)
            {
                if (!o.IsVisible)
                    continue;

                Renderer.LightingEnabled = !o.IgnoresLighting;

                DrawShape(o, ref worldMatrix);

            }
            Graphics.ShapeBatch.End();
        }

        private void DrawObjectsWithImages(Matrix worldMatrix)
        {
            // By sorting the images first, we get objects with same
            // images in succession. This allows us to use the ImageBatch
            // class more efficiently.
            objectsWithImage.Sort(CompareByImageReference);

            // Passing null for image here does not hurt, because the first
            // ReferenceEquals-comparison calls End() without drawing a single
            // image.
            Graphics.ImageBatch.Begin(ref worldMatrix, null);

            Image previousImage = null;

            foreach (var o in objectsWithImage)
            {
                if (!o.IsVisible)
                    continue;

                Renderer.LightingEnabled = !o.IgnoresLighting;

                if (!ReferenceEquals(o.Image, previousImage))
                {
                    // Object o has different image than the previous one,
                    // so let's start a new batch with the new image. The objects
                    // should be sorted at this point.
                    Graphics.ImageBatch.End();
                    Graphics.ImageBatch.Begin(ref worldMatrix, o.Image);
                    previousImage = o.Image;
                }
                DrawTexture(o, ref worldMatrix);
            }

            Graphics.ImageBatch.End();
        }

        private void DrawCustomDrawables(Matrix worldMatrix)
        {
            foreach (CustomDrawable o in objectsWithDrawMethod)
            {
                if (o.IsVisible)
                    o.Draw(worldMatrix);
            }
        }

        private void DrawChildObjects(Matrix worldMatrix)
        {
            Graphics.ShapeBatch.Begin(ref worldMatrix);
            Vector drawScale = new Vector(1, 1);

            for (int i = 0; i < Objects.Count; i++)
            {
                var go = Objects[i] as GameObject;
                if (go == null || go._childObjects == null || go is Window)
                    continue;

                if (go.Shape.IsUnitSize)
                    drawScale = go.Size;

                // recursively draw children, their children and so on.
                drawChildren(go._childObjects);

                void drawChildren(SynchronousList<GameObject> children)
                {
                    for (int j = 0; j < children.Count; j++)
                    {
                        GameObject go = children[j];
                        if(go.Image != null) // Tämä ei nyt ole erityisen tehokas ratkaisu, mutta harvemmin kappaleella on useita lapsiolioita, etenkään monella eri kuvalla.
                        {
                            Graphics.ImageBatch.Begin(ref worldMatrix, go.Image);
                            DrawTexture(go, ref worldMatrix);
                            Graphics.ImageBatch.End();

                        }
                        else
                        {
                            DrawShape(go, ref worldMatrix);
                        }
                        if (go._childObjects != null)
                            drawChildren(go._childObjects);
                    }
                }
            }

            Graphics.ShapeBatch.End();
        }

        private void DrawTexture(IGameObject o, ref Matrix parentTransformation)
        {
            Vector position = new Vector((float)o.Position.X, (float)o.Position.Y);
            Vector scale = new Vector((float)o.Size.X, (float)o.Size.Y);
            float rotation = o.RotateImage ? (float)o.Angle.Radians : 0;

            if (o.IsVisible)
            {
                if (o.TextureWrapSize == Vector.Diagonal)
                {
                    Graphics.ImageBatch.Draw(defaultCoords, position, scale, rotation);
                }
                else
                {
                    float wx = (float)(Math.Sign(o.TextureWrapSize.X));
                    float wy = (float)(Math.Sign(o.TextureWrapSize.Y));
                    float left = (float)(-wx / 2 + 0.5);
                    float right = (float)(wx / 2 + 0.5);
                    float top = (float)(-wy / 2 + 0.5);
                    float bottom = (float)(wy / 2 + 0.5);

                    TextureCoordinates customCoords = new TextureCoordinates()
                    {
                        TopLeft = new Vector(left, top),
                        TopRight = new Vector(right, top),
                        BottomLeft = new Vector(left, bottom),
                        BottomRight = new Vector(right, bottom),
                    };

                    if (o.TextureWrapSize.X == wx && o.TextureWrapSize.Y == wy)
                    {
                        // Draw only once
                        Graphics.ImageBatch.Draw(customCoords, position, scale, rotation);
                        return;
                    }

                    float topLeftX = -(float)(o.TextureWrapSize.X - 1) / 2;
                    float topLeftY = -(float)(o.TextureWrapSize.Y - 1) / 2;

                    Vector newScale = new Vector(
                        scale.X / (wx * (float)o.TextureWrapSize.X),
                        scale.Y / (wy * (float)o.TextureWrapSize.Y));

                    for (int y = 0; y < o.TextureWrapSize.Y; y++)
                    {
                        for (int x = 0; x < o.TextureWrapSize.X; x++)
                        {
                            Vector newPosition = position + new Vector((topLeftX + x) * newScale.X, (topLeftY + y) * newScale.Y);
                            Graphics.ImageBatch.Draw(customCoords, newPosition, newScale, rotation);
                        }
                    }
                }
            }
        }

        private void DrawShape(IGameObject o, ref Matrix parentTransformation)
        {
            Vector position = new Vector((float)o.Position.X, (float)o.Position.Y);
            Vector scale = new Vector((float)o.Size.X, (float)o.Size.Y);
            float rotation = (float)o.Angle.Radians;

            if (o.IsVisible)
            {
                Graphics.ShapeBatch.Draw(o.Shape.Cache, o.Color, position, scale, rotation);
            }
        }

        private void Draw(IGameObject o, ref Matrix parentTransformation)
        {
            Vector drawScale = new Vector(1, 1);
            if (o.Shape.IsUnitSize)
                drawScale = o.Size;

            Vector position = new Vector((float)o.Position.X, (float)o.Position.Y);
            Vector scale = new Vector((float)drawScale.X, (float)drawScale.Y);
            float rotation = o.RotateImage ? (float)o.Angle.Radians : 0;

            if (o.IsVisible)
            {
                Matrix transformation =
                    Matrix.CreateScale((float)scale.X, (float)scale.Y, 1f)
                    * Matrix.CreateRotationZ(rotation)
                    * Matrix.CreateTranslation((float)position.X, (float)position.Y, 0f)
                    * parentTransformation;

                if (o is CustomDrawable)
                {
                    ((CustomDrawable)o).Draw(parentTransformation);
                }
                else if (o.Image != null && (!o.TextureFillsShape))
                {
                    Renderer.DrawImage(o.Image, ref transformation, o.TextureWrapSize);
                }
                else if (o.Image != null)
                {
                    Renderer.DrawShape(o.Shape, ref transformation, ref transformation, o.Image, o.TextureWrapSize, o.Color);
                }
                else
                {
                    DrawShape(o, ref parentTransformation);
                }
            }
        }

        internal void GetObjectsAboutToBeAdded(List<IGameObject> result)
        {
            result.AddRange(Objects.GetObjectsAboutToBeAdded());
        }
    }
}
