using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Controls;

namespace Jypeli
{
    public partial class Widget
    {
        private ListenContext _context = new ListenContext();
        internal List<Listener> associatedListeners = new List<Listener>();

        public ListenContext ControlContext { get { return _context; } }

        /// <summary>
        /// Jos <c>true</c>, pelin sekä ikkunan alla olevien widgettien
        /// ohjaimet eivät ole käytössä kun ikkuna on näkyvissä.
        /// </summary>
        public bool IsModal { get; set; }

        public void InitControl()
        {
            if ( ControlContext == null || ControlContext.IsDestroyed )
                _context = new Controls.ListenContext();

            Objects.ItemAdded += InitChildContext;
            Objects.ItemRemoved += ResetChildContext;
        }

        private void InitChildContext( GameObject child )
        {
            ControlContexted ctxChild = child as ControlContexted;
            if ( ctxChild == null ) return;
            ctxChild.ControlContext.dynamicParent = true;
            ctxChild.ControlContext.parentObject = this;
        }

        private void ResetChildContext( GameObject child )
        {
            ControlContexted ctxChild = child as ControlContexted;
            if ( ctxChild == null ) return;
            ctxChild.ControlContext.parentObject = null;
            ctxChild.ControlContext.parentContext = null;
        }
    }
}
