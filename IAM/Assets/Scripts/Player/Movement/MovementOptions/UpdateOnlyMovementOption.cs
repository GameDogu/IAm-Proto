public abstract class UpdateOnlyMovementOption : EntityMovementOption
{

    protected override void Initialize(StateMovementHandler handler)
    {
        Register(handler);
    }

    void Register(StateMovementHandler handler)
    {
        Unregister(handler);//don't double register
        RegisterUpdateCall(handler,UpdateProcedure);
    }

    void Unregister(StateMovementHandler handler)
    {
        UnregisterUpdateCall(handler,UpdateProcedure);
    }

    protected abstract void UpdateProcedure();
}
