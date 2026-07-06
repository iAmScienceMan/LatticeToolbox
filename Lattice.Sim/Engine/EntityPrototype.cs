using System;
using System.Collections.Generic;

namespace Lattice.Sim.Engine;

public sealed class EntityPrototype
{
    public required string Id { get; init; }

    public string? Name { get; init; }

    public required IReadOnlyList<PrototypeComponent> Components { get; init; }
}

public sealed class PrototypeComponent
{
    public required Type Type { get; init; }

    public required string Yaml { get; init; }
}
