using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Scriptable Objects/Weapon")]
public class Weapon : ScriptableObject
{
    [Range(0.5f, 1)]
    public float ViewFactor = 0.8f;
    public float CameraOrthographicSize = 3;
    public WeaponType WeaponType = WeaponType.Pistel;
    public float MoveForce = 1;
    public float BulletSpeed = 35;
    public GameObject bullet;
    public Vector3 bulletFireLocation;
    public AudioClipVarience[] weaponShootClips;
    public AudioClipVarience[] weaponHitClips;
    public AudioClipVarience[] weaponTakeClips;
    [Range(0, 180)]
    public float weaponInacurracy = 5f;
    public float damageAmount = 3f;

    public AnimationCurve bulletChanceCollide;
}

[System.Serializable]
public enum WeaponType
{
    Pistel, RocketLauncher, Machete, Sniper, Rifle, Gunner, FlameThrower, GrenadeLauncher
}