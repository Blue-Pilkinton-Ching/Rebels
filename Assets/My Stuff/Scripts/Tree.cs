using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public float TimeUntilShake = 5;
    public float ShakeDistance = 0.3f;
    public float ShakeTime = 5;
    public float ShakeAmount = 4;
    public float EasePeriod = -0.7f;

    [SerializeField]
    private Collider2D triggerCollider;

    private bool wasInside;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(TryShake(collision));
        }
    }

    private IEnumerator TryShake(Collider2D collider2D)
    {
        if (!wasInside)
        {
            wasInside = true;
            yield return new WaitForSeconds(TimeUntilShake);
            if (triggerCollider.IsTouching(collider2D))
            {
                transform.DOMoveX(transform.position.x + ShakeDistance, ShakeTime / 2).SetEase(Ease.InOutFlash, ShakeAmount, -0.75f).OnComplete(() =>
                    transform.DOMoveX(transform.position.x + ShakeDistance, ShakeTime / 2).SetEase(Ease.InOutFlash, ShakeAmount, 1));
                wasInside = false;
                StartCoroutine(TryShake(collider2D));
            }
        }
    }
}
