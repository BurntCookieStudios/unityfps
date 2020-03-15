using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class pHealth : MonoBehaviourPunCallbacks
{
    #region Variablen

    public int maxHealth;
    private int currHealth;

    private Transform ui_healthBar;

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
            RefreshHealthBar();
        }
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
