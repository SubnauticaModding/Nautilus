namespace SMLHelper.API;

/// <summary>
/// A class that holds all the necessary elements of a custom battery to be patched.
/// </summary>
public class CbBattery : CbItem
{
    /// <summary>
    /// Patches the data of this instance into a new custom Battery.
    /// </summary>
    public void Patch()
    {
        Patch(ItemTypes.Battery);
    }
}