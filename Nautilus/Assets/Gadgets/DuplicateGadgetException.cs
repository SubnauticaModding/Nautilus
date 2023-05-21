using System;

namespace Nautilus.Assets.Gadgets;

/// <summary>
/// The exception that is thrown when a <see cref="Gadget"/> is attempted to be added when an existing one of the same type already exists.
/// </summary>
public class DuplicateGadgetException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DuplicateGadgetException"/> class with default properties.
    /// </summary>
    /// <param name="classId">ClassID of the Prefab, otherwise should be labeled "Uninitialized". For debugging purposes.</param>
    /// <param name="duplicateGadget">The Gadget that cannot be added.</param>
    public DuplicateGadgetException(string classId, Gadget duplicateGadget) : base
        ($"Cannot add Gadget of Type '{duplicateGadget.GetType()}' onto prefab of ClassID '{classId}' because a Gadget already exists on this prefab with the same type! Did you forget to call ICustomPrefab.RemoveGadget<TGadget>()?")
    {

    }
}