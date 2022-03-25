using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Tests.Common;
using NUnit.Framework;

namespace Jypeli.Tests.Game
{
    [TestFixture]
    [Ignore("Silk.NETin bugi estää näiden ajamisen ilman ikkunan aukaisua.")]
    public class DataStorageTest : TestClass
    {
        FileManager DataStorage;
        StorageFile tiedosto = null;

        public override void Setup()
        {
            base.Setup();
            DataStorage = Jypeli.Game.DataStorage;
        }

        public override void TearDown()
        {
            if (tiedosto != null && tiedosto.Stream.CanRead)
                tiedosto.Close();

            DataStorage.ChDir(FileLocation.DataPath);

            if (DataStorage.Exists("testi.txt"))
                DataStorage.Delete("testi.txt");

            if (DataStorage.Exists("topten.xml"))
                DataStorage.Delete("topten.xml");

            if (DataStorage.Exists("kansio"))
                DataStorage.RmDir("kansio");
            tiedosto = null;

            base.TearDown();
        }

        [Test]
        public void CreateFile()
        {
            tiedosto = DataStorage.Create("testi.txt");
            Assert.IsTrue(tiedosto.Name.EndsWith("testi.txt"));
            Assert.IsNotNull(tiedosto.Stream);
            tiedosto.Close();

            Assert.IsTrue(DataStorage.Exists("testi.txt"));

            tiedosto = DataStorage.Open("testi.txt", false);
        }

        [Test]
        public void OpenAsReadOnly()
        {
            tiedosto = DataStorage.Create("testi.txt");
            tiedosto.Close();
            tiedosto = DataStorage.Open("testi.txt", false);
            Assert.IsTrue(!tiedosto.Stream.CanWrite);
        }

        [Test]
        public void FileDoesNotExist()
        {
            DataStorage.Delete("testi.txt");
            Assert.IsFalse(DataStorage.Exists("testi.txt"));

        }

        [Test]
        public void CreateFolder()
        {
            DataStorage.MkDir("kansio");
            Assert.IsTrue(DataStorage.Exists("kansio"));
        }

        [Test]
        public void ChangeDirectory()
        {
            tiedosto = DataStorage.Create("testi.txt");
            DataStorage.MkDir("kansio");
            Assert.Greater(DataStorage.GetFileList().Count, 0);
            Assert.IsTrue(DataStorage.ChDir("kansio"));
            Assert.AreEqual(DataStorage.GetFileList().Count, 0);
        }

        [Test]
        public void ChangeToNonExistingDirectory()
        {
            Assert.IsFalse(DataStorage.ChDir("kansio"));
            Assert.IsFalse(DataStorage.CurrentDirectory.EndsWith("kansio"));
        }

        [Test]
        public void DeleteNonExisting()
        {
            Assert.DoesNotThrow(() => DataStorage.Delete("kissa"));
        }

        [Test]
        public void TopTen()
        {
            Assert.IsFalse(DataStorage.Exists("topten.xml"));
            ScoreList score = new ScoreList();
            score.Add("sata", 100);
            score.Add("yksi", 1);
            score.Add("kaksi", 2);
            DataStorage.Save(score, "topten.xml");
            Assert.IsTrue(DataStorage.Exists("topten.xml"));

            ScoreList score2 = new ScoreList();
            score2 = DataStorage.Load(score2, "topten.xml");
            Assert.AreEqual(score2, score);
        }
    }
}
