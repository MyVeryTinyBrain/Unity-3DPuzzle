using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    static ObjectPool _instance = null;
    public static ObjectPool instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("Object Pool");
                _instance = gameObject.AddComponent<ObjectPool>();
                _instance.Init();
            }
            return _instance;
        }
    }

    void Init()
    {
        pools = new Dictionary<int, Pool>();
    }

    class Pool
    {
        public int originalPrefabInstanceID { get; private set; }
        public Type originalPrefabType { get; private set; }
        public int usingCount { get; private set; }
        public int count => list.Count;
        LinkedList<ComponentEx> list;

        public Pool(int originalPrefabInstanceID, Type originalPrefabType)
        {
            this.originalPrefabInstanceID = originalPrefabInstanceID;
            this.originalPrefabType = originalPrefabType;
            this.usingCount = 0;
            this.list = new LinkedList<ComponentEx>();
        }

        public void Add(ComponentEx obj)
        {
            list.AddLast(obj);
        }

        public ComponentEx UseLast()
        {
            ComponentEx obj = list.Last.Value;
            list.RemoveLast();
            list.AddFirst(obj);

            usingCount++;
            return obj;
        }

        public bool Return(ComponentEx obj)
        {
            if (!list.Remove(obj))
                return false;

            list.AddLast(obj);

            usingCount--;
            return true;
        }
    }

    Dictionary<int, Pool> pools;

    public ComponentEx Spawn(ComponentEx prefab)
    {
        Pool pool;
        if (!pools.TryGetValue(prefab.GetInstanceID(), out pool))
        {
            pool = new Pool(prefab.GetInstanceID(), prefab.GetType());
            pools.Add(prefab.GetInstanceID(), pool);
        }

        if(pool.usingCount >= pool.count)
        {
            ComponentEx newObj = Instantiate(prefab);
            pool.Add(newObj);
        }

        ComponentEx obj = pool.UseLast();
        obj.gameObject.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        obj.OnSpawnFromPool(prefab.GetInstanceID());
        return obj;
    }

    public bool Return(ComponentEx spawnedObject)
    {
        Pool pool;
        if (!pools.TryGetValue(spawnedObject.originalPrefabInstanceID, out pool))
            return false;

        if (!pool.Return(spawnedObject))
            return false;

        spawnedObject.gameObject.transform.SetParent(transform);
        spawnedObject.gameObject.SetActive(false);
        spawnedObject.OnReturnToPool();

        return true;
    }
}
