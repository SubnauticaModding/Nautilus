using Nautilus.Json;
using System.Collections.Generic;

namespace Nautilus.Options.Attributes;

internal class ModOptionAttributeMetadata<T> where T : ConfigFile, new()
{
    public ModOptionAttribute ModOptionAttribute;
    public MemberInfoMetadata<T> MemberInfoMetadata;
    public IEnumerable<MemberInfoMetadata<T>> OnChangeMetadata;
    public IEnumerable<MemberInfoMetadata<T>> OnGameObjectCreatedMetadata;
}