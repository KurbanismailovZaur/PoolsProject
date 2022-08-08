using Redcode.Pools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IPoolObject
{
    public void OnGettingFromPool()
    {
        print("Resetted");
    }
}
