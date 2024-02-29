using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtility
{
    public static void DFS(this Transform transform, Action<Transform> action)
    {
        Stack<Transform> s = new Stack<Transform>();
        s.Push(transform);

        while (s.Count > 0)
        {
            Transform t = s.Pop();
            action(t);
            for (int i = 0; i < t.childCount; ++i)
                s.Push(t.GetChild(i));
        }
    }
}
