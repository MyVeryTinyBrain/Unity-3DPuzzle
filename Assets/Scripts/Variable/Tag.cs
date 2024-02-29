using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct Tag
{
    public string tag;

    public Tag(string tag)
    {
        this.tag = tag;
    }
    public override bool Equals(object obj)
    {
        Tag other = (Tag)obj;
        if (other == null)
            return false;
        return tag == other.tag;
    }
    public override int GetHashCode()
    {
        return tag.GetHashCode();
    }
    public static bool operator ==(Tag t0, Tag t1)
    {
        return t0.tag == t1.tag;
    }
    public static bool operator !=(Tag t0, Tag t1)
    {
        return t0.tag != t1.tag;
    }
    public static implicit operator Tag(string s)
    {
        return new Tag(s);
    }
    public static implicit operator string(Tag tag)
    {
        return tag.tag;
    }
    public override string ToString()
    {
        return tag;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Tag))]
class TagDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        property.Next(true);

        string[] tags = UnityEditorInternal.InternalEditorUtility.tags;
        int tagIndex;
        for (tagIndex = 0; tagIndex < tags.Length; ++tagIndex)
            if (tags[tagIndex] == property.stringValue)
                break;
        if (tagIndex >= tags.Length)
        {
            tagIndex = -1;
            for(int i = 0; i < tags.Length; ++i)
            {
                if (tags[i] == "Untagged")
                {
                    tagIndex = i;
                    break;
                }
            }
            if(tagIndex >= 0)
            {
                property.stringValue = tags[tagIndex];
            }
        }

        EditorGUI.BeginChangeCheck();
        tagIndex = EditorGUI.Popup(position, label.text, tagIndex, tags);
        if (EditorGUI.EndChangeCheck())
        {
            property.stringValue = tags[tagIndex];
        }
    }
}
#endif