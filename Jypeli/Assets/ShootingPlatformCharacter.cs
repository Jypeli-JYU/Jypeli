using Jypeli;
using System;

/// <summary>
/// Tasohyppelypelin hahmo, joka ampuu aseella automaattisesti määritellyin väliajoin.
/// </summary>
public class ShootingPlatformCharacter : PlatformCharacter
{
    public ShootingPlatformCharacter(double width, double height) : base(width, height)
    {
        AddedToGame += ShootingPlatformCharacter_AddedToGame;
    }

    public ShootingPlatformCharacter(double width, double height, Shape shape) : base(width, height, shape)
    {
        AddedToGame += ShootingPlatformCharacter_AddedToGame;
    }

    private TimeSpan _timeBetweenShots = TimeSpan.FromSeconds(1.0);

    /// <summary>
    /// Aika, joka kuluu ennen kuin hahmo ampuu.
    /// </summary>
    public TimeSpan TimeBetweenShots
    {
        get { return _timeBetweenShots; }
        set
        {
            _timeBetweenShots = value;
            if (timer != null)
                timer.Interval = value.TotalSeconds;
        }
    }

    /// <summary>
    /// Kohde, jota hahmo ampuu.
    /// </summary>
    public GameObject Target { get; set; }

    /// <summary>
    /// Korkein etäisyys, jonka päästä hahmo ampuu.
    /// Jos hahmo on tätä kauempana kohteesta, niin hahmo ei ammu.
    /// </summary>
    public double MaximumShootingDistance { get; set; } = double.MaxValue;

    private Timer timer;

    public override void Destroy()
    {
        base.Destroy();
        AddedToGame -= ShootingPlatformCharacter_AddedToGame;
    }

    private void ShootingPlatformCharacter_AddedToGame()
    {
        if (timer != null)
            timer.Stop();

        timer = Timer.CreateAndStart(TimeBetweenShots.TotalSeconds, Shoot);
    }

    private void Shoot()
    {
        if (Weapon == null || Target == null || IsDestroyed || !IsAddedToGame ||
            Target.IsDestroyed || !Target.IsAddedToGame)
            return;

        Vector distanceVector = Target.Position - Position;

        if (distanceVector.Magnitude > MaximumShootingDistance)
            return;

        Weapon.Shoot();
    }

    public override void Update(Time time)
    {
        if (Weapon == null || Target == null || Target.IsDestroyed || !Target.IsAddedToGame)
            return;

        Vector distanceVector = Target.Position - Position;

        if (distanceVector.Magnitude > MaximumShootingDistance)
            return;

        Weapon.Angle = distanceVector.Angle;

        base.Update(time);
    }
}
