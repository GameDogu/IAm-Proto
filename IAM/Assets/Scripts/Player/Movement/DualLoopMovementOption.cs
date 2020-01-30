public abstract class DualLoopMovementOption : EntityMovementOption
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
        RegisterFixedUpdateCall(FixedUpdateProcedure);
    }

    void Unregister()
    {
        UnregisterFixedUpdateCall(FixedUpdateProcedure);
        UnregisterUpdateCall(UpdateProcedure);
    }

    protected abstract void FixedUpdateProcedure();
    protected abstract void UpdateProcedure();
}