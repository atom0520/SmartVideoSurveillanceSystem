using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoPlayerPanelController : MonoBehaviour {

    public Text videoTitleText;
    public Text videoStreamingLogText;
    //public Text frameText;
    //public Text eventDetectedTimeText;
    //public Text messageReceivedTimeText;
    public Button returnButton;
    public Button replayButton;
    public VideoStreamingClient videoStreamingClient;

    public int replayFrameRate = 16;

    public RawImage videoPlayerView;

    Message message;

    // Use this for initialization
    void Start() {
        returnButton.onClick.AddListener(OnClickReturnButton);

        replayButton.onClick.AddListener(OnClickReplayButton);
    }

    public void Setup(Message message) {
        this.message = message;
        this.videoTitleText.text = "\""+message.videoName+ "\" - around frame "+message.eventFrame.ToString();

        this.videoPlayerView.texture = null;

        replayButton.gameObject.SetActive(false);

        this.videoStreamingLogText.text = "\n";
        
        //this.frameText.text = "Frame: " + message.eventFrame.ToString();
        //this.eventDetectedTimeText.text = "EventDetectedTime: " + message.eventDetectedTime;
        //this.messageReceivedTimeText.text = "MsgReceivedTime: " + message.messageReceivedTime;

        videoStreamingClient.StartVideoStreaming(message.videoName, message.eventFrame);
    }

    //private void OnDisable()
    //{
    //    videoStreamingClient.Disconnect();
    //}

    public void OnClickReturnButton()
    {
        GUIManager.Instance.OpenMessageListPanel();
        GUIManager.Instance.CloseVideoPlayerPanel();

    }

    public void OnClickReplayButton()
    {
        this.StopCoroutine("ReplayVideo");
        this.StartCoroutine("ReplayVideo");
    }

    IEnumerator ReplayVideo()
    {
        //Debug.Log("ReplayVideo begin");
        for(int i = 0; i < videoStreamingClient.videoFrames.Count; i++)
        {
            videoPlayerView.texture = videoStreamingClient.videoFrames[i];
            yield return new WaitForSeconds(1.0f/replayFrameRate);
        }
        //Debug.Log("ReplayVideo end");
    }
}
