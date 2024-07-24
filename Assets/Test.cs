using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Sprite spriteToCheck;

    [Button]
    void Start()
    {
        if (IsSlicedSprite(spriteToCheck))
        {
            Debug.Log("The sprite is sliced.");
        }
        else
        {
            Debug.Log("The sprite is not sliced.");
        }
    }

    bool IsSlicedSprite(Sprite sprite)
    {
        Vector4 border = sprite.border;
        return border.x > 0 || border.y > 0 || border.z > 0 || border.w > 0;
    }
}