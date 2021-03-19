using System.Collections.Generic;

namespace Jypeli
{
    public partial class FileManager
    {
        private Stack<string> prevDirs = new Stack<string>();
        protected string _currentDir;

        /// <summary>
        /// Nykyinen työhakemisto.
        /// </summary>
        public string CurrentDirectory
        {
            get { return _currentDir; }
            set { ChDir( value ); }
        }

        /// <summary>
        /// Vaihtaa nykyistä hakemistoa.
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
        /// Vaihtaa työhakemistoa jättäen edellisen hakemiston muistiin.
        /// Kutsu PopDir kun haluat palauttaa työhakemiston edelliseen arvoonsa.
        /// </summary>
        /// <param name="dir"></param>
        public void PushDir( string dir )
        {
            prevDirs.Push( _currentDir );
            ChDir( dir );
        }

        /// <summary>
        /// Palauttaa edellisen työhakemiston.
        /// Jos edellistä työhakemistoa ei ole tallennettu, säilytetään nykyinen.
        /// </summary>
        public void PopDir()
        {
            if ( prevDirs.Count > 0 )
                _currentDir = prevDirs.Pop();
        }
    }
}
