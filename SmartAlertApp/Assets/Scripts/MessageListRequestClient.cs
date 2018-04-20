using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Text;
using System;
using System.Globalization;

public class MessageListRequestClient : MonoBehaviour {

    [Serializable]
    public class RequestedMessage
    {
        public string v;
        public int f;
        public string t;
    }

    TcpClient client;

    private bool stop = false;

    const string commandID = "0002";

    const int MESSAGE_LIST_LENGTH_BYTE_NUM = 16;

    public void SyncMessageList()
    {
        //Debug.Log("MessageListRequestClient.SyncMessageList");
        DataManager.Instance.RemoveAllMessages();
        GUIManager.Instance.messageListPanelController.RemoveButtons();
        GUIManager.Instance.messageListPanelController.logText.text = "Requesting alert records...";

        client = new TcpClient();

        (new Thread(() => {

            Connect();

            SendRequest();

            GetResponce();

        })).Start();
    }

    void Connect()
    {
        while (!stop)
        {
            try
            {
                client.Connect(IPAddress.Parse(DataManager.Instance.serverIP), DataManager.Instance.serverPort);
            }
            catch (SocketException sex)
            {
                Debug.Log(sex);
            }

            if (client.Connected == true)
            {
                break;
            }
        }
    }

    void SendRequest()
    {
        //Debug.Log("MessageListRequestClient.SendRequest");
        byte[] commandIDBytesToSend = ASCIIEncoding.ASCII.GetBytes(commandID);

        NetworkStream serverStream = client.GetStream();

        bool requestSent = false;

        (new Thread(() =>
        {
            serverStream.Write(commandIDBytesToSend, 0, commandIDBytesToSend.Length);
            requestSent = true;
        })).Start();

        while (!requestSent)
        {
            System.Threading.Thread.Sleep(1);
        }
    }

    void GetResponce()
    {
        //Debug.Log("MessageListRequestClient.GetResponce");
        bool disconnected = false;

        NetworkStream serverStream = client.GetStream();

        byte[] messageStrLengthBytes= new byte[MESSAGE_LIST_LENGTH_BYTE_NUM];
        var totalReadBytesLength = 0;
        do
        {
            var read = serverStream.Read(messageStrLengthBytes, totalReadBytesLength, MESSAGE_LIST_LENGTH_BYTE_NUM - totalReadBytesLength);
            totalReadBytesLength += read;
        } while (totalReadBytesLength != MESSAGE_LIST_LENGTH_BYTE_NUM);

        int messageStrLength = BitConverter.ToInt32(messageStrLengthBytes, 0);
        //Debug.Log("messageStrLength: " + messageStrLength);

        string requestedMessagesStr="";


        byte[] buffer = new byte[1024];
        totalReadBytesLength = 0;
        using (MemoryStream ms = new MemoryStream())
        {
            int readBytesLength;
            while (true)
            {
                readBytesLength = serverStream.Read(buffer, 0, buffer.Length);
                totalReadBytesLength += readBytesLength;
                ms.Write(buffer, 0, readBytesLength);
                //Debug.Log("totalReadBytesLength: " + totalReadBytesLength);
                if (totalReadBytesLength >= messageStrLength)
                {
                    break;
                }
                    
            }

            requestedMessagesStr = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
        }

        //Debug.Log("requestedMessagesStr:"+ requestedMessagesStr);
        if(requestedMessagesStr==" ")
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GUIManager.Instance.messageListPanelController.logText.text = "No alert record yet :)";
                GUIManager.Instance.messageListPanelController.syncButton.enabled = true;
            });
            return;
        }
        //Debug.Log("requestedMessagesStr:" + requestedMessagesStr);
        CultureInfo enUS = new CultureInfo("en-US");
        string[] requestedMessageStrs = requestedMessagesStr.Split('|');
        for(int i=0;i< requestedMessageStrs.Length; i++)
        {
            //Debug.Log("requestedMessageStrs[i]:" + requestedMessageStrs[i]);
            RequestedMessage requestedMessage = JsonUtility.FromJson<RequestedMessage>(requestedMessageStrs[i]);
            
            DataManager.Instance.AddMessage(requestedMessage.v,
                requestedMessage.f,
                DateTime.ParseExact(requestedMessage.t, "yyyy,M,d,H,m,s", enUS, DateTimeStyles.None).ToString("dd-MMM-yyyy HH:mm:ss"),
                DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss"));
        }

    
        if (!disconnected)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                GUIManager.Instance.messageListPanelController.logText.text = "";
                GUIManager.Instance.messageListPanelController.AddButtons();
                //GUIManager.Instance.messageListPanelController.syncButton.enabled = true;
            });
        }
    }

    private void OnEnable()
    {
        stop = false;
    }

    private void OnDisable()
    {
        StopNetworking();
    }

    public void StopNetworking()
    {
        stop = true;

        if (client != null)
        {
            client.Close();
        }
    }
}
