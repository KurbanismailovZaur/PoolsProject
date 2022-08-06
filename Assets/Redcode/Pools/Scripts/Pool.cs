using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Pool
{
    public static Pool<T> Create<T>(T source, int count) where T : Component => Create(source, count, null);

    public static Pool<T> Create<T>(T source, int count, Transform parent) where T : Component => Create(source, count, null, null);

    public static Pool<T> Create<T>(T source, int count, Transform parent, Action<T> resetter) where T : Component => Pool<T>.Create(source, count, parent, resetter);
}

public class Pool<T> where T : Component
{
    #region Fields and properties
    private T _source;

    private int _count;

    public int Count => _count;

    private Transform _container;

    public Transform Container => _container;

    private List<T> _clones;

    private List<T> _busyObjects = new();

    private Action<T> _resetter;
    #endregion

    private Pool() { }

    internal static Pool<T> Create(T source, int count, Transform parent, Action<T> resetter)
    {
        var pool = new Pool<T>
        {
            _source = source,
            _count = Math.Max(count, 0),
            _container = parent,
            _clones = new(count),
            _resetter = resetter
        };

        return pool;
    }

    public Pool<T> SetCount(int count, bool destroyClones = true)
    {
        count = Math.Max(count, 0);

        if (count == 0)
        {
            _count = count;
            return this;
        }

        if (_count != 0 && count > _count)
        {
            _clones.Capacity = _busyObjects.Capacity = _count = count;
            return this;
        }

        if (destroyClones)
        {
            for (int i = count; i < _clones.Count; i++)
                Object.Destroy(_clones[i].gameObject);
        }

        _clones.RemoveRange(count, _clones.Count - count);
        _count = count;
        _clones.Capacity = _busyObjects.Capacity = _count;

        return this;
    }

    public Pool<T> SetContainer(Transform container, bool worldPositionStays = true)
    {
        _container = container;
        _clones.ForEach(c => c.transform.SetParent(_container, worldPositionStays));

        return this;
    }

    public Pool<T> SetResetter(Action<T> resetter)
    {
        _resetter = resetter;
        return this;
    }

    public Pool<T> Clear(bool destroyClones = true)
    {
        if (destroyClones)
            _clones.ForEach(c => Object.Destroy(c.gameObject));

        _clones.Clear();

        return this;
    }

    public Pool<T> NonLazy()
    {
        while (_clones.Count < _count)
        {
            var clone = Object.Instantiate(_source, _container);
            clone.gameObject.SetActive(false);
            
            _clones.Add(clone);
        }

        return this;
    }

    public T Get()
    {
        T clone = null;

        for (int i = 0; i < _clones.Count; i++)
        {
            if (!_busyObjects.Contains(_clones[i]))
            {
                clone = _clones[i];
                break;
            }
        }

        if (clone == null)
        {
            if (_count != 0 && _clones.Count >= _count)
                return null;

            _clones.Add(clone = Object.Instantiate(_source, _container));
        }

        _busyObjects.Add(clone);

        clone.gameObject.SetActive(true);
        _resetter?.Invoke(clone);

        return clone;
    }

    public void Take(T clone)
    {
        if (!_clones.Contains(clone))
            throw new ArgumentException("Passed clone object does not exist in pool's clones list.");

        if (!_busyObjects.Contains(clone))
            throw new ArgumentException("Passed clone object already free.");

        _busyObjects.Remove(clone);
        clone.gameObject.SetActive(false);
    }
}
