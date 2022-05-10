#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using System.Numerics;

namespace Jypeli
{
    /// <summary>
    /// Kamera. Määrittää mikä osa pelitasosta on kerralla näkyvissä.
    /// </summary>
    [Save]
    public class Camera : PositionalRW
    {
        // Huom. tätä käytetään vain jos seurataan useita olioita kerralla
        private List<GameObject> followedObjects = null;
        private Vector _pos = Vector.Zero;

        [Save] internal bool _stayInLevel = false;
        [Save] internal double _zoomFactor = 1.0;

        /// <summary>
        /// Kameran sijainti.
        /// </summary>
        [Save]
        public Vector Position
        {
            get { return _pos; }
            set { _pos = value; }
        }

        /// <summary>
        /// Kameran liikkumisnopeus.
        /// </summary>
        [Save]
        public Vector Velocity { get; set; }

        /// <summary>
        /// Kameran paikan X-koordinaatti.
        /// </summary>
        public double X
        {
            get
            {
                return _pos.X;
            }
            set
            {
                _pos.X = value;
            }
        }

        /// <summary>
        /// Kameran paikan Y-koordinaatti.
        /// </summary>
        public double Y
        {
            get
            {
                return _pos.Y;
            }
            set
            {
                _pos.Y = value;
            }
        }

        /// <summary>
        /// Kameran zoomauskerroin.
        /// Oletuksena 1.0. Mitä suurempi zoomauskerroin, sitä lähempänä kamera on (esim 2.0 on 2 x lähempänä) ja toisinpäin.
        /// </summary>
        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set { _zoomFactor = value; }
        }

        /// <summary>
        /// Jos tosi, kamera ei mene koskaan kentän ulkopuolelle.
        /// </summary>
        public bool StayInLevel
        {
            get { return _stayInLevel; }
            set { _stayInLevel = value; }
        }

        /// <summary>
        /// Olio, jota kamera seuraa. Jos <c>null</c>, mitään oliota ei seurata.
        /// </summary>
        public GameObject FollowedObject { get; set; }

        /// <summary>
        /// Seurataanko oliota (FollowedObject) x- eli vaakasuunnassa.
        /// </summary>
        public bool FollowsX { get; set; }

        /// <summary>
        /// Seurataanko oliota (FollowedObject) y- eli pystysuunnassa.
        /// </summary>
        public bool FollowsY { get; set; }

        /// <summary>
        /// Jos kamera seuraa oliota, tällä voi säätää missä kohtaa ruutua pelaaja näkyy.
        /// Toisin sanoen ruutukoordinaateissa kerrotaan, kuinka kaukana ruudun keskustasta
        /// seurattava olio näkyy.
        /// </summary>
        /// <example>
        /// Pelaajan näyttäminen ruudun alareunassa, vaakasuunnassa keskellä:
        /// <code>
        /// Camera.Follow( pelaaja );
        /// Camera.FollowOffset = new Vector( 0, Screen.Height * 0.4 );
        /// </code>
        /// </example>
        [Save] public Vector FollowOffset { get; set; }

        /// <summary>
        /// Jos kamera seuraa useita olioita, tällä voi säätää kuinka paljon vasempaan ja
        /// oikeaan reunaan jätetään tyhjää tilaa.
        /// </summary>
        public double FollowXMargin { get; set; }

        /// <summary>
        /// Jos kamera seuraa useita olioita, tällä voi säätää kuinka paljon ylä- ja
        /// alareunaan jätetään tyhjää tilaa.
        /// </summary>
        public double FollowYMargin { get; set; }

        /// <summary>
        /// Luo uuden kameran.
        /// </summary>
        internal Camera()
        {
            FollowsX = true;
            FollowsY = true;
            FollowXMargin = FollowYMargin = 600;
        }

        /// <summary>
        /// Muuntaa annetun pisteen ruutukoordinaateista maailmankoordinaatteihin.
        /// </summary>
        public Vector ScreenToWorld(Vector point)
        {
            Matrix4x4 transform =
                Matrix4x4.CreateScale(new Vector(1 / ZoomFactor, 1 / ZoomFactor)) *
                Matrix4x4.CreateTranslation(new Vector(Position.X, Position.Y));
            return point.Transform(transform);
        }

