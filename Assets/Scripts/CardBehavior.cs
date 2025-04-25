using System.Collections;
using UnityEngine;

public class CardBehavior : MonoBehaviour
{
    public Material frontMaterial;
    public Material backMaterial;
    public int cardId;

    private bool isFlipped = false;
    private bool isAnimating = false;
    private Renderer cardRenderer;

    void Start()
    {
        cardRenderer = GetComponentInChildren<Renderer>();
        SetMaterial();
    }

    void OnMouseDown()
    {
        if (!isAnimating && !isFlipped && GameManager.Instance.CanFlip())
            StartCoroutine(FlipCard());
    }

    IEnumerator FlipCard()
    {
        isAnimating = true;
        yield return Rotate(180);
        isFlipped = true;
        SetMaterial();
        GameManager.Instance.CardRevealed(this);
        isAnimating = false;
    }

    public void FlipBack()
    {
        StartCoroutine(FlipBackRoutine());
    }

    IEnumerator FlipBackRoutine()
    {
        isAnimating = true;
        yield return Rotate(-180);
        isFlipped = false;
        SetMaterial();
        isAnimating = false;
    }

    IEnumerator Rotate(float angle)
    {
        float duration = 0.3f, time = 0;
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, angle, 0);
        while (time < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, endRot, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRot;
    }

    void SetMaterial()
    {
        if (cardRenderer != null)
            cardRenderer.material = isFlipped ? frontMaterial : backMaterial;
    }
}
