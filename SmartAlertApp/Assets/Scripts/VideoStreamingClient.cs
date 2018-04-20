using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Net.Sockets;

using System.Net;
using System.IO;
using System;

using System.Threading;
using System.Text;

public class VideoStreamingClient : MonoBehaviour
{
    //public RawImage image;
    public bool enableLog = false;

    //public int port = 8010;
    //public string IP = "192.168.1.165";
    //public string IP = "127.0.0.1";
    TcpClient client;

    //Texture2D tex;
    //[HideInInspector]
    public List<Texture2D> videoFrames = new List<Texture2D>();

    private bool stop = false;

    //This must be the-same with SEND_COUNT on the server
    const int FRAME_SIZE_BYTE_NUM = 16;
    const int FRAME_NO_BYTE_NUM = 4;

    DateTime connectingBeginTime;
    DateTime requestingBeginTime;
    //double connectingTime;
    double reponseDelayTime;
    double totalElapsedTime;

    int requestedFrameNo;
    string requestedVideoName;

    const string commandID = "0001";
    //public VideoPlayerPanelController videoPlayerPanelController;

    // Use this for initialization
    void Start()
    {
        Application.runInBackground = true;

        //tex = new Texture2D(0, 0);
        
    }

    private void OnEnable()
    {
        stop = false;
    }

    private void OnDisable()
    {
        StopNetworking();
        ClearVideoFrameList();
    }

    public void StopNetworking()
    {
        stop = true;

        if (client != null)
        {
            client.Close();
        }
    }

    public void ClearVideoFrameList()
    {
        for (int i = videoFrames.Count - 1; i >= 0; i--)
        {
            videoFrames.RemoveAt(i);
        }
    }

    public void StartVideoStreaming(string videoName, int frameNo)
    {
        this.requestedVideoName = videoName;
        this.requestedFrameNo = frameNo;

        client = new TcpClient();

        (new Thread(() => {

            // if on desktop
            //client.Connect(IPAddress.Loopback, port);

            Connect();

            SendRequest();

            GetResponce();

        })).Start();
    }

