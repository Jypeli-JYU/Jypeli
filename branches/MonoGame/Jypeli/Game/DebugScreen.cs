using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli.Widgets;
using Microsoft.Xna.Framework;

namespace Jypeli
{
    public partial class Game
    {
        private string fpsText = "00";
        private int fpsSkipCounter;

        /// <summary>
        /// Debug-ruutukerros, joka näkyy kun painetaan F12.
        /// Voit lisätä olioita myös tälle kerrokselle.
        /// </summary>
        public Layer DebugLayer { get; private set; }

        /// <summary>
        /// Debug-ruutu F12-näppäimestä päällä / pois.
        /// </summary>
        public bool DebugKeyEnabled { get; set; }

        /// <summary>
        /// Debug-ruutu näkyvissä / pois.
        /// </summary>
        public bool DebugScreenVisible { get; set; }

        /// <summary>
        /// FPS-ikkuna.
        /// </summary>
        public Window FPSWindow { get; private set; }

        /// <summary>
        /// FPS-näyttö.
        /// </summary>
        public Label FPSDisplay { get; private set; }

        /// <summary>
        /// "Layers"-ikkuna. Huom. asettaa kokonsa automaattisesti.
        /// </summary>
        public Window LayerWindow { get; private set; }

        /// <summary>
        /// "Layers"-näyttö.
        /// </summary>
        public Label LayerDisplay { get; private set; }

        private void InitDebugScreen()
        {
            DebugLayer = Layer.CreateStaticLayer();
            DebugLayer.Objects.ItemAdded += OnObjectAdded;
            DebugLayer.Objects.ItemRemoved += OnObjectRemoved;

            FPSWindow = new Jypeli.Window();
            FPSWindow.IsModal = false;
            FPSWindow.Color = new Color( Color.HotPink, 100 );
            DebugLayer.Add( FPSWindow );

            FPSDisplay = new Label( "00" );
            FPSDisplay.Color = Color.HotPink;
            FPSWindow.Size = 1.5 * FPSDisplay.Size;
            FPSWindow.Add( FPSDisplay );

            FPSWindow.Right = Screen.Right - Screen.Width / 16;
            FPSWindow.Top = Screen.Top;

            LayerWindow = new Jypeli.Window();
            LayerWindow.IsModal = false;
            LayerWindow.Color = new Color( Color.Blue, 100 );
            DebugLayer.Add( LayerWindow );

            LayerDisplay = new Label( "Layers: no data" );
            LayerDisplay.TextColor = Color.White;
            LayerWindow.Add( LayerDisplay );
            LayerWindow.Size = LayerDisplay.Size;
            LayerWindow.Left = Screen.Left;
            LayerWindow.Top = Screen.Top - 200;

            DebugKeyEnabled = true;
            DebugScreenVisible = false;
        }

        private StringBuilder layerTextBuilder = new StringBuilder();
        private const string layerTextTitle = "Layers:\n";

        private void UpdateDebugScreen( Time time )
        {
            if ( DebugKeyEnabled && Keyboard.GetKeyState( Key.F12 ) == ButtonState.Pressed )
                DebugScreenVisible = !DebugScreenVisible;

            DebugLayer.Update( time );

            if ( !DebugScreenVisible )
                return;
            
            if ( fpsSkipCounter++ > 10 )
            {
                fpsSkipCounter = 0;
                fpsText = ( 1.0 / Time.SinceLastUpdate.TotalSeconds ).ToString( "F2" );
            }

            FPSDisplay.Text = fpsText;

            layerTextBuilder.Clear();
            layerTextBuilder.Append( layerTextTitle );

            for (int i = Layers.FirstIndex; i <= Layers.LastIndex; i++)
            {
                layerTextBuilder.Append( "[" );
                layerTextBuilder.Append( i );
                layerTextBuilder.Append( "]: " );
                layerTextBuilder.Append( Layers[i].Objects.Count );
                layerTextBuilder.AppendLine();
            }

            LayerDisplay.Text = layerTextBuilder.ToString();
            LayerWindow.Size = LayerDisplay.Size;
        }

        private void DrawDebugScreen()
        {
            if ( DebugScreenVisible )
                DebugLayer.Draw( Camera );
        }
    }
}
