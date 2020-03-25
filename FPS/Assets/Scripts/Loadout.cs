using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Loadout : MonoBehaviourPunCallbacks
{
    #region Variablen

    public Weapon[] loadout; //Erst-, Zweit-, Drittwaffe
    public Transform weaponParent;

    //Bulletholes, Particles on Gunshot impact
    public GameObject bulletholePrefab;
    public GameObject hitSurvacePrefab;
    public GameObject hitBloodPrefab;
    public LayerMask canBeShotSurvace;

    private Image hitmarkerImage;
    private float hitmarkerWait;

    private float currentCooldown; //based on firerate

    private int currentIndex;
    private GameObject currentWeapon;

    public AudioSource sfx;
    public AudioClip hitmarkerSound;

    private bool isReloading = false;
    public bool isAiming = false;

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        if (photonView.IsMine) foreach (Weapon a in loadout) a.Init();
        Equip(0);

        hitmarkerImage = GameObject.Find("HUD/Hitmarker/Image").GetComponent<Image>();
        hitmarkerImage.color = new Color(1, 1, 1, 0);//versichert, dass der hitmarker standardgemaess unsichtbar ist
    }

    void Update()
    {
        if (photonView.IsMine && Pause.paused) return;

        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) photonView.RPC("Equip", RpcTarget.All, 0); //RpcTarget.All = ALLE die mit dem Client ueber den Server verbunden sind, erfahren von der EquipMethode. Siehe RpcTarget-Dokumentation
            if (Input.GetKeyDown(KeyCode.Alpha2)) photonView.RPC("Equip", RpcTarget.All, 1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) photonView.RPC("Equip", RpcTarget.All, 2);

            if (Input.GetKeyDown(KeyCode.R) && !isReloading && !loadout[currentIndex].IsClipFull() && !loadout[currentIndex].StashEmpty()) StartCoroutine(Reload(loadout[currentIndex].reloadTime)); //Reload
        }

        if (currentWeapon)
        {
            if (photonView.IsMine)
            {
                Aim(Input.GetMouseButton(1));

                switch (loadout[currentIndex].weaponType)
                {
                    case Weapon.wType.Meele:
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && !isReloading)
                        {
                            if (loadout[currentIndex].FireBullet())
                            {

                            }
                            else if (!loadout[currentIndex].StashEmpty()) StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                        }
                        break;
                    case Weapon.wType.SemiAuto:
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && !isReloading)
                        {
                            if (loadout[currentIndex].FireBullet())
                            {
                                photonView.RPC("Shoot", RpcTarget.All);
                            }
                            else if (!loadout[currentIndex].StashEmpty()) StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                        }
                        break;
                    case Weapon.wType.Automatic:
                        if (Input.GetMouseButton(0) && currentCooldown <= 0 && !isReloading)
                        {
                            if (loadout[currentIndex].FireBullet())
                            {
                                photonView.RPC("Shoot", RpcTarget.All);
                            }
                            else if (!loadout[currentIndex].StashEmpty()) StartCoroutine(Reload(loadout[currentIndex].reloadTime));
                        }
                        break;
                }
                //cooldown 
                if (currentCooldown > 0) currentCooldown -= Time.deltaTime;
            }
            //weapon position Dehnung: Waffe wird durch Bewegung zurueck auf den Ausgangspunkt gesetzt
            currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f); //Lerp = Uerbergang
        }

        if (photonView.IsMine)
        {
            if (hitmarkerWait > 0)
            {
                hitmarkerWait -= Time.deltaTime;
            }
            else if (hitmarkerImage.color.a > 0)
            {
                hitmarkerImage.color = Color.Lerp(hitmarkerImage.color, new Color(1, 1, 1, 0), Time.deltaTime * 3); //Lerp = Uebergang
            }
        }
    }

    #endregion

    #region Private-Methoden

    [PunRPC] // Methode wird zu allen Systemen vom Client gesendet, die mit ihm durch den Server verknuepft sind => In diesem Fall koennen Gegner einen Waffenwechsel des Klienten sehen
    private void Equip(int _i)
    {
        if (currentWeapon != null)
        {
            if (isReloading) StopCoroutine("Reload"); //Wenn gerade nachgeladen wird, wird automatisch der Prozess beendet, da wir ein IEnumerator haben und wir somit durch "StopCoroutine([NAMEN DER METHODE])" den Prozess beenden koennen.
            isReloading = false;
            Destroy(currentWeapon); //ausgeruestete Waffe vorm Equip einer anderen entfernen.
        }
        currentIndex = _i;

        GameObject newEquipment = Instantiate(loadout[_i].prefab, weaponParent.position, weaponParent.rotation, weaponParent) as GameObject;
        newEquipment.transform.localPosition = Vector3.zero; //um sicher zu gehen, dass die Waffe aufjedenfall an der Position des Parents ist.
        newEquipment.transform.localEulerAngles = Vector3.zero; //^
        newEquipment.GetComponent<Sway>().isMine = photonView.IsMine; //Sway nur fuer den eigenen Spieler

        currentWeapon = newEquipment;
    }

    private void Aim(bool _isAiming)
    {
        isAiming = _isAiming;
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

    [PunRPC]
    private void Shoot()
    {
        Transform spawn = transform.Find("Cameras/Normal Camera"); //Raycast aus der Sicht des Spielers, um mittig yzu schiessen.
                                                                   //Ort aus dem der Spieler schiesst.
                                                                   
        if (photonView.IsMine)
        {
            sfx.PlayOneShot(loadout[currentIndex].sound);
        }

        //bloom
        Vector3 bloom = spawn.position + spawn.forward * 1000f;
        bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * spawn.up;
        bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * spawn.right;
        //bloom in eine Richtung fuer den Raycast umwandeln
        bloom -= spawn.position;
        bloom.Normalize();

        //cooldown
        currentCooldown = loadout[currentIndex].firerate;

        //Raycast
        RaycastHit hit = new RaycastHit(); //Objekt, das von dem Raycast getroffen wird.
        if (Physics.Raycast(spawn.position, bloom, out hit, 1000f, canBeShotSurvace))
        {
            if (hit.collider.gameObject.tag == "Player") //einen Spieler im Netzwerk anschiessen
            {
                GameObject newImpactVFX = Instantiate(hitBloodPrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
                newImpactVFX.transform.LookAt(hit.point + hit.normal); //um aus Richtung des interagierten Objekts "zu gucken"

                Destroy(newImpactVFX, 1f); //nach 1 sekunden wird das Loch zerstoert
                if (photonView.IsMine)
                {
                    //RPC Call zum Schaden hinzufuegen des Gegners
                    hit.collider.transform.root.gameObject.GetPhotonView().RPC("TakeDamage", RpcTarget.All, loadout[currentIndex].damage, PhotonNetwork.LocalPlayer.ActorNumber);

                    //Hitmarker anzeigen
                    hitmarkerImage.color = Color.white;
                    sfx.PlayOneShot(hitmarkerSound);
                    hitmarkerWait = 0.25f; //Zeit die der Hitmarker zum verschwinden brauch
                }
            }
            else
            {
                GameObject newHole = Instantiate(bulletholePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject; //hit.point, Ort, an dem der Raycast mit dem Objekt interagiert
                                                                                                                                        //hit.normal * 0.001f => direkt ueber der Layer des Objektes, wir wollen bullethole nicht unter dem Objekt haben.
                newHole.transform.LookAt(hit.point + hit.normal); //um aus Richtung des interagierten Objekts "zu gucken"

                GameObject newImpactVFX = Instantiate(hitSurvacePrefab, hit.point + hit.normal * 0.001f, Quaternion.identity) as GameObject;
                newImpactVFX.transform.LookAt(hit.point + hit.normal); //um aus Richtung des interagierten Objekts "zu gucken"

                Destroy(newHole, 5f); //nach 5 sekunden wird das Loch zerstoert
                Destroy(newImpactVFX, 1f); //nach 1 sekunden wird das Loch zerstoert
            }
        }

        //gun fx
        if (currentWeapon != null)
        {
            if (isAiming)
            {
                currentWeapon.transform.Rotate(-loadout[currentIndex].recoil/4, 0, 0); //Rotation wird durch Sway automatisch zurueck rotiert.  
                currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback/2; //position wird in Update() automatisch zurueck gesetzt.
            }
            else
            {
                currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0); //Rotation wird durch Sway automatisch zurueck rotiert.  
                currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback; //position wird in Update() automatisch zurueck gesetzt.
            }
        }
    }

    [PunRPC]
    private void TakeDamage(int _amount, int _actor)
    {
        GetComponent<Player>().TakeDamage(_amount, _actor);
    }

    private IEnumerator Reload(float _wait)
    {
            isReloading = true;
            currentWeapon.SetActive(false); //UEBERGANGSWEISE, SOLANGE ES KEINE ANIMATION GIBT

            yield return new WaitForSeconds(_wait); //Wartet die angegebene Zeit bevor das naechste ausgefuehrt wird
                                                    //"Friert" nur die Methode, nicht das Programm ein
            loadout[currentIndex].Reload();
            currentWeapon.SetActive(true); //UEBERGANGSWEISE, SOLANGE ES KEINE ANIMATION GIBT
            isReloading = false;
    }

    #endregion

    #region Public-Methoden

    public void RefreshAmmo(Text _text) //UI
    {
        int clip = loadout[currentIndex].GetClip();
        int stash = loadout[currentIndex].GetStash();

        _text.text = clip.ToString("D2") + " / " + stash.ToString("D2"); //D2 = 2 Stellig
    }

    #endregion

}
