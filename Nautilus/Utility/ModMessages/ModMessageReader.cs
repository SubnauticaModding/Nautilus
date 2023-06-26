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
}