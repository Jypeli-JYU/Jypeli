using System;
using System.Collections.Generic;
using Jypeli.Controls;
using Jypeli.GameObjects;

namespace Jypeli
{
    /// <summary>
    /// Ikkuna, joka antaa käyttäjän valita yhden annetuista vaihtoehdoista.
    /// </summary>
    public class MultiSelectWindow : Window
    {
        static readonly Key[] keys = { Key.D1, Key.D2, Key.D3, Key.D4, Key.D5, Key.D6, Key.D7, Key.D8, Key.D9, Key.D0 };

        private int _defaultCancel = 0;
        private List<Listener> _defaultListeners = new List<Listener>(4);

        private int _selectedIndex = -1;
        private bool _buttonColorSet = false;
        private Font _font;

        /// <summary>
        /// Kysymys.
        /// </summary>
        public Label QuestionLabel;

        /// <summary>
        /// Painonappulat järjestyksessä.
        /// </summary>
        public PushButton[] Buttons { get; private set; }

        /// <summary>
        /// Fontti.
        /// </summary>
        public Font Font
        {
            get
            {
                return this._font;
            }
            set
            {
                QuestionLabel.Font = value;

                for ( int i = 0; i < Buttons.Length; i++ )
                {
                    Buttons[i].Font = value;
                }
            }
        }

        /// <summary>
        /// Mitä valitaan kun käyttäjä painaa esc tai takaisin-näppäintä.
        /// Laittomalla arvolla (esim. negatiivinen) em. näppäimistä ei tapahdu mitään.
        /// </summary>
        public int DefaultCancel
        {
            get { return _defaultCancel; }
            set
            {
                _defaultCancel = value;
                if ( IsAddedToGame ) AddDefaultControls();
            }
        }

