using System;
using System.Collections.Generic;
using System.Linq;

namespace Lattice.Sim.Engine;

public sealed class EntityManager
{
    private readonly List<int> _entities = new();
    private readonly Dictionary<Type, Dictionary<int, object>> _stores = new();
    private int _nextId;

    public IReadOnlyList<int> Entities => _entities;

    public int CreateEntity()
    {
        int id = ++_nextId;
        _entities.Add(id);
        return id;
    }

    public void DestroyEntity(int entityId)
    {
        _entities.Remove(entityId);
        foreach (Dictionary<int, object> store in _stores.Values)
        {
            store.Remove(entityId);
        }
    }

    public void AddComponent<T>(int entityId, T component)
        where T : class
        => StoreFor(typeof(T))[entityId] = component;

    public void AddComponent(int entityId, object component)
        => StoreFor(component.GetType())[entityId] = component;

    public void RemoveComponent<T>(int entityId)
        where T : class
    {
        if (_stores.TryGetValue(typeof(T), out Dictionary<int, object>? store))
        {
            store.Remove(entityId);
        }
    }

    public T GetComponent<T>(int entityId)
        where T : class
    {
        if (StoreFor(typeof(T)).TryGetValue(entityId, out object? component))
        {
            return (T)component;
        }

        throw new InvalidOperationException(
            $"Entity {entityId} has no component of type {typeof(T).Name}.");
    }

    public bool TryGetComponent<T>(int entityId, out T component)
        where T : class
    {
        if (StoreFor(typeof(T)).TryGetValue(entityId, out object? found))
        {
            component = (T)found;
            return true;
        }

        component = null!;
        return false;
    }

    public bool HasComponent<T>(int entityId)
        where T : class
        => StoreFor(typeof(T)).ContainsKey(entityId);

    public IEnumerable<int> Query<T>()
        where T : class
        => StoreFor(typeof(T)).Keys.OrderBy(static id => id);

    public IEnumerable<int> Query<T1, T2>()
        where T1 : class
        where T2 : class
    {
        Dictionary<int, object> second = StoreFor(typeof(T2));
        return StoreFor(typeof(T1)).Keys
            .Where(second.ContainsKey)
            .OrderBy(static id => id);
    }

    private Dictionary<int, object> StoreFor(Type type)
    {
        if (!_stores.TryGetValue(type, out Dictionary<int, object>? store))
        {
            store = new Dictionary<int, object>();
            _stores[type] = store;
        }

        return store;
    }
}
