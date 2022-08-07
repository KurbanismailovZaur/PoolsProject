using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Helper class to create generic pools.
/// </summary>
public static class Pool
{
    /// <summary>
    /// Create pool.
    /// </summary>
    /// <typeparam name="T">Pool type.</typeparam>
    /// <param name="source">Pool object source.</param>
    /// <param name="count">Maximum objects count in pool.</param>
    /// <returns></returns>
    public static Pool<T> Create<T>(T source, int count) where T : Component => Create(source, count, null);

    /// <summary>
    /// <inheritdoc cref="Create{T}(T, int)"/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="Create{T}(T, int)"/></typeparam>
    /// <param name="source"><inheritdoc cref="Create{T}(T, int)" path="/param[@name='source']"/></param>
    /// <param name="count"><inheritdoc cref="Create{T}(T, int)" path="/param[@name='count']"/></param>
    /// <param name="container">Container object for pool objects.</param>
    /// <returns></returns>
    public static Pool<T> Create<T>(T source, int count, Transform container) where T : Component => Create(source, count, container, null);

    /// <summary>
    /// <inheritdoc cref="Create{T}(T, int)"/>
    /// </summary>
    /// <typeparam name="T"><inheritdoc cref="Create{T}(T, int)"/></typeparam>
    /// <param name="source"><inheritdoc cref="Create{T}(T, int)" path="/param[@name='source']"/></param>
    /// <param name="count"><inheritdoc cref="Create{T}(T, int)" path="/param[@name='count']"/></param>
    /// <param name="container"><inheritdoc cref="Create{T}(T, int, Transform)" path="/param[@name='container']"/></param>
    /// <param name="resetter">Reset function, which will be called before pool objects will be given to the client</param>
    /// <returns></returns>
    public static Pool<T> Create<T>(T source, int count, Transform container, Action<T> resetter) where T : Component => Pool<T>.Create(source, count, container, resetter);
}

public class Pool<T> where T : Component
{
    #region Fields and properties
    private T _source;

    private int _count;

    /// <summary>
    /// Maximum objects in pool.
    /// </summary>
    public int Count => _count;

    private Transform _container;

    /// <summary>
    /// Container for pool objects.
    /// </summary>
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

    /// <summary>
    /// Sets new maximum count for pool objects.
    /// </summary>
    /// <param name="count">Maximum objects count.</param>
    /// <param name="destroyClones">
    /// Do we need destroy already exists pool objects, or keep they live?<br/>
    /// These objects will no longer be controlled by the pool.
    /// </param>
    /// <returns>The pool.</returns>
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

    /// <summary>
    /// Sets new container for pool objects. Already exists pool objects will be reparented.
    /// </summary>
    /// <param name="container">New container for pool objects.</param>
    /// <param name="worldPositionStays">Do we need save pool objects world position?</param>
    /// <returns>The pool.</returns>
    public Pool<T> SetContainer(Transform container, bool worldPositionStays = true)
    {
        _container = container;
        _clones.ForEach(c => c.transform.SetParent(_container, worldPositionStays));

        return this;
    }

    /// <summary>
    /// Sets new reset function, which will be called before pool objects will be given to the client.
    /// <param name="resetter">New reset function.</param>
    /// <returns>The pool.</returns>
    public Pool<T> SetResetter(Action<T> resetter)
    {
        _resetter = resetter;
        return this;
    }

    /// <summary>
    /// Clears all pool objects.
    /// </summary>
    /// <param name="destroyClones">
    /// Do we need destroy already exists pool objects, or keep they live?<br/>
    /// These objects will no longer be controlled by the pool.
    /// </param>
    /// <returns></returns>
    public Pool<T> Clear(bool destroyClones = true)
    {
        if (destroyClones)
            _clones.ForEach(c => Object.Destroy(c.gameObject));

        _clones.Clear();

        return this;
    }

    /// <summary>
    /// Immediately create all pool objects. Not works when <b>Count</b> is <b>0</b>./>
    /// </summary>
    /// <returns>The pool.</returns>
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

    /// <summary>
    /// Try get object from pool. If all objects are busy, then <see langword="null"/> will be returned.<br/>
    /// You need to remember to call <see cref="Take"/> method to return the object to the pool.
    /// </summary>
    /// <returns>Object from pool.</returns>
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

    /// <summary>
    /// Marks object as free after it was busy with calling <see cref="Get"/> method.
    /// </summary>
    /// <param name="clone">Pool object to mark as free.</param>
    /// <exception cref="ArgumentException">Throwed when <paramref name="clone"/> not exist on pool, or not busy.</exception>
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