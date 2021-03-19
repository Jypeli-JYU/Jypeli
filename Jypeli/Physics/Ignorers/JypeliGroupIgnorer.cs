#region MIT License
/*
 * Copyright (c) 2005-2008 Jonathan Mark Porter. http://physics2d.googlepages.com/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal 
 * in the Software without restriction, including without limitation the rights to 
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
 * the Software, and to permit persons to whom the Software is furnished to do so, 
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be 
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
 * PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion



#if UseDouble
using Scalar = System.Double;
#else
using Scalar = System.Single;
#endif
using System;
using Jypeli.Physics;

namespace Jypeli
{
    public class JypeliGroupIgnorer : Ignorer
    {
        /// <summary>
        /// Vanha törmäysryhmä yhteensopivuutta varten.
        /// </summary>
        public int LegacyGroup { get; set; }

        /// <summary>
        /// Törmäysmaski: 0 = törmää, 1 = ei törmää.
        /// </summary>
        int IgnoreMask;

        public override bool BothNeeded
        {
            get { return true; }
        }

        public override bool CanCollide( IPhysicsBody thisBody, IPhysicsBody otherBody, Ignorer other )
        {
            JypeliGroupIgnorer jOther = other as JypeliGroupIgnorer;
            if ( jOther == null ) return true;

            return ( this.LegacyGroup == 0 || jOther.LegacyGroup == 0 || this.LegacyGroup != jOther.LegacyGroup ) && ( this.IgnoreMask & jOther.IgnoreMask ) == 0;
        }

        public JypeliGroupIgnorer()
        {
        }

        public JypeliGroupIgnorer( params int[] groups )
        {
            for ( int i = 0; i < groups.Length; i++ )
                AddGroup( groups[i] );
        }

        public void AddGroup( int groupIndex )
        {
            if ( groupIndex > 32 )
                throw new ArgumentException( "A maximum of 32 groups is supported." );

            if ( groupIndex <= 0 )
                throw new ArgumentException( "Collision group indexes start from 1." );

            IgnoreMask |= ( 1 << ( groupIndex - 1 ) );
        }

        public void RemoveGroup( int groupIndex )
        {
            if ( groupIndex > 32 )
                throw new ArgumentException( "A maximum of 32 groups is supported." );

            if ( groupIndex <= 0 )
                throw new ArgumentException( "Collision group indexes start from 1." );

            IgnoreMask &= (int)( uint.MaxValue - ( 1 << ( groupIndex - 1 ) ) );
        }

        public bool TestGroupIgnore( int groupIndex )
        {
            if ( LegacyGroup != 0 && LegacyGroup == groupIndex )
                return true;

            if ( groupIndex > 32 )
                throw new ArgumentException( "A maximum of 32 groups is supported." );

            if ( groupIndex <= 0 )
                throw new ArgumentException( "Collision group indexes start from 1." );

            return ( this.IgnoreMask & ( 1 << ( groupIndex - 1 ) ) ) != 0;
        }
    }
}
