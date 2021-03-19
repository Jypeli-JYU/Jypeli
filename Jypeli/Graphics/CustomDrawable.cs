using Microsoft.Xna.Framework;

namespace Jypeli
{
    /// <summary>
    /// Rajapinta olioille, joilla on oma Draw-metodi.
    /// </summary>
    public interface CustomDrawable
    {
        bool IsVisible { get; set; }
        void Draw( Matrix parentTransformation );
    }
}
