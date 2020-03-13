using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Gun[] loadout; //Erst-, Zweit-, Drittwaffe
    public Transform weaponParent;

    private GameObject currentWeapon;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
    }

    void Equip(int _i)
    {
        if (currentWeapon != null) Destroy(currentWeapon); //ausgeruestete Waffe vorm Equip einer anderen entfernen.

        GameObject newEquipment = Instantiate(loadout[_i].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newEquipment.transform.localPosition = Vector3.zero; //um sicher zu gehen, dass die Waffe aufjedenfall an der Position des Parents ist.
        newEquipment.transform.localEulerAngles = Vector3.zero; //^

        currentWeapon = newEquipment;
    }
}
