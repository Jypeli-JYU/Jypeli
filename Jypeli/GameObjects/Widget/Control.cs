using System.Collections.Generic;
using Jypeli.Controls;

namespace Jypeli
{
    public partial class Widget
    {
        private ListenContext _context = new ListenContext();

        /// <summary>
        /// Tähän listaan lisätyt kuuntelijat tuhotaan automaattisesti
        /// kun Widget poistetaan pelistä.
        /// </summary>
        internal List<Listener> associatedListeners = new List<Listener>();

        /// <summary>
        /// Tämän Widgetin ohjainkuuntelijoiden konteksti
        /// </summary>
        public ListenContext ControlContext { get { return _context; } }

        /// <summary>
        /// Jos <c>true</c>, pelin sekä ikkunan alla olevien widgettien
        /// ohjaimet eivät ole käytössä kun ikkuna on näkyvissä.
        /// </summary>
        public bool IsModal { get; set; }

        /// <summary>
        /// Kaappaako hiiren, eli meneekö hiiren tapahtumat tämän alla sijaitsevalle oliolle
        /// </summary>
        public bool CapturesMouse { get; set; }

        /// <summary>
        /// Kaappaako hiirtä tällä hetkellä, eli:
        /// <c>CapturesMouse</c> on true ja hiiri on tämän olion päällä, tai
        /// Jokin tämän lapsista kaappaa hiirtä.
        /// </summary>
        public bool IsCapturingMouse
        {
            get
            {
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

        /// <summary>
        /// Alustaa Widgetin ohjaimet käyttöön.
        /// Sinun ei tarvitse kutsua tätä
        /// </summary>
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
