using System.Collections;
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

    private Manager manager;

    #endregion

    #region Monobehaviour Callbacks

    void Start()
    {
        manager = GameObject.Find("Manager").GetComponent<Manager>();
        currHealth = maxHealth;
        if (photonView.IsMine)
        {
            ui_healthBar = GameObject.Find("HUD/Health/Bar").transform;
            ui_Username = GameObject.Find("HUD/Username/Text").GetComponent<Text>();

            photonView.RPC("SyncProfile", RpcTarget.All, MainMenu.myProfile.username, MainMenu.myProfile.level, MainMenu.myProfile.xp, MainMenu.myProfile.currency);

            RefreshHealthBar();
            ui_Username.text = MainMenu.myProfile.username;
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

    public void TakeDamage(int _amount)
    {
        if (photonView.IsMine)
        {
            currHealth -= _amount;
            RefreshHealthBar();

            if(currHealth <= 0)
            {
                manager.Spawn();
                manager.ChangeStat_S(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);
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
