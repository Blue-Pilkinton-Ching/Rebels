using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static SpawnController Singleton;

    public float rangeForPlayerToSpawn = 7;
    private Transform[] spawnLocations;
    private void Awake()
    {
        Singleton = this;
        spawnLocations = transform.GetComponentsInChildren<Transform>();
    }
    public Vector3 GetSpawnLocation()
    {
        int val = Random.Range(0, Singleton.spawnLocations.Length - 1);
        
        bool withinRangeOfPlayer = false;

        for (int i = val; i < Singleton.spawnLocations.Length + val; i++)
        {
            int index;

            if (val + i > Singleton.spawnLocations.Length - 1)
            {
                index = i - (Singleton.spawnLocations.Length - 1);
            }
            else
            {
                index = i;
            }

            foreach (PlayerController player in PlayerController.Players)
            {
                if (player == null)
                {
                    continue;
                }

                if (Vector2.Distance(player.transform.position, Singleton.spawnLocations[index].position) <= Singleton.rangeForPlayerToSpawn)
                {
                    withinRangeOfPlayer = true;
                    break;
                }
            }

            if (!withinRangeOfPlayer)
            {
                val = index;
                break;
            }
        }

        return spawnLocations[val].position;
    }
}
