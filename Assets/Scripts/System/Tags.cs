using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tags : MonoBehaviour
{
    static Tags _instance = null;
    public static Tags instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("Tags");
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<Tags>();
            }
            return _instance;
        }
    }

    public string LaserReflectorTag = "LaserReflector";
}