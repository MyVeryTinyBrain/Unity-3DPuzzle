using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentEx : MonoBehaviour
{
    bool quit = false;
    public int originalPrefabInstanceID { get; private set; } = int.MinValue;

    public bool isPoolingObject => (originalPrefabInstanceID != int.MinValue);

    protected virtual void OnApplicationQuit()
    {
        quit = true;
    }

    protected virtual void Awake()
    {
        if (this is INotificationListener)
            NotificationSystem.instance.AddListener(this as INotificationListener);
    }

    protected virtual void Start() { }
    protected virtual void Update() { }
    protected virtual void LateUpdate() { }

    protected virtual void OnDestroy()
    {
        if (!quit)
        {
            if (this is INotificationListener)
            {
                if (NotificationSystem.instance.isActiveAndEnabled && NotificationSystem.instance.gameObject.activeInHierarchy)
                    NotificationSystem.instance.RemoveListener(this as INotificationListener);
            }
        }
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { }

    public virtual void OnSpawnFromPool(int instanceID)
    {
        originalPrefabInstanceID = instanceID;
    }

    public virtual void OnReturnToPool()
    {
    }

    public void SetChildLayers(int layer)
    {
        TransformUtility.DFS(transform, (t) => { t.gameObject.layer = layer; });
    }
}
