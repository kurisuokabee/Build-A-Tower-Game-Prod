using UnityEngine;

public class TowerSectionGhost : MonoBehaviour
{
    public void SetSprite(TowerSection section)
    {
        SpriteRenderer ghostRenderer =
            GetComponent<SpriteRenderer>();

        Transform child = FindChildRecursive(section.transform, "Normal Sprite");

        if (child != null)
        {
            SpriteRenderer sectionRenderer = child.GetComponent<SpriteRenderer>();

            ghostRenderer.sprite = sectionRenderer.sprite;
            ghostRenderer.flipX = sectionRenderer.flipX;
            ghostRenderer.flipY = sectionRenderer.flipY;
        }

    }
    
    public void SetAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform result = FindChildRecursive(child, childName);

            if (result != null)
                return result;
        }

        return null;
    }
}