using System;
using System.Text;
using System.IO;

namespace Jypeli
{
    public partial class FileManager
    {
        internal static string SanitizeFileName( string fileName )
        {
            StringBuilder newName = new StringBuilder( fileName );
            newName.Replace( "[]", "Array" ).Replace( "()", "Func" );

            for ( int i = 0; i < newName.Length; i++ )
            {
                char c = newName[i];
                if ( !Char.IsLetterOrDigit( c ) && c != '-' && c != '_' && c != '.' )
                    newName[i] = '_';
            }

            return newName.ToString();
        }

        internal void MakeAbsolute( ref string path )
        {
            if ( !Path.IsPathRooted( path ) )
                path = Path.GetFullPath( Path.Combine( _currentDir, path ) );
        }
    }
}
