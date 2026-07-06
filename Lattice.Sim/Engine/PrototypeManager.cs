using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Lattice.Sim.Engine;

public sealed class PrototypeManager
{
    private readonly ComponentFactory _components;
    private readonly Dictionary<string, EntityPrototype> _prototypes = new(StringComparer.Ordinal);

    private readonly IDeserializer _rawDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly ISerializer _fieldSerializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    private readonly IDeserializer _componentDeserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public PrototypeManager(ComponentFactory components)
    {
        _components = components;
    }

    public IReadOnlyDictionary<string, EntityPrototype> Prototypes => _prototypes;

    public bool Has(string id) => _prototypes.ContainsKey(id);

    public EntityPrototype Get(string id)
        => _prototypes.TryGetValue(id, out EntityPrototype? prototype)
            ? prototype
            : throw new InvalidOperationException($"Unknown prototype '{id}'.");

    public void LoadDirectory(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (string path in Directory
                     .EnumerateFiles(directory, "*.yaml", SearchOption.AllDirectories)
                     .OrderBy(static p => p, StringComparer.Ordinal))
        {
            LoadYaml(File.ReadAllText(path), Path.GetFileName(path));
        }
    }

    public void LoadYaml(string yaml, string source = "<inline>")
    {
        List<RawPrototype>? raws = _rawDeserializer.Deserialize<List<RawPrototype>>(yaml);
        if (raws is null)
        {
            return;
        }

        foreach (RawPrototype raw in raws)
        {
            if (string.IsNullOrWhiteSpace(raw.Id))
            {
                throw new InvalidOperationException($"A prototype in '{source}' is missing an id.");
            }

            List<PrototypeComponent> components = new();
            foreach (Dictionary<string, object?> entry in raw.Components ?? new List<Dictionary<string, object?>>())
            {
                if (!entry.TryGetValue("type", out object? typeValue) || typeValue is not string typeName)
                {
                    throw new InvalidOperationException(
                        $"A component of prototype '{raw.Id}' in '{source}' is missing its 'type'.");
                }

                Type type = _components.GetType(typeName);
                Dictionary<string, object?> fields = entry
                    .Where(pair => !string.Equals(pair.Key, "type", StringComparison.Ordinal))
                    .ToDictionary(pair => pair.Key, pair => pair.Value);

                components.Add(new PrototypeComponent
                {
                    Type = type,
                    Yaml = _fieldSerializer.Serialize(fields),
                });
            }

            EntityPrototype prototype = new()
            {
                Id = raw.Id,
                Name = raw.Name,
                Components = components,
            };

            if (!_prototypes.TryAdd(raw.Id, prototype))
            {
                throw new InvalidOperationException($"Duplicate prototype id '{raw.Id}' in '{source}'.");
            }
        }
    }

    public int SpawnEntity(EntityManager entities, string prototypeId)
    {
        EntityPrototype prototype = Get(prototypeId);
        int entity = entities.CreateEntity();

        foreach (PrototypeComponent component in prototype.Components)
        {
            object instance = _componentDeserializer.Deserialize(new StringReader(component.Yaml), component.Type)
                              ?? Activator.CreateInstance(component.Type)!;
            entities.AddComponent(entity, instance);
        }

        return entity;
    }

    private sealed class RawPrototype
    {
        public string Id { get; set; } = string.Empty;

        public string? Name { get; set; }

        public List<Dictionary<string, object?>>? Components { get; set; }
    }
}
