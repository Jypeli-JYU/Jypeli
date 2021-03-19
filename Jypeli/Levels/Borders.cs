using System;
using AdvanceMath;

namespace Jypeli
{
    public partial class Level
    {
        private Surface CreateBorder( Direction direction, double restitution, bool isVisible, Image borderImage, Color borderColor )
        {
            Surface s = Surface.Create( this, direction );
            s.Restitution = restitution;
            s.IsVisible = isVisible;
            s.Image = borderImage;
            s.Color = borderColor;
            game.Add( s );
            return s;
        }

        private Surface CreateBorder( Direction direction, double min, double max, int points, double restitution, bool isVisible, Image borderImage, Color borderColor )
        {
            Surface s = Surface.Create( this, direction, min, max, points );
            s.Restitution = restitution;
            s.IsVisible = isVisible;
            s.Image = borderImage;
            s.Color = borderColor;
            game.Add( s );
            return s;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        public Surfaces CreateBorders()
        {
            return CreateBorders( 1, true );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateBorders( bool isVisible )
        {
            return CreateBorders( 1, isVisible );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateBorders( double restitution, bool isVisible )
        {
            return CreateBorders( restitution, isVisible, Color.Gray );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateBorders( double restitution, bool isVisible, Color borderColor )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, null, borderColor );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, null, borderColor );
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, null, borderColor );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, null, borderColor );
            return borders;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateBorders( double restitution, bool isVisible, Image borderImage )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, borderImage, Color.Gray );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, borderImage, Color.Gray );
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, borderImage, Color.Gray );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, borderImage, Color.Gray );
            return borders;
        }

        /// <summary>
        /// Lisää kaikille kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        public Surfaces CreateHorizontalBorders()
        {
            return CreateHorizontalBorders( 1, true );
        }

        /// <summary>
        /// Lisää kaikille kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateHorizontalBorders( bool isVisible )
        {
            return CreateHorizontalBorders( 1, isVisible );
        }

        /// <summary>
        /// Lisää kaikille kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateHorizontalBorders( double restitution, bool isVisible )
        {
            return CreateHorizontalBorders( restitution, isVisible, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateHorizontalBorders( double restitution, bool isVisible, Color borderColor )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, null, borderColor );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, null, borderColor );
            return borders;
        }

        /// <summary>
        /// Lisää kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateVerticalBorders( double restitution, bool isVisible, Color borderColor )
        {
            Surfaces borders = new Surfaces();
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, null, borderColor );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, null, borderColor );
            return borders;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateHorizontalBorders( double restitution, bool isVisible, Image borderImage )
        {
            Surfaces borders = new Surfaces();
            borders.l = CreateBorder( Direction.Left, restitution, isVisible, borderImage, Color.Gray );
            borders.r = CreateBorder( Direction.Right, restitution, isVisible, borderImage, Color.Gray );
            return borders;
        }

        /// <summary>
        /// Lisää kaikille kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        public Surfaces CreateVerticalBorders()
        {
            return CreateVerticalBorders( 1, true );
        }

        /// <summary>
        /// Lisää kaikille kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateVerticalBorders( bool isVisible )
        {
            return CreateVerticalBorders( 1, isVisible );
        }

        /// <summary>
        /// Lisää kaikille kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public Surfaces CreateVerticalBorders( double restitution, bool isVisible )
        {
            return CreateVerticalBorders( restitution, isVisible, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän pystysivuille reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateVerticalBorders( double restitution, bool isVisible, Image borderImage )
        {
            Surfaces borders = new Surfaces();
            borders.t = CreateBorder( Direction.Up, restitution, isVisible, borderImage, Color.Gray );
            borders.b = CreateBorder( Direction.Down, restitution, isVisible, borderImage, Color.Gray );
            return borders;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateBorders( double min, double max, int points, double restitution, Color borderColor )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, null, borderColor );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, null, borderColor );
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, null, borderColor );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, null, borderColor );
            return s;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateBorders( double min, double max, int points, double restitution, Image borderImage )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, borderImage, Color.Gray );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, borderImage, Color.Gray );
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, borderImage, Color.Gray );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, borderImage, Color.Gray );
            return s;
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        public Surfaces CreateBorders( double min, double max, int points, double restitution )
        {
            return CreateBorders( min, max, points, restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kaikille kentän sivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        public Surfaces CreateBorders( double min, double max, int points )
        {
            return CreateBorders( min, max, points, 1, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points, double restitution )
        {
            return CreateHorizontalBorders( min, max, points, restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points, double restitution, Image borderImage )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, borderImage, Color.Gray );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, borderImage, Color.Gray );
            return s;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points, double restitution, Color borderColor )
        {
            Surfaces s = new Surfaces();
            s.l = CreateBorder( Direction.Left, min, max, points, restitution, true, null, borderColor );
            s.r = CreateBorder( Direction.Right, min, max, points, restitution, true, null, borderColor );
            return s;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        public Surfaces CreateHorizontalBorders( double min, double max, int points )
        {
            return CreateHorizontalBorders( min, max, points, 1 );
        }

        /// <summary>
        /// Lisää kentän pystysivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points, double restitution )
        {
            return CreateVerticalBorders( min, max, points, restitution, Color.Gray );
        }

        /// <summary>
        /// Lisää kentän pystysivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderImage">Reunojen kuva / tekstuuri.</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points, double restitution, Image borderImage )
        {
            Surfaces s = new Surfaces();
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, borderImage, Color.Gray );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, borderImage, Color.Gray );
            return s;
        }

        /// <summary>
        /// Lisää kentän pystysivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="borderColor">Reunojen väri.</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points, double restitution, Color borderColor )
        {
            Surfaces s = new Surfaces();
            s.t = CreateBorder( Direction.Up, min, max, points, restitution, true, null, borderColor );
            s.b = CreateBorder( Direction.Down, min, max, points, restitution, true, null, borderColor );
            return s;
        }

        /// <summary>
        /// Lisää kentän vaakasivuille epätasaiset reunat, joihin oliot voivat törmätä.
        /// </summary>
        /// <param name="min">Reunan minimipaksuus.</param>
        /// <param name="max">Reunan maksimipaksuus.</param>
        /// <param name="points">Pisteiden määrä (kuinka vaihtelevaa maasto on).</param>
        public Surfaces CreateVerticalBorders( double min, double max, int points )
        {
            return CreateVerticalBorders( min, max, points, 1 );
        }

        private PhysicsObject CreateBorder( double width, double height )
        {
            PhysicsObject b = PhysicsObject.CreateStaticObject( width, height );
            b.Color = Color.Gray;
            //b.Body.CollisionIgnorer = ignorerForBorders;
            game.Add( b );
            return b;
        }

        /// <summary>
        /// Lisää kenttään vasemman reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateLeftBorder( double restitution, bool isVisible )
        {
            double thickness = this.GetBorderThickness();
            PhysicsObject b = CreateBorder( thickness, this.Height );
            b.Position = new Vector( Left - ( thickness / 2 ), Center.Y );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään oikean reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateRightBorder( double restitution, bool isVisible )
        {
            double thickness = this.GetBorderThickness();
            PhysicsObject b = CreateBorder( thickness, this.Height );
            b.Position = new Vector( Right + ( thickness / 2 ), Center.Y );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään yläreunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateTopBorder( double restitution, bool isVisible )
        {
            double thickness = this.GetBorderThickness();
            PhysicsObject b = CreateBorder( this.Width + ( 2 * thickness ), thickness );
            b.Angle = Angle.FromRadians( Math.PI );
            b.Position = new Vector( Center.X, Top + ( thickness / 2 ) );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään alareunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        /// <param name="restitution">Reunojen kimmoisuus (0.0 = pysäyttää, 1.0 = kimpoaa täydellä voimalla)</param>
        /// <param name="isVisible">Reunan näkyvyys <c>true</c>, jos näkyvät reunat, muuten <c>false</c>.</param>
        public PhysicsObject CreateBottomBorder( double restitution, bool isVisible )
        {
            double thickness = GetBorderThickness();
            PhysicsObject b = CreateBorder( Width + ( 2 * thickness ), thickness );
            b.Angle = Angle.Zero;
            b.Position = new Vector( Center.X, Bottom - ( thickness / 2 ) );
            b.Restitution = restitution;
            b.IsVisible = isVisible;
            return b;
        }

        /// <summary>
        /// Lisää kenttään vasemman reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateLeftBorder()
        {
            return CreateLeftBorder( 1, true );
        }

        /// <summary>
        /// Lisää kenttään oikean reunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateRightBorder()
        {
            return CreateRightBorder( 1, true );
        }

        /// <summary>
        /// Lisää kenttään yläreunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateTopBorder()
        {
            return CreateTopBorder( 1, true );
        }

        /// <summary>
        /// Lisää kenttään alareunan.
        /// </summary>
        /// <returns>Uusi reuna.</returns>
        public PhysicsObject CreateBottomBorder()
        {
            return CreateBottomBorder( 1, true );
        }

        /// <summary>
        /// Helppo tapa lisätä kenttään epätasainen maasto.
        /// Maasto kuvataan luettelemalla Y-koordinaatteja vasemmalta oikealle lukien. Kahden Y-koordinaatin
        /// väli on aina sama.
        /// </summary>
        /// <param name="heights">Y-koordinaatit lueteltuna vasemmalta oikealle.</param>
        /// <param name="scale">Vakio, jolla jokainen Y-koordinaatti kerrotaan. Hyödyllinen,
        /// jos halutaan muuttaa koko maaston korkeutta muuttamatta jokaista pistettä yksitellen.
        /// Tavallisesti arvoksi kelpaa 1.0.</param>
        /// <remarks>
        /// Huomaa, että maastossa ei voi olla kahta pistettä päällekkäin.
        /// </remarks>
        public PhysicsObject CreateGround( double[] heights, double scale )
        {
            return CreateGround( heights, scale, null );
        }

        /// <summary>
        /// Helppo tapa lisätä kenttään epätasainen maasto.
        /// Maasto kuvataan luettelemalla Y-koordinaatteja vasemmalta oikealle lukien. Kahden Y-koordinaatin
        /// väli on aina sama.
        /// </summary>
        /// <param name="heights">Y-koordinaatit lueteltuna vasemmalta oikealle.</param>
        /// <param name="scale">Vakio, jolla jokainen Y-koordinaatti kerrotaan. Hyödyllinen,
        /// jos halutaan muuttaa koko maaston korkeutta muuttamatta jokaista pistettä yksitellen.
        /// Tavallisesti arvoksi kelpaa 1.0.</param>
        /// <param name="image">Maastossa käytettävä kuva.</param>
        /// <returns></returns>
        public PhysicsObject CreateGround( double[] heights, double scale, Image image )
        {
            var ground = new Surface( this.Width, heights, scale );
            ground.Position = new Vector( Center.X, Bottom - ( MathHelper.Max( heights ) / 2 ) );
            ground.Image = image;
            game.Add( ground );
            return ground;
        }

        internal double GetBorderThickness()
        {
            return this.Width / 10;
        }
    }
}
