using Jypeli.Controls;

namespace Jypeli
{
    /// <summary>
    /// Käyttöliittymän komponentti.
    /// </summary>
    public partial class Widget : GameObject, ControlContexted, CustomDrawable
    {
        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="animation"></param>
        public Widget( Animation animation )
            : base( animation )
        {
            InitAppearance();
            InitControl();
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="layout"></param>
        public Widget( ILayout layout )
            : base( layout )
        {
            InitAppearance();
            InitControl();
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="shape"></param>
        public Widget( double width, double height, Shape shape )
            : base( width, height, shape )
        {
            InitAppearance();
            InitControl();
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Widget( double width, double height )
            : this( width, height, Shape.Rectangle )
        {
        }
    }
}
