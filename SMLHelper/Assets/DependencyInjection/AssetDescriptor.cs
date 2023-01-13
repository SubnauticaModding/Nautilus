using System;
using SMLHelper.Assets;

namespace SMLHelper.DependencyInjection;

public class AssetDescriptor
{
    public Type AssetType { get; }
    public Type ImplementationType { get; }
    public object ImplementationInstance { get; internal set; }

    public AssetDescriptor(Type assetType, Type implementationType, object implementationInstance)
    {
        AssetType = assetType;
        ImplementationType = implementationType;
        ImplementationInstance = implementationInstance;
    }


    public static AssetDescriptor ServiceSingleton
    (
        Type serviceType, 
        Type serviceImplementation,
        object implementationInstance = null)
    {
        return new AssetDescriptor(serviceType, serviceImplementation, implementationInstance);
    }
    
    public static AssetDescriptor ServiceSingleton
    (
        Type serviceType,
        object implementationInstance)
    {
        return ServiceSingleton(serviceType, implementationInstance.GetType(), implementationInstance);
    }
}