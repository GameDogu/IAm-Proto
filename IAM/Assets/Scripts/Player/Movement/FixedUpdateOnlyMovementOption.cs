public abstract class FixedUpdateOnlyMovementOption : EntityMovementOption
{
    protected override void Initialize()
    {
        Register();
    }

    public override void Stop()
    {
        Unregister();
    }

    void Register()
    {
        Unregister();//don't double register
        RegisterUpdateCall(FixedUpdateProcedure);
    }

    void Unregister()
    {
        UnregisterUpdateCall(FixedUpdateProcedure);
    }

    protected abstract void FixedUpdateProcedure();
}
