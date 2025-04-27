using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public ARRaycastManager raycastManager;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI errorsText; 
    public TextMeshProUGUI bestScoreText; 

    private int score = 0;
    private int errors = 0;
    private int bestScore = 0;
    public int maxErrors = 5;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private bool cardsPlaced = false;

    private CardBehavior firstCard;
    private CardBehavior secondCard;
    private List<CardBehavior> spawnedCards = new List<CardBehavior>();
    private int matchCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bestScore = PlayerPrefs.GetInt("BestScore", 0); 
    }

    void Start()
    {
        UpdateScoreText();
        UpdateErrorsText();
        UpdateBestScoreText();
    }

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
        CardSpawner.Instance.GenerateCardIds();
        CardSpawner.Instance.SpawnCards(center);
        cardsPlaced = true;
    }

    public void RegisterCard(CardBehavior card)
    {
        spawnedCards.Add(card);
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
            score += 10;
            UpdateScoreText();

            if (matchCount == (CardSpawner.Instance.gridRows * CardSpawner.Instance.gridCols) / 2)
            {
                Invoke("WinGame", 1f);
            }
        }
        else
        {
            errors++;
            UpdateErrorsText();
            firstCard.FlipBack();
            secondCard.FlipBack();

            if (errors >= maxErrors)
            {
                Invoke("LoseGame", 1f);
            }
        }

        firstCard = null;
        secondCard = null;
    }

    void WinGame()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "¡Ganaste!";
        }

        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
            UpdateBestScoreText();
        }

        Invoke("ResetGame", 3f);
    }

    void LoseGame()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(true);
            gameOverText.text = "Perdiste";
        }
        Invoke("ResetGame", 3f);
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
        score = 0;
        errors = 0;

        UpdateScoreText();
        UpdateErrorsText();

        if (gameOverText != null)
            gameOverText.gameObject.SetActive(false);

        CardSpawner.Instance.GenerateCardIds();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
            scoreText.text = "Puntaje: " + score;
    }

    void UpdateErrorsText()
    {
        if (errorsText != null)
            errorsText.text = "Errores: " + errors + "/" + maxErrors;
    }

    void UpdateBestScoreText()
    {
        if (bestScoreText != null)
            bestScoreText.text = "Mejor Puntaje: " + bestScore;
    }
}