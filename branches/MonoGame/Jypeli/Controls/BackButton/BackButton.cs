﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jypeli.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

using XnaGamePad = Microsoft.Xna.Framework.Input.GamePad;
//using XnaButtons = Microsoft.Xna.Framework.Input.Buttons;

namespace Jypeli
{
    /// <summary>
    /// Puhelimen (tai peliohjaimen) takaisin-näppäin.
    /// </summary>
    public class BackButton : Controller<bool, Button>
    {
        internal override bool GetState()
        {
            return XnaGamePad.GetState( PlayerIndex.One ).IsButtonDown( Buttons.Back );
        }

        private static bool ButtonDown( bool prev, bool curr )
        {
            return curr;
        }

        /// <summary>
        /// Kuuntelee puhelimen takaisin-näppäintä.
        /// </summary>
        /// <param name="handler">Tapahtumankäsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        public Listener Listen( Action handler, string helpText )
        {
            return AddListener( BackButton.ButtonDown, Button.Back, "Back", helpText, handler );
        }

        /// <summary>
        /// Kuuntelee puhelimen takaisin-näppäintä.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler">Tapahtumankäsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p">Parametri.</param>
        public Listener Listen<T>( Action handler, string helpText, T p )
        {
            return AddListener( BackButton.ButtonDown, Button.Back, "Back", helpText, handler, p );
        }

        /// <summary>
        /// Kuuntelee puhelimen takaisin-näppäintä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="handler">Tapahtumankäsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">1. parametri.</param>
        /// <param name="p2">2. parametri.</param>
        public Listener Listen<T1, T2>( Action handler, string helpText, T1 p1, T2 p2 )
        {
            return AddListener( BackButton.ButtonDown, Button.Back, "Back", helpText, handler, p1, p2 );
        }

        /// <summary>
        /// Kuuntelee puhelimen takaisin-näppäintä.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="handler">Tapahtumankäsittelijä.</param>
        /// <param name="helpText">Ohjeteksti.</param>
        /// <param name="p1">1. parametri.</param>
        /// <param name="p2">2. parametri.</param>
        /// <param name="p3">3. parametri.</param>
        public Listener Listen<T1, T2, T3>( Action handler, string helpText, T1 p1, T2 p2, T3 p3 )
        {
            return AddListener( BackButton.ButtonDown, Button.Back, "Back", helpText, handler, p1, p2, p3 );
        }

        public void Enable()
        {
            EnableAll();
        }

        public void Disable()
        {
            DisableAll();
        }
    }
}
