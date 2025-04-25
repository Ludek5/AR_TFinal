using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CardSpawner : MonoBehaviour
{
    public GameObject cardPrefab;
    public ARRaycastManager raycastManager;

    public int gridRows = 4;
    public int gridCols = 4;
    public float spacing = 0.45f;

    private bool cardsPlaced = false;
    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && !cardsPlaced)
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 spawnCenter = Camera.main.transform.position + forward * 1.2f;
            PlaceCards(spawnCenter);
            cardsPlaced = true;
        }
#else
        if (Input.touchCount > 0 && !cardsPlaced)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    Vector3 spawnCenter = hitPose.position + Vector3.forward * 0.5f;
                    PlaceCards(spawnCenter);
                    cardsPlaced = true;
                }
            }
        }
#endif
    }

    void PlaceCards(Vector3 center)
    {
        float startX = center.x - (gridCols / 2f) * spacing;
        float startY = center.y;
        float startZ = center.z;

        int totalCards = gridRows * gridCols;
        List<int> cardIds = new List<int>();

        for (int i = 0; i < totalCards / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }

        Shuffle(cardIds);

        int index = 0;
        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                Vector3 pos = new Vector3(startX + j * spacing, startY - i * spacing, startZ);
                Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward);
                GameObject card = Instantiate(cardPrefab, pos, rot);
                card.transform.localScale = Vector3.one * 0.2f;
                card.GetComponent<CardBehavior>().cardId = cardIds[index++];
            }
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[rand];
            list[rand] = temp;
        }
    }
}



