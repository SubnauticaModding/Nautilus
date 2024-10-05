using System;
using BepInEx;
using Nautilus.Assets;
using Nautilus.Assets.PrefabTemplates;
using Nautilus.Handlers;
using ProtoBuf;
using UnityEngine;

namespace Nautilus.Examples;

[BepInPlugin("com.snmodding.nautilus.serializablebehaviour", "Nautilus Serializable Behaviour Example Mod", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.snmodding.nautilus")]
internal class SerializableBehaviourExample : BaseUnityPlugin
{
    private void Awake()
    {
        // Don't forget to register your serializable types
        ProtobufSerializerHandler.RegisterAllSerializableTypesInAssembly();

        var stopwatchSign = new CustomPrefab(PrefabInfo.WithTechType("StopwatchSign"));
        var stopwatchSignTemplate = new CloneTemplate(stopwatchSign.Info, TechType.Sign);
        stopwatchSignTemplate.ModifyPrefab += go => go.AddComponent<StopwatchSignExample>();
        stopwatchSign.SetGameObject(stopwatchSignTemplate);
        stopwatchSign.Register();
    }
}

[ProtoContract]
internal class RealtimeCounter : MonoBehaviour, IProtoEventListener
{
    public DateTimeOffset StartedCounting { get; private set; } = DateTimeOffset.UtcNow;

    // If you plan on updating your mod and in particular, updating the serialized MonoBehaviours' fields,
    // you will need to update the version number and adapt your data in the OnProtoDeserialize method.
    // If you're just making a new serializable MonoBehaviour, set version to 1
    // If you don't care about updating your mod, you can just ignore everything concerning the version variable
    [ProtobufSerializerHandler.SubnauticaSerialized(1)]
    public int version = 2;

    // Variable to be serialized
    [ProtobufSerializerHandler.SubnauticaSerialized(2)]
    public double totalRealtimeMs;

    // This is an example of how you can give a default value to your serialized variables
    [ProtobufSerializerHandler.SubnauticaSerialized(3)]
    public long firstLaunchUnixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // Below are two listeners from the optional interface IProtoEventListener:
    // OnProtoDeserialize happens after this MonoBehaviour's GameObject full hierarchy is deserialized
    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        // This happens when this MonoBehaviour is loaded for the first time after being updated (serialized version was 1 but it should be 2)
        // You can find another example in Subnautica's code, like WaterParkCreature.OnProtoDeserialize
        if (version == 1)
        {
            // This has no particular meaning but stands as an example of how you can adapt your data in case of updating your serialized variables
            firstLaunchUnixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 1000;
        }

        StartedCounting = DateTimeOffset.UtcNow;
        Debug.Log($"Deserialized realtime counter with {totalRealtimeMs}ms [first launch at: {DateTimeOffset.FromUnixTimeMilliseconds(firstLaunchUnixTimeMs)}]");
    }

    // OnProtoSerialize happens before this MonoBehaviour's GameObject full hierarchy is serialized
    public void OnProtoSerialize(ProtobufSerializer serializer)
    {
        // Ensure the serialized version is the newest one
        version = 2;

        // Doing some stuff according to your needs
        totalRealtimeMs += (DateTimeOffset.UtcNow - StartedCounting).TotalMilliseconds;
        StartedCounting = DateTimeOffset.UtcNow;
        Debug.Log($"Serialized realtime counter with {totalRealtimeMs}ms [first launch at: {DateTimeOffset.FromUnixTimeMilliseconds(firstLaunchUnixTimeMs)}]");
    }
}

[ProtoContract]
internal class StopwatchSignExample : MonoBehaviour, IProtoEventListener
{
    [ProtobufSerializerHandler.SubnauticaSerialized(1)]
    public int version = 1;

    [ProtobufSerializerHandler.SubnauticaSerialized(2)]
    public float timePassed;
    
    [ProtobufSerializerHandler.SubnauticaSerialized(3)]
    public int serializations;
    
    [ProtobufSerializerHandler.SubnauticaSerialized(4)]
    public int deserializations;
    
    private Sign _sign;

    private void Start()
    {
        _sign = GetComponent<Sign>();
    }
    
    private void Update()
    {
        if (_sign) _sign.signInput.inputField.text = timePassed.ToString("#.0") + $"\nSerializations: {serializations}" + $"\nDeserializations: {deserializations}";
        timePassed += Time.deltaTime;
    }
    
    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        deserializations++;
    }

    public void OnProtoSerialize(ProtobufSerializer serializer)
    {
        version = 1;
        serializations++;
    }
}