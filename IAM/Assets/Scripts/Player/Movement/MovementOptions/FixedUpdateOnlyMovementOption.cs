/// <summary>
/// movement option relying only on fixed update loop
/// </summary>
public abstract class FixedUpdateOnlyMovementOption : EntityMovementOption
{
    /// <summary>
    /// register to fixed update loop of handler
    /// </summary>
    /// <param name="handler">a states movemnet handler</param>
    protected override void Initialize(StateMovementHandler handler)
    {
        Register(handler);
    }

    /// <summary>
    /// register fixed update procedure to fixed update event of handler,ensuring no double registering happens
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    void Register(StateMovementHandler handler)
    {
        Unregister(handler);//don't double register
        RegisterFixedUpdateCall(handler,FixedUpdateProcedure);
    }

    /// <summary>
    /// unregister from handlers fixed update event
    /// </summary>
    /// <param name="handler"></param>
    void Unregister(StateMovementHandler handler)
    {
        RegisterFixedUpdateCall(handler,FixedUpdateProcedure);
    }

    /// <summary>
    /// fixed update procedure to implement, called with every unity fixed update cycle
    /// </summary>
    protected abstract void FixedUpdateProcedure();
}
