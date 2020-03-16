using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;


//ZUM VERSTEHEN DER FUNKTIONEN WIRD EVENTUELL DIE DOKUMENTATION BENOETIGT (PHOTON EVENTS):
//https://doc.photonengine.com/en-us/pun/v2/gameplay/rpcsandraiseevent

public class PlayerInfo
{
    public ProfileData profile;
    public int actor;
    public short kills; //short um Speicher zu sparen, da die Werte der Kills und Tode eh nicht so hoch sein werden.
    public short deaths; //short um Speicher zu sparen, da die Werte der Kills und Tode eh nicht so hoch sein werden.

    public PlayerInfo(ProfileData _p, int _a, short _k, short _d)
    {
        this.profile = _p;
        this.actor = _a;
        this.kills = _k;
        this.deaths = _d;
    }
}

public class Manager : MonoBehaviour, IOnEventCallback //Call und Recieve Events
{
    public string playerPrefab;
    public Transform[] spawnPoints;

    public List<PlayerInfo> playerInfo = new List<PlayerInfo>(); //Infos aller Spieler der Lobby bzw. Raum
    public int myIndex; //Hierdurch haben wir die Stelle der Liste, an dem Sich die Infos des Client befinden.

    public enum EventCodes : byte //Events des Spielers; Hier werden weitere Events wie bspw. EndGame spaeter hinzugefuegt;
    {
        NewPlayer,
        UpdatePlayers,
        ChangeStat
    }

    private void Start()
    {
        ValidateConnection();
        NewPlayer_S(MainMenu.myProfile); //S = Sending (Bezogen auf das Event)
        Spawn();
    }

    #region Photon

