using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static SpawnController Singleton;

    public float rangeForPlayerToSpawn = 7;
    public Transform[] spawnLocations;
    private void Awake()
    {
        Singleton = this;
        spawnLocations = transform.GetComponentsInChildren<Transform>();
    }
    public Vector3 GetSpawnLocation()
    {
        int val = Random.Range(0, spawnLocations.Length);
        bool withinRangeOfPlayer = false;

        for (int i = val; i < spawnLocations.Length; i++)
        {
            if (i > spawnLocations.Length)
            {
                val = i - spawnLocations.Length;
            }
            else
            {
                val = i;
            }

            foreach (PlayerController player in PlayerController.Players)
            {
                if (Vector2.Distance(player.transform.position, spawnLocations[val].position) <= rangeForPlayerToSpawn)
                {
                    withinRangeOfPlayer = true;
                    break;
                }
            }

            if (!withinRangeOfPlayer)
            {
                break;
            }
        }

        return spawnLocations[val].position;
    }
}
