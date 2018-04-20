using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageButton : MonoBehaviour {

    public Button button;
    public Text videoNameText;
    public Text eventFrameText;
    public Text eventDetectedTimeText;
    public Text messageReceivedTimeText;

    private Message message;

    // Use this for initialization
    void Start () {
        button.onClick.AddListener(OnClick);
	}
	
	public void Setup (Message currentMessage) {
        message = currentMessage;
        videoNameText.text = "VideoName: "+message.videoName;
        eventFrameText.text = "EventFrame: "+message.eventFrame.ToString();
        eventDetectedTimeText.text = "EventDetectedTime: "+message.eventDetectedTime;
        messageReceivedTimeText.text = "MsgReceivedTime: "+message.messageReceivedTime;
    }

    public void OnClick()
    {
        GUIManager.Instance.CloseMessageListPanel();
        GUIManager.Instance.OpenVideoPlayerPanel(this.message);
    }
}
