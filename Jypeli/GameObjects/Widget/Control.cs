using System.Collections.Generic;
using Jypeli.Controls;

namespace Jypeli
{
    public partial class Widget
    {
        private ListenContext _context = new ListenContext();

        /// <summary>
        /// T�h�n listaan lis�tyt kuuntelijat tuhotaan automaattisesti
        /// kun Widget poistetaan pelist�.
        /// </summary>
        internal List<Listener> associatedListeners = new List<Listener>();

        public ListenContext ControlContext { get { return _context; } }

        /// <summary>
        /// Jos <c>true</c>, pelin sek� ikkunan alla olevien widgettien
        /// ohjaimet eiv�t ole k�yt�ss� kun ikkuna on n�kyviss�.
        /// </summary>
        public bool IsModal { get; set; }

        public bool CapturesMouse { get; set; }
        public bool IsCapturingMouse
        {
            get
            {
                // A widget IsCapturingMouse if either:
                //     it CapturesMouse and is under the cursor
                // or:
                //     one of its children IsCapturingMouse.

                if (CapturesMouse && Game.Mouse.IsCursorOn(this))
                    return true;

                foreach (var o in Objects)
                {
                    if (o is Widget w && w.IsCapturingMouse)
                        return true;
                }

                return false;
            }
        }

        public void InitControl()
        {
            if ( ControlContext == null || ControlContext.IsDestroyed )
                _context = new Controls.ListenContext();

            Objects.ItemAdded += InitChildContext;
            Objects.ItemRemoved += ResetChildContext;

            Removed += RemoveListeners;
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

        private void RemoveListeners()
        {
            associatedListeners.ForEach(l => l.Destroy());
            associatedListeners.Clear();
        }
    }
}
