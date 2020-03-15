using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Health : MonoBehaviourPunCallbacks
{
    #region Variablen

    public int maxHealth;
    private int currHealth;

    #endregion

    #region Monobehaviour Callbacks

    void Start()
    {
        currHealth = maxHealth;
    }

    #endregion

    #region Methoden

    public void TakeDamage(int _amount)
    {
        if (photonView.IsMine)
        {
            currHealth -= _amount;
            Debug.Log(currHealth);

            if(currHealth <= 0)
            {
                Debug.Log("You died!");
            }
        }
    }

    #endregion

}
