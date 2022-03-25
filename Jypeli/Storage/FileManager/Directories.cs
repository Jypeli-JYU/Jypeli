using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            set { ChDir(value); }
        }

        /// <summary>
        /// Vaihtaa työhakemistoa jättäen edellisen hakemiston muistiin.
        /// Kutsu PopDir kun haluat palauttaa työhakemiston edelliseen arvoonsa.
        /// </summary>
        /// <param name="dir"></param>
        public void PushDir(string dir)
        {
            prevDirs.Push(_currentDir);
            ChDir(dir);
        }

        /// <summary>
        /// Palauttaa edellisen työhakemiston.
        /// Jos edellistö työhakemistoa ei ole tallennettu, säilytetään nykyinen.
        /// </summary>
        public void PopDir()
        {
            if (prevDirs.Count > 0)
                _currentDir = prevDirs.Pop();
        }

        /// <summary>
        /// Vaihtaa työhakemistoa.
        /// </summary>
        /// <param name="path">Hakemistopolku.</param>
        /// <returns>Onnistuiko hakemiston vaihtaminen (palauttaa false jos hakemistoa ei ole).</returns>
        public virtual bool ChDir(string path)
        {
            Initialize();
            MakeAbsolute(ref path);

            if (!FMAssert(Directory.Exists, false, false, path))
                return false;

            _currentDir = path;
            return true;
        }

        /// <summary>
        /// Luo uuden hakemiston.
        /// </summary>
        /// <param name="path">Hakemistopolku.</param>
        public virtual void MkDir(string path)
        {
            Initialize();
            MakeAbsolute(ref path);
            FMAssert(Directory.CreateDirectory, true, null, path);
        }

        /// <summary>
        /// Tuhoaa hakemiston. Heittää poikkeuksen jos hakemisto ei ole tyhjä.
        /// Ei heitä poikkeusta, jos hakemistoa ei ole olemassa.
        /// </summary>
        /// <param name="path"></param>
        public virtual void RmDir(string path)
        {
            Initialize();
            MakeAbsolute(ref path);
            FMAssert(Directory.Delete, true, path);
        }

        /// <summary>
        /// Antaa listan nykyisessä hakemistossa olevista tiedostoista.
        /// </summary>
        /// <returns></returns>
        public virtual IList<string> GetFileList()
        {
            Initialize();
            string[] fileList = FMAssert(Directory.GetFiles, false, new string[] { }, _currentDir);
            return fileList.ToList<string>().AsReadOnly();
        }
    }
}
