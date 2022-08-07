using Redcode.Pools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

interface IMyClass<out T> where T : Component
{
    T GetValue();
}

class MyClass<T> : IMyClass<T> where T : Component
{
    public T GetValue() => default(T);
}

public class Test : MonoBehaviour
{
    [SerializeField]
    private PoolManager _manager;

    private IEnumerator Start()
    {
        IMyClass<Component> t = new MyClass<MonoBehaviour>();
        IMyClass<MonoBehaviour> t2 = (IMyClass<MonoBehaviour>)t;
        print(t2.GetValue());

        var poolEnemy = _manager.GetPool<Enemy>();
        var poolBullet = _manager.GetPool<Bullet>();

        for (int i = 0; i < 8; i++)
        {
            yield return new WaitForSeconds(1f);

            var enemy = poolEnemy.Get();

            if (enemy != null)
                enemy.name = Time.time.ToString();
        }

        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(1f);

            var bullet = poolBullet.Get();

            if (bullet != null)
                bullet.name = Time.time.ToString();
        }
    }
}