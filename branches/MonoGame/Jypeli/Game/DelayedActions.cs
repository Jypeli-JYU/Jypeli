﻿#region MIT License
/*
 * Copyright (c) 2013 University of Jyväskylä, Department of Mathematical
 * Information Technology.
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

/*
 * Authors: Tero Jäntti, Tomi Karppinen, Janne Nikkanen.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jypeli
{
    public partial class Game
    {
        Queue<Action> PendingActions = new Queue<Action>();

        /// <summary>
        /// Tapahtuu kun Game.Instance on alustettu.
        /// </summary>
        public static event Action InstanceInitialized;

        /// <summary>
        /// Tapahtuu kun peli lopetetaan.
        /// </summary>
        public static new event Action Exiting;

        /// <summary>
        /// Suorittaa aliohjelman kun peli on varmasti alustettu.
        /// </summary>
        /// <param name="actionMethod">Suoritettava aliohjelma.</param>
        public static void AssertInitialized( Action actionMethod )
        {
            if ( Instance != null )
                actionMethod();
            else
                InstanceInitialized += actionMethod;
        }

        /// <summary>
        /// Suorittaa aliohjelman seuraavalla päivityksellä.
        /// </summary>
        /// <param name="action"></param>
        public static void DoNextUpdate( Action action )
        {
            if ( Instance != null )
                Instance.PendingActions.Enqueue( action );
            else
                InstanceInitialized += action;
        }

        /// <summary>
        /// Suorittaa aliohjelman seuraavalla päivityksellä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        public static void DoNextUpdate<T1>( Action<T1> action, T1 p1 )
        {
            DoNextUpdate( delegate { action( p1 ); } );
        }

        /// <summary>
        /// Suorittaa aliohjelman seuraavalla päivityksellä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="action"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public static void DoNextUpdate<T1, T2>( Action<T1, T2> action, T1 p1, T2 p2 )
        {
            DoNextUpdate( delegate { action( p1, p2 ); } );
        }

        /// <summary>
        /// Suorittaa aliohjelman kun peli on varmasti alustettu.
        /// </summary>
        /// <param name="actionMethod">Suoritettava aliohjelma.</param>
        public static void AssertInitialized<T1>( Action<T1> actionMethod, T1 o1 )
        {
            if ( Instance != null )
                actionMethod( o1 );
            else
                InstanceInitialized += delegate { actionMethod( o1 ); };
        }

        protected override void OnExiting( object sender, EventArgs args )
        {
            if ( Exiting != null )
                Exiting();

            base.OnExiting( sender, args );
        }

        private void ExecutePendingActions()
        {
            while ( PendingActions.Count > 0 )
                PendingActions.Dequeue()();
        }
    }
}
