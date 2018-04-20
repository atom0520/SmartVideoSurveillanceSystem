// Copyright 2016 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class FCMHandler : MonoBehaviour {

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    //private string topic = "TestTopic";

    public MessageListPanelController messageListPanelController;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    protected virtual void Start() {
        //InitializeFirebase();
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                //InitializeFirebase();
                //UnityMainThreadDispatcher.Instance().Enqueue(() =>
                //{
                    
                //});
            } else {
            Debug.LogError(
                "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Setup message event handlers.
    //void InitializeFirebase() {
    //    //Firebase.Messaging.FirebaseMessaging.Subscribe(topic);
    //    Debug.Log("Firebase Messaging Initialized\n");
    //}

    public virtual void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e) {
        Debug.Log("Received a new message");
        var notification = e.Message.Notification;
        if (notification != null) {
            Debug.Log("title: " + notification.Title);
            Debug.Log("body: " + notification.Body);
        }
        if (e.Message.From.Length > 0)
            Debug.Log("from: " + e.Message.From);
        if (e.Message.Link != null) {
            Debug.Log("link: " + e.Message.Link.ToString());
        }
        if (e.Message.Data.Count > 0) {
            Debug.Log("data:");
            foreach (System.Collections.Generic.KeyValuePair<string, string> iter in
                    e.Message.Data) {
                Debug.Log("  " + iter.Key + ": " + iter.Value);
            }
            if (e.Message.Data.ContainsKey("alert"))
            {
                //Message newMessage = new Message();
                //newMessage.videoName = e.Message.Data["videoName"];
                //newMessage.eventFrame = int.Parse(e.Message.Data["eventFrame"]);
                //newMessage.eventDetectedTime = e.Message.Data["eventTime"];

                //newMessage.messageReceivedTime = System.DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");

                AudioManager.Instance.PlaySoundEffect("alert");

                //DataManager.Instance.AddMessage(newMessage);
                DataManager.Instance.SaveData();

                messageListPanelController.messageListRequestClient.SyncMessageList();
            }
        }
    }

    public virtual void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token) {
        Debug.Log("Received Registration Token: " + token.Token);
        DataManager.Instance.deviceToken = token.Token;
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    // End our messaging session when the program exits.
    public void OnDestroy() {
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
    }

    // Output text to the debug log text field, as well as the console.
    //public void DebugLog(string s) {
    //    print(s);
    //    logText += s + "\n";

    //    while (logText.Length > kMaxLogSize) {
    //        int index = logText.IndexOf("\n");
    //        logText = logText.Substring(index + 1);
    //    }

    //    scrollViewVector.y = int.MaxValue;
    //    }

    //    // Render the log output in a scroll view.
    //    void GUIDisplayLog() {
    //    scrollViewVector = GUILayout.BeginScrollView (scrollViewVector);
    //    GUILayout.Label(logText);
    //    GUILayout.EndScrollView();
    //}

    // Render the buttons and other controls.
    //void GUIDisplayControls(){
    //    if (UIEnabled) {
    //        controlsScrollViewVector =
    //            GUILayout.BeginScrollView(controlsScrollViewVector);
    //        GUILayout.BeginVertical();

    //        GUILayout.BeginHorizontal();
    //        GUILayout.Label("Topic:", GUILayout.Width(Screen.width * 0.20f));
    //        topic = GUILayout.TextField(topic);
    //        GUILayout.EndHorizontal();

    //        if (GUILayout.Button("Subscribe")) {
    //        Firebase.Messaging.FirebaseMessaging.Subscribe(topic);
    //        DebugLog("Subscribed to " + topic);
    //        }
    //        if (GUILayout.Button("Unsubscribe")) {
    //        Firebase.Messaging.FirebaseMessaging.Unsubscribe(topic);
    //        DebugLog("Unsubscribed from " + topic);
    //        }
    //        GUILayout.EndVertical();
    //        GUILayout.EndScrollView();
    //    }
    //}

    // Render the GUI:
    //void OnGUI() {
    //    GUI.skin = fb_GUISkin;
    //    if (dependencyStatus != Firebase.DependencyStatus.Available) {
    //        GUILayout.Label("One or more Firebase dependencies are not present.");
    //        GUILayout.Label("Current dependency status: " + dependencyStatus.ToString());
    //        return;
    //    }

    //    Rect logArea;
    //    Rect controlArea;

    //    if (Screen.width < Screen.height) {
    //        // Portrait mode
    //        controlArea = new Rect(0.0f, 0.0f, Screen.width, Screen.height * 0.5f);
    //        logArea = new Rect(0.0f, Screen.height * 0.5f, Screen.width, Screen.height * 0.5f);
    //    } else {
    //        // Landscape mode
    //        controlArea = new Rect(0.0f, 0.0f, Screen.width * 0.5f, Screen.height);
    //        logArea = new Rect(Screen.width * 0.5f, 0.0f, Screen.width * 0.5f, Screen.height);
    //    }

    //    GUILayout.BeginArea(new Rect(0.0f, 0.0f, Screen.width, Screen.height));

    //    GUILayout.BeginArea(logArea);
    //    GUIDisplayLog();
    //    GUILayout.EndArea();

    //    GUILayout.BeginArea(controlArea);
    //    GUIDisplayControls();
    //    GUILayout.EndArea();

    //    GUILayout.EndArea();
    //}
}
