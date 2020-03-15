using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MainMenu : MonoBehaviourPunCallbacks
{
    #region Photon Unity Network: PUN

    //Photon Unity Network: PUN
    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; //Sagt dem Server, welche Szene geladen ist: Bsp.: Host aendert Szene -> alle aendern automatisch Szene
        Connect();
    }

    public override void OnConnectedToMaster() //Callback, wenn der Spieler zum Server connected ist, um dann erst ein Spiel zu joinen oder zu erstellen, etc. pp.
    {
        Debug.Log("CONNECTED!");
        Join();

        base.OnConnectedToMaster(); 
    }

    public override void OnJoinedRoom() //Callback, wenn der Spieler dem Raum/der Szene beigetreten ist, um dann erst zu bewegen, schiessen, etc. pp.
    {
        StartGame();


        base.OnConnectedToMaster();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //Callback, wenn der Spieler dem Raum/der Szene nicht beitreten konnte, da dieser Voll war oder kein Raum vorhanden war, um eine neue Lobby zu erstellen, etc. pp.
    {
        Create();

        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Connect() //Connect zum generellen Server und nicht Join ins Spiel
    {
        Debug.Log("Trying to Connect...");
        PhotonNetwork.GameVersion = "0.0.0"; //Version des Spieles, sodass nur Spieler gleicher Version zusammen spielen koennen.
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Join()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void Create() //Create a match
    {
        PhotonNetwork.CreateRoom("");
    }

    public void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)//nur Host ist im Raum
        {
            PhotonNetwork.LoadLevel(1); //(Alle Spieler laden automatisch, siehe "public void OnEnable()"
        }
    }

    #endregion
}
