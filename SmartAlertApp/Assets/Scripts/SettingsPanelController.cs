using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour {

    public Button confirmButton;
    public Button cancelButton;
    public InputField serverIPInputField;
    public InputField serverPortInputField;

	// Use this for initialization
	void Start () {
        confirmButton.onClick.AddListener(OnClickConfirmButton);
        cancelButton.onClick.AddListener(OnClickCancelButton);
    }

    private void OnEnable()
    {
        serverIPInputField.text = DataManager.Instance.serverIP;
        serverPortInputField.text = DataManager.Instance.serverPort.ToString();
    }

    public void OnClickConfirmButton()
    {
        DataManager.Instance.serverIP = serverIPInputField.text;
        DataManager.Instance.serverPort = int.Parse(serverPortInputField.text);
        DataManager.Instance.SaveData();
        GUIManager.Instance.CloseSettingsPanel();
    }

    public void OnClickCancelButton()
    {
        GUIManager.Instance.CloseSettingsPanel();
    }
}
