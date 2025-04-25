using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject cardPrefab;
    public ARRaycastManager raycastManager;
    public int gridRows = 4;
    public int gridCols = 4;
    public float spacing = 1.2f;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool cardsPlaced = false;

    private CardBehavior firstCard;
    private CardBehavior secondCard;

    private List<int> cardIds = new List<int>();
    private List<CardBehavior> spawnedCards = new List<CardBehavior>();
    private int matchCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        GenerateCardIds();
    }

    void Update()
    {
        if (Input.touchCount > 0 && !cardsPlaced)
        {
            Touch touch = Input.GetTouch(0);
            if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                PlaceCards(hitPose.position);
                cardsPlaced = true;
            }
        }
    }

    void GenerateCardIds()
    {
        cardIds.Clear();
        for (int i = 0; i < (gridRows * gridCols) / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }
        Shuffle(cardIds);
    }

    void PlaceCards(Vector3 center)
    {
        float startX = center.x - (gridCols / 2f) * spacing;
        float startY = center.y + 0.2f; // Altura fija (puedes ajustar)
        float startZ = center.z + 0.5f; // Un poco delante del plano detectado

        int index = 0;

        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                Vector3 pos = new Vector3(startX + j * spacing, startY - i * spacing, startZ);
                Quaternion rotation = Quaternion.LookRotation(Camera.main.transform.forward); // Mira hacia la cámara

                GameObject cardObj = Instantiate(cardPrefab, pos, rotation);
                CardBehavior card = cardObj.GetComponent<CardBehavior>();
                card.cardId = cardIds[index++];
                spawnedCards.Add(card);
            }
        }
    }


    public bool CanFlip()
    {
        return secondCard == null;
    }

    public void CardRevealed(CardBehavior card)
    {
        if (firstCard == null)
        {
            firstCard = card;
        }
        else if (secondCard == null)
        {
            secondCard = card;
            StartCoroutine(CheckMatch());
        }
    }

    IEnumerator CheckMatch()
    {
        yield return new WaitForSeconds(1f);

        if (firstCard.cardId == secondCard.cardId)
        {
            matchCount++;
            if (matchCount == (gridRows * gridCols) / 2)
                Invoke("ResetGame", 2f); // Reiniciar tras pequeño delay
        }
        else
        {
            firstCard.FlipBack();
            secondCard.FlipBack();
        }

        firstCard = null;
        secondCard = null;
    }

    void ResetGame()
    {
        foreach (var card in spawnedCards)
        {
            Destroy(card.gameObject);
        }
        spawnedCards.Clear();
        firstCard = null;
        secondCard = null;
        matchCount = 0;
        cardsPlaced = false;
        GenerateCardIds();
    }

    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}



