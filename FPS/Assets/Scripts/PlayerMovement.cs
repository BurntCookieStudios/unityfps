using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variablen

    public static bool cursorLocked = true;

    #region Bewegung - Variablen
    //Bewegung
    private float speed = 100f;

    private Rigidbody rig;
    #endregion

    #region Umsehen - Variablen
    //Umsehen
    public Transform player;
    public Transform pCam;

    public float xSensitivity;
    public float ySensitivity;
    private float maxAngle = 90f;

    private Quaternion camCenter;
    #endregion

    #endregion

    private void Start()
    {
        Camera.main.enabled = false;
        camCenter = pCam.localRotation;
        rig = GetComponent<Rigidbody>();
    }

    void Update()
    {
        SetY();
        SetX();

        UpdateLockCursor();
    }

    void FixedUpdate() //FIXEDUpdate, um unabhaengig von der Leistung u. Verbindung des Clients wie gewollt zu laufen.(=> Multiplayer)
    {
        float hmove = Input.GetAxisRaw("Horizontal");
        float vmove = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(hmove, 0, vmove);
        direction.Normalize(); //findet eine Bewegung Horizontal und Vertical statt, wäre der Spieler in der Theorie doppelt so schnell; Vermieden wird dies durch ".Normalize()".

        rig.velocity = transform.TransformDirection(direction) * speed * Time.deltaTime; //transform.TransformDirection(), um in die Richtung zu laufen, wohin der Spieler schaut. //Time.deltaTime (siehe FixedUpdate())
    }

    #region Methoden

    #region Umsehen - Funktionen
    /*
     * Setzt die Sicht des Spielers der horizontalen Achse (Y), durch Mausbewegung
     * Dreht die Spieler Kamera
     * Maximale Höhe und Tiefe (Clamping)
     */
    void SetY() //Vertikale Sicht
    {
        float input = Input.GetAxis("Mouse Y") * ySensitivity * Time.deltaTime;
        Quaternion adjust = Quaternion.AngleAxis(input, -Vector3.right); //Quaternion = Vier dimensionaler Vektor; ist Datentyp der Rotation.
        Quaternion delta = pCam.localRotation * adjust; //https://www.youtube.com/watch?v=k11IDExB-oU Minute 8

        if (Quaternion.Angle(camCenter, delta) < maxAngle)
        {
            pCam.localRotation = delta;
        }
    }

    /*
    * Setzt die Sicht des Spielers der horizontalen Achse (X), durch Mausbewegung
    * Dreht den Spieler 
    */
    void SetX() //Horizontale Sicht
    {
        float input = Input.GetAxis("Mouse X") * xSensitivity * Time.deltaTime;
        Quaternion adjust = Quaternion.AngleAxis(input, -Vector3.down); //Quaternion = Vier dimensionaler Vektor; ist Datentyp der Rotation.
        Quaternion delta = player.localRotation * adjust; 
        player.localRotation = delta;
    }

    void UpdateLockCursor()
    {
        if (cursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                cursorLocked = true;
            }
        }
    }
    #endregion

    #endregion
}
