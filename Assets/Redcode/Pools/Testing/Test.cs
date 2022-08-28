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

    [SerializeField]
    private Enemy _enemyPrefab;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);

        // Creates enemy pool with an endless number of clones.
        var pool = Pool.Create(_enemyPrefab, 0);
    }
}