    void Connect()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += "Connecting to server...\n";
        });
        connectingBeginTime = System.DateTime.Now;

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
                //connectingTime = DateTime.Now.Subtract(connectingBeginTime).TotalSeconds;
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += ("Client connected!\n");
                    //GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += ("Connecting time: " + String.Format("{0:0.000}", connectingTime) + "s\n");
                });
                break;
            }
        }
    }

    void SendRequest()
    {
        byte[] commandIDBytesToSend = ASCIIEncoding.ASCII.GetBytes(commandID);

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += "Sending request...\n";
        });

        byte[] frameNoBytesToSend = new byte[FRAME_NO_BYTE_NUM];

        Array.Clear(frameNoBytesToSend, 0, frameNoBytesToSend.Length);
        byte[] frameNoBytes = BitConverter.GetBytes(requestedFrameNo);
        frameNoBytes.CopyTo(frameNoBytesToSend, 0);
        
        //byte[] videoNameBytesToSend = Encoding.UTF8.GetBytes(requestedVideoName);
        byte[] videoNameBytesToSend = ASCIIEncoding.ASCII.GetBytes(requestedVideoName);

        NetworkStream serverStream = client.GetStream();

        bool requestSent = false;

        (new Thread(() =>
        {
            serverStream.Write(commandIDBytesToSend, 0, commandIDBytesToSend.Length);
            serverStream.Write(frameNoBytesToSend, 0, frameNoBytesToSend.Length);
            serverStream.Write(videoNameBytesToSend, 0, videoNameBytesToSend.Length);

            requestSent = true;
            requestingBeginTime = DateTime.Now;
        })).Start();

        while (!requestSent)
        {
            System.Threading.Thread.Sleep(1);
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += "Request sent!\n";
        });


    }

    void GetResponce()
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += "Waiting for reponse...\n";
        });

        //While loop in another Thread is fine so we don't block main Unity Thread
        (new Thread(()=>{
            while (!stop)
            {
                //Read Image Count
                int imageSize = readImageByteSize(FRAME_SIZE_BYTE_NUM);

                if (imageSize == 0)
                {
                    totalElapsedTime = DateTime.Now.Subtract(connectingBeginTime).TotalSeconds;
               
                    UnityMainThreadDispatcher.Instance().Enqueue(() =>
                    {
                        GUIManager.Instance.videoPlayerPanelController.replayButton.gameObject.SetActive(true);
                        GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += ("Video streaming completed!\nTotal elapsed time: "+ String.Format("{0:0.000}", totalElapsedTime) +"s\n");
                    });
                    break;
                }

                //Read Image Bytes and Display it
                readFrameByteArray(imageSize);
            }
        })).Start();
    }

    //Converts the data size to byte array and put result to the fullBytes array
    void byteLengthToFrameByteArray(int byteLength, byte[] fullBytes)
    {
        //Clear old data
        Array.Clear(fullBytes, 0, fullBytes.Length);
        //Convert int to bytes
        byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
        //Copy result to fullBytes
        bytesToSendCount.CopyTo(fullBytes, 0);
    }

    //Converts the byte array to the data size and returns the result
    int frameByteArrayToByteLength(byte[] frameBytesLength)
    {
        int byteLength = BitConverter.ToInt32(frameBytesLength, 0);
        return byteLength;
    }

    /////////////////////////////////////////////////////Read Image SIZE from Server///////////////////////////////////////////////////
    private int readImageByteSize(int size)
    {
        bool disconnected = false;

        NetworkStream serverStream = client.GetStream();
        byte[] imageBytesCount = new byte[size];
        var total = 0;
        do
        {
            var read = serverStream.Read(imageBytesCount, total, size - total);
            //Debug.LogFormat("Client recieved {0} bytes", total);
            if (read == 0)
            {
                disconnected = true;
                break;
            }
            total += read;
        } while (total != size);

        int byteLength;

        if (disconnected)
        {
            byteLength = -1;
        }
        else
        {
            byteLength = frameByteArrayToByteLength(imageBytesCount);
        }
        return byteLength;
    }

    /////////////////////////////////////////////////////Read Image Data Byte Array from Server///////////////////////////////////////////////////
    private void readFrameByteArray(int size)
    {
        bool disconnected = false;

        NetworkStream serverStream = client.GetStream();
        byte[] imageBytes = new byte[size];
        var total = 0;
        do
        {
            var read = serverStream.Read(imageBytes, total, size - total);
            //Debug.LogFormat("Client recieved {0} bytes", total);

            if (read == 0)
            {                
                disconnected = true;
                break;
            }
            total += read;
        } while (total != size);

        bool readyToReadAgain = false;

        if (!disconnected)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                if (GUIManager.Instance.videoPlayerPanelController.videoPlayerView.texture == null)
                {
                    reponseDelayTime = DateTime.Now.Subtract(requestingBeginTime).TotalSeconds;
                    GUIManager.Instance.videoPlayerPanelController.videoStreamingLogText.text += ("Get first frame!\nResponce delay time: " + String.Format("{0:0.000}", reponseDelayTime) + "s\nContinue streaming video...\n");
                }

                LoadImageToTexture(imageBytes);
                readyToReadAgain = true;
            });
        }

        //Wait until old Image is displayed
        while (!readyToReadAgain)
        {
            System.Threading.Thread.Sleep(1);
        }
    }

    void LoadImageToTexture(byte[] receivedImageBytes)
    {
        Texture2D tex = new Texture2D(0,0);
        tex.LoadImage(receivedImageBytes);

        GUIManager.Instance.videoPlayerPanelController.videoPlayerView.texture = tex;

        videoFrames.Add(tex);
    }  

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit");
        StopNetworking();
    }

}