using System.Globalization;
using Content.Shared.Humanoid;
using Robust.Shared.Reflection;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using Robust.Shared.Utility;

namespace Content.Shared._Afterlight.Sprite;

[TypeSerializer]
public sealed class ALSpriteLayerKeySerializer : ITypeSerializer<ALSpriteLayerKey, ValueDataNode>
{
    public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
        IDependencyCollection dependencies, ISerializationContext? context = null)
    {
        return new ValidatedValueNode(node);
    }

    public ALSpriteLayerKey Read(ISerializationManager serializationManager, ValueDataNode node,
        IDependencyCollection dependencies, SerializationHookContext hookCtx, ISerializationContext? context = null, ISerializationManager.InstantiationDelegate<ALSpriteLayerKey>? instanceProvider = null)
    {
        if (Parse.TryInt32(node.Value, out var intValue))
            return new ALSpriteLayerKey(null, null, intValue);

        if (Enum.TryParse(node.Value, out HumanoidVisualLayers layer))
            return new ALSpriteLayerKey(layer, null, null);

        return new ALSpriteLayerKey(null, node.Value, null);
    }

    public DataNode Write(ISerializationManager serializationManager, ALSpriteLayerKey value, IDependencyCollection dependencies,
        bool alwaysWrite = false, ISerializationContext? context = null)
    {
        if (value.Int != null)
            return new ValueDataNode(value.Int.Value.ToString(CultureInfo.InvariantCulture));

        if (value.Enum != null)
            return new ValueDataNode(value.Enum.ToString());

        if (value.String != null)
            return new ValueDataNode(value.String);

        return ValueDataNode.Null();
    }
}
