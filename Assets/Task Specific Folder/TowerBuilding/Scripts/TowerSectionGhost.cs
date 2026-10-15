using UnityEngine;

public class TowerSectionGhost : MonoBehaviour
{
    public void SetSprite(TowerSection section)
    {
        SpriteRenderer ghostRenderer =
            GetComponent<SpriteRenderer>();

        SpriteRenderer sectionRenderer =
            section.GetComponent<SpriteRenderer>();

        ghostRenderer.color = sectionRenderer.color;

        // For future use when the sprite art is available
        // ghostRenderer.sprite = sectionRenderer.sprite;
        // ghostRenderer.flipX = sectionRenderer.flipX;
        // ghostRenderer.flipY = sectionRenderer.flipY;
    }
    
    public void SetAlpha(float alpha)
    {
        SpriteRenderer spriteRenderer =
            GetComponent<SpriteRenderer>();

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}