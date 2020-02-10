public abstract class DualLoopMovementOption : EntityMovementOption
{

    protected override void Initialize(StateMovementHandler handler)
    {
        Register(handler);
    }

    void Register(StateMovementHandler handler)
    {
        Unregister(handler);//don't double register
        RegisterUpdateCall(handler,UpdateProcedure);        
        RegisterFixedUpdateCall(handler,FixedUpdateProcedure);
    }

    void Unregister(StateMovementHandler handler)
    {
        UnregisterFixedUpdateCall(handler,FixedUpdateProcedure);
        UnregisterUpdateCall(handler,UpdateProcedure);
    }

    protected abstract void FixedUpdateProcedure();
    protected abstract void UpdateProcedure();
}