        /// <summary>
        /// Muuntaa annetun pisteen maailmankoordinaateista ruutukoordinaatteihin.
        /// </summary>
        public Vector WorldToScreen(Vector point)
        {
            Matrix4x4 transform =
                Matrix4x4.CreateTranslation(-new Vector(Position.X, Position.Y)) *
                Matrix4x4.CreateScale(new Vector(ZoomFactor, ZoomFactor));
            return point.Transform(transform);
        }

        /// <summary>
        /// Muuntaa annetun pisteen ruutukoordinaateista maailmankoordinaatteihin
        /// ottaen huomioon oliokerroksen suhteellisen siirtymän.
        /// </summary>
        public Vector ScreenToWorld(Vector point, Layer layer)
        {
            if (layer == null)
                return ScreenToWorld(point);
            if (layer.IgnoresZoom)
            {
                return point.Transform(Matrix4x4.CreateTranslation(-new Vector(Game.Screen.Size.X / 2, Game.Screen.Size.Y / 2)) *
                                        Matrix4x4.CreateTranslation(new Vector(Position.X * layer.RelativeTransition.X, -Position.Y * layer.RelativeTransition.Y)) *
                                        Matrix4x4.CreateScale(new Vector(1, -1)));
            }

            Matrix4x4 transform =
                Matrix4x4.CreateTranslation(-new Vector(Game.Screen.Size.X / 2, Game.Screen.Size.Y / 2)) *
                Matrix4x4.CreateScale(new Vector(1 / ZoomFactor, 1 / ZoomFactor)) *
                Matrix4x4.CreateTranslation(new Vector(Position.X * layer.RelativeTransition.X, -Position.Y * layer.RelativeTransition.Y)) *
                Matrix4x4.CreateScale(new Vector(1, -1));
            return point.Transform(transform);
        }

        /// <summary>
        /// Muuntaa annetun pisteen maailmankoordinaateista ruutukoordinaatteihin
        /// ottaen huomioon oliokerroksen suhteellisen siirtymän.
        /// </summary>
        public Vector WorldToScreen(Vector point, Layer layer)
        {
            if (layer == null)
                return WorldToScreen(point);
            if (layer.IgnoresZoom)
                return point.Transform(Matrix4x4.CreateScale(new Vector(1, -1)) *
                                        Matrix4x4.CreateTranslation(new Vector(-Position.X * layer.RelativeTransition.X, Position.Y * layer.RelativeTransition.Y)) *
                                        Matrix4x4.CreateTranslation(new Vector(Game.Screen.Size.X / 2, Game.Screen.Size.Y / 2)));
            Matrix4x4 transform =
                           Matrix4x4.CreateScale(new Vector(1, -1)) *
                           Matrix4x4.CreateTranslation(new Vector(-Position.X * layer.RelativeTransition.X, Position.Y * layer.RelativeTransition.Y)) *
                           Matrix4x4.CreateScale(new Vector(ZoomFactor, ZoomFactor)) *
                           Matrix4x4.CreateTranslation(new Vector(Game.Screen.Size.X / 2, Game.Screen.Size.Y / 2));
            return point.Transform(transform);
        }

        /// <summary>
        /// Liikuttaa kameraa.
        /// </summary>
        /// <param name="v">Kameran liikevektori.</param>
        public void Move(Vector v)
        {
            Position += new Vector(v.X / ZoomFactor, v.Y / ZoomFactor);
        }

        /// <summary>
        /// Zoomaa.
        /// </summary>
        /// <param name="zoom">
        /// Zoomauskerroin. Ykköstä suurempi (esim. 1.5) lähentää ja
        /// ykköstä pienempi (esim. 0.5) zoomaa kauemmas.
        /// </param>
        public void Zoom(double zoom)
        {
            ZoomFactor *= zoom;
        }

        /// <summary>
        /// Resetoi kameran (keskittää, laittaa zoomin oletusarvoon ja lopettaa seuraamisen).
        /// </summary>
        public void Reset()
        {
            Position = Vector.Zero;
            Velocity = Vector.Zero;
            ZoomFactor = 1.0f;
            StopFollowing();
        }

