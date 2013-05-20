#region MIT License
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
using Jypeli.Controls;

namespace Jypeli
{
    public partial class Game : ControlContexted
    {
        private ListenContext _context = new ListenContext() { Active = true };

        public Keyboard Keyboard { get; private set; }
        public Mouse Mouse { get; private set; }
        public TouchPanel TouchPanel { get; private set; }

        /// <summary>
        /// Pelin pääohjainkonteksti.
        /// </summary>
        public ListenContext ControlContext
        {
            get { return Instance._context; }
        }

        public bool IsModal
        {
            get { return false; }
        }

        private void InitControls()
        {
            Keyboard = new Keyboard();
            Mouse = new Mouse();
            TouchPanel = new TouchPanel();
        }

        private void UpdateControls()
        {
            Keyboard.Update();
            Mouse.Update();
            TouchPanel.Update();
        }

        private void ActivateObject( ControlContexted obj )
        {
            obj.ControlContext.Active = true;

            if ( obj.IsModal )
            {
                Game.Instance.ControlContext.SaveFocus();
                Game.Instance.ControlContext.Active = false;

                foreach ( Layer l in Layers )
                {
                    foreach ( IGameObject lo in l.Objects )
                    {
                        ControlContexted co = lo as ControlContexted;
                        if ( lo == obj || co == null )
                            continue;

                        co.ControlContext.SaveFocus();
                        co.ControlContext.Active = false;
                    }
                }
            }
        }

        private void DeactivateObject( ControlContexted obj )
        {
            obj.ControlContext.Active = false;

            if ( obj.IsModal )
            {
                Game.Instance.ControlContext.RestoreFocus();

                foreach ( Layer l in Layers )
                {
                    foreach ( IGameObject lo in l.Objects )
                    {
                        ControlContexted co = lo as ControlContexted;
                        if ( lo == obj || co == null )
                            continue;

                        co.ControlContext.RestoreFocus();
                    }
                }
            }
        }
    }
}
