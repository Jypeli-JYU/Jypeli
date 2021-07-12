using Microsoft.Xna.Framework;
using System;
using System.ComponentModel;

namespace Jypeli.GameObjects
{
    /// <summary>
    /// Kaikille peliolioille yhteinen kantaluokka
    /// </summary>
    public abstract class GameObjectBase : Tagged, Destroyable
    {
        private Layer _layer = null;
        private Brain _brain = Brain.None;
        private TimeSpan _maxLifetime = TimeSpan.MaxValue;
        private bool _rotateImage = true;

        /// <summary>
        /// Peli.
        /// </summary>
        public Game Game
        {
            get { return Game.Instance; }
        }

        /// <summary>
        /// Kerros, jolle peliolio on lisätty.
        /// </summary>
        public Layer Layer
        {
            get
            {
                if ( Parent != null ) return Parent.Layer;
                return _layer;
            }
            set
            {
                _layer = value;
            }
        }

        /// <summary>
        /// Pyöritetäänkö kuvaa olion kulman mukana.
        /// </summary>
        public bool RotateImage
        {
            get { return _rotateImage; }
            set { _rotateImage = value; }
        }

        /// <summary>
        /// Olio, jonka lapsiolio tämä olio on. Jos <c>null</c>, olio ei
        /// ole minkään olion lapsiolio.
        /// </summary>
        public IGameObject Parent { get; set; }

        /// <summary>
        /// Tarvitseeko olio päivittämistä. Kun perit oman luokkasi
        /// tästä luokasta, aseta tämä arvoon <c>true</c>, kun luokan olio
        /// tarvitsee päivittämistä.
        /// </summary>
        /// <remarks>
        /// Jos tämä asetetaan arvoon <code>true</code>, olion <c>Update</c>-metodia
        /// kutsutaan säännöllisesti (noin 25 kertaa sekunnissa) sen jälkeen, kun olio
        /// on lisätty peliin.
        /// 
        /// Tämä ei ole oletuksena <c>true</c>, jotta vältetään pelin hidastuminen
        /// turhien päivityskutsujen takia. Jos esimerkiksi kenttä koostuu tuhansista
        /// seinäpalikoista, näiden kaikkien päivittäminen koko ajan olisi turhaa.
        /// </remarks>
        public bool IsUpdated { get; set; }

        /// <summary>
        /// Vapaasti asetettava muuttuja.
        /// </summary>
        /// <example>
        /// Tagia voi käyttää vaikkapa olion keräämien pisteiden tallettamiseen:
        /// <code>
        /// hahmo.Tag = 5;
        /// </code>
        /// Kun luet pisteitä, täytyy arvo muuntaa oikean tyyppiseksi kertomalla tyypin
        /// nimi suluissa:
        /// <code>
        /// int pisteitä = (int)hahmo.Tag;
        /// </code>
        /// </example>
        public object Tag { get; set; }

        /// <summary>
        /// Olion aivot.
        /// Ks. <c>Brain</c>
        /// </summary>
        public virtual Brain Brain
        {
            get
            {
                return _brain;
            }
            set
            {
                _brain.Owner = null;
                _brain = ( ( value != null ) ? value : Brain.None );
                _brain.Owner = (IGameObject)this;
                _brain.AddToGameEvent();
                if ( value != null )
                    IsUpdated = true;
            }
        }

        /// <summary>
        /// Tapahtuu, kun olio lisätään peliin.
        /// </summary>
        public event Action AddedToGame;

        /// <summary>
        /// Kutsutaan kun olio lisätään peliin.
        /// </summary>
        public void OnAddedToGame()
        {
            AddedToGame?.Invoke();
            Brain.AddToGameEvent();
        }

        /// <summary>
        /// Tapahtuu, kun olio poistetaan pelistä (tuhotaan tai ei).
        /// </summary>
        public event Action Removed;

        /// <summary>
        /// Kutsutaan kun olio poistetaan pelistä.
        /// </summary>
        public void OnRemoved()
        {
            if ( Removed != null )
                Removed();
        }

        /// <summary>
        /// Onko olio tuhottu.
        /// </summary>
        public bool IsDestroyed { get; private set; }

