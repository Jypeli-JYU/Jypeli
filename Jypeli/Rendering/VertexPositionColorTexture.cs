using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Vector2 = System.Numerics.Vector2;

namespace Jypeli.Rendering
{// TODO: Halutaanko käyttää tätä yhtä structia kaikkeen?
    public struct VertexPositionColorTexture
    {
        public Vector3 Position; // Tämähän voisi oikeasti olla myös 2d-vektori, mutta pidetään kolmiulotteisena jos joskus halutaan tuoda mahdollisuus 3d-grafiikkaan.
        public Vector4 Color;
        public Vector2 TexCoords;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector textureUV)
        {
            // Värit pitää antaa floattina väliltä 0.0 - 1.0
            this.Position = position;
            this.Color = new Vector4(color.RedComponent / 255f, color.GreenComponent / 255f, color.BlueComponent / 255f, color.AlphaComponent / 255f);
            TexCoords = new Vector2((float)textureUV.X, (float)textureUV.Y);
        }

        public void SetColor(System.Drawing.Color color)
        {
            Color.X = color.R / 255f;
            Color.Y = color.G / 255f;
            Color.Z = color.B / 255f;
            Color.W = color.A / 255f;
        }

        public void SetColor(Color color)
        {
            Color.X = color.RedComponent / 255f;
            Color.Y = color.GreenComponent / 255f;
            Color.Z = color.BlueComponent / 255f;
            Color.W = color.AlphaComponent / 255f;
        }

        public void SetTexCoords(Vector coords)
        {
            TexCoords.X = (float)coords.X;
            TexCoords.Y = (float)coords.Y;
        }

        public override string ToString()
        {
            return $"Position: {Position}, Color: {Color}, Texture: {TexCoords}";
        }
    }
}
