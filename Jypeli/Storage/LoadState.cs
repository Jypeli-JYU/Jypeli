using System;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace Jypeli
{
    public class LoadState : IDisposable
    {
        public StorageFile File { get; private set; }

        private FileManager manager;
        private XmlReader reader;
        private List<String> objectsRead;

        internal LoadState(FileManager manager, string fileName)
        {
            this.manager = manager;
            File = manager.Open(fileName, false);
            BeginReadXml();
            objectsRead = new List<string>();
        }

        internal LoadState(StorageFile file, string fileName)
        {
            File = file;
            BeginReadXml();
            objectsRead = new List<string>();
        }

        public void EndLoad()
        {
            reader.Close();
            File.Close();
        }

        public void Dispose()
        {
            reader.Close();
            File.Close();
        }

        private void ResetFile()
        {
            reader.Close();
            reader = null;
            objectsRead.Clear();

            if (File.Stream.CanSeek)
            {
                File.Stream.Seek(0, System.IO.SeekOrigin.Begin);
            }
            else
            {
                string fileName = File.Name;
                File.Close();
                File = manager.Open(fileName, false);
            }

            BeginReadXml();
        }

        private void BeginReadXml()
        {
            if (reader != null)
                return;

            reader = XmlReader.Create(File.Stream);
            reader.Read();
            if (!reader.IsStartElement("State"))
                throw new ArgumentException("File is corrupted or not a save state file.");
        }

        public T Load<T>(T obj, string name)
        {
            return (T)Load(obj, typeof(T), name);
        }

        public object Load(object obj, Type type, string name)
        {
            if (objectsRead.Contains(name))
            {
                // Start from beginning
                ResetFile();
            }
            else
                BeginReadXml();

            int depth = 0;

            while (!reader.EOF)
            {
                reader.Read();

                if (reader.NodeType == XmlNodeType.None)
                    throw new IOException("Error loading object: reader not initialized");

                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (depth == 0)
                    {
                        if (reader.Name != "Object")
                            throw new XmlException("Unexpected element in save state file: " + reader.Name);

                        string objName = reader.GetAttribute("Name");
                        if (string.IsNullOrEmpty(objName))
                            throw new XmlException("Unnamed object in save state file");

                        if (name == objName)
                            return File.LoadData(reader, type, obj);
                        else
                            objectsRead.Add(objName);
                    }

                    if (!reader.IsEmptyElement)
                        depth++;
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    depth--;
                    if (depth < 0)
                        throw new InvalidOperationException("Error loading object: parse depth < 0");
                }
            }

            throw new KeyNotFoundException("Object with name " + name + " could not be found in file " + File.Name);
        }
    }
}
