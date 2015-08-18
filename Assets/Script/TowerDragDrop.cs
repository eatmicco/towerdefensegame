using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class TowerDragDrop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    public TowerManager towerManager;
    public MinionManager minionManager;
    public GameObject towerPrefab;
    public int id;

    private GameObject draggedGO_;
    private Vector3 startPosition_;

    public void OnBeginDrag(PointerEventData eventData)
    {
        draggedGO_ = Instantiate(towerPrefab) as GameObject;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        startPosition_ = worldPos;
        draggedGO_.transform.localPosition = startPosition_;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        draggedGO_.transform.localPosition = worldPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(draggedGO_);
        draggedGO_ = null;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(eventData.position);
        worldPos.z = 0;
        GameObject tower = towerManager.AddTower(id, worldPos);
        minionManager.obstacles.Add(tower.transform);
    }
}
