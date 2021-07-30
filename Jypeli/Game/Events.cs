using System;

namespace Jypeli
{
    public partial class Game
    {
        /// <summary>
        /// Vapaamuotoinen tapahtumankäsittelijä.
        /// </summary>
        public class CustomEventHandler : Destroyable, Updatable
        {
            Func<bool> condition;
            Action handler;

            /// <summary>
            /// Luo uuden tapahtumankäsittelijän.
            /// </summary>
            /// <param name="condition">Ehto</param>
            /// <param name="handler">Käsittelijä</param>
            internal CustomEventHandler( Func<bool> condition, Action handler )
            {
                this.condition = condition;
                this.handler = handler;
            }

            /// <summary>
            /// Onko käsittelijä tuhottu.
            /// </summary>
            public bool IsDestroyed { get; private set; }

            /// <summary>
            /// Päivitetäänkö.
            /// </summary>
            public bool IsUpdated { get { return true; } }

            /// <summary>
            /// Tapahtuu, kun tapahtumankäsittelijä tuhotaan.
            /// </summary>
            public event Action Destroyed;

            /// <summary>
            /// Tuhoaa tapahtumankäsittelijän.
            /// </summary>
            public void Destroy()
            {
                IsDestroyed = true;
                if ( Destroyed != null )
                    Destroyed();
            }

            /// <summary>
            /// Päivittää tapahtumankäsittelijää (Jypeli kutsuu)
            /// </summary>
            /// <param name="time"></param>
            public void Update( Time time )
            {
                if ( condition() )
                    handler();
            }
        }

        /// <summary>
        /// Kutsutaan kun näppäimistöltä syötetään tekstiä.
        /// </summary>
        public event EventHandler<char> TextInput;

        internal void CallTextInput(object sender, char key)
        {
            TextInput(sender, key);
        }

        private SynchronousList<CustomEventHandler> handlers = null;

        /// <summary>
        /// Lisää vapaamuotoisen tapahtumankäsittelijän.
        /// </summary>
        /// <param name="condition">Ehto josta tapahtuma laukeaa.</param>
        /// <param name="handler">Kutsuttava funktio.</param>
        public CustomEventHandler AddCustomHandler( Func<bool> condition, Action handler)
        {
            if ( handlers == null )
                handlers = new SynchronousList<CustomEventHandler>();

            var handlerObj = new CustomEventHandler( condition, handler );
            handlers.Add( handlerObj );
            return handlerObj;
        }

        /// <summary>
        /// Lisää vapaamuotoisen tapahtumankäsittelijän.
        /// </summary>
        /// <typeparam name="T">Olion tyyppi.</typeparam>
        /// <param name="obj">Olio, jota tapahtuma koskee.</param>
        /// <param name="condition">Ehto josta tapahtuma laukeaa.</param>
        /// <param name="handler">Kutsuttava funktio.</param>
        /// <returns></returns>
        public CustomEventHandler AddCustomHandler<T>( T obj, Predicate<T> condition, Action<T> handler )
        {
            return this.AddCustomHandler( () => condition( obj ), () => handler( obj ) );
        }

        /// <summary>
        /// Lisää vapaamuotoisen tapahtumankäsittelijän.
        /// </summary>
        /// <typeparam name="T1">Olion 1 tyyppi.</typeparam>
        /// <typeparam name="T2">Olion 2 tyyppi.</typeparam>
        /// <param name="obj1">Ensimmäinen olio, jota tapahtuma koskee.</param>
        /// <param name="obj2">Toinen olio, jota tapahtuma koskee.</param>
        /// <param name="condition">Ehto josta tapahtuma laukeaa.</param>
        /// <param name="handler">Kutsuttava funktio.</param>
        /// <returns></returns>
        public CustomEventHandler AddCustomHandler<T1, T2>( T1 obj1, T2 obj2, Func<T1, T2, bool> condition, Action<T1, T2> handler )
        {
            return this.AddCustomHandler( () => condition( obj1, obj2 ), () => handler( obj1, obj2 ) );
        }

        /// <summary>
        /// Lisää vapaamuotoisen tapahtumankäsittelijän.
        /// </summary>
        /// <typeparam name="T1">Olion 1 tyyppi.</typeparam>
        /// <typeparam name="T2">Olion 2 tyyppi.</typeparam>
        /// <typeparam name="T3">Olion 3 tyyppi.</typeparam>
        /// <param name="obj1">Ensimmäinen olio, jota tapahtuma koskee.</param>
        /// <param name="obj2">Toinen olio, jota tapahtuma koskee.</param>
        /// <param name="obj3">Kolmas olio, jota tapahtuma koskee.</param>
        /// <param name="condition">Ehto josta tapahtuma laukeaa.</param>
        /// <param name="handler">Kutsuttava funktio.</param>
        /// <returns></returns>
        public CustomEventHandler AddCustomHandler<T1, T2, T3>( T1 obj1, T2 obj2, T3 obj3, Func<T1, T2, T3, bool> condition, Action<T1, T2, T3> handler )
        {
            return this.AddCustomHandler( () => condition( obj1, obj2, obj3 ), () => handler( obj1, obj2, obj3 ) );
        }

        /// <summary>
        /// Kutsuu tapahtumankäsittelijöitä.
        /// </summary>
        protected void UpdateHandlers( Time time )
        {
            if (handlers == null)
                return;

            handlers.Update( time );
        }
    }
}
