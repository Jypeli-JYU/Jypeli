using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public abstract class TypeHelper
    {
#if !WINRT

        public static bool InheritsFrom( Type actual, Type expected )
        {
            Type t = actual;

            do
            {
                if ( t == expected ) return true;
                t = t.BaseType;
            } while ( t != null && t != typeof( object ) );

            return false;
        }
#endif

        public static string ToString( Type type )
        {
            /* When objects of class Type are stored, things get a little
             * tricky:
             * 
             * The parameter type may be the type object of an instance
             * of class Type (typeof(Type).GetType()). Such a type object
             * has type System.RuntimeType, which is internal to the system.
             * As such, creating instances of it is not allowed.
             * 
             * In order to be able to read back the type objects, their type
             * should be System.Type instead of System.RuntimeType.
             */
            var runtimeType = type.GetType();
            if ( type == runtimeType )
            {
                return typeof( Type ).AssemblyQualifiedName;
            }

            StringBuilder sb = new StringBuilder( type.AssemblyQualifiedName );

#if WINRT
            if ( !type.IsConstructedGenericType ) return sb.ToString();
            Type[] genargs = type.GenericTypeArguments;
#else
            if ( !type.ContainsGenericParameters ) return sb.ToString();
            Type[] genargs = type.GetGenericArguments();
#endif

            sb.Append( '<' );

            for ( int i = 0; i < genargs.Length; i++ )
            {
                sb.Append( TypeHelper.ToString( genargs[i] ) );
                sb.Append( ',' );
            }

            sb[sb.Length - 1] = '>';
            return sb.ToString();
        }

        public static Type Parse( string typeStr )
        {
            int genOpen = typeStr.IndexOf( '<' );
            int genClose = typeStr.IndexOf( '>' );

            if ( genOpen < 0 && genClose < 0 ) return Type.GetType( typeStr );
            if ( genOpen >= 0 && genClose < 0 ) throw new ArgumentException( "Unterminated < in type string: " + typeStr );
            if ( genOpen < 0 && genClose >= 0 ) throw new ArgumentException( "Unexpected > in type string: " + typeStr );

            Type parsedType = Type.GetType( typeStr.Substring( 0, genOpen ) );
            string[] genargStrings = typeStr.Substring( genOpen + 1, genClose - genOpen - 1 ).Split( ',' );
            Type[] genargs = new Type[genargStrings.Length];

            for ( int i = 0; i < genargStrings.Length; i++ )
            {
                genargs[i] = TypeHelper.Parse( genargStrings[i] );
            }

            return parsedType;
        }
    }
}
