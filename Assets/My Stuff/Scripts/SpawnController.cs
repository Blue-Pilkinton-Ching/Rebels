using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnController : MonoBehaviour
{
    public static SpawnController Singleton;

    public float rangeForPlayerToSpawn = 7;
    private Transform[] spawnLocations;
    private void Awake()
    {
        Singleton = this;
        spawnLocations = new Transform[transform.childCount];

        for (int i = 0; i < spawnLocations.Length; i++)
        {
            if (transform.GetChild(i).gameObject.activeInHierarchy)
            {
                spawnLocations[i] = transform.GetChild(i);
            }
        }
    }
    public Vector3 GetSpawnLocation()
    {
        int randomIndex = Random.Range(0, Singleton.spawnLocations.Length);

        Debug.Log(randomIndex);

        return Singleton.spawnLocations[randomIndex].position;
    }
}
