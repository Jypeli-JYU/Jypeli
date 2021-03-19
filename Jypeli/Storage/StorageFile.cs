using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using System.Xml;
using System.Collections;
using System.Globalization;

namespace Jypeli
{
    /// <summary>
    /// Tiedosto.
    /// </summary>
    public class StorageFile : IDisposable
    {
        public string Name { get; set; }
        public Stream Stream { get; private set; }

        internal StorageFile( string name, Stream stream )
        {
            Name = name;
            Stream = stream;
            if ( Stream == null ) Stream = new MemoryStream();
        }

        public void Close()
        {
            Stream.Close();
        }

        public void Dispose()
        {
            Stream.Close();
        }

        internal static BindingFlags AllOfInstance = BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        string GenToString( Type type, object obj )
        {
            if ( type == typeof( string ) )
                return (string)obj;

            IFormatProvider provider = NumberFormatInfo.InvariantInfo;

            try
            {
                object[] args = new object[] { provider };
                return (string)type.InvokeMember( "ToString", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, obj, args );
            }
            catch ( MissingMethodException )
            {
                object[] args = new object[] { };
                return (string)type.InvokeMember( "ToString", BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, obj, args );
            }
        }

        object GenParse( Type type, string str )
        {
            if ( type == typeof( string ) )
                return str;

            if ( type == typeof(Type) )
            {
                return TypeHelper.Parse( str );
            }

            try
            {
                object[] args = new object[] { str, NumberFormatInfo.InvariantInfo };
                return type.InvokeMember( "Parse", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, args );
            }
            catch ( MissingMethodException )
            {
                object[] args = new object[] { str };
                return type.InvokeMember( "Parse", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, args );
            }
        }

        internal object LoadData( XmlReader reader, Type type, object obj )
        {
            Dictionary<string, string> metadata = null;
            int depth = reader.Depth;

            while ( reader.Read() )
            {
                if ( ( reader.NodeType == XmlNodeType.EndElement ) && ( reader.Depth <= depth ) )
                    break;

                if ( reader.NodeType != XmlNodeType.Element )
                    continue;

                if ( reader.Name == "Meta" )
                {
                    if ( metadata == null ) metadata = new Dictionary<string, string>();

                    reader.MoveToFirstAttribute();
                    do
                    {
                        metadata.Add( reader.Name, reader.ReadContentAsString() );
                    } while ( reader.MoveToNextAttribute() );
                }

                if ( reader.IsEmptyElement )
                    continue;

                Destroyable dObj = obj as Destroyable;

                // A Type object has no reasonable default value.
                if ( ( type != typeof( Type ) ) && ( obj == null || ( dObj != null && dObj.IsDestroyed ) ) )
                {
                    object tag = obj is Tagged ? ( (Tagged)obj ).Tag : null;
                    string stag = tag as string;

                    if ( !TryCreateObject( type, stag, out obj ) )
                        throw new NullReferenceException( "Object of type " + type.Name + " must be initialized before it can be loaded since there is no factory method or default constructor for it." );
                }

                if ( reader.Name == "Field" || reader.Name == "Property" )
                {
                    string mName = reader.GetAttribute( "Name" );
                    Type mType = TypeHelper.Parse( reader.GetAttribute( "Type" ) );

                    if ( reader.Name == "Property" )
                    {
                        PropertyInfo propInfo = type.GetProperty( mName, BindingFlags.GetProperty | AllOfInstance );
                        if ( propInfo == null ) throw new ArgumentException( type.Name + " does not contain property " + mName );
                        object mValue = propInfo.GetValue( obj, null );
                        mValue = LoadData( reader, mType, mValue );
                        propInfo.SetValue( obj, mValue, null );
                    }
                    else
                    {
                        FieldInfo fieldInfo = type.GetField( mName, BindingFlags.GetField | AllOfInstance );
                        if ( fieldInfo == null ) throw new ArgumentException( type.Name + " does not contain field " + mName );
                        object mValue = fieldInfo.GetValue( obj );
                        mValue = LoadData( reader, mType, mValue );
                        fieldInfo.SetValue( obj, mValue );
                    }
                }
                else if ( reader.Name == "SerializedObject" )
                {
                    // Used for legacy support only

                    reader.Read();
                    object objValue = null;

                    using ( MemoryStream dataStream = new MemoryStream( Encoding.Unicode.GetBytes( reader.Value ) ) )
                    {
                        XmlSerializer serializer = new XmlSerializer( type );
                        objValue = serializer.Deserialize( dataStream );
                    }

                    while ( !reader.EOF && reader.NodeType != XmlNodeType.EndElement ) reader.Read();

                    obj = objValue;
                }
                else if ( reader.Name == "Value" )
                {
                    reader.Read();

                    if ( type.IsEnum )
                        obj = Enum.Parse( type, reader.Value, false );
                    else
                        obj = GenParse( type, reader.Value );

                    while ( !reader.EOF && reader.NodeType != XmlNodeType.EndElement ) reader.Read();
                }
                else if ( reader.Name == "Array" )
                {
                    Type mType = TypeHelper.Parse( reader.GetAttribute( "Type" ) );
                    obj = LoadArray( reader, mType, obj );
                }
                else if ( reader.Name == "List" )
                {
                    Type mType = TypeHelper.Parse( reader.GetAttribute( "Type" ) );
                    int itemCount = int.Parse( reader.GetAttribute( "Count" ) );
                    obj = LoadList( reader, mType, itemCount, obj );
                }
            }

