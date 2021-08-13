using System.ComponentModel;
using System.Diagnostics;

namespace Jypeli
{
    public partial class Game
    {
        // Real time passed, including paused time
        private static Time currentRealTime = new Time();

        // Game time passed
        private static Time currentTime = new Time();

        /// <summary>
        /// Ajetaanko peliä kiinteällä aika-askeleella.
        /// Eli pelin aika "hidastuu" jos tietokoneen tehot eivät riitä reaaliaikaiseen päivitykseen.
        /// </summary>
        public static bool FixedTimeStep { get; set; }

        /// <summary>
        /// Onko peli pysähdyksissä.
        /// </summary>
        public bool IsPaused { get; set; }

        /// <summary>
        /// Peliaika. Sisältää tiedon siitä, kuinka kauan peliä on pelattu (Time.SinceStartOfGame)
        /// ja kuinka kauan on viimeisestä pelin päivityksestä (Time.SinceLastUpdate).
        /// Tätä päivitetään noin 30 kertaa sekunnissa kun peli ei ole pause-tilassa.
        /// </summary>
        public static Time Time
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Todellinen peliaika. Sisältää tiedon siitä, kuinka kauan peliä on pelattu (Time.SinceStartOfGame)
        /// ja kuinka kauan on viimeisestä pelin päivityksestä (Time.SinceLastUpdate).
        /// Tätä päivitetään noin 30 kertaa sekunnissa, myös pause-tilassa.
        /// </summary>
        public static Time RealTime
        {
            get { return currentTime; }
        }

        /// <summary>
        /// Asettaa pelin pauselle, tai jatkaa peliä.
        /// Toimii samoin kuin IsPaused-ominaisuus
        /// </summary>
        public void Pause()
        {
            IsPaused = !IsPaused;
        }

        /// <summary>
        /// Poistaa kaikki ajastimet.
        /// </summary>
        public void ClearTimers()
        {
            Timer.ClearAll();
        }

        /// <summary>
        /// Ajetaan Updaten sijaan kun peli on pysähdyksissä.
        /// </summary>
        protected virtual void PausedUpdate( Time time )
        {
            foreach ( var layer in Layers )
            {
                // Update the UI components only
                layer.Objects.Update( time, o => o is Widget );
            }

            Timer.UpdateAll( time, t => t.IgnorePause );
        }

        protected void Update(double dt){
            //Debug.Write(dt);
            //Debug.Write("     ");
            if(FixedTimeStep)
                currentTime.Advance(1 / 60.0);
            else
                currentTime.Advance(dt);
            this.Update(currentTime);
        }

        protected void OnDraw(double dt)
        {
            //Debug.WriteLine(dt);
            Draw(Time);
        }

        /// <summary>
        /// Ajetaan kun pelin tilannetta päivitetään. Päivittämisen voi toteuttaa perityssä luokassa
        /// toteuttamalla tämän metodin. Perityn luokan metodissa tulee kutsua kantaluokan metodia.
        /// </summary>
        protected virtual void Update(Time time)
        {
            UpdateControls(time);
            this.Camera.Update(time);
            Layers.Update(time);
            Timer.UpdateAll(time);
            UpdateDebugScreen(currentRealTime);
            UpdateHandlers(time);
            ExecutePendingActions();
        }
        /*
        /// <summary>
        /// Ajetaan kun pelin tilannetta päivitetään.
        /// </summary>
        /// <param name="gameTime"></param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected override void Update( Time time )
        {
            if ( !loadContentHasBeenCalled || !beginHasBeenCalled )
            {
                // No updates until both LoadContent and Begin have been called
                return;
            }

            currentRealTime.Advance( gameTime );

#if ANDROID
            if (IsActive && !VirtualKeyboard.Visible)
#else
            if (IsActive)
#endif
                UpdateControls( currentTime );

            /*if ( DataStorage.IsUpdated )
                DataStorage.Update( currentRealTime );*/

            // The update in derived classes.
            /*
            if ( !IsPaused )
            {
                currentTime.Advance( gameTime );
                this.Update( currentTime );
            }
            else
            {
                this.PausedUpdate( currentRealTime );
            }

            UpdateDebugScreen( currentRealTime );

            base.Update( gameTime );
        }*/
    }
}
