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
        var poolEnemy = _manager.GetPool<Enemy>("Enemies");
        var enemy = poolEnemy.Get();

        yield return new WaitForSeconds(3f);

        poolEnemy.Take(enemy);
        print("Completed");
    }
}