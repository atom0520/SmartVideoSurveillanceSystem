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

public class SubscriptionRequestClient : MonoBehaviour {

    TcpClient client;

    private bool stop = false;

    const string commandID = "0003";

    public void Subscribe()
    {
        Debug.Log("SubscriptionRequestClient.Subscribe");
        
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
        Debug.Log("SubscriptionRequestClient.SendRequest");
        byte[] commandIDBytesToSend = ASCIIEncoding.ASCII.GetBytes(commandID);
        print("DataManager.Instance.deviceToken:" + DataManager.Instance.deviceToken);
        byte[] deviceTokenBytesToSend = ASCIIEncoding.ASCII.GetBytes(DataManager.Instance.deviceToken);
        print("deviceTokenBytesToSend:" + deviceTokenBytesToSend);
        NetworkStream serverStream = client.GetStream();

        bool requestSent = false;

        (new Thread(() =>
        {
            serverStream.Write(commandIDBytesToSend, 0, commandIDBytesToSend.Length);
            serverStream.Write(deviceTokenBytesToSend, 0, deviceTokenBytesToSend.Length);
            requestSent = true;
        })).Start();

        while (!requestSent)
        {
            System.Threading.Thread.Sleep(1);
        }
    }

    void GetResponce()
    {
        Debug.Log("SubscriptionRequestClient.GetResponce");

        NetworkStream serverStream = client.GetStream();

        string result="";

        byte[] buffer = new byte[16];
        using (MemoryStream ms = new MemoryStream())
        {
            int readBytesLength;
            while (true)
            {
                readBytesLength = serverStream.Read(buffer, 0, buffer.Length);

                ms.Write(buffer, 0, readBytesLength);

                if (readBytesLength < buffer.Length)
                {
                    break;
                }
                    
            }

            result = Encoding.ASCII.GetString(ms.ToArray(), 0, (int)ms.Length);
        }

        //Debug.Log("requestedMessagesStr:"+ requestedMessagesStr);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (result == "1")
            {
                GUIManager.Instance.OpenPopupMessagePanel("", "Subscribe successfully! ^0^");
            }
            else if (result == "2")
            {
                GUIManager.Instance.OpenPopupMessagePanel("", "You have already subsribed 0 0");
            }
            else
            {
                GUIManager.Instance.OpenPopupMessagePanel("", "Subscription Failed :(");
            }

            GUIManager.Instance.messageListPanelController.subscribeButton.enabled = true;
        });


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
