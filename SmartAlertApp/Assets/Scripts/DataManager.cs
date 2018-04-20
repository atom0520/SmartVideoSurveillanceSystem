using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
    //public List<Message> messageList;
    public string serverIP;
    public int serverPort;
}

public class DataManager : Singleton<DataManager>
{
    public List<Message> messageList;
    public string serverIP;
    public int serverPort;
    public string deviceToken;

    override protected void Awake () {
        InstanceSet += () => {
            LoadData();
        };
        
        base.Awake();
	}

    public void SaveData()
    {

        UserData dataToSave = new UserData();
        //dataToSave.messageList = this.messageList;
        dataToSave.serverIP = this.serverIP;
        dataToSave.serverPort = this.serverPort;
        DataSaver.SaveData(dataToSave, "userData");
    }

    void LoadData()
    {
        
        UserData loadedData = DataSaver.LoadData<UserData>("userData");

        if (loadedData != null)
        {
            //this.messageList = loadedData.messageList;
            this.serverIP = loadedData.serverIP;
            this.serverPort = loadedData.serverPort;
            return;
        }

        if (this.messageList == null)
        {
            this.messageList = new List<Message>();
        }

        this.serverIP = "127.0.0.1";
        this.serverPort = 8010;

        SaveData();
        //this.messageList = new List<Message>();
    }

    //public void AddMessage(Message messageToAdd)
    //{
    //    messageList.Add(messageToAdd);
    //}

    public void AddMessage(string videoName, int eventFrame, string eventDetectedTime, string messageReceivedTime)
    {
        Message newMessage = new Message();
        newMessage.videoName = videoName;
        newMessage.eventFrame = eventFrame;
        newMessage.eventDetectedTime = eventDetectedTime;
        newMessage.messageReceivedTime = messageReceivedTime;

        messageList.Add(newMessage);
    }

    public void RemoveAllMessages()
    {
        for (int i = messageList.Count - 1; i >= 0; i--)
        {
            messageList.RemoveAt(i);
        }
    }

    public void RemoveMessage(Message messageToRemove)
    {
        for (int i = messageList.Count - 1; i >= 0; i--)
        {
            if (messageList[i] == messageToRemove)
            {
                messageList.RemoveAt(i);
            }
        }
    }
}


