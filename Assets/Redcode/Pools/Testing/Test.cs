using Redcode.Pools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Test : MonoBehaviour
{
    [SerializeField]
    private PoolManager _manager;

    private IEnumerator Start()
    {
        var enemy = _manager.GetFromPool<Enemy>(0);

        yield return new WaitForSeconds(2f);
        _manager.TakeToPool(0, enemy);
    }
}