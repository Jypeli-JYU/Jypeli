using System;

namespace Jypeli.Widgets
{
    /// <summary>
    /// Widget, joka voidaan asettaa näyttämään halutun mittarin arvoa.
    /// </summary>
    public abstract class BindableWidget : Widget
    {
        private bool updateSet = false;

        /// <summary>
        /// Mittari, jonka arvoa kontrolli seuraa.
        /// Jos kontrollia ei ole kiinnitetty mittariin, se käyttää omaa sisäistä mittariaan.
        /// </summary>
        public Meter Meter { get; private set; }

        /// <summary>
        /// Onko komponentti sidottu mittariin.
        /// </summary>
        public bool Bound { get; private set; }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="animation"></param>
        public BindableWidget( Animation animation )
            : base( animation )
        {
            CreateInnerMeter();
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public BindableWidget( double width, double height )
            : base( width, height )
        {
            CreateInnerMeter();
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="shape"></param>
        public BindableWidget( double width, double height, Shape shape )
            : base( width, height, shape )
        {
            CreateInnerMeter();
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="layout"></param>
        public BindableWidget( ILayout layout )
            : base( layout )
        {
            CreateInnerMeter();
        }

        private void CreateInnerMeter()
        {
            Bound = false;
            UnsetChangedEvent();
            Meter = new DoubleMeter( 0, 0, 100 );
            SetChangedEvent();
            AddedToGame += UpdateValue;
        }

        /// <summary>
        /// Asettaa tapahtuman, joka reagoi Meter.Value muutokseen kutsumalla UpdateValue-metodia.
        /// </summary>
        protected void SetChangedEvent()
        {
            if ( updateSet ) return;

            if ( Meter is IntMeter ) ( (IntMeter)Meter ).Changed += UpdateIntValue;
            else if ( Meter is DoubleMeter ) ( (DoubleMeter)Meter ).Changed += UpdateDoubleValue;
            else throw new InvalidOperationException( "Meter is of unknown type!" );

            updateSet = true;
        }

        /// <summary>
        /// Poistaa käytöstä tapahtuman, joka reagoi Meter.Value muutokseen kutsumalla UpdateValue-metodia.
        /// Käytä tätä, kun haluat asettaa mittarin arvon kontrollin sisällä.
        /// Älä unohda kutsua SetChangedEvent muutoksen jälkeen!
        /// </summary>
        protected void UnsetChangedEvent()
        {
            if ( !updateSet ) return;

            if ( Meter is IntMeter ) ( (IntMeter)Meter ).Changed -= UpdateIntValue;
            else if ( Meter is DoubleMeter ) ( (DoubleMeter)Meter ).Changed -= UpdateDoubleValue;
            else throw new InvalidOperationException( "Meter is of unknown type!" );

            updateSet = false;
        }

        /// <summary>
        /// Asettaa kontrollin seuraamaan mittarin arvoa.
        /// </summary>
        public virtual void BindTo( Meter meter )
        {
            UnsetChangedEvent();
            Meter = meter;
            Bound = true;
            SetChangedEvent();
            UpdateValue();
        }

        /// <summary>
        /// Lopettaa mittarin arvon seuraamisen.
        /// </summary>
        public virtual void Unbind()
        {
            CreateInnerMeter();
        }

        private void UpdateIntValue( int oldValue, int newValue )
        {
            UpdateValue();
        }

        private void UpdateDoubleValue( double oldValue, double newValue )
        {
            UpdateValue();
        }

        /// <summary>
        /// Kutsutaan automaattisesti, kun mittarin arvo on muuttunut.
        /// Ylikirjoita tämä koodilla, joka muuttaa widgetin ulkonäköä asianmukaisesti.
        /// </summary>
        protected abstract void UpdateValue();
    }
}
