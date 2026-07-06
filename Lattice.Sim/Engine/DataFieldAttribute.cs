using System;

namespace Lattice.Sim.Engine;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class DataFieldAttribute : Attribute
{
    public DataFieldAttribute(string? tag = null)
    {
        Tag = tag;
    }

    public string? Tag { get; }
}
