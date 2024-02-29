using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIItem : UIComponentEx
{
    [SerializeField]
    RawImage itemImage;

    [SerializeField]
    Image backgroundImage;

    public void SetItemImage(Texture texture)
    {
        itemImage.texture = texture;
    }

    public void SetBackgroundColor(Color color)
    {
        backgroundImage.color = color;
    }
}
