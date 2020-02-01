public abstract class FixedUpdateOnlyMovementOption : EntityMovementOption
{
    protected override void Initialize(StateMovementHandler handler)
    {
        Register(handler);
    }

    void Register(StateMovementHandler handler)
    {
        Unregister(handler);//don't double register
        RegisterFixedUpdateCall(handler,FixedUpdateProcedure);
    }

    void Unregister(StateMovementHandler handler)
    {
        RegisterFixedUpdateCall(handler,FixedUpdateProcedure);
    }

    protected abstract void FixedUpdateProcedure();
}
