using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName="New Gun", menuName="Gun")]
public class Gun : ScriptableObject
{
    public string name;
    public float firerate;
    public float aimSpeed; //Schnelligkeit in Sight / Scope einer Waffe zu gehen (bsp.: Sniper langsamer als Pistole)
    public GameObject prefab;
}
