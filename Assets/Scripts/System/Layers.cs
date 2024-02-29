using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Layers : MonoBehaviour
{
    static Layers _instance = null;
    public static Layers instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject("Layers");
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<Layers>();
            }
            return _instance;
        }
    }

    public int CharacterLayerIndex { get; private set; }
    public int FittingOwningPropLayerIndex { get; private set; }
    public int StoredOwningPropLayerIndex { get; private set; }
    public int OwningPropLayerIndex { get; private set; }
    public int PropLayerIndex { get; private set; }
    public int OutlinePropLayerIndex { get; private set; }

    private void Awake()
    {
        CharacterLayerIndex = LayerMask.NameToLayer("Character");
        FittingOwningPropLayerIndex = LayerMask.NameToLayer("FittingOwningProp");
        StoredOwningPropLayerIndex = LayerMask.NameToLayer("StoredOwningProp");
        OwningPropLayerIndex = LayerMask.NameToLayer("OwningProp");
        PropLayerIndex = LayerMask.NameToLayer("Prop");
        OutlinePropLayerIndex = LayerMask.NameToLayer("OutlineProp");
    }
}
