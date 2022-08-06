using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class Test : MonoBehaviour
{
    [SerializeField]
    private Transform _cube;

    private IEnumerator Start()
    {
        var pool = Pool.Create(_cube, 3, transform, cube => cube.name = Time.time.ToString());
        yield return new WaitForSeconds(1f);
    }
}