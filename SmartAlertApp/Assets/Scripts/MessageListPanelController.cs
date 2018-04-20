using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Message
{
    public string videoName;
    public int eventFrame;
    public string eventDetectedTime;
    public string messageReceivedTime;
}

public class MessageListPanelController : MonoBehaviour {

    public Transform contentPanel;
    public SimpleObjectPool buttonObjectPool;

    public Button settingsButton;
    public Button syncButton;
    public Button subscribeButton;

    public MessageListRequestClient messageListRequestClient;
    public SubscriptionRequestClient subscriptionRequestClient;

    public Text logText;

	// Use this for initialization
	void Start () {
        Debug.Log("MessageListPanelController.Start");
        //RefreshDisplay();
        settingsButton.onClick.AddListener(OnClickSettingsButton);
        syncButton.onClick.AddListener(OnClickSyncButton);
        subscribeButton.onClick.AddListener(OnClickSubscribeButton);

    }

    //public void RefreshDisplay()
    //{
    //    //Debug.Log("RefreshDisplay");
    //    messageListRequestClient.SyncMessageList();
    //    //RemoveButtons();
    //    //AddButtons();
    //}
    private void OnEnable()
    {
        logText.text = "";
        syncButton.enabled = true;
        subscribeButton.enabled = true;
    }

    public void AddButtons()
    {
        for(int i=0; i < DataManager.Instance.messageList.Count; i++)
        {
            Message message = DataManager.Instance.messageList[i];
            GameObject newButton = buttonObjectPool.GetObject();
   
            newButton.transform.SetParent(contentPanel);
            newButton.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            MessageButton messageButton = newButton.GetComponent<MessageButton>();
            messageButton.Setup(message);
        }
    }

    public void RemoveButtons()
    {
        while (contentPanel.childCount > 0)
        {
            GameObject buttonToRemove = contentPanel.GetChild(0).gameObject;
            buttonObjectPool.ReturnObject(buttonToRemove);
        }
    }

    public void OnClickSettingsButton()
    {
        GUIManager.Instance.OpenSettingsPanel();
    }

    public void OnClickSyncButton()
    {
        //syncButton.enabled = false;
        messageListRequestClient.SyncMessageList();
    }

    public void OnClickSubscribeButton()
    {
        subscribeButton.enabled = false;
        if (DataManager.Instance.deviceToken == "")
        {
            GUIManager.Instance.OpenPopupMessagePanel("", "No device token received");
            subscribeButton.enabled = true;
            return;
        }
        subscriptionRequestClient.Subscribe();
    }
}
