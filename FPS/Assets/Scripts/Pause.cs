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

    public Dropdown resolutionDropdown;
    private Resolution[] resolutions;

    public Dropdown qualityDropdown;

    public void Start()
    {
        //Auswahlmoeglichkeiten der Aufloesung basierend auf den Bildschirm des Spielers
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++) 
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();

        qualityDropdown.value = QualitySettings.GetQualityLevel();

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

    public void SetResolution ( int _resolutionIndex)
    {
        Resolution resolution = resolutions[_resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality (int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void Back()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(false);
    }
}
