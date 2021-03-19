namespace Jypeli
{
    /// <summary>
    /// Apuluokka kaikille olioille
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Palauttaa hajautuskoodin usean olion kokoelmalle.
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static int GetHashCode(params object[] objects)
        {
            if ( objects.Length == 1 ) return objects[0].GetHashCode();

            int hc = objects.Length;
            for ( int i = 0; i < objects.Length; ++i )
            {
                hc = unchecked( hc * 314159 + objects[i].GetHashCode() );
            }

            return hc;
        }
    }
}
