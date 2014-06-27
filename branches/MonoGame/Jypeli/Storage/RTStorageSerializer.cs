using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;
using System.Xml;

namespace Jypeli.Storage
{
    public static class RTStorageSerializer
    {
        private static bool IsEnumerable( object obj )
        {
            Type objType = obj.GetType();

            if ( !objType.IsConstructedGenericType )
                return false;

            return objType.GetGenericTypeDefinition().GetTypeInfo().ImplementedInterfaces.Contains( typeof(IEnumerable<>) );
        }

        public static void Serialize( XmlWriter writer, Type type, object obj, bool saveAllFields )
        {
            if ( IsEnumerable( obj ) )
            {
                SaveEnumerable( writer, type, obj );
            }

            /*if ( obj is Array )
            {
                SaveArray( writer, type, obj );
                return;
            }

            if ( IsList(obj) )
            {
                SaveList( writer, type, obj );
                return;
            }*/

            if ( obj is Type )
            {
                writer.WriteElementString( "Value", ( obj as Type ).AssemblyQualifiedName );
            }

            TypeInfo typeInfo = type.GetTypeInfo();

            bool hasSaveAttrib = saveAllFields || typeInfo.GetCustomAttribute( typeof( SaveAttribute ), true ) != null;
            bool hasSaveAllFields = saveAllFields || typeInfo.GetCustomAttribute( typeof( SaveAllFieldsAttribute ), true ) != null;

            if ( typeInfo.IsClass && type != typeof( string ) && !hasSaveAttrib )
                return;

            if ( !hasSaveAttrib || ( typeInfo.IsPrimitive && typeInfo.IsEnum ) )
            {
                writer.WriteElementString( "Value", GenToString( type, obj ) );
                return;
            }

            writeMetadata( writer, type, obj );

            foreach ( PropertyInfo prop in type.GetRuntimeProperties() )
            {
                if ( IsIllegalType( prop.PropertyType ) )
                    continue;

                if ( prop.GetCustomAttribute( typeof( SaveAttribute ), true ) == null )
                    continue;

                object propValue = prop.GetValue( obj, null );

                if ( propValue == null )
                    continue;

                Type st = propValue.GetType();
                SaveMember( writer, "Property", st, prop, propValue, saveAllFields );
            }

            foreach ( FieldInfo field in type.GetRuntimeFields() )
            {
                if ( IsIllegalType( field.FieldType ) )
                    continue;

                if ( !hasSaveAllFields && field.GetCustomAttribute( typeof( SaveAttribute ), true ) == null )
                    continue;

                object fieldValue = field.GetValue( obj );

                if ( fieldValue == null )
                    continue;

                Type st = fieldValue.GetType();
                SaveMember( writer, "Field", st, field, fieldValue, saveAllFields );
            }
        }

