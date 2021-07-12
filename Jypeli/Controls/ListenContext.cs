using System;
using System.Collections.Generic;

namespace Jypeli.Controls
{
    /// <summary>
    /// Kuuntelukonteksti ohjaimia varten
    /// </summary>
    public class ListenContext : Destroyable
    {
        internal static ListenContext Null = new ListenContext() { Active = false };

        private bool _active = false;

        /// <summary>
        /// Onko tämä konteksti tällä hetkellä aktiivinen
        /// </summary>
        public bool Active
        {
            get
            {
                if ( destroyed ) return false;
                if ( Parent != null && !Parent.Active ) return false;
                return _active;
            }

            set
            {
                if ( value != _active )
                {
                    _active = value;
                    if ( value && Activated != null ) Activated();
                    if ( !value && Deactivated != null ) Deactivated();
                }
            }
        }

        internal bool dynamicParent = false;
        internal ListenContext parentContext = null;
        internal ControlContexted parentObject = null;

        /// <summary>
        /// Kuuluuko tämä konteksti johonkin toiseen kontekstiin.
        /// </summary>
        public ListenContext Parent
        {
            get
            {
                if ( dynamicParent && parentObject == null ) return null;
                return ( dynamicParent ? parentObject.ControlContext : parentContext );
            }
        }

        private Stack<bool> savedStates = null;

        /// <summary>
        /// Tapahtuu kun konteksti aktivoidaan.
        /// </summary>
        public event Action Activated;

        /// <summary>
        /// Tapahtuu kun konteksti passivoidaan.
        /// </summary>
        public event Action Deactivated;

        /// <summary>
        /// Muodostaa uuden ohjainkontekstin
        /// </summary>
        public ListenContext()
        {
        }

        private ListenContext( ListenContext parent )
        {
            this.parentContext = parent;
        }

        private ListenContext( ControlContexted parentObj )
        {
            this.dynamicParent = true;
            this.parentObject = parentObj;
        }

        /// <summary>
        /// Muodostaa uuden ohjainkontekstin tämän lapseksi
        /// </summary>
        /// <returns></returns>
        public ListenContext CreateSubcontext()
        {
            return new ListenContext( this );
        }

        internal void SaveFocus()
        {
            if ( savedStates == null ) savedStates = new Stack<bool>();
            savedStates.Push( Active );
        }

        internal void RestoreFocus()
        {
            if ( savedStates == null || savedStates.Count == 0 ) return;
            Active = savedStates.Pop();
        }

        /// <summary>
        /// Aktivoi tämän ohjainkontekstin
        /// </summary>
        public void Enable() { Active = true; }

        /// <summary>
        /// Asettaa tämän ohjainkontekstin pois käytöstä.
        /// </summary>
        public void Disable() { Active = false; }

        #region Destroyable Members

        bool destroyed = false;

        /// <summary>
        /// Onko konteksti tuhottu
        /// </summary>
        public bool IsDestroyed
        {
            get { return destroyed; }
        }

        /// <summary>
        /// Ajetaan kun konteksti tuhotaan
        /// </summary>
        public event Action Destroyed;

        /// <summary>
        /// Tuhoaa kontekstin
        /// </summary>
        public void Destroy()
        {
            destroyed = true;
            savedStates = null;
            if ( Destroyed != null ) Destroyed();
        }

        #endregion
    }

    public interface ControlContexted
    {
        ListenContext ControlContext { get; }
        bool IsModal { get; }
    }
}
