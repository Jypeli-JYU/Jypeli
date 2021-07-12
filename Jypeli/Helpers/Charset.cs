namespace Jypeli
{
    /// <summary>
    /// Yleisesti käytettyjä merkkijoukkoja.
    /// Voit yhdistellä merkkijoukkoja +:lla.
    /// </summary>
    public static class Charset
    {
        /// <summary>
        /// Numerot 0 - 9.
        /// </summary>
        public static string Digits = "0123456789";

        /// <summary>
        /// Aakkoset pienillä kirjaimilla a - ö.
        /// </summary>
        public static string LowerCaseAlphabet = "abcdefghijklmnopqrstuvwxyzåäö";

        /// <summary>
        /// Aakkoset isoilla kirjaimilla A - Ö.
        /// </summary>
        public static string UpperCaseAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ";

        /// <summary>
        /// Aakkoset pienillä kirjaimilla a - z.
        /// </summary>
        public static string LowerCaseAlphabetUS = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Aakkoset isoilla kirjaimilla A - Z.
        /// </summary>
        public static string UpperCaseAlphabetUS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        /// <summary>
        /// Lauseessa käytettävät välimerkit.
        /// </summary>
        public static string Punctuation = ".,:;-_ ?!";

        /// <summary>
        /// Yleisimmät rahayksikköjen tunnukset.
        /// </summary>
        public static string Currency = "€$£¥";

        /// <summary>
        /// Erikoismerkit.
        /// </summary>
        public static string Special = Currency + Punctuation + "%#+-*/\\%&|{[<=>]}\"'`¤^~";

        /// <summary>
        /// Aakkoset pienillä ja isoilla kirjaimilla a - ö, A - Ö
        /// </summary>
        public static string Alphabet = LowerCaseAlphabet + UpperCaseAlphabet;

        /// <summary>
        /// Aakkoset pienillä ja isoilla kirjaimilla a - z, A - Z
        /// </summary>
        public static string AlphabetUS = LowerCaseAlphabetUS + UpperCaseAlphabetUS;

        /// <summary>
        /// Aakkoset pienillä ja isoilla kirjaimilla a - ö, A - Ö + numerot
        /// </summary>
        public static string Alphanumeric = LowerCaseAlphabet + UpperCaseAlphabet + Digits;

        /// <summary>
        /// Aakkoset pienillä ja isoilla kirjaimilla a - z, A - Z + numerot
        /// </summary>
        public static string AlphanumericUS = LowerCaseAlphabetUS + UpperCaseAlphabetUS + Digits;

        /// <summary>
        /// Kaikki suomalaisella näppäimistöllä kirjoitettavissa olevat merkit.
        /// </summary>
        public static string FinnishKeyboard = Alphanumeric + Special;
    }
}