        /// <summary>
        /// Seuraa yhtä tai useampaa peliobjektia.
        /// </summary>
        /// <param name="gameobjects">Seurattavat peliobjektit.</param>
        public void Follow(params GameObject[] gameobjects)
        {
            FollowsX = true;
            FollowsY = true;

            if (gameobjects.Length == 0)
                return;
            if (gameobjects.Length == 1)
            {
                FollowedObject = gameobjects[0];
                return;
            }

            FollowedObject = new GameObject(1.0, 1.0);
            followedObjects = new List<GameObject>(gameobjects);
            UpdateAvgPoint();
        }

        /// <summary>
        /// Seuraa jotakin peliobjektia X- eli vaakasuunnassa.
        /// </summary>
        /// <param name="gameobjects">Seurattavat peliobjektit.</param>
        public void FollowX(params GameObject[] gameobjects)
        {
            Follow(gameobjects);
            FollowsX = true;
            FollowsY = false;
        }

        /// <summary>
        /// Seuraa jotakin peliobjektia Y- eli pystysuunnassa.
        /// </summary>
        /// <param name="gameobjects">Seurattavat peliobjektit.</param>
        public void FollowY(params GameObject[] gameobjects)
        {
            Follow(gameobjects);
            FollowsX = false;
            FollowsY = true;
        }

        /// <summary>
        /// Lopettaa olio(iden) seuraamisen.
        /// </summary>
        public void StopFollowing()
        {
            if (followedObjects != null)
            {
                followedObjects = null;
                FollowedObject.Destroy();
            }

            FollowedObject = null;
        }

        private void UpdateAvgPoint()
        {
            FollowedObject.Position = followedObjects.ConvertAll<GameObject, Vector>((GameObject o) => { return o.Position; }).Average();

            double maxDx = followedObjects.ConvertAll<GameObject, double>((GameObject o) => { return Math.Abs(o.X - FollowedObject.X); }).Max();
            double maxDy = followedObjects.ConvertAll<GameObject, double>((GameObject o) => { return Math.Abs(o.Y - FollowedObject.Y); }).Max();

            double zoomX = Game.Screen.Width / (2 * maxDx + FollowXMargin);
            double zoomY = Game.Screen.Height / (2 * maxDy + FollowYMargin);

            ZoomFactor = Math.Min(zoomX, zoomY);
        }

        /// <summary>
        /// Zoomaa ja sijoittaa kameran niin, että parametreina annettua alue näkyy kokonaan ruudulla.
        /// </summary>
        /// <param name="bottomLeft">Alueen vasen alanurkka.</param>
        /// <param name="topRight">Alueen oikea ylänurkka.</param>
        public void ZoomTo(Vector bottomLeft, Vector topRight)
        {
            ZoomTo(bottomLeft.X, bottomLeft.Y, topRight.X, topRight.Y);
        }

        /// <summary>
        /// Sijoittelee kameran annettuun suorakulmioon
        /// </summary>
        /// <param name="rectangle"></param>
        public void ZoomTo(BoundingRectangle rectangle)
        {
            ZoomTo(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top);
        }

        /// <summary>
        /// Sijoittelee kameran annettuun suorakulmioon
        /// annetulla marginaalilla
        /// </summary>
        /// <param name="rectangle"></param>
        /// <param name="borderSize"></param>
        public void ZoomTo(BoundingRectangle rectangle, double borderSize)
        {
            ZoomTo(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top, borderSize);
        }

        /// <summary>
        /// Keskittää ja zoomaa kameran niin, että kaikki objektit näkyvät ruudulla.
        /// </summary>
        public void ZoomToAllObjects()
        {
            ZoomToAllObjects(0);
        }

        /// <summary>
        /// Zoomaa ja sijoittaa kameran siten, että kaikki pelioliot ovat yhtäaikaa näkyvissä.
        /// </summary>
        /// <param name="borderSize">Reunalle jätettävä tila (jos negatiivinen, niin osa kentästä jää piiloon).</param>
        public void ZoomToAllObjects(double borderSize)
        {
            // Do the real zoom next update so all objects waiting to be added are added before that
            Game.DoNextUpdate(DoZoomToAllObjects, borderSize);
        }

