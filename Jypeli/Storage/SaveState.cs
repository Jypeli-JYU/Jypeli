using System;
using System.Text;
using System.Xml;
using System.IO;

namespace Jypeli
{
    public class SaveState : IDisposable
    {
        public StorageFile File { get; private set; }
        
        private XmlWriter writer;
        private XmlWriterSettings settings;
        private bool closed = false;

        internal SaveState( FileManager manager, string fileName )
        {
            File = manager.Open( fileName, true );
            settings = new XmlWriterSettings();
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Encoding = Encoding.UTF8;
            settings.Indent = true;
            BeginWriteXml();
            writer.WriteStartDocument();
            writer.WriteStartElement( "State" );
        }

        internal void BeginWriteXml()
        {
            if ( closed )
                throw new IOException( "Tried to write to a closed state file." );

            if ( writer == null )
                writer = XmlWriter.Create( File.Stream, settings );
        }

        public void Dispose()
        {
            EndSave();
        }

        public void Save<T>( object obj, string name )
        {
            Save( obj, typeof( T ), name );
        }

        public void Save( object obj, Type objType, string name )
        {
            BeginWriteXml();
            writer.WriteStartElement( "Object" );
            writer.WriteAttributeString( "Name", name );

#if WINDOWS_STOREAPP
            writer.WriteAttributeString( "TypeAssembly", objType.AssemblyQualifiedName );
#else
            writer.WriteAttributeString( "TypeAssembly", objType.Assembly.FullName );
#endif
            
            writer.WriteAttributeString( "Type", objType.Name );
            File.SaveData( writer, objType, obj, false );
            writer.WriteEndElement();
        }

        public void EndSave()
        {
            if ( !closed )
            {
                BeginWriteXml();
                writer.WriteEndElement();
                writer.WriteEndDocument();
                closed = true;
            }

            if ( writer != null )
            {
#if WINDOWS_STOREAPP
                writer.Dispose();
#else
                writer.Close();
#endif
                writer = null;
            }

            if ( File != null )
            {
                File.Close();
                File = null;
            }
        }
    }
}
