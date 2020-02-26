/// <summary>
/// movement option relying on both update loop and fixed update loop
/// </summary>
public abstract class DualLoopMovementOption : EntityMovementOption
{

    /// <summary>
    /// registers everything to the correct handler events
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    protected override void Initialize(StateMovementHandler handler)
    {
        Register(handler);
    }

    /// <summary>
    /// registers fixed update procedure to fixed update event, and update to the update event, making sure nothing double registers
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    void Register(StateMovementHandler handler)
    {
        Unregister(handler);//don't double register
        RegisterUpdateCall(handler,UpdateProcedure);        
        RegisterFixedUpdateCall(handler,FixedUpdateProcedure);
    }

    /// <summary>
    /// unregister update and fixed update procedurees from the handlers events
    /// </summary>
    /// <param name="handler">a states movement handler</param>
    void Unregister(StateMovementHandler handler)
    {
        UnregisterFixedUpdateCall(handler,FixedUpdateProcedure);
        UnregisterUpdateCall(handler,UpdateProcedure);
    }

    /// <summary>
    /// fixed update procedure to implement, called with every unity fixed update cycle
    /// </summary>
    protected abstract void FixedUpdateProcedure();

    /// <summary>
    ///  update procedure to implement, called with every unity update cycle
    /// </summary>
    protected abstract void UpdateProcedure();
}