using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    #region Variablen

    public Gun[] loadout; //Erst-, Zweit-, Drittwaffe
    public Transform weaponParent;

    private int currentIndex;
    private GameObject currentWeapon;

    #endregion

    #region Monobehaviour Callbacks

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);

        if (currentWeapon) Aim(Input.GetMouseButton(1));
    }

    #endregion

    #region Methoden

    private void Equip(int _i)
    {
        if (currentWeapon != null) Destroy(currentWeapon); //ausgeruestete Waffe vorm Equip einer anderen entfernen.

        currentIndex = _i;

        GameObject newEquipment = Instantiate(loadout[_i].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newEquipment.transform.localPosition = Vector3.zero; //um sicher zu gehen, dass die Waffe aufjedenfall an der Position des Parents ist.
        newEquipment.transform.localEulerAngles = Vector3.zero; //^

        currentWeapon = newEquipment;
    }

    private void Aim(bool _isAiming)
    {
        Transform anchor = currentWeapon.transform.Find("Anchor");
        Transform state_ads = currentWeapon.transform.Find("States/ADS");
        Transform state_hip = currentWeapon.transform.Find("States/Hip");

        if (_isAiming)
        {
            anchor.position = Vector3.Lerp(anchor.position, state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed); //in Visir gehen
        }
        else
        {
            anchor.position = Vector3.Lerp(anchor.position, state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed); //aus Visir gehen

        }
    }

    #endregion
}