        /// <summary>
        /// Kuinka mones nappula on valittuna (alkaa nollasta)
        /// </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                if ( !IsAddedToGame ) RememberSelection = true;
                SelectButton( value );
            }
        }

        /// <summary>
        /// Valittu nappula.
        /// </summary>
        public PushButton SelectedButton
        {
            get { return _selectedIndex >= 0 || _selectedIndex >= Buttons.Length ? Buttons[_selectedIndex] : null; }
        }

        /// <summary>
        /// Nappulan oletusväri.
        /// </summary>
        public override Color Color
        {
            get
            {
                return base.Color;
            }
            set
            {
                if ( !_buttonColorSet ) _setButtonColor( Color.Darker( value, 40 ) );
                base.Color = value;
            }
        }

        /// <summary>
        /// Valitun nappulan väri.
        /// </summary>
        public Color SelectionColor
        {
            get { return SelectedButton.Color; }
            set
            {
                SelectedButton.Color = value;
                SelectButton( _selectedIndex );
            }
        }

        /// <summary>
        /// Muistetaanko missä kohtaa kursori oli viime kerralla kun ikkuna näytettiin.
        /// </summary>
        public bool RememberSelection { get; set; }

        /// <summary>
        /// Tapahtuma joka tapahtuu kun nappia painetaan.
        /// Ottaa parametrikseen painonapin indeksin (alkaen nollasta).
        /// </summary>
        public event Action<int> ItemSelected;

        /// <summary>
        /// Luo uuden monivalintaikkunan.
        /// </summary>
        /// <param name="question">Kysymys.</param>
        /// <param name="buttonTexts">Nappien tekstit merkkijonoina.</param>
        public MultiSelectWindow( string question, params string[] buttonTexts )
            : base()
        {
            if ( buttonTexts.Length == 0 ) throw new InvalidOperationException( "You must add at least one button" );

            VerticalScrollLayout layout = new VerticalScrollLayout();
            layout.LeftPadding = layout.RightPadding = 20;
            layout.BottomPadding = 30;
            layout.Spacing = 20;
            this.Layout = layout;

            QuestionLabel = new Label( question );
            Add( QuestionLabel );

            Buttons = new PushButton[buttonTexts.Length];
            this._font = Font.Default;
            for ( int i = 0; i < buttonTexts.Length; i++ )
            {
                PushButton button = new PushButton( buttonTexts[i] );
                button.Tag = i;
                button.Clicked += new Action( delegate { ButtonClicked( (int)button.Tag ); } );
#if WINDOWS_PHONE
                if ( Game.Instance.Phone.DisplayResolution == DisplayResolution.Large )
                    button.TextScale = new Vector(2, 2);
                else if ( Game.Instance.Phone.DisplayResolution == DisplayResolution.HD720 )
                    button.TextScale = new Vector( 3, 3 );
#endif
                Add( button );
                Buttons[i] = button;
            }

            AddedToGame += InitOnAdd;
            Removed += DeinitOnRemove;
        }

        private void InitOnAdd()
        {
            AddControls();
            AddDefaultControls();
#if !WINDOWS_PHONE && !ANDROID
            SelectButton( ( RememberSelection && _selectedIndex >= 0 ) ? _selectedIndex : 0 );
#endif
        }

        private void DeinitOnRemove()
        {
            _defaultListeners.ForEach(l => l.Destroy());
            _defaultListeners.Clear();
        }

        private void SelectButton( int p )
        {
            UnselectButton();
            if ( p < 0 || p >= Buttons.Length ) return;

            _selectedIndex = p;
            SelectedButton.SetState(PushButton.State.Selected);
        }

        private void UnselectButton()
        {
            if ( _selectedIndex < 0 ) return;
            SelectedButton.SetState(PushButton.State.Released);
            _selectedIndex = -1;
        }

        public void AddItemHandler( int item, Action handler )
        {
            Buttons[item].Clicked += handler;
        }

        public void AddItemHandler<T1>( int item, Action<T1> handler, T1 p1 )
        {
            Buttons[item].Clicked += delegate { handler( p1 ); };
        }

        public void AddItemHandler<T1, T2>( int item, Action<T1, T2> handler, T1 p1, T2 p2 )
        {
            Buttons[item].Clicked += delegate { handler( p1, p2 ); };
        }

        public void AddItemHandler<T1, T2, T3>( int item, Action<T1, T2, T3> handler, T1 p1, T2 p2, T3 p3 )
        {
            Buttons[item].Clicked += delegate { handler( p1, p2, p3 ); };
        }

        public void RemoveItemHandler( int item, Action handler )
        {
            Buttons[item].Clicked -= handler;
        }

        private void _setButtonColor( Color color )
        {
            for ( int i = 0; i < Buttons.Length; i++ )
            {
                Buttons[i].Color = color;
            }
            SelectButton( _selectedIndex );
        }

        public void SetButtonColor( Color color )
        {
            _setButtonColor( color );
            _buttonColorSet = true;
        }

        public void SetButtonTextColor( Color color )
        {
            for ( int i = 0; i < Buttons.Length; i++ )
            {
                Buttons[i].TextColor = color;
            }
        }

        private void AddControls()
        {
            var Keyboard = Game.Instance.Keyboard;

            for ( int i = 0; i < Math.Min( Buttons.Length, keys.Length ); i++ )
            {
                var l = Keyboard.Listen(keys[i], ButtonState.Pressed, SelectButton, null, i).InContext(this);
                associatedListeners.Add(l);
            }

            Action selectPrev = delegate { SelectButton( _selectedIndex > 0 ? _selectedIndex - 1 : Buttons.Length - 1 ); };
            Action selectNext = delegate { SelectButton( _selectedIndex < Buttons.Length - 1 ? _selectedIndex + 1 : 0 ); };
            Action confirmSelect = delegate { SelectedButton.Click(); };

            var l1 = Keyboard.Listen( Key.Up, ButtonState.Pressed, selectPrev, null ).InContext( this );
            var l2 = Keyboard.Listen( Key.Down, ButtonState.Pressed, selectNext, null ).InContext( this );
            var l3 = Keyboard.Listen( Key.Enter, ButtonState.Pressed, confirmSelect, null ).InContext( this );
            associatedListeners.AddItems(l1, l2, l3);

            foreach ( var controller in Game.Instance.GameControllers )
            {
                l1 = controller.Listen( Button.DPadUp, ButtonState.Pressed, selectPrev, null ).InContext( this );
                l2 = controller.Listen( Button.DPadDown, ButtonState.Pressed, selectNext, null ).InContext( this );
                l3 = controller.Listen( Button.A, ButtonState.Pressed, confirmSelect, null ).InContext( this );
                associatedListeners.AddItems(l1, l2, l3);
            }
        }

        private void AddDefaultControls()
        {
            _defaultListeners.ForEach(l => l.Destroy());
            _defaultListeners.Clear();

            if ( _defaultCancel >= 0 && _defaultCancel < Buttons.Length )
            {
                var l1 = Game.Instance.PhoneBackButton.Listen( Buttons[_defaultCancel].Click, null ).InContext( this );
                var l2 = Game.Instance.Keyboard.Listen( Key.Escape, ButtonState.Pressed, Buttons[_defaultCancel].Click, null ).InContext( this );
                var l3 = Game.Instance.ControllerOne.Listen( Button.B, ButtonState.Pressed, Buttons[_defaultCancel].Click, null ).InContext( this );
                _defaultListeners.AddItems(l1, l2, l3);
            }
        }

        private void ButtonClicked( int index )
        {
            if ( ItemSelected != null )
                ItemSelected( index );

            Close();
        }
    }
}
