using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class PressurePlateCombination : ComponentEx
{
    public event Action<PressurePlateCombination> OnMatchedCallback;
    public event Action<PressurePlateCombination> OnMissMatchedCallback;

    [SerializeField]
    uint width;

    [SerializeField]
    uint height;

    [SerializeField]
    char on = '1';

    [SerializeField]
    char off = '0';

    [SerializeField, TextArea]
    string combination;

    StringBuilder currentCombinationBuilder;

    [SerializeField, ReadOnly]
    string currentCombination;

    [SerializeField, ReadOnly]
    string linearCombination;

    [SerializeField, ReadOnlyInRuntime]
    List<PressurePlate> plates;

    Dictionary<PressurePlate, Vector2Int> plateIndices;

    [SerializeField]
    Vector3 boundsMargin = new Vector3(0, 5, 0);
    Coroutine characterCheckCoroutine = null;

    bool matched = false;

    [SerializeField]
    UnityEvent matchedEvent;

    public bool isCharacterColliding => characterCheckCoroutine != null;

    private void CalculateBounds(out Vector3 worldBoundsCenter, out Vector3 worldBoundsSize)
    {
        Vector3 worldMin = Vector3.one * float.MaxValue, worldMax = Vector3.one * float.MinValue;
        foreach (PressurePlate plate in plates)
        {
            if (!plate)
                continue;
            worldMin = Vector3.Min(worldMin, Vector3.Min(plate.worldSpaceBounds.min, plate.worldSpaceBounds.max));
            worldMax = Vector3.Max(worldMax, Vector3.Max(plate.worldSpaceBounds.min, plate.worldSpaceBounds.max));
        }
        worldBoundsCenter = (worldMin + worldMax) * 0.5f;
        worldBoundsSize = worldMax - worldMin;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector3 worldBoundsCenter, worldBoundsSize;
        CalculateBounds(out worldBoundsCenter, out worldBoundsSize);
        Gizmos.DrawWireCube(worldBoundsCenter, boundsMargin + worldBoundsSize);
    }
#endif

    private IEnumerator CharacterCheckCoroutine()
    {
        while (true)
        {
            Vector3 worldBoundsCenter, worldBoundsSize;
            CalculateBounds(out worldBoundsCenter, out worldBoundsSize);
            if (!Physics.CheckBox(worldBoundsCenter, (boundsMargin + worldBoundsSize) * 0.5f, Quaternion.identity, 1 << Layers.instance.CharacterLayerIndex, QueryTriggerInteraction.Collide))
                break;
            yield return YieldRule.waitForEndOfFrame;
        }
        characterCheckCoroutine = null;
        SetPlateActives(false, false);
        SetPlateOperable(true);
    }

    public virtual void OnMatched()
    {
        if (matched)
            return;

        matched = true;
        matchedEvent.Invoke();
        OnMatchedCallback?.Invoke(this);

        if (characterCheckCoroutine != null)
        {
            StopCoroutine(characterCheckCoroutine);
            characterCheckCoroutine = null;
        }

        SetPlateActives(false, false);
        SetPlateOperable(false);
    }
    
    public virtual void OnMissMatched() 
    {
        OnMissMatchedCallback?.Invoke(this);
        SetPlateActives(false, false);
        SetPlateOperable(false);
    }

    public void SetPlateOperable(bool value)
    {
        foreach (PressurePlate plate in plates)
            plate.operable = value;
    }

    public void SetPlateActives(bool active, bool notify) 
    {
        foreach (PressurePlate plate in plates)
            if (active)
                plate.MoveToActive(notify);
            else
                plate.MoveToDeactive(notify);
        ResetCombination(active);
    }

    private void ResetCombination(bool active)
    {
        currentCombinationBuilder.Clear();
        for (int i = 0; i < width * height; ++i)
            currentCombinationBuilder.Append(active ? on : off);
        currentCombination = currentCombinationBuilder.ToString();
    }

    private void SetCombination(int x, int y, bool active)
    {
        int index = x + y * (int)width;
        if (index >= currentCombinationBuilder.Length)
            return;

        currentCombinationBuilder[index] = active ? on : off;
        currentCombination = currentCombinationBuilder.ToString();

        int end = Mathf.Min(currentCombination.Length, linearCombination.Length);
        bool isValid = true;
        for (int i = 0; i < end && isValid; ++i)
            if (linearCombination[i] == off && currentCombination[i] == on)
                isValid = false;

        if(!isValid)
        {
            OnMissMatched();
        }
        else
        {
            if (currentCombination == linearCombination)
                OnMatched();
        }
    }

    private void OnPlateActived(PressurePlate plate)
    {
        Vector2Int index;
        if (plateIndices.TryGetValue(plate, out index))
            SetCombination(index.x, index.y, true);
    }

    private void OnPlateDeactived(PressurePlate plate)
    {
        Vector2Int index;
        if (plateIndices.TryGetValue(plate, out index))
            SetCombination(index.x, index.y, false);
    }

    private void OnBeginPlateContactWithPlayerController(PlayerController playerController)
    {
        if (matched)
            return;

        if (characterCheckCoroutine == null)
        {
            characterCheckCoroutine = StartCoroutine(CharacterCheckCoroutine());
        }
    }

    private void OnEndPlateContactWithPlayerController(PlayerController playerController)
    {
    }

    protected override void Awake()
    {
        base.Awake();

        StringBuilder linearCombinationBuilder = new StringBuilder();
        foreach (char c in combination)
            if (c == on || c == off)
                linearCombinationBuilder.Append(c);
        linearCombination = linearCombinationBuilder.ToString();

        currentCombinationBuilder = new StringBuilder();
        ResetCombination(false);

        plateIndices = new Dictionary<PressurePlate, Vector2Int>();

        Vector2Int index = Vector2Int.zero;
        foreach (PressurePlate plate in plates)
        {
            plate.OnActivedCallback += OnPlateActived;
            plate.OnDeactivedCallback += OnPlateDeactived;
            plate.OnBeginPlayerControllerContactCallback += OnBeginPlateContactWithPlayerController;
            plate.OnEndPlayerControllerContactCallback += OnEndPlateContactWithPlayerController;

            plateIndices.Add(plate, index);

            index.x++;
            if (index.x >= width)
            {
                index.x = 0;
                index.y++;
            }
        }
    }
}
