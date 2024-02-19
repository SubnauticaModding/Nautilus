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
    // You need to store the generated methods so they can be used in your serialize and deserialize static methods
    public static Action<RealtimeCounter, int, ProtoWriter> realtimeCounterSerializeMethodInfo;
    public static Func<RealtimeCounter, ProtoReader, RealtimeCounter> realtimeCounterDeserializeMethodInfo;
    public static Action<StopwatchSignExample, int, ProtoWriter> stopwatchSignExampleSerializeMethodInfo;
    public static Func<StopwatchSignExample, ProtoReader, StopwatchSignExample> stopwatchSignExampleDeserializeMethodInfo;

    private void Awake()
    {
        // We first generate automatically the serialize and deserialize methods for our serializable type
        var realtimeCounterMethodInfos = ProtobufSerializerHandler.GenerateSerializeAndDeserializeMethods<RealtimeCounter>();
        // And we store them somewhere easily accessible
        realtimeCounterSerializeMethodInfo = realtimeCounterMethodInfos.Item1;
        realtimeCounterDeserializeMethodInfo = realtimeCounterMethodInfos.Item2;
        
        // Do it for the stopwatch
        var stopwatchSignMethodInfos = ProtobufSerializerHandler.GenerateSerializeAndDeserializeMethods<StopwatchSignExample>();
        stopwatchSignExampleSerializeMethodInfo = stopwatchSignMethodInfos.Item1;
        stopwatchSignExampleDeserializeMethodInfo = stopwatchSignMethodInfos.Item2;

        // Don't forget to register your serializable type with the right serialize and deserialize method (those which you created manually)
        // You can pick whatever type ID (e.g. 3141592) you want as long it's not used by another mod or by Subnautica itself
        ProtobufSerializerHandler.RegisterSerializableType<RealtimeCounter>(3141592, RealtimeCounter.Serialize, RealtimeCounter.Deserialize);
        ProtobufSerializerHandler.RegisterSerializableType<StopwatchSignExample>(3141593, StopwatchSignExample.Serialize, StopwatchSignExample.Deserialize);

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

    // Below are the required static methods for Protobuf serializer to serialize and deserialize this MonoBehaviour
    // You can find more examples in ProtobufSerializerPrecompiled just like Serialize11492366 and Deserialize11492366
    // Serialize must be a "static void" with parameters (YourType, int, ProtoWriter)
    public static void Serialize(RealtimeCounter realtimeCounter, int objTypeId, ProtoWriter writer)
    {
        // If you only have basic types you only need to use the generated methods but you may need to adapt this with more complex types
        // In that situation, please pick the right WireType for your variables and adapt the following Write[Object] call

        // In this case, we only have basic types to serialize so we just invoke the generated method
        SerializableBehaviourExample.realtimeCounterSerializeMethodInfo.Invoke(realtimeCounter, objTypeId, writer);
    }

    // Deserialize must be a "static YourType" with parameters (YourType, ProtoReader)
    public static RealtimeCounter Deserialize(RealtimeCounter realtimeCounter, ProtoReader reader)
    {
        // Here you need to use the methods Read[Object] accordingly to what you wrote in Serialize: Write[Object]
        // Also the different cases must be using the same field numbers as defined in Serialize

        // In this case, we only have basic types to serialize so we just invoke the generated method
        realtimeCounter = SerializableBehaviourExample.realtimeCounterDeserializeMethodInfo.Invoke(realtimeCounter, reader);

        // Return the same object you've been modifying
        return realtimeCounter;
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
    
    // Below are two listeners from the optional interface IProtoEventListener:
    // OnProtoDeserialize happens after this MonoBehaviour's GameObject full hierarchy is deserialized
    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        deserializations++;
    }

    // OnProtoSerialize happens before this MonoBehaviour's GameObject full hierarchy is serialized
    public void OnProtoSerialize(ProtobufSerializer serializer)
    {
        version = 1;
        serializations++;
    }

    // Below are the required static methods for Protobuf serializer to serialize and deserialize this MonoBehaviour
    // You can find more examples in ProtobufSerializerPrecompiled just like Serialize11492366 and Deserialize11492366
    // Serialize must be a "static void" with parameters (YourType, int, ProtoWriter)
    public static void Serialize(StopwatchSignExample stopwatchSignExample, int objTypeId, ProtoWriter writer)
    {
        // If you only have basic types you only need to use the generated methods but you may need to adapt this with more complex types
        // In that situation, please pick the right WireType for your variables and adapt the following Write[Object] call

        // In this case, we only have basic types to serialize so we just invoke the generated method
        SerializableBehaviourExample.stopwatchSignExampleSerializeMethodInfo.Invoke(stopwatchSignExample, objTypeId, writer);
    }

    // Deserialize must be a "static YourType" with parameters (YourType, ProtoReader)
    public static StopwatchSignExample Deserialize(StopwatchSignExample stopwatchSignExample, ProtoReader reader)
    {
        // Here you need to use the methods Read[Object] accordingly to what you wrote in Serialize: Write[Object]
        // Also the different cases must be using the same field numbers as defined in Serialize

        // In this case, we only have basic types to serialize so we just invoke the generated method
        stopwatchSignExample = SerializableBehaviourExample.stopwatchSignExampleDeserializeMethodInfo.Invoke(stopwatchSignExample, reader);

        // Return the same object you've been modifying
        return stopwatchSignExample;
    }
}