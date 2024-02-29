using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NumberOnlyStringAttribute : PropertyAttribute
{
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(NumberOnlyStringAttribute))]
public class NumberOnlyStringDrawer : PropertyDrawer
{
    static readonly List<char> digits = new List<char>(){ '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(position, property, label, true);
        if (property.propertyType != SerializedPropertyType.String)
        {
            return;
        }

        if (EditorGUI.EndChangeCheck())
        {
            bool valid = true;
            for (int i = 0; i < property.stringValue.Length && valid; ++i)
                valid = digits.Contains(property.stringValue[i]);

            if (!valid)
                property.stringValue = "";
        }
    }
}
#endif