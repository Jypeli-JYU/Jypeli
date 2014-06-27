﻿using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class Peli : Game
{
    public override void Begin()
    {
        Level.BackgroundColor = Color.Gray;

        //MessageDisplay.Position = new Vector(100, 100);
        MessageDisplay.MaxMessageCount = 30;
        MessageDisplay.BackgroundColor = Color.Transparent;
        MessageDisplay.MessageTime = TimeSpan.MaxValue;
        MessageDisplay.TextColor = Color.LightGray;

        Keyboard.Listen( Key.Escape, ButtonState.Pressed, Exit, null );
        PhoneBackButton.Listen( Exit, null );

        StorageFile tiedosto = null;

        try
        {
            tiedosto = DataStorage.Create("testi.txt");
            Assert(tiedosto.Name.EndsWith("testi.txt"), "tiedosto.Name.EndsWith(\"testi.txt\")");
            AssertNotNull(tiedosto.Stream, "tiedosto.Stream");
            tiedosto.Close();

            Assert(DataStorage.Exists("testi.txt"), "DataStorage.Exists(\"testi.txt\")");

            tiedosto = DataStorage.Open("testi.txt", false);
            Assert(tiedosto.Name.EndsWith("testi.txt"), "tiedosto.Name.EndsWith(\"testi.txt\")");
            AssertNotNull(tiedosto.Stream, "tiedosto.Stream");
            Assert(!tiedosto.Stream.CanWrite, "!tiedosto.Stream.CanWrite");
            tiedosto.Close();

            DataStorage.Delete("testi.txt");
            Assert(!DataStorage.Exists("testi.txt"), "!DataStorage.Exists(\"testi.txt\")");

            MessageDisplay.Add("---");

            DataStorage.MkDir("kansio");
            Assert(DataStorage.Exists("kansio"), "DataStorage.Exists(\"kansio\")");
            Assert( DataStorage.ChDir( "kansio" ) == true, "DataStorage.ChDir( \"kansio\" ) == true" );
            AssertValue(DataStorage.GetFileList().Count, 0, "DataStorage.GetFileList().Count");
            Assert( DataStorage.Exists( "kansio" ) == false, "DataStorage.Exists(\"kansio\") == false" );
            Assert( DataStorage.ChDir( ".." ) == true, "DataStorage.ChDir( \"..\" ) == true" );
            Assert( DataStorage.Exists( "kansio" ) == true, "DataStorage.Exists(\"kansio\") == true" );
            DataStorage.RmDir( "kansio" );

            Assert( DataStorage.Exists( "kansio" ) == false, "DataStorage.Exists(\"kansio\") == false" );
            Assert( DataStorage.ChDir( "kansio" ) == false, "DataStorage.ChDir( \"kansio\" ) == false" );

            MessageDisplay.Add("---");
            MessageDisplay.Add("PASSED!", Color.LimeGreen);
        }
        catch (TestException)
        {
            if (tiedosto != null && tiedosto.Stream.CanRead)
                tiedosto.Close();

            DataStorage.Delete("testi.txt");
        }
    }

    void Assert(bool ehto, string message)
    {
        Color color = ehto ? Color.LightGreen : Color.Red;
        MessageDisplay.Add(message, color);

        if (!ehto)
            throw new TestException();
    }

    void AssertNotNull(object obj, string objName)
    {
        if (obj != null)
        {
            MessageDisplay.Add(objName + " == " + obj.ToString(), Color.LightGreen);
        }
        else
        {
            MessageDisplay.Add(objName + " == NULL", Color.Red);
            throw new TestException();
        }
    }

    void AssertValue<T>(T varValue, T expected, string varName) where T : IEquatable<T>
    {
        Assert(varValue.Equals(expected), varName + " == " + expected);
    }

    private class TestException : Exception
    {
        public TestException()
            : base()
        {
        }
    }
}
