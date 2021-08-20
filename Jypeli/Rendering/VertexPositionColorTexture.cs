using Vector3 = System.Numerics.Vector3;

namespace Jypeli.Rendering
{// TODO: Halutaanko käyttää tätä yhtä structia kaikkeen?
    public struct VertexPositionColorTexture
    {
        public Vector3 Position; // Tämähän voisi oikeasti olla myös 2d-vektori, mutta pidetään kolmiulotteisena jos joskus halutaan tuoda mahdollisuus 3d-grafiikkaan.

        public float ColorR;
        public float ColorG;
        public float ColorB;
        public float ColorA;

        public float TexCoordsX; // TODO: Ehkä voisi mielummin käyttää silkin tai system.numericsin 2d float-vektoreita?
        public float TexCoordsY;

        public VertexPositionColorTexture(Vector3 position, Color color, Vector textureUV)
        {
            // Värit pitää antaa floattina väliltä 0.0 - 1.0
            this.Position = position;
            ColorR = color.RedComponent / 255f;
            ColorG = color.GreenComponent / 255f;
            ColorB = color.BlueComponent / 255f;
            ColorA = color.AlphaComponent / 255f;
            TexCoordsX = (float)textureUV.X;
            TexCoordsY = (float)textureUV.Y;
        }

        public void SetColor(System.Drawing.Color color)
        {
            ColorR = color.R / 255f;
            ColorG = color.G / 255f;
            ColorB = color.B / 255f;
            ColorA = color.A / 255f;
        }

        public void SetTexCoords(Vector coords)
        {
            TexCoordsX = (float)coords.X;
            TexCoordsY = (float)coords.Y;
        }

        public override string ToString()
        {
            return $"Position: {Position}, Color: {ColorR}, {ColorG}, {ColorB}, {ColorA}, Texture: {TexCoordsX}, {TexCoordsY}";
        }
    }
}
