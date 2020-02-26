/// <summary>
/// Entity Movement option relying only on update loop
/// </summary>
public abstract class UpdateOnlyMovementOption : EntityMovementOption
{

    /// <summary>
    /// registers update function to movmenet handler update event
    /// </summary>
    /// <param name="handler">a states movment hanler</param>
    protected override void Initialize(StateMovementHandler handler)
    {
        Register(handler);
    }

    /// <summary>
    /// register to handler update event making sure not double registerd
    /// </summary>
    /// <param name="handler">a states movment hanler</param>
    void Register(StateMovementHandler handler)
    {
        Unregister(handler);//don't double register
        RegisterUpdateCall(handler,UpdateProcedure);
    }

    /// <summary>
    /// unregisters from handlers update event
    /// </summary>
    /// <param name="handler">a states movment hanler</param>
    void Unregister(StateMovementHandler handler)
    {
        UnregisterUpdateCall(handler,UpdateProcedure);
    }

    /// <summary>
    /// update procedure to implement that will be called every unity update
    /// </summary>
    protected abstract void UpdateProcedure();
}
