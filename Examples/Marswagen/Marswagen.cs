#region Usings
using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;
using Jypeli.Assets;
using Jypeli.Effects;
#endregion

public class Marswagen : PhysicsGame
{
    #region muuttujat
    Tank tankki;
    PhysicsObject maa;
    AssaultRifle kk;

    DoubleMeter hpMittari;
    DoubleMeter voimaMittari;
    IntMeter miesLaskuri;
    IntMeter kopteriLaskuri;

    bool tahtaysavustin;

    List<PhysicsObject> kopteriLista = new List<PhysicsObject>();
    List<PhysicsObject> tyyppiLista = new List<PhysicsObject>();

    ExplosionSystem tykkiSuuliekki;
    ExplosionSystem kopteriRajahdys;
    ExplosionSystem tykinOsuma;
    ExplosionSystem tirskahdus;
    ExplosionSystem tankkiRajahdys;
    ExplosionSystem tankkiinOsui;

    Timer kopteriAjastin;

    Image taustaKuva = LoadImage("big_landscape");
    Image tykinKuva = LoadImage("tykki");
    Image[] kopterinKuvat = LoadImages("kopteri1", "kopteri2");
    Image[] kopteriPutaaKuvat = LoadImages("kopterispin1", "kopterispin2", "kopterispin3", "kopterispin4");
    Image[] laskuvarjoKuvat = LoadImages("ukko1", "ukko2", "ukko3", "ukko4", "ukko5", "ukko6");
    Image[] ukkoPutoaaKuvat = LoadImages("ukko_putoaa");
    Image[] ukkoMaassaKuvat = LoadImages("ukko_maassa");
    Image liekinKuva = LoadImage("liekki");
    Image pisteKuva = LoadImage("piste");
    Image tankkiPartikkeli = LoadImage("tankkipartikkeli");
    Image osumaPartikkeli = LoadImage("osumapartikkeli");

    // Peilikuvat aikaisemmista. Ei voi alustaa suoraan tässä.
    Image[] laskuvarjo2Kuvat;
    Image[] ukkoMaassaOikealleKuvat;
    #endregion

    #region alustukset

    public override void Begin()
    {
        SetWindowSize(1280, 720);
        LataaKuvat();
        AloitaAlusta();
    }

    void LataaKuvat()
    {
        laskuvarjo2Kuvat = Image.Mirror(laskuvarjoKuvat);
        ukkoMaassaOikealleKuvat = Image.Mirror(ukkoMaassaKuvat);
    }

    void AloitaAlusta()
    {
        ClearAll();

        kopteriLista.Clear();
        tyyppiLista.Clear();

        LuoKentta();
        LisaaLaskurit();
        LisaaHpPalkki();
        lisaaTykinVoimaPalkki();
        LataaEfektit();
        AsetaNapit();

        kopteriAjastin = new Timer();
        kopteriAjastin.Interval = 1.2;
        kopteriAjastin.Timeout += kopteriPaivitys;
        kopteriAjastin.Start();
    }

    void LisaaLaskurit()
    {
        Widget fragiNaytto = new Widget(new VerticalLayout());
        fragiNaytto.BorderColor = Color.Black;
        fragiNaytto.Color = Color.Transparent;
        Label miesLabel = new Label();

        miesLaskuri = new IntMeter(0);
        miesLabel.BindTo(miesLaskuri);

        fragiNaytto.Add(new Label("Miehiä torjuttu: "));
        fragiNaytto.Add(miesLabel);

        fragiNaytto.X = Screen.LeftSafe + 150;
        fragiNaytto.Y = Screen.BottomSafe + 50;
        Add(fragiNaytto);

        Widget kopteriNaytto = new Widget(new VerticalLayout());
        kopteriNaytto.BorderColor = Color.Black;
        kopteriNaytto.Color = Color.Transparent;
        Label kopteriLabel = new Label();

        kopteriLaskuri = new IntMeter(0);
        kopteriLabel.BindTo(kopteriLaskuri);

        kopteriNaytto.Add(new Label("Koptereita torjuttu: "));
        kopteriNaytto.Add(kopteriLabel);

        kopteriNaytto.X = Screen.RightSafe - 150;
        kopteriNaytto.Y = Screen.BottomSafe + 50;
        Add(kopteriNaytto);
    }