            if ( metadata != null )
            {
                foreach ( KeyValuePair<string, string> entry in metadata )
                    applyMetadata( type, obj, entry.Key, entry.Value );
            }

            return obj;
        }

        private void applyMetadata( Type type, object obj, string tag, string value )
        {
#if JYPELI
            if ( TypeHelper.InheritsFrom( type, typeof( GameObject ) ) )
            {
                // Game object tags
                GameObject gobj = (GameObject)obj;

                if ( tag == "AddedToGame" && tagTrue( value ) && !gobj.IsAddedToGame )
                    Game.Instance.Add( gobj );
            }
#endif
        }

        private bool tagTrue( string value )
        {
            return value == "1" || value.ToLower() == "true";
        }

        internal void SaveData( XmlWriter writer, Type type, object obj, bool saveAllFields )
        {
            if ( obj is Array )
            {
                SaveArray( writer, type, obj );
                return;
            }

            if ( obj is IList )
            {
                SaveList( writer, type, obj );
                return;
            }

            if ( obj is Type )
            {
                writer.WriteElementString( "Value", ( obj as Type ).AssemblyQualifiedName );
            }

            bool hasSaveAttrib = saveAllFields || type.GetCustomAttributes( typeof( SaveAttribute ), true ).Length > 0;
            bool hasSaveAllFields = saveAllFields || type.GetCustomAttributes( typeof( SaveAllFieldsAttribute ), true ).Length > 0;

            if ( type.IsClass && type != typeof( string ) && !hasSaveAttrib )
                return;

            if ( !hasSaveAttrib || ( type.IsPrimitive && type.IsEnum ) )
            {
                writer.WriteElementString( "Value", GenToString( type, obj ) );
                return;
            }

            writeMetadata( writer, type, obj );

            foreach ( PropertyInfo prop in type.GetProperties( BindingFlags.GetProperty | AllOfInstance ) )
            {
                if ( IsIllegalType( prop.PropertyType ) )
                    continue;

                object[] attribs = prop.GetCustomAttributes( true );
                if ( prop.GetCustomAttributes( typeof( SaveAttribute ), true ).Length == 0 )
                    continue;

                object propValue = prop.GetValue( obj, null );

                if ( propValue == null )
                    continue;

                Type st = propValue.GetType();
                SaveMember( writer, "Property", st, prop, propValue, saveAllFields );
            }

            foreach ( FieldInfo field in type.GetFields( BindingFlags.GetField | AllOfInstance ) )
            {
                if ( IsIllegalType( field.FieldType ) )
                    continue;

                if ( !hasSaveAllFields && field.GetCustomAttributes( typeof( SaveAttribute ), true ).Length == 0 )
                    continue;

                object fieldValue = field.GetValue( obj );

                if ( fieldValue == null )
                    continue;

                Type st = fieldValue.GetType();
                SaveMember( writer, "Field", st, field, fieldValue, saveAllFields );
            }
        }

        private bool IsIllegalType( Type type )
        {
            return ( type == typeof( IntPtr ) || type == typeof( UIntPtr ) );
        }

        private void SaveMember( XmlWriter writer, string elementName, Type memberType, MemberInfo member, object memberValue, bool saveAllFields )
        {
            writer.WriteStartElement( elementName );
            writer.WriteAttributeString( "Name", member.Name );
            writer.WriteAttributeString( "Type", TypeHelper.ToString( memberType ) );
            SaveData( writer, memberType, memberValue, saveAllFields || member.GetCustomAttributes( typeof( SaveAllFieldsAttribute ), true ).Length > 0 );
            writer.WriteEndElement();
        }

        private void writeMetadata( XmlWriter writer, Type type, object obj )
        {
#if JYPELI
            if ( TypeHelper.InheritsFrom( type, typeof( GameObject ) ) && ( (GameObject)obj ).IsAddedToGame )
            {
                writer.WriteStartElement( "Meta" );
                writer.WriteAttributeString( "AddedToGame", "1" );
                writer.WriteEndElement();
            }
#endif
        }

        internal object LoadArray( XmlReader reader, Type containerType, object obj )
        {
            Type designatedItemType = null;
            Array array = (Array)obj;
            int index = 0;

