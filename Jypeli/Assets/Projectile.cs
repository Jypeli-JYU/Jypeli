namespace Jypeli.Assets
{
    /// <summary>
    /// Ammus.
    /// </summary>
    public class Projectile : PhysicsObject
    {
        private Projectile( double radius, double mass )
            : base( radius * 2, radius * 2, Shape.Circle )
        {
            Mass = mass;
        }

        private Projectile( double width, double height, double mass )
            : base( width, height, Shape.Rectangle )
        {
            Mass = mass;
        }

        /// <summary>
        /// Alustaa uuden pyöreän ammuksen kuvan kanssa.
        /// </summary>
        public Projectile( double radius, double mass, string imageName )
            : this( radius, mass )
        {
            Image = Game.LoadImageFromResources( imageName ); ;
        }

        /// <summary>
        /// Alustaa uuden pyöreän ammuksen värin kanssa.
        /// </summary>
        public Projectile( double radius, double mass, Color color )
            : this( radius, mass )
        {
            Color = color;
        }

        /// <summary>
        /// Alustaa uuden nelikulmaisen ammuksen kuvan kanssa.
        /// </summary>
        public Projectile( double width, double height, double mass, string imageName )
            : this( width, height, mass )
        {
            Image = Game.LoadImageFromResources( imageName );
        }

        /// <summary>
        /// Alustaa uuden nelikulmaisen ammuksen värin kanssa.
        /// </summary>
        public Projectile( double width, double height, double mass, Color color )
            : this( width, height, mass )
        {
            Color = color;
        }
    }
}
