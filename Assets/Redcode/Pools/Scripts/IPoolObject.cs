using UnityEngine;

namespace Redcode.Pools
{
    public interface IPoolObject
    {
        void OnGettingFromPool();
    }
}