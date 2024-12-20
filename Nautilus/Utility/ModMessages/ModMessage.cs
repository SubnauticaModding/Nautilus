using System.Text;

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

    /// <summary>
    /// Returns the message data represented as a string. 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append("Subject: ");
        sb.Append(Subject);
        sb.Append(", Recipient: ");
        sb.Append(Recipient);
        sb.Append(", Contents: ");
        
        if (Contents == null)
        {
            sb.Append("[Null]");
            return sb.ToString();
        }

        if (Contents.Length == 0)
        {
            sb.Append("[Empty]");
            return sb.ToString();
        }

        sb.Append("{ ");
        for (var i = 0; i < Contents.Length; i++)
        {
            if (Contents[i] == null)
            {
                sb.Append("Null");
            }
            else
            {
                sb.Append(Contents[i]);
                sb.Append(" (");
                sb.Append(Contents[i].GetType());
                sb.Append(')');
            }
            if (i < Contents.Length - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(" }");

        return sb.ToString();
    }
}
