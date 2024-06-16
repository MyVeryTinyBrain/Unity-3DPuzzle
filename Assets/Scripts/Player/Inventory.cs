using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : ComponentEx
{
    [SerializeField, ReadOnlyInRuntime]
    Transform itemPivot;

    [SerializeField, ReadOnlyInRuntime]
    ManualCapureCamera storedOwningPropCamera;

    [SerializeField, ReadOnlyInRuntime]
    ItemCanvas itemCanvas;

    int currentItemIndex = -1;
    public ItemObject handlingItem { get; private set; }
    Coroutine itemPickupCoroutine;
    List<ItemObject> items = new List<ItemObject>();

    public bool isHandling { get; private set; } = false;

    public ItemSocket itemSocket { get; private set; }
    ItemSocket newItemSocket;
    Coroutine fitItemToSocketCoroutine;

    readonly Vector2Int RTSize = new Vector2Int(128, 128);

    void StoreAndStopPickingUp()
    {
        if (handlingItem)
        {
            handlingItem.transform.SetParent(transform);
            handlingItem.transform.localPosition = Vector3.zero;
            handlingItem.transform.rotation = itemPivot.transform.rotation;
            handlingItem.SetChildLayers(Layers.instance.StoredOwningPropLayerIndex);
            handlingItem = null;
        }
        if (itemPickupCoroutine != null)
        {
            StopCoroutine(itemPickupCoroutine);
            itemPickupCoroutine = null;
        }
    }

    public void PickUpItem(ItemObject itemObject)
    {
        if (fitItemToSocketCoroutine != null)
        {
            StopCoroutine(fitItemToSocketCoroutine);
            fitItemToSocketCoroutine = null;
        }

        StoreAndStopPickingUp();
        AddItem(itemObject, true);

        handlingItem = itemObject;

        itemObject.transform.SetParent(itemPivot);
        itemObject.NotifyPickup();

        itemPickupCoroutine = StartCoroutine(ItemPickupCoroutine());

        CaptureOwningItems();
    }

    public void PickUpInnerItem(ItemObject itemObject)
    {
        AddItem(itemObject, false);

        itemObject.transform.SetParent(transform);
        itemObject.transform.localPosition = Vector3.zero;
        itemObject.transform.rotation = itemPivot.transform.rotation;
        itemObject.NotifyPickup();

        CaptureOwningItems();
    }

    public bool DropCurrentItem(Vector3 position)
    {
        if (currentItemIndex == -1)
            return false;

        return DropItem(currentItemIndex, position);
    }

    public bool DropItem(int index, Vector3 position)
    {
        if (itemPickupCoroutine != null)
            return false;

        ItemObject itemObject = items[index];
        if (handlingItem == itemObject)
            StoreAndStopPickingUp();

        itemObject.transform.SetParent(null);
        itemObject.transform.position = position;
        itemObject.transform.rotation = itemPivot.transform.rotation;
        itemObject.OnBeginDrop();

        RemoveItem(index);
        CaptureOwningItems();

        return true;
    }

    public bool FitCurrentItemToSocket()
    {
        if (currentItemIndex == -1)
            return false;

        if (!itemSocket)
            return false;

        return FitItemToSocket(currentItemIndex, itemSocket);
    }

    public bool FitItemToSocket(int index, ItemSocket itemSocket)
    {
        if (itemPickupCoroutine != null)
            return false;

        if (itemSocket.itemObject)
            return false;

        ClearItemSocket();

        ItemObject itemObject = items[index];
        if (handlingItem == itemObject)
            StoreAndStopPickingUp();

        itemObject.transform.SetParent(itemSocket.itemTransform);
        itemObject.transform.position = itemSocket.itemTransform.position;
        itemObject.transform.rotation = itemSocket.itemTransform.rotation;
        itemObject.OnFitToItemSocket(itemSocket);

        itemSocket.AttachItem(itemObject);

        RemoveItem(index);
        CaptureOwningItems();

        return true;
    }

    private IEnumerator ItemPickupCoroutine()
    {
        const float duration = 0.2f;
        float accumulate = 0.0f;
        Vector3 itemPickedUpPosition = handlingItem.transform.position;
        Quaternion itemPickedUpRotation = handlingItem.transform.rotation;

        while (accumulate < duration)
        {
            yield return YieldRule.waitForEndOfFrame;

            accumulate += Time.deltaTime;
            float ratio = Mathf.Clamp(accumulate / duration, 0.0f, 1.0f);
            // ���� �ð����� �������� �κ��丮�� �̵��մϴ�.
            handlingItem.transform.position = Vector3.Lerp(itemPickedUpPosition, itemPivot.transform.position, ratio);
            handlingItem.transform.rotation = Quaternion.Lerp(itemPickedUpRotation, itemPivot.transform.rotation, ratio);
        }

        handlingItem.transform.position = itemPivot.transform.position;
        handlingItem.transform.rotation = itemPivot.transform.rotation;
        itemPickupCoroutine = null;
    }

    private IEnumerator FitItemToItemSocketCoroutine()
    {
        const float duration = 0.2f;
        float accumulate = 0.0f;
        while (accumulate < duration && handlingItem)
        {
            accumulate += Time.deltaTime;
            float ratio = Mathf.Clamp(accumulate / duration, 0.0f, 1.0f);
            // ���� �ð����� �������� ������ ���� ��ġ�� �̵��մϴ�.
            handlingItem.transform.position = Vector3.Lerp(itemPivot.transform.position, itemSocket.itemTransform.position, ratio);
            handlingItem.transform.rotation = Quaternion.Lerp(itemPivot.transform.rotation, itemSocket.itemTransform.rotation, ratio);

            yield return YieldRule.waitForEndOfFrame;
        }

        if (handlingItem)
        {
            handlingItem.transform.position = itemSocket.itemTransform.position;
            handlingItem.transform.rotation = itemSocket.itemTransform.rotation;
        } 
        fitItemToSocketCoroutine = null;
    }

    private void AddItem(ItemObject itemObject, bool selectThisItem)
    {
        items.Add(itemObject);

        storedOwningPropCamera.ReserveRenderTextures(RTSize, items.Count);
        itemCanvas.SetItemCount(items.Count);
        for (int i = 0; i < items.Count; ++i)
        {
            itemCanvas.SetItemTexture(i, storedOwningPropCamera.GetRenderTexture(i));
        }

        if (selectThisItem)
            currentItemIndex = items.Count - 1;
        itemCanvas.SetItemHighlight(currentItemIndex);
    }

    private void RemoveItem(int index)
    {
        ItemObject targetItem = items[index];

        items.RemoveAt(index);
        itemCanvas.SetItemCount(items.Count);

        if (currentItemIndex >= index)
        {
            currentItemIndex = Mathf.Clamp(currentItemIndex - 1, -1, items.Count - 1);
        }
        
        storedOwningPropCamera.ReserveRenderTextures(RTSize, items.Count);
        itemCanvas.SetItemCount(items.Count);
        for (int i = 0; i < items.Count; ++i)
        {
            itemCanvas.SetItemTexture(i, storedOwningPropCamera.GetRenderTexture(i));
        }

        if (handlingItem == targetItem)
            handlingItem = null;

        SetCurrentItem(currentItemIndex);
    }

    private void CaptureOwningItems()
    {
        if (items.Count == 0)
            return;

        const float distance = 0.5f;

        // ���� ����ִ� �������� ��ġ, ȸ��, ���̾ ����մϴ�.
        Vector3 currentItemPosition = currentItemIndex >= 0 ? items[currentItemIndex].transform.position : Vector3.zero;
        Quaternion currentItemRotation = currentItemIndex >= 0 ? items[currentItemIndex].transform.rotation : Quaternion.identity;
        int currentItemLayer = currentItemIndex >= 0 ? items[currentItemIndex].gameObject.layer : Layers.instance.StoredOwningPropLayerIndex;

        // ������ ��� �������� ���̾ �ӽ÷� �����մϴ�.
        foreach (ItemObject item in items)
            item.SetChildLayers(Layers.instance.OwningPropLayerIndex);

        // ĸ�� ���� ȣ���� �Լ��Դϴ�. ������Ʈ�� ĸ���ϱ� ���� ���·� �����մϴ�.
        Action<Camera, int> beginCaptureAction = (camera, index) => {
            // ĸ���� ������Ʈ�� ĸ�� ��ġ�� �̵��մϴ�.
            items[index].transform.position = transform.position;
            items[index].transform.rotation = transform.rotation;
            // ĸ�� ������ ���̾�� �����մϴ�.
            items[index].SetChildLayers(Layers.instance.StoredOwningPropLayerIndex);
            // ī�޶� �������� ���鿡 ��ġ�ϵ��� �մϴ�.
            camera.transform.position = transform.position - items[index].transform.forward * distance;
            camera.transform.rotation = items[index].transform.rotation;
        };
        // ĸ�� �Ŀ� ȣ���� �Լ��Դϴ�. ������Ʈ ���¸� �ǵ����ϴ�.
        Action<Camera, int> endCaptureAction = (camera, index) => {
            // ��� �ִ� �������̸� ���� ��ġ�� �̵���ŵ�ϴ�.
            if (index == currentItemIndex)
            {
                items[index].transform.position = currentItemPosition;
                items[index].transform.rotation = currentItemRotation;
            }
            // ���� ���̾�� �����ϴ�.
            items[index].SetChildLayers(Layers.instance.OwningPropLayerIndex);
        };
        // ��� �������� ĸ���մϴ�.
        storedOwningPropCamera.Capture(RTSize, items.Count, beginCaptureAction, endCaptureAction);

        for (int i = 0; i < items.Count; ++i)
        {
            if (i == currentItemIndex)
                items[i].SetChildLayers(currentItemLayer);
            else
                items[i].SetChildLayers(Layers.instance.StoredOwningPropLayerIndex);
        }
    }

    public void SetNextCurrentItem(int direction)
    {
        int index = currentItemIndex;
        if (items.Count == 0)
        {
            index = -1;
            return;
        }

        direction = Mathf.Clamp(direction, -1, 1);
        index = index + direction;
        if (index < -1)
            index = items.Count - 1;
        else if (index >= items.Count)
            index = -1;

        SetCurrentItem(index);
    }

    public void SetCurrentItem(int index)
    {
        StoreAndStopPickingUp();

        itemCanvas.SetItemHighlight(index);

        currentItemIndex = index;
        if (currentItemIndex == -1)
            return;

        ItemObject itemObject = items[currentItemIndex];

        itemObject.transform.SetParent(itemPivot);
        itemObject.transform.position = itemPivot.transform.position;
        itemObject.transform.rotation = itemPivot.transform.rotation;
        itemObject.SetChildLayers(Layers.instance.OwningPropLayerIndex);

        handlingItem = itemObject;
    }

    public bool OperateHandlingItem()
    {
        if (!handlingItem)
            return false;

        if (isHandling)
            return false;

        handlingItem.transform.localPosition = new Vector3(-itemPivot.transform.localPosition.x, -itemPivot.transform.localPosition.y, 0);
        handlingItem.transform.localEulerAngles = Vector3.zero;
        handlingItem.OnBeginHandlingItem();

        isHandling = true;
        return true;
    }

    public void StopOperateHandlingItem()
    {
        if (!handlingItem)
            return;

        if (!isHandling)
            return;

        handlingItem.transform.localPosition = Vector3.zero;
        handlingItem.transform.localEulerAngles = Vector3.zero;
        handlingItem.OnEndHandlingItem();

        isHandling = false;
    }

    public void FitToItemSocket(ItemSocket socket)
    {
        if (socket.itemObject)
            return;

        newItemSocket = socket;
    }

    private void OnBeginFitToItemSocket(ItemSocket socket)
    {
        if (fitItemToSocketCoroutine != null)
        {
            StopCoroutine(fitItemToSocketCoroutine);
            fitItemToSocketCoroutine = null;
        }

        handlingItem.OnBeginFittingToItemSocket(socket);
        fitItemToSocketCoroutine = StartCoroutine(FitItemToItemSocketCoroutine());
    }

    private void OnEndFitToItemSocket(ItemSocket socket)
    {
        if (fitItemToSocketCoroutine != null)
        {
            StopCoroutine(fitItemToSocketCoroutine);
            fitItemToSocketCoroutine = null;
        }

        if (handlingItem)
        {
            handlingItem.OnEndFittingToItemSocket(socket);
            handlingItem.transform.position = itemPivot.transform.position;
            handlingItem.transform.rotation = itemPivot.transform.rotation;
        }
    }

    private void ClearItemSocket()
    {
        newItemSocket = null;
        CheckItemSocket();
    }

    private void CheckItemSocket()
    {
        if (itemPickupCoroutine != null)
            return;

        if (newItemSocket)
        {
            if (!handlingItem)
                newItemSocket = null;
            if (handlingItem && newItemSocket.targetItemName != handlingItem.objectName)
                newItemSocket = null;
        }
        if (newItemSocket != itemSocket)
        {
            if (itemSocket)
                OnEndFitToItemSocket(itemSocket);
            itemSocket = newItemSocket;
            if (newItemSocket)
                OnBeginFitToItemSocket(newItemSocket);
        }
        newItemSocket = null;
    }

    public void SetHandlingItemVisible(bool visible)
    {
        itemPivot.gameObject.SetActive(visible);
    }

    protected override void Update()
    {
        base.Update();

        CheckItemSocket();
        if(itemSocket && itemPickupCoroutine == null && fitItemToSocketCoroutine == null)
        {
            handlingItem.transform.position = itemSocket.itemTransform.position;
            handlingItem.transform.rotation = itemSocket.itemTransform.rotation;
        }
    }
}
