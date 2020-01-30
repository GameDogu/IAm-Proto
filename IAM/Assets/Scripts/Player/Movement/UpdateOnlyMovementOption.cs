public abstract class UpdateOnlyMovementOption : EntityMovementOption
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
        RegisterUpdateCall(UpdateProcedure);
    }

    void Unregister()
    {
        UnregisterUpdateCall(UpdateProcedure);
    }

    protected abstract void UpdateProcedure();
}
