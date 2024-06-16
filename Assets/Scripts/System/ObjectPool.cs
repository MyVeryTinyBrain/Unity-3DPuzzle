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

    // 인스턴스 ID로 분류된 풀 딕셔너리
    Dictionary<int, Pool> pools;
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
            // 사용 가능한 프리팹들은 뒤에 모여 있습니다.
            // 맨 뒤에 있는 프리팹을 사용 불가능한 프리팹이 모여있는,
            // 맨 앞으로 옮기고 반환합니다.
            ComponentEx obj = list.Last.Value;
            list.RemoveLast();
            list.AddFirst(obj);
            // 사용중인 프리팹 카운트 증가
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

    public ComponentEx Spawn(ComponentEx prefab)
    {
        // 생성하려는 프리팹을 담당하는 풀이 있다면 가져오고, 아니라면 생성합니다.
        Pool pool;
        if (!pools.TryGetValue(prefab.GetInstanceID(), out pool))
        {
            pool = new Pool(prefab.GetInstanceID(), prefab.GetType());
            pools.Add(prefab.GetInstanceID(), pool);
        }
        // 풀에 사용 가능한 프리팹이 프리팹을 없다면 생성합니다.
        if(pool.usingCount >= pool.count)
        {
            ComponentEx newObj = Instantiate(prefab);
            pool.Add(newObj);
        }
        // 프리팹 활성화
        ComponentEx obj = pool.UseLast();
        obj.gameObject.transform.SetParent(null);
        obj.gameObject.SetActive(true);
        obj.OnSpawnFromPool(prefab.GetInstanceID());
        return obj;
    }

    public bool Return(ComponentEx spawnedObject)
    {
        Pool pool;
        // 반환하려는 프리팹을 담당하는 풀을 가져옵니다.
        if (!pools.TryGetValue(spawnedObject.originalPrefabInstanceID, out pool))
            return false;
        // 풀에 프리팹을 반환합니다.
        if (!pool.Return(spawnedObject))
            return false;
        // 프리팹 비활성화
        spawnedObject.gameObject.transform.SetParent(transform);
        spawnedObject.gameObject.SetActive(false);
        spawnedObject.OnReturnToPool();
        return true;
    }
}
