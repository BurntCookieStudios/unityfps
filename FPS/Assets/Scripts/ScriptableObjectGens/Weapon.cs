using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public enum wType
    {
        Mainhand,
        Offhand,
        Meele,
    }

    public wType weaponType;
    public string name;
    public float firerate;
    public int damage;
    public float bloom;
    public float recoil;
    public float kickback;
    public float aimSpeed; //Schnelligkeit in Sight / Scope einer Waffe zu gehen (bsp.: Sniper langsamer als Pistole)
    public GameObject prefab;
}
