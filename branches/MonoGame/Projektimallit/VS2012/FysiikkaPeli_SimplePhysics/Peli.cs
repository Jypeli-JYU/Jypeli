public class $safeprojectname$ : PhysicsGame
{
    public override void Begin()
    {
        // Kirjoita ohjelmakoodisi tähän

        PhoneBackButton.Listen( ConfirmExit, "Lopeta peli" );
        Keyboard.Listen( Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli" );
    }
}
