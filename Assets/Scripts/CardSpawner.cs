using System.Collections.Generic;
using UnityEngine;

public class CardSpawner : MonoBehaviour
{
    public static CardSpawner Instance { get; private set; }

    public GameObject cardPrefab;

    public int gridRows = 4;
    public int gridCols = 4;
    public float spacingX = 0.22f;
    public float spacingY = 0.3f;
    public float cardScale = 0.2f;

    public List<Material> cardFrontMaterials; // Materiales de frente
    public Material cardBackMaterial; // Material trasero

    private List<int> cardIds = new List<int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenerateCardIds()
    {
        cardIds.Clear();
        for (int i = 0; i < (gridRows * gridCols) / 2; i++)
        {
            cardIds.Add(i);
            cardIds.Add(i);
        }
        Shuffle(cardIds);
    }

    public void SpawnCards(Vector3 center)
    {
        float startX = center.x - (gridCols / 2f) * spacingX;
        float startY = center.y;
        float startZ = center.z;

        int index = 0;

        for (int i = 0; i < gridRows; i++)
        {
            for (int j = 0; j < gridCols; j++)
            {
                Vector3 pos = new Vector3(startX + j * spacingX, startY - i * spacingY, startZ);
                Quaternion rot = Quaternion.LookRotation(Camera.main.transform.forward);

                GameObject card = Instantiate(cardPrefab, pos, rot);
                card.transform.localScale = Vector3.one * cardScale;

                // Asignar ID
                CardBehavior cardBehavior = card.GetComponent<CardBehavior>();
                cardBehavior.cardId = cardIds[index];

                // Asignar materiales
                if (cardIds[index] >= 0 && cardIds[index] < cardFrontMaterials.Count)
                {
                    cardBehavior.SetFrontMaterial(cardFrontMaterials[cardIds[index]]);
                }
                cardBehavior.SetBackMaterial(cardBackMaterial);

                GameManager.Instance.RegisterCard(cardBehavior);

                index++;
            }
        }
    }

    private void Shuffle(List<int> list)
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