﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class Player : MonoBehaviourPunCallbacks
{
    #region Variablen

    public int maxHealth;
    private int currHealth;

    [HideInInspector] public ProfileData playerProfile;

    private Transform ui_healthBar;
    private Text ui_Username;
    private Text ui_Ammo;
    private GameObject crosshair;

    private Image damageDisplayImage;
    private float damageDisplayWait;

    private Manager manager;
    private Loadout loadout;

    private bool pause;
    #endregion

    #region Monobehaviour Callbacks

    void Start()
    {
        damageDisplayImage = GameObject.Find("HUD/DamageDisplay/Image").GetComponent<Image>();
        crosshair = GameObject.Find("HUD/Crosshair");
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        loadout = GetComponent<Loadout>();
        currHealth = maxHealth;
        if (photonView.IsMine)
        {

            damageDisplayImage.color = new Color(255, 0, 0, 0);//versichert, dass das DamageDisplayt standardgemaess unsichtbar ist

            ui_healthBar = GameObject.Find("HUD/Health/Bar").transform;
            ui_Username = GameObject.Find("HUD/Profile/Username/Text").GetComponent<Text>();
            ui_Ammo = GameObject.Find("HUD/Ammo/Text").GetComponent<Text>();

            photonView.RPC("SyncProfile", RpcTarget.All, MainMenu.myProfile.username, MainMenu.myProfile.level, MainMenu.myProfile.xp, MainMenu.myProfile.currency);

            //UI Refreshes
            RefreshHealthBar();
            loadout.RefreshAmmo(ui_Ammo);
            ui_Username.text = MainMenu.myProfile.username;
        }
    }

    private void Update()
    {
        //pause
        if (pause)
        {
            GameObject.Find("Pause").GetComponent<Pause>().TogglePause();
        }

        if (Pause.paused)
        {
            pause = false;
        }

        if (photonView.IsMine)
        {
            //Controls
            pause = Input.GetKeyDown(KeyCode.Escape);

            if (damageDisplayWait > 0)
            {
                damageDisplayWait -= Time.deltaTime;
            }
            else if (damageDisplayImage.color.a > 0)
            {
                damageDisplayImage.color = Color.Lerp(damageDisplayImage.color, new Color(255, 0, 0, 0), Time.deltaTime * 3); //Lerp = Uebergang
            }

            loadout.RefreshAmmo(ui_Ammo);

            if (loadout.isAiming && crosshair.activeSelf) crosshair.SetActive(false);
            else if (!loadout.isAiming && !crosshair.activeSelf)
            {
                crosshair.SetActive(true);
            }
        }
    }

    #endregion

    #region PUN

    [PunRPC]
    private void SyncProfile(string _uname, int _level, int _xp, int _currency) //gibt ProfilDaten des Spielers an alle Spieler des Raumes weiter => Alle Spieler kennen alle Namen der Spieler in dem Raum
    {
        playerProfile = new ProfileData(_uname, _level, _xp, _currency);
    }

    #endregion

    #region Methoden

    public void TakeDamage(int _amount, int _actor)
    {
        if (photonView.IsMine)
        {
            //DamageDisplay anzeigen
            damageDisplayImage.color = new Color(255, 0, 0, 0.2f);
            damageDisplayWait = 0.1f; //Zeit die das DamageDisplay zum verschwinden brauch

            currHealth -= _amount;
            RefreshHealthBar();

            if(currHealth <= 0)
            {
                manager.Spawn();
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

                if (_actor >= 0) //Wenn der Gegner durch naturelle Dinge stirbt, sei es Fall damage, etc
                    manager.ChangeStat_S(_actor, 0, 1);

                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    public void RefreshHealthBar()
    {
        float health_ratio = (float)currHealth / (float)maxHealth; //Prozent der Lebenspunkte von der Maximalen Lebenspunkte
        ui_healthBar.localScale = new Vector3(health_ratio, 1, 1);
    }

    #endregion

}
