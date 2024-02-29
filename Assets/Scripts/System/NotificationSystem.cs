using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface INotificationListener
{
    void OnNotification(MonoBehaviour component, string message, ref bool keepNotification, params object[] parameters);
}

public class NotificationSystem : MonoBehaviour
{
    static NotificationSystem _instance = null;
    public static NotificationSystem instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("Notification System");
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<NotificationSystem>();
                _instance.Init();
            }
            return _instance;
        }
    }

    void Init()
    {
        listenersSet = new HashSet<INotificationListener>();
        listeners = new List<INotificationListener>();
    }

    HashSet<INotificationListener> listenersSet = null;
    List<INotificationListener> listeners = null;

    public bool AddListener(INotificationListener listener)
    {
        if (!listenersSet.Add(listener))
            return false;
        listeners.Add(listener);
        return true;
    }

    public bool RemoveListener(INotificationListener listener)
    {
        if (!listenersSet.Remove(listener))
            return false;
        listeners.Remove(listener);
        return true;
    }

    public bool IsListener(INotificationListener listener)
    {
        return listenersSet.Contains(listener);
    }

    public void OptimizeListeners()
    {
        HashSet<INotificationListener> optimizedListenersSet = new HashSet<INotificationListener>();
        List<INotificationListener> optimizedListeners = new List<INotificationListener>();
        foreach (INotificationListener listener in listeners)
        {
            bool valid = listener != null;
            if (valid && listener is ComponentEx)
            {
                ComponentEx componentEx = listener as ComponentEx;
                valid = componentEx.isActiveAndEnabled && componentEx.gameObject.activeInHierarchy;
            }
                
            if (valid)
            {
                optimizedListeners.Add(listener);
                optimizedListenersSet.Add(listener);
            }
        }
        listenersSet = optimizedListenersSet;
        listeners = optimizedListeners;
    }

    public void Notify(MonoBehaviour component, string message, params object[] parameters)
    {
        bool keepNotification = true;
        for(int i = 0; i < listeners.Count && keepNotification; ++i)
            listeners[i].OnNotification(component, message, ref keepNotification, parameters);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OptimizeListeners();
    }

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
}
