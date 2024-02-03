using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static SpawnController Singleton;

    private List<Transform> spawnLocations = new List<Transform>();
    private void Awake()
    {
        Singleton = this;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                spawnLocations.Add(transform.GetChild(i));
            }
        }

    }
    public Vector3 GetSpawnLocation()
    {
        int randomIndex = Random.Range(0, Singleton.spawnLocations.Count);

        return Singleton.spawnLocations[randomIndex].position;
    }
}
