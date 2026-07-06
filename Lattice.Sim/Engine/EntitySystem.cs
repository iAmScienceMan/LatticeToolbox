using System;
using System.Collections.Generic;

namespace Lattice.Sim.Engine;

public abstract class EntitySystem : SystemBase
{
    [Dependency]
    protected readonly EntityManager Entities = null!;

    [Dependency]
    protected readonly EventBus Bus = null!;

    [Dependency]
    protected readonly PrototypeManager Prototypes = null!;

    protected T Comp<T>(int entity)
        where T : class
        => Entities.GetComponent<T>(entity);

    protected T? CompOrNull<T>(int entity)
        where T : class
        => Entities.TryGetComponent(entity, out T found) ? found : null;

    protected bool TryComp<T>(int entity, out T component)
        where T : class
        => Entities.TryGetComponent(entity, out component);

    protected bool HasComp<T>(int entity)
        where T : class
        => Entities.HasComponent<T>(entity);

    protected T EnsureComp<T>(int entity)
        where T : class, new()
    {
        if (Entities.TryGetComponent(entity, out T existing))
        {
            return existing;
        }

        T added = new();
        Entities.AddComponent(entity, added);
        return added;
    }

    protected T AddComp<T>(int entity)
        where T : class, new()
    {
        T added = new();
        Entities.AddComponent(entity, added);
        return added;
    }

    protected void RemComp<T>(int entity)
        where T : class
        => Entities.RemoveComponent<T>(entity);

    protected int Spawn(string prototypeId)
    {
        int entity = Prototypes.SpawnEntity(Entities, prototypeId);
        Bus.PublishDirected(entity, new ComponentInit());
        return entity;
    }

    protected void Del(int entity)
        => Entities.DestroyEntity(entity);

    protected IEnumerable<int> EntityQuery<T>()
        where T : class
        => Entities.Query<T>();

    protected IEnumerable<int> EntityQuery<T1, T2>()
        where T1 : class
        where T2 : class
        => Entities.Query<T1, T2>();

    protected void SubscribeLocalEvent<TComp, TEvent>(Action<int, TComp, TEvent> handler)
        where TComp : class
    {
        Bus.SubscribeDirected<TEvent>((entity, payload) =>
        {
            if (Entities.TryGetComponent(entity, out TComp component))
            {
                handler(entity, component, payload);
            }
        });
    }
}
