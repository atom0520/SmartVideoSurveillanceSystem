using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : Singleton<GUIManager>
{

    public GameObject messageListPanel;
    public GameObject videoPlayerPanel;
    public GameObject settingsPanel;
    public GameObject popupMessagePanel;

    [HideInInspector]
    public MessageListPanelController messageListPanelController;
    [HideInInspector]
    public VideoPlayerPanelController videoPlayerPanelController;
    [HideInInspector]
    public PopupMessagePanelController popupMessagePanelController;
    void Awake () {
        InstanceSet += () => {
            this.messageListPanelController = this.messageListPanel.GetComponent<MessageListPanelController>();
            this.videoPlayerPanelController = this.videoPlayerPanel.GetComponent<VideoPlayerPanelController>();
            this.popupMessagePanelController = this.popupMessagePanel.GetComponent<PopupMessagePanelController>();
        };
        base.Awake();		
	}

    public void OpenMessageListPanel()
    {
        messageListPanel.SetActive(true);       
        //messageListPanelController.RefreshDisplay();
    }

    public void CloseMessageListPanel()
    {
        messageListPanel.SetActive(false);
    }

    public void OpenVideoPlayerPanel(Message message)
    {
        videoPlayerPanel.SetActive(true);
        videoPlayerPanelController.Setup(message);
    }

    public void CloseVideoPlayerPanel()
    {
        videoPlayerPanel.SetActive(false);        
    }

    public void OpenSettingsPanel()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false);
    }

    public void OpenPopupMessagePanel(string titleText, string contentText)
    {
        popupMessagePanelController.Setup(titleText, contentText);
        popupMessagePanel.SetActive(true);
    }

    public void ClosePopupMessagePanel()
    {
        popupMessagePanel.SetActive(false);
    }
}