    void LisaaHpPalkki()
    {
        hpMittari = new DoubleMeter(100);
        hpMittari.MaxValue = 100.0;
        hpMittari.MinValue = 0.0;
        hpMittari.LowerLimit += HpLoppu;

        BarGauge hpPalkki = new BarGauge(40, 300);
        hpPalkki.Y = Screen.BottomSafe + 20;
        hpPalkki.Angle -= Angle.FromRadians(Math.PI / 2);
        hpPalkki.BindTo(hpMittari);
        hpPalkki.BorderColor = Color.Black;
        hpPalkki.Color = Color.Black;
        hpPalkki.BarColor = Color.Red;

        Add(hpPalkki);
    }

    void lisaaTykinVoimaPalkki()
    {
        voimaMittari = new DoubleMeter(0);
        voimaMittari.MaxValue = 150000.0;
        voimaMittari.MinValue = 0.0;

        BarGauge voimaPalkki = new BarGauge(20, 300);
        voimaPalkki.Y = Screen.BottomSafe + 60;
        voimaPalkki.Angle -= Angle.FromRadians(Math.PI / 2);
        voimaPalkki.BindTo(voimaMittari);
        voimaPalkki.BorderColor = Color.Black;
        voimaPalkki.Color = Color.Black;
        voimaPalkki.BarColor = Color.Green;
        
        Add(voimaPalkki);
    }

    void LataaEfektit()
    {
        tykkiSuuliekki = new ExplosionSystem(liekinKuva, 50);
        tykkiSuuliekki.MinScale = 11;
        tykkiSuuliekki.MaxScale = 55;
        tykkiSuuliekki.MaxVelocity = 3;
        tykkiSuuliekki.MinVelocity = 1;
        tykkiSuuliekki.MaxAcceleration = 5;
        Add(tykkiSuuliekki);

        kopteriRajahdys = new ExplosionSystem(liekinKuva, 50);
        Add(kopteriRajahdys);

        tykinOsuma = new ExplosionSystem(liekinKuva, 50);
        tykinOsuma.MinScale = 22;
        tykinOsuma.MaxScale = 110;
        Add(tykinOsuma);

        tirskahdus = new ExplosionSystem(pisteKuva, 50);
        tirskahdus.MinScale = 5;
        tirskahdus.MaxScale = 10;
        tirskahdus.MaxVelocity = 3;
        tirskahdus.MinVelocity = 1;
        tirskahdus.MaxAcceleration = 0.5;
        tirskahdus.MaxLifetime = 0.2;
        Add(tirskahdus);

        tankkiRajahdys = new ExplosionSystem(tankkiPartikkeli, 150);
        tankkiRajahdys.MinScale = 60;
        tankkiRajahdys.MaxScale = 300;
        tankkiRajahdys.MinAcceleration = 0.1;
        tankkiRajahdys.MaxAcceleration = 0.2;
        tankkiRajahdys.MinLifetime = 20.0;
        tankkiRajahdys.MaxLifetime = 30.0;
        tankkiRajahdys.MaxVelocity = 0.1;
        Add(tankkiRajahdys);

        tankkiinOsui = new ExplosionSystem(osumaPartikkeli, 30);
        tankkiinOsui.MinScale = 12;
        tankkiinOsui.MaxScale = 30;
        tankkiinOsui.MaxAcceleration = 0.5;
        tankkiinOsui.MinLifetime = 0.2;
        tankkiinOsui.MaxLifetime = 0.5;
        Add(tankkiinOsui);
    }

    #endregion

    #region kenttajutut

    void LuoKentta()
    {
        Level.Width = 2 * Screen.Width;
        Level.Height = 2 * Screen.Height;
        maa = PhysicsObject.CreateStaticObject(Level.Width, 200.0);
        maa.Y = Level.Bottom + 100.0;
        maa.Color = Color.DarkOrange;
        Add(maa);

        Level.CreateLeftBorder().Tag = "reuna";
        Level.CreateRightBorder().Tag = "reuna";
        Level.CreateTopBorder().Tag = "reuna";

        Level.Background.Image = taustaKuva;
        Level.Background.FitToLevel();

        LuoTankki();

        Camera.ZoomToLevel();

        Gravity = new Vector(0, -800);
    }

    void LuoTankki()
    {
        tankki = new Tank(100, 40);
        tankki.Y = maa.Y + (0.5 * maa.Height) + tankki.Height;
        tankki.Mass = 20;

        tankki.Cannon.Power.MaxValue = double.PositiveInfinity;
        tankki.Cannon.ProjectileCollision += tykkiOsui;
        tankki.Cannon.Image = tykinKuva;
        tankki.Cannon.Ammo.Value = int.MaxValue;

        kk = new AssaultRifle(50, 20);
        kk.X += 25;
        kk.TimeBetweenUse = new TimeSpan(0, 0, 0, 0, 300);
        kk.Ammo.Value = int.MaxValue;
        kk.ProjectileCollision += luotiOsui;
        tankki.Add(kk);

        Add(tankki);
    }

