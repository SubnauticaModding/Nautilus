using System;

namespace Nautilus.Utility.ModMessages;

/// <summary>
/// Basic implementation of the abstract <see cref="ModMessageReader"/> that runs an action when a message with the given subject is received.
/// </summary>
public sealed class BasicModMessageReader : ModMessageReader
{
    private string _subject;

    private Action<object[]> _action;

    /// <summary>
    /// Creates a message reader that runs the given <paramref name="action"/> when a message with the given <paramref name="subject"/> is received.
    /// </summary>
    /// <param name="subject">The subject that this reader is looking for.</param>
    /// <param name="action">The action that is run for any message with the given <paramref name="subject"/>.</param>
    public BasicModMessageReader(string subject, Action<object[]> action)
    {
        _subject = subject;
        _action = action;
    }

    /// <inheritdoc/>
    protected internal override void OnReceiveMessage(ModMessage message)
    {
        if (message.Subject == _subject)
        {
            _action.Invoke(message.Contents);
        }
    }
}