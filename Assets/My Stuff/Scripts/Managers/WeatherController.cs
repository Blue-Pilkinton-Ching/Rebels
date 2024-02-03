using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public static WeatherController Singleton;
    public Material ShadowMaterial;
    public float LightAngle;
    public float DayLength = 20;
    int shadowTimeID;

    private void Awake()
    {
        Singleton = this;
        ShadowMaterial.SetFloat("_Sun_Angle", LightAngle);

        shadowTimeID = Shader.PropertyToID("_Time_Of_Day");
    }

    private void Start()
    {
        InvokeRepeating("UpdateTime", 0, 0.1f);
    }

    void UpdateTime()
    {
        ShadowMaterial.SetFloat(
            shadowTimeID,
            NetworkManager.Singleton.ServerTime.TimeAsFloat / DayLength
        );
    }
}
