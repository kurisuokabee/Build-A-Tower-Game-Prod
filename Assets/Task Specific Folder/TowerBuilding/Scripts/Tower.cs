using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [SerializeField] private TowerSection sectionPrefab;
    [SerializeField] private TowerSection startingBase;

    private List<TowerSection> sections = new();



    public void InsertTowerSection(int index, TowerSection section)
    {
        section.transform.SetParent(transform);

        sections.Insert(index, section);

        RebuildTower();
    }

    private void RebuildTower()
    {
        float currentY = 0;

        // Position sections from bottom to top
        for (int i = sections.Count - 1; i >= 0; i--)
        {
            TowerSection section = sections[i];

            section.transform.localPosition =
                new Vector3(0, currentY + section.height / 2f, 0);

            currentY += section.height; 
        }

        // Put the starting base on top
        startingBase.transform.localPosition =
            new Vector3(0, currentY + startingBase.height / 2f, 0);
    }

    public int GetInsertionIndex(float mouseY)
    {
        for (int i = 0; i < sections.Count; i++)
        {
            TowerSection section = sections[i];

            float sectionTop =
                section.transform.position.y + section.height / 2f;

            if (mouseY > sectionTop)
            {
                return i;
            }
        }

        return sections.Count;
    }

    public Vector3 GetInsertionPosition(int index)
    {
        float y = 0;

        for (int i = sections.Count - 1; i >= index; i--)
        {   
            y += sections[i].height;
        }

        return transform.position + new Vector3(0, y, 0);
    }

    public float GetTopY()
    {
        return startingBase.transform.position.y +  
            startingBase.height / 2f;
    }
    
    public float GetBottomY()
    {
        return transform.position.y;
    }

    
}