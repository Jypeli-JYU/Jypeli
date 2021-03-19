using System;
using System.Collections.Generic;

namespace Jypeli.Controls
{
    public class ListenContext : Destroyable
    {
        internal static ListenContext Null = new ListenContext() { Active = false };

        private bool _active = false;

        public bool Active
        {
            //get { return _active && !destroyed; }
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

        public void Enable() { Active = true; }
        public void Disable() { Active = false; }

        #region Destroyable Members

        bool destroyed = false;

        public bool IsDestroyed
        {
            get { return destroyed; }
        }

        public event Action Destroyed;

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
