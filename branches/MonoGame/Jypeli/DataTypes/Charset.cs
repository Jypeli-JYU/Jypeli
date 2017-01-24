namespace Jypeli
{
    /// <summary>
    /// Yleisesti k�ytettyj� merkkijoukkoja.
    /// Voit yhdistell� merkkijoukkoja +:lla.
    /// </summary>
    public static class Charset
    {
        /// <summary>
        /// Numerot 0 - 9.
        /// </summary>
        public static string Digits = "0123456789";

        /// <summary>
        /// Aakkoset pienill� kirjaimilla a - �.
        /// </summary>
        public static string LowerCaseAlphabet = "abcdefghijklmnopqrstuvwxyz���";

        /// <summary>
        /// Aakkoset isoilla kirjaimilla A - �.
        /// </summary>
        public static string UpperCaseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ���";

        /// <summary>
        /// Aakkoset pienill� kirjaimilla a - z.
        /// </summary>
        public static string LowerCaseAlphabetUS = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Aakkoset isoilla kirjaimilla A - Z.
        /// </summary>
        public static string UpperCaseAlphabetUS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Lauseessa k�ytett�v�t v�limerkit.
        /// </summary>
        public static string Punctuation = ".,:;-_ ?!";

        /// <summary>
        /// Yleisimm�t rahayksikk�jen tunnukset.
        /// </summary>
        public static string Currency = "�$��";

        /// <summary>
        /// Erikoismerkit.
        /// </summary>
        public static string Special = Currency + Punctuation + "%#+-*/\\%&|{[<=>]}\"'`�^~";

        /// <summary>
        /// Aakkoset pienill� ja isoilla kirjaimilla a - �, A - �
        /// </summary>
        public static string Alphabet = LowerCaseAlphabet + UpperCaseAlphabet;

        /// <summary>
        /// Aakkoset pienill� ja isoilla kirjaimilla a - z, A - Z
        /// </summary>
        public static string AlphabetUS = LowerCaseAlphabetUS + UpperCaseAlphabetUS;

        /// <summary>
        /// Aakkoset pienill� ja isoilla kirjaimilla a - �, A - � + numerot
        /// </summary>
        public static string Alphanumeric = LowerCaseAlphabet + UpperCaseAlphabet + Digits;

        /// <summary>
        /// Aakkoset pienill� ja isoilla kirjaimilla a - z, A - Z + numerot
        /// </summary>
        public static string AlphanumericUS = LowerCaseAlphabetUS + UpperCaseAlphabetUS + Digits;

        /// <summary>
        /// Kaikki suomalaisella n�pp�imist�ll� kirjoitettavissa olevat merkit.
        /// </summary>
        public static string FinnishKeyboard = Alphanumeric + Special;
    }
}
