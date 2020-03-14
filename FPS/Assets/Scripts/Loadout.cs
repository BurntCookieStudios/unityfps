using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loadout : MonoBehaviour
{
    #region Variablen

    public Weapon[] loadout; //Erst-, Zweit-, Drittwaffe
    public Transform weaponParent;

    //Bulletholes, Particles on Gunshot impact
    public GameObject bulletholePrefab;
    public GameObject hitSurvacePrefab;
    public GameObject hitBloodPrefab;
    public LayerMask canBeShotSurvace;

    private float currentCooldown; //based on firerate

    private int currentIndex;
    private GameObject currentWeapon;

    #endregion

    #region Monobehaviour Callbacks

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) Equip(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Equip(1);

        if (currentWeapon)
        {
            Aim(Input.GetMouseButton(1));
            if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
            {
                switch (loadout[currentIndex].weaponType)
                {
                    case Weapon.wType.Meele:
                        break;
                    case Weapon.wType.Offhand:
                        Shoot();
                        break;
                    case Weapon.wType.Mainhand:
                        Shoot();
                        break;
                }

            }

            //weapon position Dehnung: Waffe wird durch Bewegung zurueck auf den Ausgangspunkt gesetzt
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f); //Lerp = Uerbergang

            //cooldown 
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
        }
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

    private void Shoot()
    {
        Transform spawn = transform.Find("Cameras/Normal Camera"); //Raycast aus der Sicht des Spielers, um mittig yzu schiessen.
                                                                   //Ort aus dem der Spieler schiesst.     

        //bloom
        Vector3 bloom = spawn.position + spawn.forward * 1000f;
        bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * spawn.up;
        bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * spawn.right;
        //bloom in eine Richtung fuer den Raycast umwandeln
        bloom -= spawn.position;
        bloom.Normalize();

        //Raycast
        RaycastHit hit = new RaycastHit(); //Objekt, das von dem Raycast getroffen wird.
        if (Physics.Raycast(spawn.position, bloom, out hit, 1000f, canBeShotSurvace))
        {
            GameObject newHole = Instantiate(bulletholePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject; //hit.point, Ort, an dem der Raycast mit dem Objekt interagiert
                                                                                                                                    //hit.normal * 0.001f => direkt ueber der Layer des Objektes, wir wollen bullethole nicht unter dem Objekt haben.
            newHole.transform.LookAt(hit.point + hit.normal); //um aus Richtung des interagierten Objekts "zu gucken"

            GameObject newImpactVFX = Instantiate(hitSurvacePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
            newImpactVFX.transform.LookAt(hit.point + hit.normal); //um aus Richtung des interagierten Objekts "zu gucken"

            Destroy(newHole, 5f); //nach 5 sekunden wird das Loch zerstoert
            Destroy(newImpactVFX, 1f); //nach 1 sekunden wird das Loch zerstoert
        }

        //gun fx
        currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0); //Rotation wird durch Sway automatisch zurueck rotiert.
        currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;

        //cooldown
        currentCooldown = loadout[currentIndex].firerate;
    }

    #endregion
}
