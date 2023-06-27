namespace Nautilus.Utility.ModMessages;

/// <summary>
/// The base class of any object that receives mod messages and handles them.
/// </summary>
public abstract class ModMessageReader
{
    /// <summary>
    /// Called when any message is received.
    /// </summary>
    protected internal abstract void OnReceiveMessage(ModMessage message);

    /// <summary>
    /// Called when data is requested. Similar to a normal message, but has a return value. Unlike normal messages, data requests can NOT be held.
    /// </summary>
    /// <param name="message">The basic message data for this request.</param>
    /// <param name="returnValue">The object that is returned from this method, if any. Otherwise should be <see langword="default"/>.</param>
    /// <returns>True if this method is willing to respond to the message's particular subject, false otherwise. If TRUE is returned, all other readers will be ignored.</returns>
    protected internal virtual bool TryHandleDataRequest(ModMessage message, out object returnValue)
    {
        returnValue = default;
        return false;
    }
}