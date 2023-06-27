namespace Nautilus.Utility.ModMessages;

/// <summary>
/// An instance of a message that is either sent instantly or held until received.
/// </summary>
public readonly struct ModMessage
{
    /// <summary>
    /// The address of the <see cref="ModInbox"/> that the message will go to. In C# terms, this is analogous to the class name.
    /// </summary>
    public string Recipient { get; }

    /// <summary>
    /// The subject of the message. Determines the purpose of a message. In C# terms, this is analogous to the method name.
    /// </summary>
    public string Subject { get; }

    /// <summary>
    /// Any arbitrary data sent through the message. Optional. In C# terms, this is analogous to the method's parameters.
    /// </summary>
    public object[] Contents { get; }

    /// <summary>
    /// Creates an instance of a message.
    /// </summary>
    /// <param name="recipient">The address of the <see cref="ModInbox"/> that the message will go to. In C# terms, this is analogous to the class name.</param>
    /// <param name="subject">The subject of the message. Determines the purpose of a message. In C# terms, this is analogous to the method name.</param>
    /// <param name="contents">Any arbitrary data sent through the message. Optional. In C# terms, this is analogous to the method's parameters.</param>
    public ModMessage(string recipient, string subject, object[] contents)
    {
        Recipient = recipient;
        Subject = subject;
        Contents = contents;
    }
}
