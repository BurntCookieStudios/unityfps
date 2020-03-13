using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variablen

    public static bool cursorLocked = true;

    #region Bewegung - Variablen
    //Bewegung
    private Rigidbody rig;
    private float moveSpeed = 180f;

    //Sprint
    private float sprintModifier = 1.5f;

    private float baseFOV;
    private float sprintFOVModifier = 1.2f;
    public Camera normalCam;

    //Jump
    private float jumpForce = 100f;
    public LayerMask ground;
    public Transform groundDetector; //Punkt von dem geprueft werden soll, ob er sich auf dem Boden befindet.
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
        baseFOV = normalCam.fieldOfView;
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
        //Axis
        float hmove = Input.GetAxisRaw("Horizontal"); //RAW verhindert ein "Rutsch-Gefuehl"
        float vmove = Input.GetAxisRaw("Vertical");
        
        //Controls
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jump = Input.GetKey(KeyCode.Space);

        //States
        bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.4f, ground);
        bool isJumping = jump && isGrounded; //kann nur Springen, wenn der Spieler auf dem Boden steht.
        bool isSprinting = sprint && vmove > 0 && !isJumping && isGrounded; //Später eventuell ändern, bspw. kann man SPÄTER nicht sprinten wenn man nicht genug Stamina hat. 
                                                                            // vmove > 0, um zu pruefen, ob der Spieler sich vorwaerts bewegt => verhindert Rueckwaerts Sprinten 
                                                                            //!isJumping um nicht beim Springen zu Sprinten 
                                                                            //kann nur Sprinten, wenn der Spieler auf dem Boden steht.    


        //Weitere Variablen
        float adjustedSpeed = moveSpeed;


        //Jumping:
        if (isJumping)
        {
            rig.AddForce(Vector3.up * jumpForce);
        }


        //Sprint: Verschnellerung + FOV Change
        if (isSprinting)
        {
            adjustedSpeed *= sprintModifier;
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f); //Sprint-Effekt 
                                                                                                                         //Mathf.Lerp: Uebergang von der normalen FOV zur veraenderten, damit kein Snapping entsteht.
        }
        else
            normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f); ; //KEIN Sprint-Effekt //Mathf.Lerp: Uebergang von der veraenderten FOV zur normalen, damit kein Snapping entsteht.

        //Lauf Richtung
        Vector3 direction = new Vector3(hmove, 0, vmove);
        direction.Normalize(); //findet eine Bewegung Horizontal und Vertical statt, wäre der Spieler in der Theorie doppelt so schnell; Vermieden wird dies durch ".Normalize()".

        //Bewegung
        Vector3 targetVelocity = transform.TransformDirection(direction) * adjustedSpeed * Time.deltaTime; //transform.TransformDirection(), um in die Richtung zu laufen, wohin der Spieler schaut. 
                                                                                                           //Time.deltaTime (siehe FixedUpdate())
        targetVelocity.y = rig.velocity.y; //Ohne diese Zeile Code, funktioniert Springen nicht richtig, da bei "direction", der Y-Wert 0 gesetzt wird.
        rig.velocity = targetVelocity;
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
        Quaternion delta = pCam.localRotation * adjust;

        if (Quaternion.Angle(camCenter, delta) < maxAngle) //clamping
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
