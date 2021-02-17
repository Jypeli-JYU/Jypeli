#region MIT License
/*
 * Copyright (c) 2009 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Jypeli.GameObjects;


namespace Jypeli
{
    /// <summary>
    /// Pelialueella liikkuva olio.
    /// Käytä fysiikkapeleissä <c>PhysicsObject</c>-olioita.
    /// </summary>
    [Save]
    public partial class GameObject : GameObjects.GameObjectBase, IGameObjectInternal
    {
        public List<Listener> AssociatedListeners { get; private set; }

        #region Destroyable

        /// <summary>
        /// Tuhoaa olion. Tuhottu olio poistuu pelistä.
        /// </summary>
        public override void Destroy()
        {
            this.MaximumLifetime = TimeSpan.Zero;

            DestroyChildren();

            if ( AssociatedListeners != null )
            {
                foreach ( Listener listener in AssociatedListeners )
                {
                    listener.Destroy();
                }
            }

            base.Destroy();
        }

        #endregion

        /// <summary>
        /// Alustaa uuden peliolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        public GameObject(double width, double height)
            : this(width, height, Shape.Rectangle, 0.0, 0.0)
        {
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="x">Olion sijainnin X-koordinaatti.</param>
        /// <param name="y">Olion sijainnin Y-koordinaatti.</param>
        public GameObject(double width, double height, double x, double y)
            : this(width, height, Shape.Rectangle, x, y)
        {
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto.</param>
        /// <param name="x">Olion sijainnin X-koordinaatti.</param>
        /// <param name="y">Olion sijainnin Y-koordinaatti.</param>
        public GameObject(double width, double height, Shape shape, double x, double y)
            : this(width, height, shape)
        {
            Position = new Vector(x, y);
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// </summary>
        /// <param name="width">Leveys.</param>
        /// <param name="height">Korkeus.</param>
        /// <param name="shape">Muoto.</param>
        public GameObject(double width, double height, Shape shape)
            : base()
        {
            InitDimensions(width, height, shape);
            InitAppearance();
            InitListeners();
            InitLayout(width, height);
        }

        /// <summary>
        /// Alustaa uuden peliolion.
        /// Kappaleen koko ja ulkonäkö ladataan parametrina annetusta kuvasta.
        /// </summary>
        /// <param name="animation">Kuva</param>
        public GameObject( Animation animation )
            : base()
        {
            InitDimensions( animation.Width, animation.Height, Shape.Rectangle );
            InitAppearance( animation );
            InitListeners();
            InitLayout( animation.Width, animation.Height );
        }

        /// <summary>
        /// Alustaa widgetin.
        /// </summary>
        public GameObject( ILayout layout )
            : base()
        {
            Vector defaultSize = new Vector( 100, 100 );
            InitDimensions( defaultSize.X, defaultSize.Y, Shape.Rectangle );
            InitAppearance();
            InitListeners();
            InitLayout( defaultSize.X, defaultSize.Y, layout );
        }

        private void InitListeners()
        {
            this.AssociatedListeners = new List<Listener>();
        }

        /// <summary>
        /// Peliolion päivitys. Tätä kutsutaan, kun <c>IsUpdated</c>-ominaisuuden
        /// arvoksi on asetettu <c>true</c> ja olio on lisätty peliin.
        /// <see cref="GameObjectBase.IsUpdated"/>
        /// </summary>
        /// <param name="time">Peliaika.</param>
        [EditorBrowsable( EditorBrowsableState.Never )]
        public override void Update( Time time )
        {
            base.Update( time );
            UpdateChildren( time );
            if ( _layoutNeedsRefreshing )
            {
                RefreshLayout();
                _layoutNeedsRefreshing = false;
            }
            if ( oscillators != null )
                oscillators.Update( time );
        }
    }
}