        /// <summary>
        /// Tapahtuu, kun olio tuhotaan. 
        /// </summary> 
        public event Action Destroyed;

        /// <summary>
        /// Kun olio tuhotaan
        /// </summary>
        protected void OnDestroyed()
        {
            if ( Destroyed != null )
                Destroyed();
        }

        /// <summary>
        /// Tuhoaa olion.
        /// </summary>
        public virtual void Destroy()
        {
            if ( IsDestroyed )
                return;

            IsDestroyed = true;
            OnDestroyed();
        }

        /// <summary>
        /// Olion koko pelimaailmassa.
        /// Kertoo olion äärirajat, ei muotoa.
        /// </summary>
        [Save] public abstract Vector Size { get; set; }

        /// <summary>
        /// Olion paikka. Jos olio on jonkun toisen peliolion lapsi, ks. myös (<c>RelativePosition</c>).
        /// </summary>
        [Save] public virtual Vector Position { get; set; }

        /// <summary>
        /// Olion kulma tai rintamasuunta.
        /// Nolla = osoittaa oikealle.
        /// </summary>      
        [Save] public abstract Angle Angle { get; set; }

        /// <summary>
        /// Olion luomisaika.
        /// </summary>
        public TimeSpan CreationTime { get; protected set; }

        /// <summary>
        /// Olion elinaika. Lasketaan siitä lähtien, kun olio luodaan.
        /// </summary>
        public TimeSpan Lifetime
        {
            get { return Game.Time.SinceStartOfGame - CreationTime; }
        }

        /// <summary>
        /// Olion suurin mahdollinen elinaika.
        /// Kun <c>Lifetime</c> on suurempi kuin tämä, olio kuolee.
        /// </summary>
        public TimeSpan MaximumLifetime
        {
            get { return _maxLifetime; }
            set
            {
                _maxLifetime = value;
                IsUpdated = true;
            }
        }

        /// <summary>
        /// Olion jäljellä oleva elinaika.
        /// </summary>
        [Save]
        public TimeSpan LifetimeLeft
        {
            get { return MaximumLifetime - Lifetime; }
            set
            {
                try
                {
                    MaximumLifetime = Lifetime + value;
                }
                catch ( OverflowException )
                {
                    MaximumLifetime = TimeSpan.MaxValue;
                }
            }
        }

        /// <summary>
        /// Olion leveys (X-suunnassa, leveimmässä kohdassa).
        /// </summary>
        public double Width
        {
            get { return Size.X; }
            set { Size = new Vector( value, Size.Y ); }
        }

        /// <summary>
        /// Olion korkeus (Y-suunnassa, korkeimmassa kohdassa).
        /// </summary>
        public double Height
        {
            get { return Size.Y; }
            set { Size = new Vector( Size.X, value ); }
        }

        internal Vector InitialRelativePosition { get; set; }
        internal Angle InitialRelativeAngle { get; set; }

        /// <summary>
        /// Olion paikka suhteessa sen isäntäolioon.
        /// </summary>
        public Vector RelativePosition
        {
            get
            {
                if (Parent != null)
                {
                    Vector diff = Position - Parent.Position;
                    Angle angle = diff.Angle;
                    return Vector.FromLengthAndAngle(diff.Magnitude, angle - Parent.Angle);
                }
                return Position;
            }
            set
            {
                if (Parent != null)
                    Position = value.Transform(Matrix.CreateRotationZ((float)Parent.Angle.Radians) * Matrix.CreateTranslation(Parent.Position));
                else
                    Position = value;
            }
        }

        /// <summary>
        /// Sijainti suhteessa vanhimpaan vanhempaan
        /// </summary>
        public Vector RelativePositionToMainParent
        {
            get
            {
                if (Parent != null)
                {
                    GameObject mainParent = ((GameObject)this).GetMainParent();
                    Vector diff = Position - mainParent.Position;
                    Angle angle = diff.Angle;
                    return Vector.FromLengthAndAngle(diff.Magnitude, angle - mainParent.Angle);
                }
                return RelativePosition;
            }
            set
            {
                GameObject mainParent = ((GameObject)this).GetMainParent();
                if (Parent != null)
                    Position = value.Transform(Matrix.CreateRotationZ((float)mainParent.Angle.Radians) * Matrix.CreateTranslation(mainParent.Position));
                else
                    Position = value;
            }
        }

