using System.Collections.Generic;
using Nautilus.Json;

namespace Nautilus.Options.Attributes;

internal class ModOptionAttributeMetadata<T> where T : ConfigFile, new()
{
    public ModOptionAttribute ModOptionAttribute;
    public MemberInfoMetadata<T> MemberInfoMetadata;
    public IEnumerable<MemberInfoMetadata<T>> OnChangeMetadata;
    public IEnumerable<MemberInfoMetadata<T>> OnGameObjectCreatedMetadata;
}