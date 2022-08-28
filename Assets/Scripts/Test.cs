using Redcode.Pools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField]
    private Enemy _enemyPrefab;

    private void Start()
    {
        var pool = Pool.Create(_enemyPrefab);
    }
}
