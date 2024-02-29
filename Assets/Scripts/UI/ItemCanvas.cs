using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemCanvas : UIComponentEx
{
    [SerializeField]
    UIItem itemPrefab;

    [SerializeField]
    RectTransform itemArea;

    int highlightedItemIndex = 0;
    List<UIItem> items = new List<UIItem>();

    protected override void Awake()
    {
        base.Awake();

        Vector2 sizeDelta = itemArea.sizeDelta;
        sizeDelta.x = WidthByCount(items.Count);
        itemArea.sizeDelta = sizeDelta;
    }

    public void SetItemHighlight(int index)
    {
        foreach (UIItem item in items)
            item.SetBackgroundColor(Color.clear);

        if (index == -1)
            return;

        items[index].SetBackgroundColor(new Color(0, 0, 0, 0.2f));
        highlightedItemIndex = index;
    }

    public void SetItemTexture(int index, Texture texture)
    {
        items[index].SetItemImage(texture);
    }

    Vector2 PositionOfIndex(int index)
    {
        return new Vector2(index * itemPrefab.rectTransform.rect.width, 0);
    }

    float WidthByCount(int count)
    {
        return count * itemPrefab.rectTransform.rect.width;
    }

    public void SetItemCount(int n)
    {
        if (items.Count == n)
            return;

        if (items.Count < n)
        {
            for(int i = items.Count; i < n; ++i)
            {
                UIItem newItem = Instantiate<UIItem>(itemPrefab, rectTransform);
                //newItem.rectTransform.SetParent(rectTransform);
                newItem.rectTransform.anchoredPosition = PositionOfIndex(i);
                items.Add(newItem);
            }
        }
        else if (items.Count > n)
        {
            for(int i = n; i < items.Count; ++i)
                Destroy(items[i].gameObject);
            items.RemoveRange(n, items.Count - n);
        }

        Vector2 sizeDelta = itemArea.sizeDelta;
        sizeDelta.x = WidthByCount(items.Count);
        itemArea.sizeDelta = sizeDelta;
    }
}
