namespace SMLHelper.Assets.Gadgets;

/// <summary>
/// Represents a class that handles registers to game code.
/// </summary>
public abstract class Gadget
{
    protected readonly ICustomPrefab prefab;

    /// <summary>
    /// Constructs a gadget instance.
    /// </summary>
    /// <param name="prefab">The custom prefab to operate on.</param>
    public Gadget(ICustomPrefab prefab)
    {
        this.prefab = prefab;
    }

    /// <summary>
    /// Where the data actually gets registered to the game.<br/>
    /// This is called after prefab register and PostRegisters in <see cref="CustomPrefab.Register"/>.
    /// </summary>
    protected internal abstract void Build();
}