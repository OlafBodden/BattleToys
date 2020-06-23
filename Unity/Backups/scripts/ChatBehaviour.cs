using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;
using TMPro;


public class ChatBehaviour : NetworkBehaviour 
{


    [SerializeField] private GameObject chatUI=null;
    [SerializeField] private TMP_Text chatText=null;
    [SerializeField] private TMP_InputField inputField=null;

    private static event Action<string> OnMessage;

    public override void OnStartAuthority()
    {
        //chatUI.SetActive(true);
        OnMessage+=HandleNewMessage;

        
    }

    [ClientCallback]
    private void OnDestroy()
    {
        if (!hasAuthority) { return;}

        OnMessage-=HandleNewMessage;


    }

    private void HandleNewMessage(string message)
    {
        chatText.text +=message;

    }

    [Client]
    public void Send(string message)
    {
        message=inputField.text;
        Debug.Log("Want to send: " + message);
        //if (!Input.GetKeyDown(KeyCode.Return)) {return;}
        //if (string.IsNullOrWhiteSpace(message)) {return;}

        Debug.Log("now sending to server");

        CmdSendMessage(message);

        inputField.text=string.Empty;


    }

    [Command]
    private void CmdSendMessage(string message)
    {
        RpcHandleMessage($"[{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
    }



}