        private void DoZoomToAllObjects(double borderSize)
        {
            if (Game.Instance.ObjectCount > 0)
                ZoomTo(Game.Instance.Level.FindObjectLimits(), borderSize);
        }

        /// <summary>
        /// Zoomaa ja sijoittaa kameran niin, että parametreina annettua alue näkyy kokonaan ruudulla.
        /// </summary>
        /// <param name="left">Alueen vasemman reunan x-koordinaatti.</param>
        /// <param name="bottom">Alueen alareunan y-koordinaatti.</param>
        /// <param name="right">Alueen oikean reunan x-koordinaatti.</param>
        /// <param name="top">Alueen yläreunan y-koordinaatti.</param>
        public void ZoomTo(double left, double bottom, double right, double top)
        {
            ZoomTo(left, bottom, right, top, 0);
        }

        internal void ZoomTo(double left, double bottom, double right, double top, double borderSize)
        {
            double screenWidth = (double)Game.Screen.Width;
            double screenHeight = (double)Game.Screen.Height;
            double width = right - left;
            double height = top - bottom;

            Position = new Vector(left + width / 2, bottom + height / 2);

            if ((width / height) >= (screenWidth / screenHeight))
                this.ZoomFactor = screenWidth / (width + borderSize);
            else
                this.ZoomFactor = screenHeight / (height + borderSize);
        }

        /// <summary>
        /// Zoomaa ja keskittää kameran siten, että koko kenttä on näkyvissä kerralla.
        /// </summary>
        public void ZoomToLevel()
        {
            ZoomToLevel(0);
        }

        /// <summary>
        /// Zoomaa ja keskittää kameran siten, että koko kenttä on näkyvissä kerralla. Tällöin kamera ei seuraa mitään oliota.
        /// </summary>
        /// <param name="borderSize">Reunalle jätettävä tila (jos negatiivinen, niin osa kentästä jää piiloon).</param>
        public void ZoomToLevel(double borderSize)
        {
            FollowedObject = null;
            Level level = Game.Instance.Level;
            ZoomTo(level.Left, level.Bottom, level.Right, level.Top, borderSize);
        }

        /// <summary>
        /// Ajetaan kun pelitilannetta päivitetään.
        /// </summary>
        /// <param name="time">Peliaika</param>
        internal void Update(Time time)
        {
            Position += Velocity * time.SinceLastUpdate.TotalSeconds;

            if (FollowedObject != null)
            {
                Vector center = ScreenToWorld(Vector.Zero);
                Vector worldOffset = ScreenToWorld(FollowOffset);

                // Update the average point if following multiple objects
                if (followedObjects != null)
                    UpdateAvgPoint();

                if (FollowsX && FollowsY)
                    Position = FollowedObject.Position + (worldOffset - center);
                else if (FollowsX)
                    X = FollowedObject.X + (worldOffset.X - center.X);
                else if (FollowsY)
                    Y = FollowedObject.Y + (worldOffset.Y - center.Y);
            }

            if (StayInLevel)
            {
                double screenWidth = (double)Game.Screen.Width;
                double screenHeight = (double)Game.Screen.Height;
                Level level = Game.Instance.Level;

                double zoomedWidth = level.Width * ZoomFactor;
                double zoomedHeight = level.Height * ZoomFactor;

                double viewAreaWidth = screenWidth / ZoomFactor;
                double viewAreaHeight = screenHeight / ZoomFactor;

                if (zoomedWidth < screenWidth || zoomedHeight < screenHeight)
                {
                    ZoomFactor = Math.Max(screenWidth / level.Width, screenHeight / level.Height);
                }

                if ((Position.X - (viewAreaWidth / 2)) < level.Left)
                {
                    _pos.X = level.Left + (viewAreaWidth / 2);
                }
                else if (Position.X + (viewAreaWidth / 2) > level.Right)
                {
                    _pos.X = level.Right - (viewAreaWidth / 2);
                }

                if (Position.Y - (viewAreaHeight / 2) < level.Bottom)
                {
                    _pos.Y = level.Bottom + (viewAreaHeight / 2);
                }
                else if (Position.Y + (viewAreaHeight / 2) > level.Top)
                {
                    _pos.Y = level.Top - (viewAreaHeight / 2);
                }
            }
        }
    }
}
