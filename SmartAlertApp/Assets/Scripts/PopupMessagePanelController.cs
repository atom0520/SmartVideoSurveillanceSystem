using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessagePanelController : MonoBehaviour
{
    public Text titleText;
    public Text contentText;
    public Button confirmButton;

    // Use this for initialization
    void Start()
    {
        confirmButton.onClick.AddListener(OnClickConfirmButton);
    }

    public void Setup(string titleText, string contentText)
    {
        this.titleText.text = titleText;
        this.contentText.text = contentText;
    }

    public void OnClickConfirmButton()
    {
        GUIManager.Instance.ClosePopupMessagePanel();
    }
}
