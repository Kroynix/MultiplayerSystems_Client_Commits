using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChatBehaviour : MonoBehaviour
{
    public InputField chat;
    public TMP_Text chatBox;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(chat.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            chatBox.text += "\n" + chat.text;
            chat.text = "";
            chat.ActivateInputField();
        }
    }
}
