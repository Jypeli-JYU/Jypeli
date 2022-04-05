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

using System.Collections.Generic;
using Jypeli.Controls;
using Silk.NET.Input;

namespace Jypeli
{
    public partial class Game : ControlContexted
    {
        private ListenContext context;
        private List<IController> controllers;

        /// <summary>
        /// Näppäimistö.
        /// </summary>
        public Keyboard Keyboard { get; private set; }

        /// <summary>
        /// Hiiri.
        /// </summary>
        public Mouse Mouse { get; private set; }

        /// <summary>
        /// Kiihtyvyysanturi.
        /// </summary>
        public Accelerometer Accelerometer
        {
            get { return Device.Accelerometer; }
        }

        /// <summary>
        /// Kosketusnäyttö
        /// </summary>
        public TouchPanel TouchPanel { get; private set; }

        /// <summary>
        /// Puhelimen takaisin-näppäin.
        /// </summary>
        public BackButton PhoneBackButton { get; private set; }

        /// <summary>
        /// Lista kaikista peliohjaimista järjestyksessä.
        /// </summary>
        public List<GamePad> GameControllers { get; private set; }

        /// <summary>
        /// Ensimmäinen peliohjain.
        /// </summary>
        public GamePad ControllerOne { get { return GameControllers[0]; } }

        /// <summary>
        /// Toinen peliohjain.
        /// </summary>
        public GamePad ControllerTwo { get { return GameControllers[1]; } }

        /// <summary>
        /// Kolmas peliohjain.
        /// </summary>
        public GamePad ControllerThree { get { return GameControllers[2]; } }

        /// <summary>
        /// Neljäs peliohjain.
        /// </summary>
        public GamePad ControllerFour { get { return GameControllers[3]; } }

        private IInputContext inputContext;

        /// <summary>
        /// Pelin pääohjainkonteksti.
        /// </summary>
        public ListenContext ControlContext
        {
            get { return Instance.context; }
        }

        /// <summary>
        /// Onko ohjauskonteksti modaalinen (ei)
        /// </summary>
        public bool IsModal
        {
            get { return false; }
        }

        private void InitControls()
        {
            inputContext = Window.CreateInput();
            context = new ListenContext() { Active = true };

            Keyboard = new Keyboard(inputContext);
            Mouse = new Mouse(Screen, inputContext);
            PhoneBackButton = new BackButton();
            TouchPanel = new TouchPanel();

            controllers = new List<IController>();
            GameControllers = new List<GamePad>(4);

            GameControllers.Add(new GamePad(inputContext, 0));
            GameControllers.Add(new GamePad(inputContext, 1));
            GameControllers.Add(new GamePad(inputContext, 2));
            GameControllers.Add(new GamePad(inputContext, 3));
            controllers.AddRange(GameControllers);

            controllers.Add(Mouse);

            controllers.Add(Keyboard);

            controllers.Add(Accelerometer);
            controllers.Add(TouchPanel);

            controllers.Add(PhoneBackButton);

        }

        private void UpdateControls(Time gameTime)
        {
            controllers.ForEach(c => c.Update());
            GameControllers.ForEach(g => g.UpdateVibrations(gameTime));
        }

        /// <summary>
        /// Poistaa kaikki ohjainkuuntelijat.
        /// </summary>
        public void ClearControls()
        {
            controllers.ForEach(c => c.Clear());
        }

        /// <summary>
        /// Näyttää kontrollien ohjetekstit.
        /// </summary>
        public void ShowControlHelp()
        {
            controllers.ForEach(c => MessageDisplay.Add(c.GetHelpTexts()));
        }

        /// <summary>
        /// Näyttää kontrollien ohjetekstit tietylle ohjaimelle.
        /// </summary>
        public void ShowControlHelp(IController controller)
        {
            MessageDisplay.Add(controller.GetHelpTexts());
        }

        private void ActivateObject(ControlContexted obj)
        {
            obj.ControlContext.Active = true;

            if (obj.IsModal)
            {
                Game.Instance.ControlContext.SaveFocus();
                Game.Instance.ControlContext.Active = false;

                foreach (Layer l in Layers)
                {
                    foreach (IGameObject lo in l.Objects)
                    {
                        ControlContexted co = lo as ControlContexted;
                        if (lo == obj || co == null)
                            continue;

                        co.ControlContext.SaveFocus();
                        co.ControlContext.Active = false;
                    }
                }
            }
        }

        private void DeactivateObject(ControlContexted obj)
        {
            obj.ControlContext.Active = false;

            if (obj.IsModal)
            {
                Game.Instance.ControlContext.RestoreFocus();

                foreach (Layer l in Layers)
                {
                    foreach (IGameObject lo in l.Objects)
                    {
                        ControlContexted co = lo as ControlContexted;
                        if (lo == obj || co == null)
                            continue;

                        co.ControlContext.RestoreFocus();
                    }
                }
            }
        }
    }
}