    void LuoKopteri(bool kumpi)
    {
        PhysicsObject kopteri = new PhysicsObject(200, 60);
        kopteri.Mass = 300;

        kopteri.Y = RandomGen.NextDouble(Level.Top - 600, Level.Top - 50);
        kopteri.Tag = "kopteri";
        kopteri.IgnoresCollisionResponse = true;
        kopteri.IgnoresGravity = true;
        kopteri.Animation = new Animation(kopterinKuvat);
        kopteri.Animation.Start();

        if (kumpi)
        {
            kopteri.X = Level.Left - 80;
            kopteri.Velocity = new Vector(RandomGen.NextDouble(100, 300), 0);
        }
        else
        {
            kopteri.X = Level.Right + 80;
            kopteri.Velocity = new Vector(RandomGen.NextDouble(-300, -100), 0);
            kopteri.MirrorImage();
        }

        AddCollisionHandler(kopteri, kopterinTormays);
        kopteriLista.Add(kopteri);
        Add(kopteri);
    }

    PhysicsObject LuoLaskuvarjo(double x, double y)
    {
        PhysicsObject tyyppi = new PhysicsObject(50, 80);
        tyyppi.Mass = RandomGen.NextDouble(5, 15);
        tyyppi.X = x;
        tyyppi.Y = y;
        tyyppi.LinearDamping = RandomGen.NextDouble(0.6, 0.9);
        tyyppi.CanRotate = false;
        tyyppi.IgnoresCollisionResponse = true;
        tyyppi.Tag = "tyyppi";
        AddCollisionHandler(tyyppi, laskuvarjonTormays);

        if (RandomGen.NextBool())
        {
            tyyppi.Animation = new Animation(laskuvarjoKuvat);
            tyyppi.Animation.FPS = RandomGen.NextDouble(3, 6);
        }
        else
        {
            tyyppi.Animation = new Animation(laskuvarjo2Kuvat);
            tyyppi.Animation.FPS = RandomGen.NextDouble(3, 6);
        }

        tyyppi.Animation.Start();
        Add(tyyppi);
        return tyyppi;
    }

    PlatformCharacter LuoLaskeutunut(double x, double y)
    {
        PlatformCharacter laskeutunutTyyppi = new PlatformCharacter(20, 40);
        laskeutunutTyyppi.Y = y + 10;
        laskeutunutTyyppi.X = x;
        laskeutunutTyyppi.CanRotate = false;
        laskeutunutTyyppi.Tag = "laskeutunut";
        laskeutunutTyyppi.AnimIdle = new Animation(ukkoMaassaOikealleKuvat);
        laskeutunutTyyppi.Weapon = new AssaultRifle(40, 25);

        Timer laskeutuneenAjastin = new Timer();
        laskeutuneenAjastin.Interval = 1.5;
        laskeutuneenAjastin.Timeout += delegate { laskeutunutAmpuu(laskeutunutTyyppi, laskeutuneenAjastin); };
        laskeutuneenAjastin.Start();

        AddCollisionHandler(laskeutunutTyyppi, laskeutuneenTormays);
        Add(laskeutunutTyyppi);
        return laskeutunutTyyppi;
    }

    #endregion

    #region tapahtumat

    //Laskeutuneen tyypin rintamasuunta
    double LaskeAmpujanSuunta(PlatformCharacter ampuja)
    {
        return Math.Sign(tankki.X - ampuja.X);
    }

    //Laskeutuneen tyypin aseen kulma
    Angle LaskeAmpujanKulma(PlatformCharacter ampuja, double suunta)
    {
        Vector v = new Vector(Math.Sign(tankki.X - ampuja.X), RandomGen.NextDouble(-0.1, 0.1));
        return v.Angle;
    }

    void laskeutunutAmpuu(PlatformCharacter ampuja, Timer sender)
    {
        if (ampuja != null && !ampuja.IsDestroyed)
        {
            double suunta = LaskeAmpujanSuunta(ampuja);
            ampuja.Walk(suunta);

            ampuja.Weapon.Angle = LaskeAmpujanKulma(ampuja, suunta);

            PhysicsObject ammus = ampuja.Weapon.Shoot();
            if (ammus != null)
            {
                ammus.Size = new Vector(10, 10);
                ammus.IgnoresCollisionResponse = true;
                AddCollisionHandler(ammus, laskeutuneenLuotiOsui);
            }

            ampuja.Walk(suunta * 200);

        }
        else
        {
            sender.Stop();
        }

    }