        public static object Deserialize( XmlReader reader, Type type, object obj )
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
                        PropertyInfo propInfo = type.GetRuntimeProperty( mName );
                        if ( propInfo == null ) throw new ArgumentException( type.Name + " does not contain property " + mName );
                        object mValue = propInfo.GetValue( obj, null );
                        mValue = Deserialize( reader, mType, mValue );
                        propInfo.SetValue( obj, mValue, null );
                    }
                    else
                    {
                        FieldInfo fieldInfo = type.GetRuntimeField( mName );
                        if ( fieldInfo == null ) throw new ArgumentException( type.Name + " does not contain field " + mName );
                        object mValue = fieldInfo.GetValue( obj );
                        mValue = Deserialize( reader, mType, mValue );
                        fieldInfo.SetValue( obj, mValue );
                    }
                }
                else if ( reader.Name == "Value" )
                {
                    reader.Read();

                    if ( type.GetTypeInfo().IsEnum )
                        obj = Enum.Parse( type, reader.Value, false );
                    else
                        obj = GenParse( type, reader.Value );

                    while ( !reader.EOF && reader.NodeType != XmlNodeType.EndElement ) reader.Read();
                }
                else if ( reader.Name == "Enumerable" )
                {
                    Type mType = TypeHelper.Parse( reader.GetAttribute( "Type" ) );
                    LoadEnumerable( reader, mType, ref obj );
                }
            }

            if ( metadata != null )
            {
                foreach ( KeyValuePair<string, string> entry in metadata )
                    applyMetadata( type, obj, entry.Key, entry.Value );
            }

            return obj;
        }

        private static void SaveMember( XmlWriter writer, string elementName, Type memberType, MemberInfo member, object memberValue, bool saveAllFields )
        {
            writer.WriteStartElement( elementName );
            writer.WriteAttributeString( "Name", member.Name );
            writer.WriteAttributeString( "Type", TypeHelper.ToString( memberType ) );
            Serialize( writer, memberType, memberValue, saveAllFields || member.GetCustomAttribute( typeof( SaveAllFieldsAttribute ), true ) != null );
            writer.WriteEndElement();
        }

        private static object InvokeMethod( Type type, object obj, string methodName )
        {
            MethodInfo methodInfo = type.GetRuntimeMethod( methodName, new Type[] {} );
            if ( methodInfo == null )
                throw new MissingMemberException();

            return methodInfo.Invoke( obj, new object[] { } );
        }

        internal static void LoadEnumerable( XmlReader reader, Type containerType, ref object obj )
        {
            if ( obj == null )
                obj = Activator.CreateInstance( containerType );

            Type itemSuperType = containerType.GenericTypeArguments[0];

            MethodInfo addMethod = containerType.GetRuntimeMethod( "Add", new Type[] { itemSuperType } );
            if ( addMethod == null )
                throw new MissingMemberException();     // todo

            while ( reader.Read() && !( reader.NodeType == XmlNodeType.EndElement && reader.Name == "Enumerable" ) )
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
                //int itemindex = int.Parse( reader.GetAttribute( "Index" ) );
                object item = null;

                if ( tag != null )
                {
                    if ( !TryCreateObject( itemType, tag, out item ) )
                        throw new MissingMemberException( String.Format( "No factory method for type {0}, tag {1}", itemType.Name, tag ) );
                }

                item = Deserialize( reader, itemType, item );
                addMethod.Invoke( obj, new object[] { item } );
            }
        }

        internal static void SaveEnumerable( XmlWriter writer, Type containerType, object obj )
        {
            IEnumerable<string> test = null;
            IEnumerator<string> en = test.GetEnumerator();

            en.Reset();

            writer.WriteStartElement( "Enumerable" );
            writer.WriteAttributeString( "Type", TypeHelper.ToString( containerType ) );

            object enumerator = InvokeMethod( containerType, obj, "GetEnumerator" );
            Type enumeratorType = enumerator.GetType();
            InvokeMethod( enumeratorType, enumerator, "Reset" );

            PropertyInfo itemInfo = enumeratorType.GetRuntimeProperty( "Current" );
            int i = 0;

            while( (bool)InvokeMethod( enumerator.GetType(), enumerator, "MoveNext" ) )
            {
                object item = itemInfo.GetValue( enumerator );
                Type itemType = item.GetType();

                writer.WriteStartElement( "Item" );
                writer.WriteAttributeString( "Type", TypeHelper.ToString( itemType ) );
                writer.WriteAttributeString( "Index", i.ToString() );

                if ( itemType.GetTypeInfo().ImplementedInterfaces.Contains( typeof( Jypeli.Tagged ) ) )
                {
                    string tag = itemType.GetRuntimeProperty( "Tag" ).GetValue( item ).ToString();
                    writer.WriteAttributeString( "Tag", tag );
                }

                Serialize( writer, itemType, item, false );
                
                writer.WriteEndElement();
                i++;
            }

            writer.WriteAttributeString( "Count", i.ToString() );
            writer.WriteEndElement();
        }

        private static void applyMetadata( Type type, object obj, string tag, string value )
        {
#if JYPELI
            if ( type.GetTypeInfo().ImplementedInterfaces.Contains( typeof( IGameObject ) ) )
            {
                // Game object tags
                IGameObject gobj = (IGameObject)obj;
                
                if ( tag == "AddedToGame" && tagTrue( value ) && !gobj.IsAddedToGame )
                    Game.Instance.Add( gobj );
            }
#endif
        }

        private static void writeMetadata( XmlWriter writer, Type type, object obj )
        {
#if JYPELI
            if ( type.GetTypeInfo().ImplementedInterfaces.Contains( typeof( IGameObject ) ) && ( (IGameObject)obj ).IsAddedToGame )
            {
                writer.WriteStartElement( "Meta" );
                writer.WriteAttributeString( "AddedToGame", "1" );
                writer.WriteEndElement();
            }
#endif
        }

        private static bool tagTrue( string value )
        {
            return value == "1" || value.ToLower() == "true";
        }

        private static bool IsIllegalType( Type type )
        {
            return ( type == typeof( IntPtr ) || type == typeof( UIntPtr ) );
        }

        private static bool TryCreateObject( Type type, string tag, out object obj )
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
                    catch ( MissingMemberException )
                    {
                        obj = null;
                        return false;
                    }
                }
            }
        }

        private static string GenToString( Type type, object obj )
        {
            if ( type == typeof( string ) )
                return (string)obj;

            MethodInfo method = type.GetRuntimeMethod( "ToString", new Type[] { typeof(IFormatProvider) } );

            if ( method != null )
            {
                IFormatProvider provider = NumberFormatInfo.InvariantInfo;
                return (string)method.Invoke( obj, new object[] { NumberFormatInfo.InvariantInfo } );
            }

            method = type.GetRuntimeMethod( "ToString", new Type[] { } );

            if ( method != null )
            {
                return (string)method.Invoke( obj, new object[] { } );
            }

            throw new ArgumentException( "Object does not contain a ToString() method." );
        }

        private static object GenParse( Type type, string str )
        {
            if ( type == typeof( string ) )
                return str;

            if ( type == typeof( Type ) )
            {
                return TypeHelper.Parse( str );
            }

            MethodInfo method = type.GetRuntimeMethod( "Parse", new Type[] { typeof(string), typeof( IFormatProvider ) } );

            if ( method != null )
            {
                IFormatProvider provider = NumberFormatInfo.InvariantInfo;
                return method.Invoke( null, new object[] { str, provider } );
            }

            method = type.GetRuntimeMethod( "Parse", new Type[] { typeof(string), typeof( IFormatProvider ) } );

            if ( method != null )
            {
                return method.Invoke( null, new object[] { str } );
            }

            throw new ArgumentException( "Object does not contain a Parse() method." );
        }
    }
}
