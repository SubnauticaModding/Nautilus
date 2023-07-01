using System.Collections.Generic;

namespace Nautilus.Utility.ModMessages;

internal class GlobalMessage
{
    // the message's data

    private ModMessage Message { get; }

    // a list of all the addresses that have already received the message

    private readonly List<string> _sentAddresses = new();

    public GlobalMessage(ModMessage message)
    {
        Message = message;
    }

    public bool TrySendMessageToInbox(ModInbox inbox)
    {
        if (!inbox.AcceptsGlobalMessages || !inbox.IsAcceptingMessages || _sentAddresses.Contains(inbox.Address))
            return false;

        inbox.ReceiveMessage(Message);
        _sentAddresses.Add(inbox.Address);
        return true;
    }
}
