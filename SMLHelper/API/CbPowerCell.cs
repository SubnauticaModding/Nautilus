namespace SMLHelper.API;

/// <summary>
/// A class that holds all the necessary elements of a custom power cell to be patched.
/// </summary>
public class CbPowerCell : CbItem
{
    /// <summary>
    /// Patches the data of this instance into a new custom Power Cell.
    /// </summary>
    public void Patch()
    {
        Patch(ItemTypes.PowerCell);
    }
}