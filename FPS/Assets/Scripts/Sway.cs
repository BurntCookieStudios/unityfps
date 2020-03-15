using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Sway : MonoBehaviourPunCallbacks
{
    #region Variablen

    private float intensity = 1.5f;
    private float smooth = 10f;

    private Quaternion origin_rotation; //default rotation

    #endregion

    #region Monobehaviour Callbacks

    private void Start()
    {
        origin_rotation = transform.localRotation;
    }

    private void Update()
    {
        UpdateSway();
    }

    #endregion

    #region Methoden

    private void UpdateSway()
    {
        //Ccontrols
        float x_mouse = Input.GetAxis("Mouse X");
        float y_mouse = Input.GetAxis("Mouse Y");
        
        //Rotations Berechnung
        Quaternion x_adjust = Quaternion.AngleAxis(-intensity * x_mouse, Vector3.up); //Vector3.up - Rotation um die Z-Achse
        Quaternion y_adjust = Quaternion.AngleAxis(intensity * y_mouse, Vector3.right); //Vector3.right - Rotation um die X-Achse
        Quaternion target_rotation = origin_rotation * x_adjust * y_adjust;

        //Zuweisung der Rotation
        transform.localRotation = Quaternion.Lerp(transform.localRotation, target_rotation, Time.deltaTime * smooth); //Lerp fuer Uebergang, anstatt sofortiges snapping
    }

    #endregion
}