        /// <summary>
        /// Olion kulma suhteessa vanhempaan.
        /// Jos olio ei ole minkään toisen peliolion lapsiolio,
        /// tämä on sama kuin <c>Angle</c>.
        /// </summary>
        public Angle RelativeAngle
        {
            get
            {
                if ( Parent != null )
                    return this.Angle - Parent.Angle;
                return Angle;
            }
            set
            {
                if (Parent != null)
                    Angle = Parent.Angle + value;
                else Angle = value;
            }
        }

        /// <summary>
        /// Olion kulma suhteessa vanhimpaan vanhempaan.
        /// Jos olio ei ole minkään toisen peliolion lapsiolio,
        /// tämä on sama kuin <c>Angle</c>.
        /// </summary>
        public Angle RelativeAngleToMainParent
        {
            get
            {
                if (Parent != null)
                {
                    GameObject mainParent = ((GameObject)this).GetMainParent();
                    return this.Angle - mainParent.Angle;
                }
                return RelativeAngle;
            }
            set
            {
                if (Parent != null)
                {
                    GameObject mainParent = ((GameObject)this).GetMainParent();
                    Angle = mainParent.Angle + value;
                }
                else
                    Angle = value;
            }
        }

        /// <summary>
        /// Olion vasemman reunan x-koordinaatti.
        /// </summary>
        public double Left
        {
            get { return Position.X - 0.5 * ( Size.Y * Math.Abs( Angle.Sin ) + Size.X * Math.Abs( Angle.Cos ) ); }
            set { Position = new Vector( value + Size.X / 2, Position.Y ); }
        }

        /// <summary>
        /// Olion oikean reunan x-koordinaatti.
        /// </summary>
        public double Right
        {
            get { return Position.X + 0.5 * ( Size.Y * Math.Abs( Angle.Sin ) + Size.X * Math.Abs( Angle.Cos ) ); }
            set { Position = new Vector( value - Size.X / 2, Position.Y ); }
        }

        /// <summary>
        /// Olion yläreunan y-koordinaatti.
        /// </summary>
        public double Top
        {
            get { return Position.Y + 0.5 * ( Size.X * Math.Abs( Angle.Sin ) + Size.Y * Math.Abs( Angle.Cos ) ); }
            set { Position = new Vector( Position.X, value - Size.Y / 2 ); }
        }

        /// <summary>
        /// Olion alareunan y-koordinaatti.
        /// </summary>
        public double Bottom
        {
            get { return Position.Y - 0.5 * ( Size.X * Math.Abs( Angle.Sin ) + Size.Y * Math.Abs( Angle.Cos ) ); }
            set { Position = new Vector( Position.X, value + Size.Y / 2 ); }
        }

        /// <summary>
        /// Olion vasemman reunan suhteellinen x-koordinaatti.
        /// </summary>
        public double RelativeLeft
        {
            get { return RelativePosition.X - (0.5 * (Size.Y * Math.Abs(RelativeAngle.Sin) + Size.X * Math.Abs(RelativeAngle.Cos))); }
            set { RelativePosition = new Vector(value + (Size.X / 2), RelativePosition.Y); }
        }

        /// <summary>
        /// Olion oikean reunan suhteellinen x-koordinaatti.
        /// </summary>
        public double RelativeRight
        {
            get { return RelativePosition.X + (0.5 * (Size.Y * Math.Abs(RelativeAngle.Sin) + Size.X * Math.Abs(RelativeAngle.Cos))); }
            set { RelativePosition = new Vector(value - (Size.X / 2), RelativePosition.Y); }
        }

        /// <summary>
        /// Olion yläreunan suhteellinen y-koordinaatti.
        /// </summary>
        public double RelativeTop
        {
            get { return RelativePosition.Y + 0.5 * (Size.X * Math.Abs(RelativeAngle.Sin) + Size.Y * Math.Abs(RelativeAngle.Cos)); }
            set { RelativePosition = new Vector(RelativePosition.X, value - Size.Y / 2); }
        }

