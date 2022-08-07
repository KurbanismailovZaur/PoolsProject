using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Redcode.Pools
{
    [Serializable]
    public struct PoolData
    {
        [SerializeField]
        private Component _component;

        public Component Component => _component;

        [SerializeField]
        private int _count;

        public int Count => _count;

        [SerializeField]
        private Transform _container;

        public Transform Container => _container;

        [SerializeField]
        private bool _nonLazy;

        public bool NonLazy => _nonLazy;
    }

    public class PoolManager : MonoBehaviour
    {
        [SerializeField]
        private List<PoolData> _poolsData;

        private readonly List<Pool<Component>> _pools = new();

        private void Awake()
        {
            foreach (var poolData in _poolsData)
            {
                var pool = Pool.Create(poolData.Component, poolData.Count, poolData.Container);

                if (poolData.NonLazy)
                    pool.NonLazy();

                _pools.Add(pool);
            }
        }

        public Pool<Component> GetPool<T>(int index) where T : Component => _pools[index];

        public Pool<Component> GetPool<T>() where T : Component => _pools.Find(p => p.Source is T);
    }
}
