using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
public class Weapon : ScriptableObject
{
    public enum wType
    {
        Automatic,
        SemiAuto,
        Shotgun,
        Grenade,
        Meele,
    }

    public wType weaponType;
    public string name;
    public float firerate;
    public int damage;
    public int ammo; //insgesamte munition
    public int clipsize; //munition bis zum reload
    public float bloom;
    public float recoil;
    public float kickback;
    public float aimSpeed; //Schnelligkeit in Sight / Scope einer Waffe zu gehen (bsp.: Sniper langsamer als Pistole)
    public float reloadTime;
    public GameObject prefab;
    public AudioClip sound;

    private int clip; //current amo
    private int stash; //current ammo clip

    public void Init()
    {
        stash = ammo;
        clip = clipsize;
    }

    public bool FireBullet() //schaut, ob munition vorhanden sind, sodass geschossen werden kann
    {
        if (clip > 0)
        {
            clip--;
            return true;
        }
        else return false;
    }

    public bool IsClipFull()
    {
        if (clip == clipsize) return true;
        else return false;
    }

    public bool StashEmpty()
    {
        if (stash == 0) return true;
        else return false;
    }

    public void Reload()
    {
        stash += clip;
        clip = Mathf.Min(clipsize, stash);
        stash -= clip;
    }

    public int GetStash() { return stash; } //to update ui
    public int GetClip() { return clip;  } //to update ui
}
