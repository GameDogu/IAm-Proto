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
        RegisterFixedUpdateCall(FixedUpdateProcedure);
    }

    void Unregister()
    {
        RegisterFixedUpdateCall(FixedUpdateProcedure);
    }

    protected abstract void FixedUpdateProcedure();
}
