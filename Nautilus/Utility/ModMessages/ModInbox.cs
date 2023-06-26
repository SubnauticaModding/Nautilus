using System;
using System.Collections.Generic;

namespace Nautilus.Utility.ModMessages;

/// <summary>
/// An object with an address. Receives mail and allows it to be read at any time. Any held messages (messages sent before this inbox was created) will not be read
/// until the <see cref="ReadAnyHeldMessages"/> method is called.
/// </summary>
public class ModInbox
{
    private List<ModMessageReader> _messageReaders = new List<ModMessageReader>();

    /// <summary>
    /// The address of this inbox. Conventionally should match the mod's GUID.
    /// </summary>
    public string Address { get; }

    /// <summary>
    /// If <see langword="false"/>, this inbox will not automatically read messages and will instead put any received messages on hold. If you are setting this property to
    /// <see langword="true"/> at a later time, you still need to call <see cref="ReadAnyHeldMessages"/> to catch up.
    /// </summary>
    public bool IsAcceptingMessages { get; set; }

    /// <summary>
    /// Constructs an inbox with the given <paramref name="address"/>.
    /// </summary>
    /// <param name="address">The address of this inbox. Other mods will use this string to contact this mod. Conventionally should match the mod's GUID. This parameter should NOT be
    /// changed if any other mod is already using it!</param>
    /// <param name="acceptingMessages">If <see langword="false"/>, this inbox will not automatically read messages and will instead put any received messages on hold.</param>
    public ModInbox(string address, bool acceptingMessages = true)
    {
        Address = address;
        IsAcceptingMessages = acceptingMessages;
    }

    /// <summary>
    /// Adds an object that reads and handles any received messages.
    /// </summary>
    /// <param name="reader">The instance of the <see cref="ModMessageReader"/> to register.</param>
    public void AddMessageReader(ModMessageReader reader)
    {
        _messageReaders.Add(reader);
    }

    internal void ReceiveMessage(ModMessage message)
    {
        foreach (var reader in _messageReaders)
        {
            try
            {
                reader.OnReceiveMessage(message);
            }
            catch (Exception e)
            {
                InternalLogger.Error("Exception caught in messaging system: " + e);
            }
        }
    }


    /// <summary>
    /// Reads any messages that were sent to this address before the inbox was created.
    /// </summary>
    public void ReadAnyHeldMessages()
    {
        ModMessageSystem.SendHeldMessagesToInbox(this);
    }
}