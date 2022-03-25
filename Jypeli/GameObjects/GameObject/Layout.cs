namespace Jypeli
{
    public partial class GameObject
    {
        private ILayout _layout = null;

        private Sizing _horizontalSizing = Sizing.FixedSize;
        private Sizing _verticalSizing = Sizing.FixedSize;
        private Vector _preferredSize = new Vector(50, 50);
        private bool _sizeByLayout = true;
        private bool _layoutNeedsRefreshing = false;

        /// <summary>
        /// Koon asettaminen vaakasuunnassa, kun olio on
        /// asettelijan sisällä.
        /// </summary>
        public virtual Sizing HorizontalSizing
        {
            get
            {
                if (SizingByLayout && (Layout != null))
                    return Layout.HorizontalSizing;
                return _horizontalSizing;
            }
            set
            {
                _horizontalSizing = value;
                NotifyParentAboutChangedSizingAttributes();
            }
        }

        /// <summary>
        /// Koon asettaminen pystysuunnassa, kun olio on
        /// asettelijan sisällä.
        /// </summary>
        public virtual Sizing VerticalSizing
        {
            get
            {
                if (SizingByLayout && (Layout != null))
                    return Layout.VerticalSizing;
                return _verticalSizing;
            }
            set
            {
                _verticalSizing = value;
                NotifyParentAboutChangedSizingAttributes();
            }
        }

        /// <summary>
        /// Koko, jota oliolla tulisi olla asettelijan sisällä. Todellinen koko voi olla
        /// pienempi, jos tilaa ei ole tarpeeksi.
        /// </summary>
        public virtual Vector PreferredSize
        {
            get
            {
                if (Layout != null)
                    return Layout.PreferredSize;
                return _preferredSize;
            }
            set
            {
                _preferredSize = value;
                NotifyParentAboutChangedSizingAttributes();
            }
        }

        /// <summary>
        /// Onko olion koko asettelijan muokkaama
        /// </summary>
        public bool SizingByLayout
        {
            get { return _sizeByLayout; }
            set { _sizeByLayout = value; }
        }

        /// <summary>
        /// Should be called whenever properties that might affect layouts
        /// are changed.
        /// </summary>
        internal protected void NotifyParentAboutChangedSizingAttributes()
        {
            if (Parent == null)
            {
                _layoutNeedsRefreshing = true;
            }
            else if (Parent is GameObject)
            {
                ((GameObject)Parent).NotifyParentAboutChangedSizingAttributes();
            }
        }

        /// <summary>
        /// Asettelija lapsiolioille. Asettaa lapsiolioiden koon sekä paikan.
        /// </summary>
        public ILayout Layout
        {
            get { return _layout; }
            set
            {
                if (_layout != null)
                {
                    ILayout old = _layout;
                    _layout = null;
                    old.Parent = null;
                }

                InitChildren();
                _layout = value;
                _layout.Parent = this;
                NotifyParentAboutChangedSizingAttributes();
            }
        }

        /// <summary>
        /// Alustaa asettelijan käyttöön
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void InitLayout(double width, double height)
        {
            autoResizeChildObjects = false;
            this.PreferredSize = new Vector(width, height);
        }

        /// <summary>
        /// Alustaa asettelijan käyttöön
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="layout"></param>
        public void InitLayout(double width, double height, ILayout layout)
        {
            this.Layout = layout;
            InitLayout(width, height);
        }

        /// <summary>
        /// Päivittää lapsiolioiden paikat ja koot, jos widgetille on asetettu asettelija.
        /// Tätä metodia EI yleensä tarvitse kutsua itse, sillä asettelija
        /// päivitetään automaattisesti jokaisella framella. Tämä metodi on
        /// tarpeellinen esim. silloin, kun widgetille on lisätty lapsiolioita
        /// (tai muutettu niiden ominaisuuksia) ja niiden paikat tarvitsee päivittää
        /// välittömästi lisäyksen jälkeen. Silloinkin tätä tarvitsee kutsua vain kerran,
        /// viimeisimmän muutoksen jälkeen.
        /// </summary>
        public void RefreshLayout()
        {
            if (Layout != null)
            {
                _childObjects.UpdateChanges();

                // First, lets ask how big the child objects need to be.
                UpdateSizeHints();

                // Then, lets set the size accordingly, if we are allowed to do so.
                if (SizingByLayout)
                {
                    Vector newSize = Layout.PreferredSize;
                    Vector maxSize = this.GetMaximumSize();

                    if (newSize.X > maxSize.X)
                        newSize.X = maxSize.X;
                    if (newSize.Y > maxSize.Y)
                        newSize.Y = maxSize.Y;

                    _size = newSize;
                }

                // Finally, lets position the child objects into the space we have available
                // for them.
                UpdateLayout(_size);
            }
        }

        /// <summary>
        /// Recursively updates the preferred sizes (and other parameters that
        /// affect the layout) of the object and its child objects.
        /// </summary>
        private void UpdateSizeHints()
        {
            if (_childObjects == null)
                return;

            foreach (var child in _childObjects)
            {
                child.UpdateSizeHints();
            }

            if (Layout != null)
                Layout.UpdateSizeHints(_childObjects.items);
        }

        /// <summary>
        /// Recursively updates the layouts of the object and
        /// its child objects. <c>UpdateSizeHints()</c> must have
        /// been called before this, because the layout needs to
        /// know how big the objects need to be.
        /// </summary>
        /// <param name="maximumSize">The actual size that is allocated for the layout.</param>
        private void UpdateLayout(Vector maximumSize)
        {
            if (Layout != null)
                Layout.Update(Objects.items, maximumSize);

            if (_childObjects != null)
            {
                foreach (var child in _childObjects)
                {
                    child.UpdateLayout(child.Size);
                }
            }
        }

        /// <summary>
        /// Antaa widgetin maksimikoon siinä tapauksessa, että kokoa ei
        /// ole annettu rakentajassa (tai tarkemmin sanoen muuttujan <c>SizingByLayout</c>
        /// arvo on <c>true</c>). Olio ei siis automaattisesti kasva tätä isommaksi.
        /// </summary>
        /// <returns></returns>
        protected virtual Vector GetMaximumSize()
        {
            return new Vector(double.PositiveInfinity, double.PositiveInfinity);
        }
    }
}
