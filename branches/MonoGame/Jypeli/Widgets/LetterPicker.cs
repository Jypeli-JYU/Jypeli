using System;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    /// <summary>
    /// Kirjaimen valitsin.
    /// </summary>
    public class LetterPicker : Label
    {
        private int _selectedIndex;
        private char _selectedCharacter;
        private readonly SynchronousList<Listener> _controls = new SynchronousList<Listener>();

        private Touch _touch = null;
        private double _touchStart = 0;
        private double _touchEnd = 0;
        private double _touchVelocity = 0;

        /// <summary>
        /// Tapahtuu kun kirjainta muutetaan.
        /// </summary>
        public event Action<LetterPicker> LetterChanged;

        /// <summary>
        /// Merkit joita käytetään.
        /// </summary>
        public string Charset { get; set; }

        public override Font Font
        {
            get { return base.Font; }
            set { base.Font = value; UpdateSize(); }
        }

        public override Vector Size
        {
            get { return base.Size; }
            set { base.Size = value; UpdateSize(); }
        }

        /// <summary>
        /// Valitun merkin indeksi.
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = AdvMod( value, Charset.Length );
                _selectedCharacter = Charset[_selectedIndex];
                Text = _selectedCharacter.ToString();
                OnLetterChanged();
            }
        }

        /// <summary>
        /// Valittu merkki.
        /// </summary>
        public char SelectedCharacter
        {
            get { return _selectedCharacter; }
            set
            {
                SelectedIndex = Charset.IndexOf( value );
                _selectedCharacter = value;
                Text = value.ToString();
                OnLetterChanged();
            }
        }

        /// <summary>
        /// Nuoli ylöspäin.
        /// </summary>
        public Widget UpArrow { get; set; }

        /// <summary>
        /// Nuoli alaspäin.
        /// </summary>
        public Widget DownArrow { get; set; }

        /// <summary>
        /// Alustaa uuden kirjainvalitsimen.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="charset">Käytettävät merkit.</param>
        /// <param name="initialCharacter">Oletusmerkki</param>
        public LetterPicker( double width, double height, string charset = "", char initialCharacter = 'a' )
            : base( width, height )
        {
            Charset = ( charset.Length > 0 ) ? charset : Jypeli.Charset.Alphanumeric + " ";
            base.Font = Font.DefaultLarge;
            SelectedCharacter = initialCharacter;
            YMargin = 10;

            UpArrow = new Widget( this.Width, 10, Shape.Triangle );
            UpArrow.Top = this.Height / 2;
            UpArrow.Color = Color.Red;
            Add( UpArrow );

            DownArrow = new Widget( this.Width, 10, Shape.Triangle );
            DownArrow.Bottom = -this.Height / 2;
            DownArrow.Angle = Angle.StraightAngle;
            DownArrow.Color = Color.Red;
            Add( DownArrow );

            AddedToGame += AddControls;
            Removed += RemoveControls;
        }

        internal LetterPicker Clone()
        {
            // TODO: make a better clone method, maybe using the DataStorage load / save system
            var lpClone = new LetterPicker( this.Width, this.Height, this.Charset, this.SelectedCharacter );
            lpClone.Font = this.Font;
            lpClone.Color = this.Color;
            lpClone.BorderColor = this.BorderColor;
            lpClone.TextColor = this.TextColor;
            lpClone.TextScale = this.TextScale;
            lpClone.SizeMode = this.SizeMode;
            lpClone.UpArrow.Color = this.UpArrow.Color;
            lpClone.DownArrow.Color = this.DownArrow.Color;
            lpClone.UpArrow.Animation = this.UpArrow.Animation;
            lpClone.DownArrow.Animation = this.DownArrow.Animation;
            lpClone.UpArrow.Angle = this.UpArrow.Angle;
            lpClone.DownArrow.Angle = this.DownArrow.Angle;
            return lpClone;
        }

        private void UpdateSize()
        {
            UpArrow.Width = this.Width;
            DownArrow.Width = this.Width;

            UpArrow.Top = this.Height / 2;
            DownArrow.Bottom = -this.Height / 2;
        }

        private void OnLetterChanged()
        {
            if ( LetterChanged != null )
                LetterChanged( this );
        }

        private void AddControls()
        {
            _controls.Add( Game.TouchPanel.ListenOn( this, ButtonState.Pressed, StartDrag, null ).InContext( this ) );
            _controls.Add( Game.TouchPanel.Listen( ButtonState.Released, EndDrag, null ).InContext( this ) );
        }

        private void RemoveControls()
        {
            // SynchronousList removes every destroyed object automatically
            _controls.ForEach( c => c.Destroy() );
        }

        private void StartDrag( Touch touch )
        {
            if ( _touch != null )
                return;

            _touch = touch;
            _touchStart = touch.PositionOnScreen.Y;
        }

        private void EndDrag( Touch touch )
        {
            if ( _touch != touch )
                return;

            double delta = _touch != null ? ( _touch.PositionOnScreen.Y - _touchStart ) / TextSize.Y : 0;
            _touch = null;
            _touchEnd = touch.PositionOnScreen.Y;

            if ( touch.MovementOnScreen.Y < 1 )
            {
                // Set the index now
                SelectedIndex += (int)Math.Round( delta );
                _touchStart = _touchEnd = 0;
            }
            else
            {
                // Continue scrolling
                _touchVelocity = 100 * touch.MovementOnScreen.Y;
            }
        }

        private int AdvMod( int x, int n )
        {
            if ( n <= 0 ) return -1;
            while ( x < 0 ) x += n;
            while ( x >= n ) x -= n;
            return x;
        }

        public override void Update( Time time )
        {
            if ( _touch != null )
                _touchEnd = _touch.PositionOnScreen.Y;

            else if ( _touchVelocity > 0 )
            {
                _touchEnd += Math.Sign( _touchEnd ) * _touchVelocity * time.SinceLastUpdate.TotalSeconds;
                _touchVelocity *= 0.95;

                if ( _touchVelocity < 1.5 * TextSize.Y )
                {
                    double delta = ( _touchEnd - _touchStart ) / TextSize.Y;
                    SelectedIndex += (int)Math.Round( delta );
                    _touchStart = _touchEnd = 0;
                    _touchVelocity = 0;
                }
            }

            _controls.UpdateChanges();
            base.Update( time );
        }

        public override void Draw( Matrix parentTransformation, Matrix transformation )
        {
            if ( _touchStart == 0 && _touchEnd == 0 )
            {
                base.Draw( parentTransformation, transformation );
                return;
            }

            double delta = ( _touchEnd - _touchStart ) / TextSize.Y;
            int indexDelta = (int)Math.Round( delta );
            double yDelta = delta - indexDelta;  // for smooth scrolling

            for ( int i = -1; i <= 1; i++ )
            {
                Matrix m = Matrix.CreateScale( TextScale )
                           * Matrix.CreateRotationZ( (float)-Angle.Radians )
                           * Matrix.CreateTranslation( (float)Position.X, (float)( Position.Y - ( i - yDelta ) * TextSize.Y ), 0 )
                           * parentTransformation;

                int ci = AdvMod( this.SelectedIndex + indexDelta + i, this.Charset.Length );
                Renderer.DrawText( Charset[ci].ToString(), ref m, Font, TextColor );
            }
        }
    }
}
