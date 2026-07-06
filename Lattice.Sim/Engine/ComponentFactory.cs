using System;
using System.Collections.Generic;
using System.Reflection;

namespace Lattice.Sim.Engine;

public sealed class ComponentFactory
{
    private readonly Dictionary<string, Type> _byName = new(StringComparer.Ordinal);

    public ComponentFactory(IEnumerable<Assembly> assemblies)
    {
        foreach (Assembly assembly in assemblies)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<RegisterComponentAttribute>() is null)
                {
                    continue;
                }

                if (type.IsAbstract || !typeof(IComponent).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException(
                        $"[RegisterComponent] type {type.FullName} must be a concrete IComponent.");
                }

                string name = NameOf(type);
                if (!_byName.TryAdd(name, type))
                {
                    throw new InvalidOperationException(
                        $"Two components register the same name '{name}': {_byName[name].FullName} and {type.FullName}.");
                }
            }
        }
    }

    public IReadOnlyCollection<string> RegisteredNames => _byName.Keys;

    public static string NameOf(Type type)
    {
        const string suffix = "Component";
        string name = type.Name;
        return name.Length > suffix.Length && name.EndsWith(suffix, StringComparison.Ordinal)
            ? name[..^suffix.Length]
            : name;
    }

    public Type GetType(string name)
        => _byName.TryGetValue(name, out Type? type)
            ? type
            : throw new InvalidOperationException(
                $"Unknown component '{name}'. Give the class a [RegisterComponent] attribute.");
}