    //Hallitse koptereiden ja laskuvarjomiesten tiheyttä & määrää jne täältä ja ajastimen intervalista
    void kopteriPaivitys()
    {
        for (int i = 0; i < kopteriLista.Count; i++)
        {
            if (kopteriLista[i].IsDestroyed) kopteriLista.Remove(kopteriLista[i]);
            else if (kopteriLista[i].Left > Level.Right && kopteriLista[i].Velocity.X > 0)
            {
                kopteriLista[i].X = Level.Left - kopteriLista[i].Width;
                kopteriLista[i].Y = RandomGen.NextDouble(Level.Top - 600, Level.Top - 50);
            }
            else if (kopteriLista[i].Right < Level.Left && kopteriLista[i].Velocity.X < 0)
            {
                kopteriLista[i].X = Level.Right + kopteriLista[i].Width;
                kopteriLista[i].Y = RandomGen.NextDouble(Level.Top - 600, Level.Top - 50);
            }
        }

        for (int j = 0; j < kopteriLista.Count; j++)
        {
            if (RandomGen.NextInt(0, 4) < 1)
            {
                int k = RandomGen.NextInt(0, kopteriLista.Count);

                if (kopteriLista[k].Left > Level.Left && kopteriLista[k].Right < Level.Right)
                {
                    tyyppiLista.Add(LuoLaskuvarjo(kopteriLista[k].X, kopteriLista[k].Y));
                }
            }
        }

        if (RandomGen.NextInt(0, 5) < 1)
        {
            LuoKopteri(RandomGen.NextBool());
        }

    }

    void HpLoppu()
    {
        tankki.Destroy();
        tankkiRajahdys.AddEffect(tankki.Position, 150);
        Timer.SingleShot(10, AloitaAlusta);

    }

    void kopterinTormays(PhysicsObject kopteri, PhysicsObject kohde)
    {
        if (kohde.Tag.ToString() == "reuna")
        {
            if (kopteri.Velocity.Y < 0)
            {
                kopteri.Destroy();
                kopteriLaskuri.Value++;
            }
        }
        else if (kohde == maa)
        {
            kopteriRajahdys.AddEffect(kopteri.Position, 50);
            kopteri.Destroy();
            kopteriLaskuri.Value++;
        }
    }

    void laskuvarjonTormays(PhysicsObject tyyppi, PhysicsObject kohde)
    {
        if (kohde != tankki && kohde != maa && kohde.Tag != "reuna" && kohde.Tag != "tankki")
            return;

        tyyppiLista.Remove(tyyppi);
        tyyppi.Destroy();

        if (kohde == tankki)
        {
            tirskahdus.AddEffect(tyyppi.Position, 2);
            miesLaskuri.Value++;
        }
        else if (kohde == maa)
        {
            if (tyyppi.LinearDamping < 1)
            {
                tyyppiLista.Add(LuoLaskeutunut(tyyppi.X, tyyppi.Y));
                return;
            }
            else
            {
                tirskahdus.AddEffect(tyyppi.Position, 2);
                miesLaskuri.Value++;
                return;
            }
        }
    }

    void laskeutuneenTormays(PhysicsObject tyyppi, PhysicsObject kohde)
    {
        if (kohde == tankki)
        {
            tirskahdus.AddEffect(tyyppi.Position, 50);
            tyyppi.Destroy();
            miesLaskuri.Value++;
        }
    }

