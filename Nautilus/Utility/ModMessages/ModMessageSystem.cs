using System.Collections.Generic;

namespace Nautilus.Utility.ModMessages;

/// <summary>
/// A messaging system for cross-mod communication with <see cref="ModInbox"/> instances. Allows for ultra-soft dependencies and attempts to eliminate race conditions.
/// </summary>
public static class ModMessageSystem
{
    static ModMessageSystem()
    {
        SaveUtils.RegisterOnStartLoadingEvent(OnStartLoading);
    }

    private static bool _allowedToHoldGlobalMessages = true;

    // address - inbox
    private static Dictionary<string, ModInbox> _inboxes = new Dictionary<string, ModInbox>();

    // recipient - message
    private static Dictionary<string, List<ModMessage>> _heldMessages = new Dictionary<string, List<ModMessage>>();

    private static List<GlobalMessage> _globalMessages = new List<GlobalMessage>();

    /// <summary>
    /// Sends a single message to a <see cref="ModInbox"/>. If the message is not read immediately, it will be held until read.
    /// </summary>
    /// <param name="recipient">The address of the <see cref="ModInbox"/> that the message will go to. In C# terms, this is analogous to the class name.</param>
    /// <param name="subject">The subject of the message. Determines the purpose of a message. In C# terms, this is analogous to the method name.</param>
    /// <param name="contents">Any arbitrary data sent through the message. Optional. In C# terms, this is analogous to the method's parameters.</param>
    public static void Send(string recipient, string subject, params object[] contents)
    {
        Send(new ModMessage(recipient, subject, contents));
    }

    /// <summary>
    /// <para>Sends a global message to every <see cref="ModInbox"/> that exists, and even to ones that will exist in the future.
    /// If a message is not read immediately by any inbox, it will be held until read.</para>
    /// <para>IMPORTANT: Global messages can NOT be held after patch time has completed (once you have left the main menu).</para>
    /// </summary>
    /// <param name="subject">The subject of the message. Determines the purpose of a message. In C# terms, this is analogous to the method name.</param>
    /// <param name="contents">Any arbitrary data sent through the message. Optional. In C# terms, this is analogous to the method's parameters.</param>
    public static void SendGlobal(string subject, params object[] contents)
    {
        var globalMessage = new GlobalMessage(new ModMessage(null, subject, contents));
        foreach (var inbox in _inboxes.Values)
        {
            globalMessage.TrySendMessageToInbox(inbox);
        }
        if (_allowedToHoldGlobalMessages)
        {
            _globalMessages.Add(globalMessage);
        }
    }

    /// <summary>
    /// Sends a single message to a <see cref="ModInbox"/>. If the message is not read immediately, it will be held until read.
    /// </summary>
    /// <param name="messageInstance">The message to send.</param>
    public static void Send(ModMessage messageInstance)
    {
        if (_inboxes.TryGetValue(messageInstance.Recipient, out var inbox) && inbox.IsAcceptingMessages)
        {
            inbox.ReceiveMessage(messageInstance);
            return;
        }

        // add to held messages instead:

        if (!_heldMessages.TryGetValue(messageInstance.Recipient, out var heldMessageList))
            _heldMessages.Add(messageInstance.Recipient, new List<ModMessage>());

        heldMessageList.Add(messageInstance);
    }

    /// <summary>
    /// <para>Sends a single message to a <see cref="ModInbox"/> and attempts to receive a value.</para>
    /// <para>If the message is not read immediately (i.e. the inbox is closed or has not been created yet), it will be DELETED, and not held!</para>
    /// </summary>
    /// <param name="messageInstance">The message to send.</param>
    /// <param name="result">The data that is received, if any.</param>
    /// <returns>True if any <see cref="ModMessageReader"/> on the receiving end handled the message and returned a value.</returns>
    public static bool SendDataRequest(ModMessage messageInstance, out object result)
    {
        // if the message responds immediately:

        if (_inboxes.TryGetValue(messageInstance.Recipient, out var inbox) && inbox.IsAcceptingMessages && inbox.ReceiveDataRequest(messageInstance, out result))
        {
            return true;
        }

        // otherwise, who cares? just return false

        result = default;
        return false;
    }

    /// <summary>
    /// Registers an inbox so that it can receive mail. Please note that this does NOT automatically read any messages on the <paramref name="inbox"/> that were sent before it was
    /// registered. For that you must call its <see cref="ModInbox.ReadAnyHeldMessages"/> method.
    /// </summary>
    /// <param name="inbox">The inbox to register.</param>
    public static void RegisterInbox(ModInbox inbox)
    {
        _inboxes[inbox.Address] = inbox;
    }

    internal static void SendHeldMessagesToInbox(ModInbox inbox)
    {
        // this is a necessary check for the sake of consistency
        if (!inbox.IsAcceptingMessages)
            return;

        if (_heldMessages.TryGetValue(inbox.Address, out var messageList))
        {
            foreach (var message in messageList)
            {
                inbox.ReceiveMessage(message);
            }
            _heldMessages[inbox.Address].Clear();
        }
        foreach (var globalMessage in _globalMessages)
        {
            globalMessage.TrySendMessageToInbox(inbox);
        }
    }

    // Once game time has started, stop holding global messages and remove any held global messages from patch time
    private static void OnStartLoading()
    {
        _globalMessages.Clear();
        _allowedToHoldGlobalMessages = false;
    }
}