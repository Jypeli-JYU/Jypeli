using System;
using System.Collections.Generic;
using Jypeli;

namespace SpeedKing
{
    public class Peli : Game
    {
        List<GameObject> painikkeet;
        List<GameObject> painamattomat;

        Animation painike0animaatio;
        Animation painike1animaatio;
        Animation painike2animaatio;
        Animation painike3animaatio;

        const double painikkeenLeveys = 170;
        const double painikkeenKorkeus = painikkeenLeveys;
        Timer painikkeidenSytytin;
        Timer nopeutusAjastin;
        Timer aikaaPainaaUuttaNappia;

        Label pistenaytto;

        int painikkeidenMaara;
        int montakoPainikettaPainettu;
        int edellinenSytytetty = 5;

        bool peliPaattynyt;

        Sound piip;

        // ScoreList topten = new ScoreList(10, false, 0);
        //--EasyHighScore topten = new EasyHighScore();

        public override void Begin()
        {
            ClearAll();
            Phone.DisplayResolution = DisplayResolution.Large;
            //--topten.Text = "SpeedKing Hall of Fame";

            painikkeet = new List<GameObject>();
            painamattomat = new List<GameObject>();
            painikkeidenMaara = 4;

            Level.Width = 800;
            Level.Height = 480;

            Level.Background.Image = LoadImage("alkukuva");
            Level.Background.Size = new Vector(Level.Width, Level.Height);
            TouchPanel.Listen(ButtonState.Pressed, delegate(Touch t) { AlustaPeli(); }, null);
            PhoneBackButton.Listen(Exit, "");

        }

        void AlustaPeli()
        {
            Level.Background.Image = LoadImage("tausta");
            Level.Background.Size = new Vector(Level.Width, Level.Height);
            Level.BackgroundColor = Color.Black;
            MessageDisplay.TextColor = Color.White;
            Camera.ZoomToLevel();

            //if (DataStorage.Exists("topten.xml"))
            //{
            //    //topfive = DataStorage.Load<ScoreList>(topfive, "topten.xml");
            //}

            LisaaPainikkeet();
            LuoAjastimet();
            LataaAanet();

            LisaaPistenaytto();
            AloitaPeliJaResetoiKosketus();
        }

        /*public override void Continue()
        {
            Begin();
        }*/

        void LataaAanet()
        {
            SoundEffect siniaalto = LoadSoundEffect("sinewave");
            piip = siniaalto.CreateSound();
        }


        void LuoAjastimet()
        {
            painikkeidenSytytin = new Timer();
            painikkeidenSytytin.Timeout += SytytaJokinPainike;

            nopeutusAjastin = new Timer();
            nopeutusAjastin.Interval = 5.1;
            nopeutusAjastin.Timeout += NopeutaPelia;

            aikaaPainaaUuttaNappia = new Timer();
            aikaaPainaaUuttaNappia.Interval = 3;
            aikaaPainaaUuttaNappia.Timeout += PeliPaattyy;
        }


        void LisaaPistenaytto()
        {
            pistenaytto = new Label(montakoPainikettaPainettu.ToString());
            pistenaytto.X = 0;
            pistenaytto.Y = -150;
            pistenaytto.TextColor = Color.White;
            pistenaytto.Font = Font.DefaultLargeBold;
            pistenaytto.SizeMode = TextSizeMode.StretchText;
            pistenaytto.Size *= 3;
            Add(pistenaytto);
        }

        void LisaaPainikkeet()
        {
            Image[] painike0kuvat = LoadImages("0", "00");
            Image[] painike1kuvat = LoadImages("1", "11");
            Image[] painike2kuvat = LoadImages("2", "22");
            Image[] painike3kuvat = LoadImages("3", "33");
            painike0animaatio = new Animation(painike0kuvat);
            painike1animaatio = new Animation(painike1kuvat);
            painike2animaatio = new Animation(painike2kuvat);
            painike3animaatio = new Animation(painike3kuvat);
            painike0animaatio.FPS = 12;
            painike1animaatio.FPS = 12;
            painike2animaatio.FPS = 12;
            painike3animaatio.FPS = 12;

            for (int i = 0; i < painikkeidenMaara; i++)
            {
                Add(LuoPainike());
            }
        }

        GameObject LuoPainike()
        {
            GameObject painike = new GameObject(painikkeenLeveys, painikkeenKorkeus);
            painikkeet.Add(painike);
            painike.X = -((painikkeidenMaara / 2) * (painikkeenLeveys + 20)) +
                        ((painikkeet.Count) * (painikkeenLeveys + 20)) - (painikkeenLeveys / 2);
            painike.Tag = painikkeet.Count.ToString();
            painike.Image = LoadImage((painikkeet.Count - 1).ToString() + "_");
            return painike;
        }

        void LisaaNappaimet()
        {
            Keyboard.Listen(Key.A, ButtonState.Pressed, NappainPainettu, null, 0);
            Keyboard.Listen(Key.S, ButtonState.Pressed, NappainPainettu, null, 1);
            Keyboard.Listen(Key.D, ButtonState.Pressed, NappainPainettu, null, 2);
            Keyboard.Listen(Key.F, ButtonState.Pressed, NappainPainettu, null, 3);

            TouchPanel.Listen(ButtonState.Pressed, NayttoaPainettu, null);
            PhoneBackButton.Listen(Begin, "");

            #region debug

            Keyboard.Listen(Key.Left, ButtonState.Down, delegate()
            {
                pistenaytto.X -= 2;
                MessageDisplay.Add("" + pistenaytto.X + " " + pistenaytto.Y);
            }, null);
            Keyboard.Listen(Key.Right, ButtonState.Down, delegate()
            {
                pistenaytto.X += 2;
                MessageDisplay.Add("" + pistenaytto.X + " " + pistenaytto.Y);
            }, null);
            Keyboard.Listen(Key.Down, ButtonState.Down, delegate()
            {
                pistenaytto.Y -= 2;
                MessageDisplay.Add("" + pistenaytto.X + " " + pistenaytto.Y);
            }, null);
            Keyboard.Listen(Key.Up, ButtonState.Down, delegate()
            {
                pistenaytto.Y += 2;
                MessageDisplay.Add("" + pistenaytto.X + " " + pistenaytto.Y);
            }, null);

            #endregion
        }

