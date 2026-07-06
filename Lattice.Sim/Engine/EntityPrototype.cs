using System;
using System.Collections.Generic;

namespace Lattice.Sim.Engine;

public sealed class EntityPrototype
{
    private readonly Func<IReadOnlyList<IComponent>> _factory;
    private IReadOnlyList<IComponent>? _template;

    public EntityPrototype(string id, string? name, Func<IReadOnlyList<IComponent>> factory)
    {
        Id = id;
        Name = name;
        _factory = factory;
    }

    public string Id { get; }

    public string? Name { get; }

    public IReadOnlyList<IComponent> Instantiate() => _factory();

    public IReadOnlyList<IComponent> Template => _template ??= _factory();

    public bool TryGetComponent<T>(out T component)
        where T : class
    {
        foreach (IComponent candidate in Template)
        {
            if (candidate is T match)
            {
                component = match;
                return true;
            }
        }

        component = null!;
        return false;
    }
}
