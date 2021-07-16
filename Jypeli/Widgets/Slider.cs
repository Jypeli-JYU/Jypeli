#region MIT License
/*
 * Copyright (c) 2018 University of Jyväskylä, Department of Mathematical
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


namespace Jypeli.Widgets
{
    /// <summary>
    /// Liukusäädin
    /// </summary>
    public class Slider : BindableWidget
    {
        private bool pressedDown = false;
        private Touch touchObject = null;

        private Color _activeColor = Color.Red;
        private Color _inactiveColor = Color.DarkGray;

        /// <summary>
        /// Liukuva nuppi.
        /// </summary>
        public Widget Knob { get; private set; }

        /// <summary>
        /// Nupin väri kun hiiri on nupin päällä.
        /// </summary>
        public Color ActiveColor
        {
            get { return _activeColor; }
            set { _activeColor = value; }
        }

        /// <summary>
        /// Nupin väri kun hiiri ei ole nupin päällä.
        /// </summary>
        public Color InactiveColor
        {
            get { return _inactiveColor; }
            set { _inactiveColor = value; }
        }

        /// <summary>
        /// Ura, jossa liukusäädin liukuu.
        /// </summary>
        public Widget Track { get; private set; }

        /// <summary>
        /// Luo uuden liukusäätimen.
        /// </summary>
        /// <param name="width">Säätimen leveys.</param>
        /// <param name="height">Säätimen korkeus.</param>
        public Slider(double width, double height)
            : base(width, height)
        {
            Color = Color.Transparent;
            CapturesMouse = true;

            Track = new Widget(width, height / 3);
            Add(Track);

            Knob = new Widget(height, height, Shape.Circle);
            Knob.Color = Color.DarkGray;
            Add(Knob);

            AddedToGame += InitializeControls;
        }

        /// <summary>
        /// Luo uuden liukusäätimen.
        /// Sitoo liukusäätimen arvon mittarin arvoon.
        /// </summary>
        /// <param name="width">Säätimen leveys.</param>
        /// <param name="height">Säätimen korkeus.</param>
        /// <param name="meter">Mittari</param>
        public Slider(double width, double height, Meter meter)
            : this(width, height)
        {
            BindTo(meter);
        }

        private void InitializeControls()
        {
            /*var l1 = Game.Mouse.ListenOn(this, MouseButton.Left, ButtonState.Pressed, MousePress, null).InContext(this);
            var l2 = Game.Mouse.Listen(MouseButton.Left, ButtonState.Released, MouseRelease, null).InContext(this);
            var l3 = Game.Mouse.ListenMovement(1.0, MouseMove, null).InContext(this);

            var l4 = Game.TouchPanel.ListenOn(this, ButtonState.Pressed, TouchPress, null).InContext(this);
            var l5 = Game.TouchPanel.Listen(ButtonState.Released, TouchRelease, null).InContext(this);
            var l6 = Game.TouchPanel.Listen(ButtonState.Down, TouchMove, null).InContext(this);

            associatedListeners.AddItems(l1, l2, l3, l4, l5, l6);*/
        }

        /// <inheritdoc/>
        public override void BindTo(Meter meter)
        {
            pressedDown = false;
            base.BindTo(meter);
        }

        /// <inheritdoc/>
        public override void Unbind()
        {
            pressedDown = false;
            base.Unbind();
        }

        /// <summary>
        /// Päivittää liukusäätimen nupin arvoa vastaavaan sijaintiin
        /// </summary>
        protected override void UpdateValue()
        {
            if (Knob != null && Track != null && Meter != null)
                Knob.RelativePosition = new Vector(Track.RelativeLeft + Track.Width * Meter.RelativeValue, 0);
        }

        private void GenMove(Vector newPos)
        {
            Vector u = Vector.FromLengthAndAngle(1, this.Angle);
            double newVal = newPos.ScalarProjection(u);

            if (newVal < Track.RelativeLeft)
                Knob.RelativeLeft = Track.RelativeLeft;
            else if (newVal > Track.RelativeRight)
                Knob.RelativeRight = Track.RelativeRight;
            else
                Knob.RelativePosition = new Vector(newVal, 0);

            Meter.RelativeValue = (newVal - Track.RelativeLeft) / Track.Width;
        }

        private void MousePress()
        {
            if (pressedDown) return;
            UnsetChangedEvent();
            pressedDown = true;
        }

        private void MouseMove()
        {
            /*Knob.Color = pressedDown || Game.Mouse.IsCursorOn(this) ? _activeColor : _inactiveColor;

            if (pressedDown)
                GenMove(Game.Mouse.PositionOnScreen - this.Position);*/
        }

        private void MouseRelease()
        {
            if (!pressedDown) return;
            pressedDown = false;
            SetChangedEvent();
        }

        private void TouchPress(Touch touch)
        {
            if (touchObject != null) return;
            UnsetChangedEvent();
            touchObject = touch;
            Knob.Color = _activeColor;
        }

        private void TouchMove(Touch touch)
        {
            if (touchObject == touch)
                GenMove(touch.PositionOnScreen - this.Position);
        }

        private void TouchRelease(Touch touch)
        {
            if (touchObject == null) return;
            touchObject = null;
            SetChangedEvent();
            Knob.Color = _inactiveColor;
        }
    }
}