        void NayttoaPainettu(Touch kosketus)
        {
            for (int i = 0; i < painikkeet.Count; i++)
            {
                if (painikkeet[i].IsInside(kosketus.PositionOnWorld))
                {
                    NappainPainettu(i);
                    return;
                }
            }
        }

        void NappainPainettu(int painikeIndeksi)
        {
            Phone.Vibrate(50);
            if (painamattomat.Count < 1 || painamattomat[0].Tag.ToString() != (painikeIndeksi + 1).ToString())
            {
                PeliPaattyy();
            }
            else
            {
                painamattomat.RemoveAt(0);
                aikaaPainaaUuttaNappia.Reset();
                pistenaytto.Text = (++montakoPainikettaPainettu).ToString();
            }
        }

        void PeliPaattyy()
        {
            Phone.Vibrate(1500);
            peliPaattynyt = true;
            ClearControls();
            PhoneBackButton.Listen(Begin, "");
            painikkeidenSytytin.Stop();
            nopeutusAjastin.Stop();
            aikaaPainaaUuttaNappia.Stop();
            painamattomat.Clear();
            for (int i = 0; i < painikkeet.Count; i++)
            {
                Sytyta(i);
            }
            Timer.SingleShot(1, NaytaTopTenJaSalliUudenPelinAloitus);
        }

        void NaytaTopTenJaSalliUudenPelinAloitus()
        {
            //topten.CongratulationText = "Anna nimi";
            //topten.ScreenList.BackGroundColor = Color.DarkGray;
            //topten.Show(montakoPainikettaPainettu);
            //HighScoreWindow h = new HighScoreWindow("SpeedKing", "Hall of fame", "You reached top ten!", topten, montakoPainikettaPainettu);
            //h.Width = 700;
            //h.Height = 400;
            //h.Closed += NimiAnnettu;
            //Add(h);
            //--topten.EnterAndShow(montakoPainikettaPainettu);

            Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaPeli, null);
            TouchPanel.Listen(ButtonState.Pressed, delegate { AloitaPeliJaResetoiKosketus(); }, null);
        }

        void AloitaPeliJaResetoiKosketus()
        {
            ClearControls();
            AloitaPeli();
        }

        void AloitaPeli()
        {
            peliPaattynyt = false;
            LisaaNappaimet();

            for (int i = 0; i < painikkeet.Count; i++)
            {
                Sammuta(i);
            }
            montakoPainikettaPainettu = 0;
            pistenaytto.Text = "0";
            painikkeidenSytytin.Interval = 1;
            painikkeidenSytytin.Start();
            nopeutusAjastin.Start();
            aikaaPainaaUuttaNappia.Start();
        }

        void NopeutaPelia()
        {
            painikkeidenSytytin.Interval /= 1.11;
            nopeutusAjastin.Interval = painikkeidenSytytin.Interval * 7.0 + 0.1;
        }

        void SytytaJokinPainike()
        {
            int sytytettavanIndeksi;
            // Arvotaan sytytettävä nappula, pitää olla eri kuin edellinen sytytetty
            do
            {
                sytytettavanIndeksi = RandomGen.NextInt(0, painikkeet.Count);
            } while (sytytettavanIndeksi == edellinenSytytetty);

            Sytyta(sytytettavanIndeksi);
            painamattomat.Add(painikkeet[sytytettavanIndeksi]);
            Timer.SingleShot(painikkeidenSytytin.Interval / 1.4, delegate() { Sammuta(sytytettavanIndeksi); });
            Timer.SingleShot(painikkeidenSytytin.Interval / 2, SammutaAani);
            switch (sytytettavanIndeksi)
            {
                case 0:
                    piip.Pitch = -1;
                    break;
                case 1:
                    piip.Pitch = -.75;
                    break;
                case 2:
                    piip.Pitch = -0.4;
                    break;
                case 3:
                    piip.Pitch = -0.15;
                    break;
            }

            piip.Play();
        }

        void Sytyta(int i)
        {
            switch (i)
            {
                case 0:
                    painikkeet[i].Animation = painike0animaatio;
                    break;
                case 1:
                    painikkeet[i].Animation = painike1animaatio;
                    break;
                case 2:
                    painikkeet[i].Animation = painike2animaatio;
                    break;
                case 3:
                    painikkeet[i].Animation = painike3animaatio;
                    break;
            }
            painikkeet[i].Animation.Start();
            edellinenSytytetty = i;
        }

        void SammutaAani()
        {
            piip.Stop();
        }

        void Sammuta(int i)
        {
            if (!peliPaattynyt) // Ei sammuteta nappulaa jos peli on päättynyt.
            {
                painikkeet[i].Animation.Stop();
                painikkeet[i].Image = LoadImage(i.ToString() + "_");
            }
        }

        protected override void Update(Time time)
        {
            base.Update(time);
        }
    }
}
