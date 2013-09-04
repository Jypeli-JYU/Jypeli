using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Peli : Game
{
    StringList vastaukset = new StringList();
    ScoreList hiscoret;

    public override void Begin()
    {
        SetWindowSize( 1024, 768 );

        IsMouseVisible = true;
        Phone.DisplayOrientation = DisplayOrientation.Portrait;
        Phone.DisplayResolution = DisplayResolution.Large;

        PushButton inputNappula = new PushButton( 300, 50, "InputWindow (1)" );
        inputNappula.Top = Screen.Top - 20;
        inputNappula.Width = 2 * Screen.Width / 3;
        inputNappula.Clicked += new Action( ShowInputWindow );
        //Add( inputNappula );

        PushButton viestiNappula = new PushButton( 300, 50, "MessageWindow (2)" );
        viestiNappula.Top = Screen.Top - 100;
        viestiNappula.Width = 2 * Screen.Width / 3;
        viestiNappula.Clicked += new Action( ShowMessageWindow );
        Add( viestiNappula );

        PushButton listaNappula = new PushButton( 300, 50, "StringListWindow (3)" );
        listaNappula.Top = Screen.Top - 180;
        listaNappula.Width = 2 * Screen.Width / 3;
        listaNappula.Clicked += new Action( ShowListWindow );
        //Add( listaNappula );

        PushButton hiscoreNappula = new PushButton( 300, 50, "Parhaat pisteet (4)" );
        hiscoreNappula.Top = Screen.Top - 260;
        hiscoreNappula.Width = 2 * Screen.Width / 3;
        hiscoreNappula.Clicked += new Action( ShowHighScores );
        //Add( hiscoreNappula );

        PushButton valintaNappula = new PushButton( 300, 50, "Monivalinta (5)" );
        valintaNappula.Top = Screen.Top - 340;
        valintaNappula.Width = 2 * Screen.Width / 3;
        valintaNappula.Clicked += new Action( ShowMultiSelect );
        //Add( valintaNappula );

        PushButton AddItemNappula = new PushButton( "Add item (6)" );
        AddItemNappula.Top = Screen.Bottom + 340;
        AddItemNappula.Width = 2 * Screen.Width / 6;
        AddItemNappula.Clicked += AddRandomAnswer;
        //Add( AddItemNappula );

        PushButton AddTenItemsButton = new PushButton( "Add ten items (7)" );
        AddTenItemsButton.Top = Screen.Bottom + 260;
        AddTenItemsButton.Width = 2 * Screen.Width / 6;
        AddTenItemsButton.Clicked += AddTenItems;
        //Add( AddTenItemsButton );

        hiscoret = new ScoreList( 10, false, 0 );
        //hiscoret = DataStorage.TryLoad<ScoreList>( hiscoret, "pisteet.xml" );

        GameObject liikkio = new GameObject( 30, 30 );
        liikkio.Y = hiscoreNappula.Y - 50;
        Add( liikkio );
        Keyboard.Listen( Key.Left, ButtonState.Down, Liikuta, null, liikkio, -20 * Vector.UnitX );
        Keyboard.Listen( Key.Right, ButtonState.Down, Liikuta, null, liikkio, 20 * Vector.UnitX );
        Keyboard.Listen( Key.Up, ButtonState.Down, Liikuta, null, liikkio, 20 * Vector.UnitY );
        Keyboard.Listen( Key.Down, ButtonState.Down, Liikuta, null, liikkio, -20 * Vector.UnitY );

        Keyboard.Listen( Key.D1, ButtonState.Pressed, ShowInputWindow, "InputWindow" );
        Keyboard.Listen( Key.D2, ButtonState.Pressed, ShowMessageWindow, "MessageWindow" );
        Keyboard.Listen( Key.D3, ButtonState.Pressed, ShowListWindow, "StringListWindow" );
        Keyboard.Listen( Key.D4, ButtonState.Pressed, ShowHighScores, "HighScore" );
        Keyboard.Listen( Key.D5, ButtonState.Pressed, ShowMultiSelect, "MultiSelectWindow" );
        Keyboard.Listen( Key.D6, ButtonState.Pressed, AddRandomAnswer, "Add one item" );
        Keyboard.Listen( Key.D7, ButtonState.Pressed, AddTenItems, "Add ten items" );

        Keyboard.Listen( Key.Escape, ButtonState.Pressed, ConfirmExit, "Exit" );

        ShowControlHelp();
    }

    void AddTenItems()
    {
        for ( int i = 0; i < 10; i++ )
            AddRandomAnswer();
    }

    void AddRandomAnswer()
    {
        string[] choises =
        {
            "foo",
            "bar",
            "kissa",
            "hevonen",
            "MUU MUU",
            "BÄÄ BÄÄ",
            "Ihanaa Leijonat, Ihanaa",
        };
        vastaukset.Add( choises[RandomGen.NextInt( choises.Length )] );
        MessageDisplay.Add( "Added" );
    }

    void Liikuta( GameObject olio, Vector suunta )
    {
        olio.Position += suunta;
    }

    void ShowInputWindow()
    {
        InputWindow kysymysIkkuna = new InputWindow( "Vastaa kysymykseen" );
        kysymysIkkuna.TextEntered += new InputWindow.InputWindowHandler( ProcessInput );
        Add( kysymysIkkuna );
    }

    void ProcessInput( InputWindow sender )
    {
        string vastaus = sender.InputBox.Text;
        vastaukset.Add( vastaus );
    }

    void ShowMessageWindow()
    {
        string viimeVastaus = vastaukset.Count > 0 ? vastaukset[vastaukset.Count - 1] : "Et ole syöttänyt vielä mitään!";
        MessageWindow vastausIkkuna = new MessageWindow( viimeVastaus );
        Add( vastausIkkuna );
    }

    protected override void Update( Time time )
    {
        base.Update( time );
    }

    void ShowListWindow()
    {
        StringListWindow listaIkkuna = new StringListWindow( "Tässä on lista syöttämistäsi vastauksista" );
        listaIkkuna.List.Bind( vastaukset );
        Add( listaIkkuna );
    }

    void ShowHighScores()
    {
        //HighScoreWindow topIkkuna = new HighScoreWindow(
        //    "Eniten syötettyjä vastauksia:",
        //    "Onneksi olkoon, pääsit listalle pisteillä %p! Anna nimesi:", hiscoret, vastaukset.Count );
        //topIkkuna.Closed += new Jypeli.Widgets.Window.WindowHandler( SaveHighscores );
        //Add( topIkkuna );
    }

    //void SaveHighscores( Window sender )
    //{
    //    DataStorage.Save<ScoreList>( hiscoret, "pisteet.xml" );
    //}

    void ShowMultiSelect()
    {
        try
        {
            MultiSelectWindow monivalinta = new MultiSelectWindow( "Valitse näistä:", vastaukset.ToArray<string>() );
            monivalinta.ItemSelected += new Action<int>( monivalinta_ItemSelected );
            monivalinta.DefaultCancel = -1;
            Add( monivalinta );
        }
        catch ( InvalidOperationException )
        {
            MessageWindow msg = new MessageWindow( "Lista on tyhjä, monivalintaa ei voi näyttää" );
            Add( msg );
        }
    }

    void monivalinta_ItemSelected( int i )
    {
        MessageDisplay.Add( "Valintasi: " + vastaukset[i] );
    }
}