    //RECIEVE EVENT
    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code >= 200) return; //Photon reserviert automatisch 200-255, somit koennen wir nur unter 199 benutzen, deshalb wird nachgefragt, siehe Dokumentation

        EventCodes e = (EventCodes)photonEvent.Code; //Anlass des Ausfuehrens nehmen
        object[] o = (object[])photonEvent.CustomData; //Code unwrappen

        switch (e) //Handeln jeh nach Anlass
        {
            case EventCodes.NewPlayer:
                NewPlayer_R(o); //R = Recieve (Bezogen auf das Event) 
                                //Sended den Code zur passenden Event-Recieve-Methode
                break;

            case EventCodes.UpdatePlayers:
                UpdatePlayers_R(o); //R = Recieve (Bezogen auf das Event) 
                                    //Sended den Code zur passenden Event-Recieve-Methode
                break;

            case EventCodes.ChangeStat:
                ChangeStat_R(o); //R = Recieve (Bezogen auf das Event) 
                                 //Sended den Code zur passenden Event-Recieve-Methode
                break;
        }
    }

    #endregion

    #region Methoden

    public void Spawn()
    {
        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Instantiate(playerPrefab, spawn.position, spawn.rotation);
    }

    private void ValidateConnection() //Guckt ob ein Spiel noch laeuft oder ob der Spieler noch dazu connected ist oder ob seine Verbindung abgebrochen wurde. Ist seine Verbindung getrennt worden, wird er ins Hauptmenu geschickt.
    {
        if (PhotonNetwork.IsConnected) return;
        SceneManager.LoadScene(0);
    }

    #endregion

    #region Events 

    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void NewPlayer_S (ProfileData _p) //Sendet die profileinformationen zum Server
    {
        object[] package = new object[7]; //laenge Dimension des Package, entspricht die Anzahl der benoetigten Variablen, hier die Profildaten.
                                          //ProfileData und PlayerInfo in ein Object array convertieren um es einfacher senden zu koennen.
        //package array Werte zuweisen
        package[0] = _p.username;
        package[1] = _p.level;
        package[2] = _p.xp;
        package[3] = _p.currency;
        package[4] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[5] = (short) 0; //Hat 0 Kills, da es ja ein neuer Spieler ist
        package[6] = (short) 0; //Hat 0 Tode, da es ja ein neuer Spieler ist

        PhotonNetwork.RaiseEvent( //Oeffnet ein Event -> Ein neuer Spieler, also muss jeder anderer sofort dieses Event recieven.
            (byte)EventCodes.NewPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient }, //WIRD NUR AN MASTERCLIENT ALSO HOST GESENDET
            new SendOptions { Reliability = true }
        );
    }

    public void NewPlayer_R(object[] _data) //EINEN NEUEN SPIELER EMPFAENGT NUR DER MASTERCLIENT, UND FUEGT IN IN SEINE LISTE EIN, DIESE LISTE STELLT ER DANN ALLEN SPILERN ZUR VERFUEGUNG (Sicherster Weg)
    {
        PlayerInfo p = new PlayerInfo (
            new ProfileData(
                (string)_data[0],
                (int)_data[1],
                (int)_data[2],
                (int)_data[3]
            ),
            (int)_data[4],
            (short)_data[5],
            (short)_data[6]
        );

        playerInfo.Add(p); //Die Informationen empfangen und an die Liste anfuegen (=> RECIEVE);

        UpdatePlayers_S(playerInfo); //Sendet die neue Liste zum Server
    }

    public void UpdatePlayers_S(List<PlayerInfo> _info)
    {
        //2 Dimensionales object Array
        object[] package = new object[_info.Count];
        for (int i = 0; i < _info.Count; i++)
        {
            object[] piece = new object[7];

            piece[0] = _info[i].profile.username;
            piece[1] = _info[i].profile.level;
            piece[2] = _info[i].profile.xp;
            piece[3] = _info[i].profile.currency;
            piece[4] = _info[i].actor;
            piece[5] = _info[i].kills;
            piece[6] = _info[i].deaths;

            package[i] = piece;
        }

        PhotonNetwork.RaiseEvent( //Sendet die neue Liste, convertiert zu einem object array und somit ein 2 Dimensionales, an alle Spieler des Raumes, der Lobby
            (byte)EventCodes.UpdatePlayers, 
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All }, 
            new SendOptions { Reliability = true }
        );        
    }

    public void UpdatePlayers_R(object[] _data) //Jeder Spieler fuehrt diese Methode mehrfach aus
                                                //Hier werden die dimensionen aus einem object-Array in die Liste jedes Spielers "entpackt"
    {
        playerInfo = new List<PlayerInfo>();

        for (int i = 0; i < _data.Length; i++)
        {
            object[] extract = (object[])_data[i];

            PlayerInfo p = new PlayerInfo(
                new ProfileData(
                    (string)extract[0],
                    (int)extract[1],
                    (int)extract[2],
                    (int)extract[3]
                ),
                (int)extract[4],
                (short)extract[5],
                (short)extract[6]
            );

            playerInfo.Add(p);

            if (PhotonNetwork.LocalPlayer.ActorNumber == p.actor) myIndex = i; //Guckt, welcher Spieler der Liste man selbst ist und speichert diese Information
        }
    }

    public void ChangeStat_S (int _actor, byte _stat, byte _amt) //_amt = amount
                                                                 //Aendert und sendet die Statistik _stat, des Spielers der Liste an Stelle _actors, um die Menge _amount, an alle Spieler
    {
        object[] package = new object[] { _actor, _stat, _amt };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ChangeStat,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All},
            new SendOptions { Reliability = true}
            );       
    }
    
    public void ChangeStat_R (object[] _data)
    {
        //Package "auspacken"
        int actor = (int)_data[0];
        byte stat = (byte)_data[1];
        byte amt = (byte)_data[2];

        for (int i = 0; i < playerInfo.Count; i++) //alle Spieler der Liste...
        {
            if (playerInfo[i].actor == actor)//... nach dem gesuchten Spieler durchsuchen und ...
            {
                switch (stat)
                {
                    case 0: //kills
                        playerInfo[i].kills += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username}: kills = {playerInfo[i].kills}"); //... dessen Kill-Anzahl um den Wert amt erhoehen.
                        break;

                    case 1: //deaths
                        playerInfo[i].deaths += amt;
                        Debug.Log($"Player {playerInfo[i].profile.username}: deaths = {playerInfo[i].deaths}"); //... dessen Tode-Anzahl um den Wert amt erhoehen.
                        break;
                }

                return;
            }
        }
    }

    #endregion
}

