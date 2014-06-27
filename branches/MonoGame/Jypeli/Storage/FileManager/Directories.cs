using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class FileManager
    {
        private Stack<string> prevDirs = new Stack<string>();
        protected string _currentDir;

        /// <summary>
        /// Nykyinen ty�hakemisto.
        /// </summary>
        public string CurrentDirectory
        {
            get { return _currentDir; }
            set { ChDir( value ); }
        }

        /// <summary>
        /// Vaihtaa nykyist� hakemistoa.
        /// </summary>
        /// <param name="path">Hakemistopolku</param>
        /// <returns>Vaihdettiinko hakemistoa</returns>
        public abstract bool ChDir( string path );
        
        /// <summary>
        /// Luo uuden hakemiston.
        /// </summary>
        /// <param name="path">Luotavan hakemiston nimi.</param>
        public abstract void MkDir( string path );

        /// <summary>
        /// Poistaa hakemiston.
        /// </summary>
        /// <param name="path">Poistettavan hakemiston nimi.</param>
        public abstract void RmDir( string path );

        /// <summary>
        /// Vaihtaa ty�hakemistoa j�tt�en edellisen hakemiston muistiin.
        /// Kutsu PopDir kun haluat palauttaa ty�hakemiston edelliseen arvoonsa.
        /// </summary>
        /// <param name="dir"></param>
        public void PushDir( string dir )
        {
            prevDirs.Push( _currentDir );
            ChDir( dir );
        }

        /// <summary>
        /// Palauttaa edellisen ty�hakemiston.
        /// Jos edellist� ty�hakemistoa ei ole tallennettu, s�ilytet��n nykyinen.
        /// </summary>
        public void PopDir()
        {
            if ( prevDirs.Count > 0 )
                _currentDir = prevDirs.Pop();
        }
    }
}
