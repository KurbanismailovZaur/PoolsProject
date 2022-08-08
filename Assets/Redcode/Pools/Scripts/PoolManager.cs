using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Redcode.Pools
{
    [Serializable]
    public struct PoolData
    {
        [SerializeField]
        private string _name;

        public string Name => _name;

        [SerializeField]
        private Component _component;

        public Component Component => _component;

        [SerializeField]
        [Min(0)]
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
        private List<PoolData> _pools;

        private readonly List<IPool<Component>> _poolsObjects = new();

        private void Awake()
        {
            var namesGroups = _pools.Select(p => p.Name).GroupBy(n => n).Where(g => g.Count() > 1);

            if (namesGroups.Count() > 0)
                throw new Exception($"Pool Manager already contains pool with name \"{namesGroups.First().Select(g => g).First()}\"");
            
            var poolsType = typeof(List<IPool<Component>>);
            var poolsAddMethod = poolsType.GetMethod("Add");
            var genericPoolType = typeof(Pool<>);

            foreach (var poolData in _pools)
            {
                var poolType = genericPoolType.MakeGenericType(poolData.Component.GetType());
                var createMethod = poolType.GetMethod("Create", BindingFlags.Static | BindingFlags.NonPublic);

                var pool = createMethod.Invoke(null, new object[] { poolData.Component, poolData.Count, poolData.Container });

                if (poolData.NonLazy)
                {
                    var nonLazyMethod = poolType.GetMethod("NonLazy");
                    nonLazyMethod.Invoke(pool, null);
                }

                poolsAddMethod.Invoke(_poolsObjects, new object[] { pool });
            }
        }

        public IPool<T> GetPool<T>(int index) where T : Component => (IPool<T>)_poolsObjects[index];

        public IPool<T> GetPool<T>() where T : Component => (IPool<T>)_poolsObjects.Find(p => p.Source is T);

        public IPool<T> GetPool<T>(string name) where T : Component => (IPool<T>)_poolsObjects[_pools.FindIndex(p => p.Name == name)];
    }
}
