using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public static bool paused = false;
    private bool disconnecting = false;

    public void Start()
    {
        Text valueText = GameObject.Find("Settings/Buttons/Sensitivity/Value").GetComponent<Text>();
        if(valueText)
            valueText.text = (PlayerMovement.sensitivity / 200).ToString("F2");
    }

    public void TogglePause()
    {
        if (disconnecting) return;

        if (transform.GetChild(1).gameObject.activeSelf) transform.GetChild(1).gameObject.SetActive(false);

        paused = !paused;

        transform.GetChild(0).gameObject.SetActive(paused);
        Cursor.lockState = (paused) ? CursorLockMode.None : CursorLockMode.Confined;
        Cursor.visible = paused;
    }

    public void Quit()
    {
        disconnecting = true;
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }

    public void Settings()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
    }

    //Settings
    public void SetSensitivity(float _sens)
    {
        Text valueText = GameObject.Find("Settings/Buttons/Sensitivity/Value").GetComponent<Text>();
        PlayerMovement.sensitivity = _sens;
        valueText.text = (PlayerMovement.sensitivity / 200).ToString("F2");
    }

    public void Back()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(false);
    }
}
