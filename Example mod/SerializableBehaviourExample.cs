using System;
using BepInEx;
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
        // Don't forget to register your serializable type
        // You can pick whatever type ID you want as long it's not used by another mod or by Subnautica itself
        ProtobufSerializerHandler.RegisterSerializableType<RealtimeCounter>(3141592, RealtimeCounter.Serialize, RealtimeCounter.Deserialize);
    }
}

[ProtoContract]
internal class RealtimeCounter : MonoBehaviour, IProtoEventListener
{
    public DateTimeOffset StartedCounting { get; private set; } = DateTimeOffset.UtcNow;

    // If you plan on distributing updates to your mod and maybe updating the serialized MonoBehaviours' fields,
    // you will need to update the version number and adapt your data in the OnProtoDeserialize method.
    // If you're just making a new serializable MonoBehaviour, set version to 1
    // If you don't care about updating your mod, you can just ignore everything concerning the version variable
    [ProtoMember(1), NonSerialized]
    public int version = 2;

    // Variable to be serialized
    [ProtoMember(2), NonSerialized]
    public double totalRealtimeMs;

    // This is an example of how you can give a default value to your serialized variables
    [ProtoMember(3), NonSerialized]
    public long firstLaunchUnixTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    // Some listeners from the optional interface IProtoEventListener
    // OnProtoDeserialize happens after this MonoBehaviour's GameObject full hierarchy is deserialized
    public void OnProtoDeserialize(ProtobufSerializer serializer)
    {
        // This happens when this MonoBehaviour is loaded for the first time after being updated
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
        version = 2;

        totalRealtimeMs += (DateTimeOffset.UtcNow - StartedCounting).TotalMilliseconds;
        StartedCounting = DateTimeOffset.UtcNow;
        Debug.Log($"Serialized realtime counter with {totalRealtimeMs}ms [first launch at: {DateTimeOffset.FromUnixTimeMilliseconds(firstLaunchUnixTimeMs)}]");
    }

    // The required static methods for Protobuf serializer to serialize and deserialize this MonoBehaviour
    // You can find more examples in ProtobufSerializerPrecompiled just like Serialize11492366 and Deserialize11492366
    // Serialize must be a static void with parameters (YourType, int, ProtoWriter)
    public static void Serialize(RealtimeCounter realtimeCounter, int objTypeId, ProtoWriter writer)
    {
        // If you only have basic types this will be straightforward but you may need to adapt this with more complex ones
        // Please pick the right WireType for your variables and adapt the following Write[Object] call
        ProtoWriter.WriteFieldHeader(1, WireType.Variant, writer);
        ProtoWriter.WriteInt32(realtimeCounter.version, writer);

        ProtoWriter.WriteFieldHeader(2, WireType.Fixed64, writer);
        ProtoWriter.WriteDouble(realtimeCounter.totalRealtimeMs, writer);

        ProtoWriter.WriteFieldHeader(3, WireType.Fixed64, writer);
        ProtoWriter.WriteInt64(realtimeCounter.firstLaunchUnixTimeMs, writer);
    }

    // Deserialize must be a static YourType with parameters (YourType, ProtoReader)
    public static RealtimeCounter Deserialize(RealtimeCounter realtimeCounter, ProtoReader reader)
    {
        // Here you need to use the methods Read[Object] accordingly to what you wrote in Serialize: Write[Object]
        // Also the different cases must be using the same field numbers as defined in Serialize
        for (int i = reader.ReadFieldHeader(); i > 0; i = reader.ReadFieldHeader())
        {
            switch (i)
            {
                case 1:
                    // We used ProtoWriter.WriteInt32 so here we need reader.ReadInt32
                    realtimeCounter.version = reader.ReadInt32();
                    break;

                case 2:
                    realtimeCounter.totalRealtimeMs = reader.ReadInt64();
                    break;

                case 3:
                    realtimeCounter.firstLaunchUnixTimeMs = reader.ReadInt64();
                    break;

                default:
                    reader.SkipField();
                    break;
            }
        }
        // Return the same object you've been modifying
        return realtimeCounter;
    }
}