using BepInEx.Logging;
using UnityEngine;

namespace SMLHelper.DependencyInjection;

public interface IGreeting
{
    void Greetings();
}

public class SayHi : IGreeting
{
    private ManualLogSource _logger;

    [InjectionSetup]
    private void Setup(ManualLogSource logger)
    {
        _logger = logger;
    }
    
    public void Greetings()
    {
        ErrorMessage.AddMessage("Hello World");
        _logger.LogDebug("Hello World from SayHi");
    }
}