        /// <summary>
        /// Olion alareunan suhteellinen y-koordinaatti.
        /// </summary>
        public double RelativeBottom
        {
            get { return RelativePosition.Y - (0.5 * (Size.X * Math.Abs(RelativeAngle.Sin) + Size.Y * Math.Abs(RelativeAngle.Cos))); }
            set { RelativePosition = new Vector(RelativePosition.X, value + (Size.Y / 2)); }
        }

        /// <summary>
        /// Olion paikan X-koordinaatti.
        /// </summary>
        public double X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Vector( value, Position.Y );
            }
        }

        /// <summary>
        /// Olion paikan Y-koordinaatti.
        /// </summary>
        public double Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Vector( Position.X, value );
            }
        }

        /// <summary>
        /// Olion koordinaatiston X-yksikkökantavektori.
        /// </summary>
        public Vector UnitX
        {
            get { return Vector.FromAngle( Angle ); }
        }

        /// <summary>
        /// Olion koordinaatiston Y-yksikkökantavektori.
        /// </summary>
        public Vector UnitY
        {
            get { return UnitX.LeftNormal; }
        }

        /// <summary>
        /// Olion koordinaatiston suhteellinen X-yksikkökantavektori.
        /// </summary>
        public Vector RelativeUnitX
        {
            get { return Vector.FromAngle( RelativeAngle ); }
        }

        /// <summary>
        /// Olion koordinaatiston suhteellinen Y-yksikkökantavektori.
        /// </summary>
        public Vector RelativeUnitY
        {
            get { return RelativeUnitX.LeftNormal; }
        }

        /// <summary>
        /// Animaatio. Voi olla <c>null</c>, jolloin piirretään vain väri.
        /// </summary>
        public abstract Animation Animation { get; set; }

        /// <summary>
        /// Olion kuva. Voi olla <c>null</c>, jolloin piirretään vain väri.
        /// </summary>
        public Image Image
        {
            get
            {
                if ( Animation != null )
                    return Animation.CurrentFrame;
                return null;
            }
            set
            {
                Animation = value;
            }
        }

        /// <summary>
        /// Kaikille peliobjekteille yhteinen kantaluokka
        /// </summary>
        protected GameObjectBase()
        {
            this.CreationTime = Game.Time.SinceStartOfGame;
            this.Tag = "";
        }

        /// <summary>
        /// Yrittää siirtyä annettuun paikkaan annetulla nopeudella.
        /// Laukaisee annetun aliohjelman, kun paikkaan on päästy.
        /// </summary>
        /// <param name="location">Paikka johon siirrytään</param>
        /// <param name="speed">
        /// Nopeus (paikkayksikköä sekunnissa) jolla liikutaan.
        /// Nopeus on maksiminopeus. Jos välissä on hitaampaa maastoa tai
        /// esteitä, liikkumisnopeus voi olla alle sen.
        /// </param>
        /// <param name="doWhenArrived">
        /// Aliohjelma, joka ajetaan kun paikkaan on päästy.
        /// Voi olla null, jos ei haluta mitään aliohjelmaa.
        /// </param>
        public abstract void MoveTo( Vector location, double speed, Action doWhenArrived );

        /// <summary>
        /// Yrittää siirtyä annettuun paikkaan annetulla nopeudella.
        /// </summary>
        /// <param name="location">Paikka johon siirrytään</param>
        /// <param name="speed">
        /// Nopeus (paikkayksikköä sekunnissa) jolla liikutaan.
        /// Nopeus on maksiminopeus. Jos välissä on hitaampaa maastoa tai
        /// esteitä, liikkumisnopeus voi olla alle sen.
        /// </param>
        public void MoveTo( Vector location, double speed )
        {
            MoveTo( location, speed, null );
        }

        /// <summary>
        /// Peliolion päivitys. Tätä kutsutaan, kun <c>IsUpdated</c>-ominaisuuden
        /// arvoksi on asetettu <c>true</c> ja olio on lisätty peliin.
        /// <see cref="IsUpdated"/>
        /// </summary>
        /// <param name="time">Peliaika.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public virtual void Update( Time time )
        {
            if ( IsDestroyed )
                return;

            if ( Lifetime > MaximumLifetime )
            {
                Destroy();
                return;
            }

            Brain.DoUpdate( time );
        }
    }
}