    void laskeutuneenLuotiOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        if (kohde.Tag.ToString() == "laskeutunut") return;
        if (kohde == tankki)
        {
            hpMittari.Value -= 10.0;
            ammus.Destroy();
            tankkiinOsui.AddEffect(ammus.Position, 30);
        }
        else ammus.Destroy();
    }

    void luotiOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        if (kohde == tankki) return;
        if (kohde.Tag.ToString() == "tyyppi")
        {
            tirskahdus.AddEffect(ammus.Position, 2);
            kohde.LinearDamping = 1;
            kohde.Animation = new Animation(ukkoPutoaaKuvat);
            ammus.Destroy();
        }
        else if (kohde.Tag.ToString() == "laskeutunut")
        {
            tirskahdus.AddEffect(ammus.Position, 2);
            kohde.Destroy();
            ammus.Destroy();
            miesLaskuri.Value++;
        }
        else if (kohde == maa || kohde.Tag.ToString() == "reuna") ammus.Destroy();
    }

    void tykkiOsui(PhysicsObject ammus, PhysicsObject kohde)
    {
        if (kohde == tankki) return;
        if (kohde.Tag.ToString() == "tyyppi")
        {
            tirskahdus.AddEffect(ammus.Position, 2);
            kohde.LinearDamping = 1;
            kohde.Animation = new Animation(ukkoPutoaaKuvat);
        }
        else if (kohde.Tag.ToString() == "kopteri")
        {
            tykinOsuma.AddEffect(ammus.Position, 2);
            kohde.IgnoresGravity = false;
            kohde.Animation = new Animation(kopteriPutaaKuvat);
            kohde.Animation.FPS = 12;
            kohde.Animation.Start();
        }
        else if (kohde.Tag.ToString() == "laskeutunut")
        {
            tirskahdus.AddEffect(ammus.Position, 2);
            kohde.Destroy();
            miesLaskuri.Value++;
        }
        else if (kohde == maa || kohde.Tag.ToString() == "reuna") ammus.Destroy();

    }

    #endregion

    #region napit

    void AsetaNapit()
    {
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, Exit, "Lopeta peli");
        Keyboard.Listen(Key.Enter, ButtonState.Pressed, AloitaAlusta, "Aloita alusta");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, null);


        Keyboard.Listen(Key.Left, ButtonState.Down, aja, "Liiku vasemmalle", tankki, 10.0);
        Keyboard.Listen(Key.Right, ButtonState.Down, aja, "Liiku oikealle", tankki, -10.0);
        Keyboard.Listen(Key.Left, ButtonState.Released, aja, "Liiku vasemmalle", tankki, 0.0);
        Keyboard.Listen(Key.Right, ButtonState.Released, aja, "Liiku oikealle", tankki, 0.0);
        Keyboard.Listen(Key.Up, ButtonState.Down, kaannaPutkea, "Käännä putkea vastapäivään", tankki, Angle.FromDegrees(2));
        Keyboard.Listen(Key.Down, ButtonState.Down, kaannaPutkea, "Käännä putkea myötäpäivään", tankki, Angle.FromDegrees(-2));
        Keyboard.Listen(Key.Up, ButtonState.Down, kaannaKK, "Käännä putkea vastapäivään", tankki, Angle.FromDegrees(2));
        Keyboard.Listen(Key.Down, ButtonState.Down, kaannaKK, "Käännä putkea myötäpäivään", tankki, Angle.FromDegrees(-2));

        Keyboard.Listen(Key.Space, ButtonState.Down, lataaTykinVoimaa, "Ammu tykillä", 1500.0);
        Keyboard.Listen(Key.Space, ButtonState.Released, ammuTykilla, null, tankki);
        Keyboard.Listen(Key.LeftControl, ButtonState.Down, ammuKK, "Ammu konekiväärillä");

        Keyboard.Listen(Key.H, ButtonState.Pressed, delegate { tahtaysavustin = !tahtaysavustin; }, "Näytä tähtäysavustin");
    }

    void lataaTykinVoimaa(double lisaVoima)
    {
        voimaMittari.Value += lisaVoima;
    }

    void ammuKK()
    {
        PhysicsObject ammus = kk.Shoot();
        if (ammus != null)
        {
            ammus.Velocity = new Vector(ammus.Velocity.X * 1.5, ammus.Velocity.Y * 1.5);
            ammus.Width = 7;
            ammus.Height = 7;
            ammus.Image = pisteKuva;
        }
    }

    void ammuTykilla(Tank t)
    {
        t.Cannon.Power.Value = voimaMittari.Value;
        PhysicsObject ammus = t.Cannon.Shoot();

        if (ammus != null)
        {
            ammus.Mass = 100;
            tykkiSuuliekki.AddEffect(ammus.Position, 25);
        }

        voimaMittari.Value = 0;
    }

    void aja(Tank t, double vaanto)
    {
        t.Accelerate(vaanto);
    }

    void kaannaPutkea(Tank t, Angle kaanto)
    {
        t.Cannon.Angle += kaanto;
    }

    void kaannaKK(Tank t, Angle kaanto)
    {
        kk.Angle += kaanto;
    }

    protected override void Paint(Canvas canvas)
    {
        if (tahtaysavustin)
        {
            Vector alku = tankki.Cannon.Position;
            Vector loppu = Vector.FromLengthAndAngle(500, tankki.Cannon.Angle) + tankki.Position;

            canvas.BrushColor = Color.Gray;

            canvas.DrawLine(alku, loppu);

        }
        base.Paint(canvas);
    }

    #endregion
}