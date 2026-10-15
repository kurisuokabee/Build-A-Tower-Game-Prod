using UnityEditor.Build;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerSectionUI : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private TowerSection sectionPrefab;
    [SerializeField] private TowerSectionGhost ghostPrefab;
    [SerializeField] private Tower tower;

    //Ghost is a preview of the tower section prefab
    private TowerSectionGhost draggedGhost;

    //Clicking from the UI
    public void OnBeginDrag(PointerEventData eventData)
    {
        draggedGhost = Instantiate(ghostPrefab);

        draggedGhost.SetSprite(sectionPrefab);
        draggedGhost.SetAlpha(1f);
    }

    //While holding mouse click
    public void OnDrag(PointerEventData eventData)
    {   
        Vector3 mousePosition = MousePosition(eventData);

        if (IsCursorOverTower(mousePosition))
        {
            int index = tower.GetInsertionIndex(mousePosition.y);

            draggedGhost.transform.position =
                tower.GetInsertionPosition(index);

            draggedGhost.SetAlpha(0.5f);
        }
        else
        {   
            draggedGhost.transform.position = mousePosition;

            draggedGhost.SetAlpha(1f);
        }
    }

    //After letting go off mouse click
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 mousePosition = MousePosition(eventData);

        if (IsCursorOverTower(mousePosition))
        {   
            int index = tower.GetInsertionIndex(mousePosition.y);

            Destroy(draggedGhost.gameObject);

            TowerSection newSection =
                Instantiate(sectionPrefab);

            tower.InsertTowerSection(index, newSection);
        }
        else
        {
            Destroy(draggedGhost.gameObject);
        }

        draggedGhost = null;
    }

    bool IsCursorOverTower(Vector3 mousePosition)
    {   
        float halfWidth = sectionPrefab.width / 2f;

        bool isOverTower =
            mousePosition.x >= tower.transform.position.x - halfWidth &&
            mousePosition.x <= tower.transform.position.x + halfWidth &&
            mousePosition.y >= tower.GetBottomY() &&
            mousePosition.y <= tower.GetTopY();

        return isOverTower;    
    }

    Vector3 MousePosition(PointerEventData eventData)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(
            eventData.position
        );

        mousePosition.z = 0;

        return mousePosition;
    }

}