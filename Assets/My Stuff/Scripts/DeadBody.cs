using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DeadBody : MonoBehaviour
{
    public float deadBodyAliveTime = 180;
    public float disapearRadius = 7;

    private void Start()
    {
        Invoke("TryDisapear", deadBodyAliveTime);
    }

    private void SetDisapear()
    {
        StartCoroutine(TryDisapear());
    }

    private IEnumerator TryDisapear()
    {
        while (true)
        {
            yield return new WaitForSeconds(2);
            if (Vector2.Distance(transform.position, PlayerController.Players[NetworkManager.Singleton.LocalClientId].transform.position) >= 7)
            {
                StopAllCoroutines();
                Destroy(gameObject);
            }
        }
    }
}