            for ( int readItems = 0;
                    readItems < array.GetLength( 0 ) &&
                    reader.Read() && !( reader.NodeType == XmlNodeType.EndElement && reader.Name == "Array" ); )
            {
                if ( reader.NodeType != XmlNodeType.Element )
                    continue;
                if ( reader.Name != "Item" )
                {
                    string name = reader.GetAttribute( "Name" );
                    if ( name == null ) name = "(undefined)";
                    throw new XmlException( "Unexpected subelement in List block: " + reader.Name + ", Name = " + name );
                }

                index = Int16.Parse( reader.GetAttribute( "Index" ) );
                designatedItemType = TypeHelper.Parse( reader.GetAttribute( "Type" ) );

                object item = array.GetValue( index );
                if ( item != null )
                {
                    Type realItemType = item.GetType();
                    if ( designatedItemType != realItemType )
                        throw new XmlException( "Array item type mismatch: expected " + designatedItemType.Name + ", got " + realItemType.Name + " at index " + index.ToString() );
                }

                item = LoadData( reader, designatedItemType, item );
                array.SetValue( item, index );

                readItems++;
            }

            return array;
        }

        private void FixListSize( ref IList list, int itemCount )
        {
            while ( list.Count < itemCount )
            {
                list.Add( null );
            }

            while ( list.Count > itemCount )
            {
                list.RemoveAt( list.Count - 1 );
            }
        }

        internal object LoadList( XmlReader reader, Type containerType, int itemCount, object obj )
        {
            object[] listObjects = new object[itemCount];
            IList list = (IList)obj;

            while ( list.Count > itemCount )
            {
                int removeIndex = list.Count - 1;
                if ( list[removeIndex] is Destroyable )
                {
                    ( (Destroyable)( list[removeIndex] ) ).Destroy();
                }
                list.RemoveAt( removeIndex );
            }

            for ( int i = 0; i < Math.Min( itemCount, list.Count ); i++ )
                listObjects[i] = list[i];
            for ( int i = list.Count; i < itemCount; i++ )
                listObjects[i] = null;

            while ( reader.Read() && !( reader.NodeType == XmlNodeType.EndElement && reader.Name == "List" ) )
            {
                if ( reader.NodeType != XmlNodeType.Element )
                    continue;
                if ( reader.Name != "Item" )
                {
                    string name = reader.GetAttribute( "Name" );
                    if ( name == null ) name = "(undefined)";
                    throw new XmlException( "Unexpected subelement in List block: " + reader.Name + ", Name = " + name );
                }

                string tag = reader.GetAttribute( "Tag" );
                Type itemType = TypeHelper.Parse( reader.GetAttribute( "Type" ) );
                int itemindex = int.Parse( reader.GetAttribute( "Index" ) );

                if ( tag != null && listObjects[itemindex] == null )
                {
                    if ( !TryCreateObject( itemType, tag, out listObjects[itemindex] ) )
                        throw new MissingMethodException( String.Format("No factory method for type {0}, tag {1}", itemType.Name, tag) );
                }

                listObjects[itemindex] = LoadData( reader, itemType, listObjects[itemindex] );
            }

            list.Clear();

            for ( int i = 0; i < listObjects.Length; i++ )
                list.Add( listObjects[i] );

            return list;
        }

        bool TryCreateObject( Type type, string tag, out object obj )
        {
            if ( type == typeof( string ) )
            {
                // Strings are immutable, and therefore have no default constructor
                obj = "";
                return true;
            }
            else
            {
                try
                {
                    // Try to create using a factory method
                    obj = Factory.FactoryCreate( type, tag );
                    return true;
                }
                catch ( KeyNotFoundException )
                {
                    try
                    {
                        // Try the default constructor
                        obj = Activator.CreateInstance( type, true );
                        return true;
                    }
                    catch ( MissingMethodException )
                    {
                        obj = null;
                        return false;
                    }
                }
            }
        }

        internal void SaveArray( XmlWriter writer, Type containerType, object obj )
        {
            Array array = (Array)obj;

            writer.WriteStartElement( "Array" );
            writer.WriteAttributeString( "Type", TypeHelper.ToString( containerType ) );

            for ( int i = 0; i < array.GetLength( 0 ); i++ )
            {
                object item = array.GetValue( i );
                Type realItemType = item.GetType();
                writer.WriteStartElement( "Item" );
                writer.WriteAttributeString( "Index", i.ToString() );
                writer.WriteAttributeString( "Type", TypeHelper.ToString( realItemType ) );
                if ( item is Tagged ) writer.WriteAttributeString( "Tag", ( (Tagged)item ).Tag.ToString() );
                SaveData( writer, realItemType, item, false );
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        internal void SaveList( XmlWriter writer, Type containerType, object obj )
        {
            IList list = (IList)obj;

            writer.WriteStartElement( "List" );
            writer.WriteAttributeString( "Type", TypeHelper.ToString( containerType ) );
            writer.WriteAttributeString( "Count", list.Count.ToString() );

            for ( int i = 0; i < list.Count; i++ )
            {
                Type realItemType = list[i].GetType();
                writer.WriteStartElement( "Item" );
                writer.WriteAttributeString( "Type", TypeHelper.ToString( realItemType ) );
                writer.WriteAttributeString( "Index", i.ToString() );
                if ( list[i] is Tagged ) writer.WriteAttributeString( "Tag", ( (Tagged)list[i] ).Tag.ToString() );
                SaveData( writer, realItemType, list[i], false );
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }
    }
}
