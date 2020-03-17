using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;


public class ProfileData
{
    public string username;
    public int level;
    public int xp;
    public int currency;

    public ProfileData()
    {
        this.username = "DEFAULT USERNAME";
        this.level = 0;
        this.xp = 0;
        this.currency = 0;
    }

    public ProfileData(string _uname, int _level, int _xp, int _currency)
    {
        this.username = _uname;
        this.level = _level;
        this.xp = _xp;
        this.currency = _currency;
    }
}

public class MainMenu : MonoBehaviourPunCallbacks
{
    public InputField unameField;
    public static ProfileData myProfile = new ProfileData();

    public void Start()
    {
        Pause.paused = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void JoinMatch()
    {
        Join();
    }

    public void CreateMatch()
    {
        Create();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

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

        base.OnConnectedToMaster(); 
    }

    public override void OnJoinedRoom() //Callback, wenn der Spieler dem Raum/der Szene beigetreten ist, um dann erst zu bewegen, schiessen, etc. pp.
    {
        StartGame();

        VerifyUsername();

        base.OnConnectedToMaster();
    }

    public override void OnJoinRandomFailed(short returnCode, string message) //Callback, wenn der Spieler dem Raum/der Szene nicht beitreten konnte, da dieser Voll war oder kein Raum vorhanden war, um eine neue Lobby zu erstellen, etc. pp.
    {
        Create();

        VerifyUsername();

        base.OnJoinRandomFailed(returnCode, message);
    }

    public void Connect() //Connect zum generellen Server und nicht Join ins Spiel
    {
        Debug.Log("Trying to Connect...");
        PhotonNetwork.GameVersion = "0.0.6"; //Version des Spieles, sodass nur Spieler gleicher Version zusammen spielen koennen.
        PhotonNetwork.ConnectUsingSettings();
    }

    public void Join()
    {
        VerifyUsername();
        PhotonNetwork.JoinRandomRoom();       
    }

    public void Create() //Create a match
    {
        VerifyUsername();
        PhotonNetwork.CreateRoom("");
    }

    public void StartGame()
    {
        VerifyUsername();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)//nur Host ist im Raum
        {
            PhotonNetwork.LoadLevel(1); //(Alle Spieler laden automatisch, siehe "public void OnEnable()"
        }
    }

    #endregion

    private void VerifyUsername()
    {
        if (string.IsNullOrEmpty(unameField.text))
        {
            myProfile.username = "USER" + Random.Range(100, 1000);
        }
        else
        {
            myProfile.username = unameField.text; //Eingegebenen Username zuweisen
        }
    }
}
