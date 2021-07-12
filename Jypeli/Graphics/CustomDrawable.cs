using Microsoft.Xna.Framework;

namespace Jypeli
{
    /// <summary>
    /// Rajapinta olioille, joilla on oma Draw-metodi.
    /// </summary>
    public interface CustomDrawable
    {
        /// <summary>
        /// Onko olio näkyvissä
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Piirtää olion ruudulle
        /// </summary>
        /// <param name="parentTransformation"></param>
        void Draw( Matrix parentTransformation );
    }
}
