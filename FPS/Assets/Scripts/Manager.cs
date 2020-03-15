using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Manager : MonoBehaviour
{
    public string playerPrefab;
    public Transform spawnPoint;

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        PhotonNetwork.Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
