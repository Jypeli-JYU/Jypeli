﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            if ( AddedToGame != null )
                AddedToGame();
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
        /// Olion paikka. Jos olio on jonkun toisen peliolion lapsi, paikka on suhteessa
        /// tämän vanhempaan (<c>Parent</c>). Muuten paikka on paikka pelimaailmassa.
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

        /// <summary>
        /// Olion absoluuttinen paikka pelimaailmassa.
        /// Jos olio ei ole minkään toisen peliolion lapsiolio,
        /// tämä on sama kuin <c>Position</c>.
        /// </summary>
        public Vector AbsolutePosition
        {
            get
            {
                if ( Parent != null )
                    return Parent.AbsolutePosition + this.Position;
                return Position;
            }
            set
            {
                if ( Parent != null )
                    Position = value - Parent.AbsolutePosition;
                else
                    Position = value;
            }
        }

        /// <summary>
        /// Olion absoluuttinen kulma pelimaailmassa.
        /// Jos olio ei ole minkään toisen peliolion lapsiolio,
        /// tämä on sama kuin <c>Angle</c>.
        /// </summary>
        public Angle AbsoluteAngle
        {
            get
            {
                if ( Parent != null )
                    return Parent.AbsoluteAngle + this.Angle;
                return Angle;
            }
            set
            {
                if ( Parent != null )
                    Angle = value - Parent.AbsoluteAngle;
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
        /// Olion vasemman reunan absoluuttinen x-koordinaatti.
        /// </summary>
        public double AbsLeft
        {
            get { return AbsolutePosition.X - 0.5 * ( Size.Y * Math.Abs( Angle.Sin ) + Size.X * Math.Abs( Angle.Cos ) ); }
        }

        /// <summary>
        /// Olion oikean reunan absoluuttinen x-koordinaatti.
        /// </summary>
        public double AbsRight
        {
            get { return AbsolutePosition.X + 0.5 * ( Size.Y * Math.Abs( Angle.Sin ) + Size.X * Math.Abs( Angle.Cos ) ); }
        }

        /// <summary>
        /// Olion yläreunan absoluuttinen y-koordinaatti.
        /// </summary>
        public double AbsTop
        {
            get { return AbsolutePosition.Y + 0.5 * ( Size.X * Math.Abs( Angle.Sin ) + Size.Y * Math.Abs( Angle.Cos ) ); }
        }

        /// <summary>
        /// Olion alareunan absoluuttinen y-koordinaatti.
        /// </summary>
        public double AbsBottom
        {
            get { return AbsolutePosition.Y - 0.5 * ( Size.X * Math.Abs( Angle.Sin ) + Size.Y * Math.Abs( Angle.Cos ) ); }
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
