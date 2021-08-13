using MLAPI;
using MLAPI.Transports.UNET;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using MLAPI.Spawning;
using UnityEngine.SceneManagement;
using MLAPI.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject MenuPanel;
    public TMP_InputField InputFieldIP;
    public Button ButtonServer;
    public Button ButtonHost;
    public Button ButtonClient;
    public string MainScene = "MainXR";

    private string correctPassword = "password31";

    private void Start()
    {
        ButtonServer.onClick.AddListener(Server);
        ButtonHost.onClick.AddListener(() => Host());
        ButtonClient.onClick.AddListener(() => Client());

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("ApprovalCheck");
        bool approve = false;
        // if the connection date is correct then approve
        string password = System.Text.Encoding.ASCII.GetString(connectionData);
        if(password == correctPassword)
        {
            // we can join
            approve = true;
        }

        //ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("13069875965682951367");

        callback(false, null, approve, new Vector3(0,0,0), Quaternion.identity);
    }

    public void Server()
    {
        NetworkManager.Singleton.StartServer();
        ChangeScene();
    }

    public void Host()
    {
        NetworkManager.Singleton.StartHost();
        ChangeScene();
    }

    public void Client()
    {
        if (string.IsNullOrEmpty(InputFieldIP.text))
        {
            InputFieldIP.text = "127.0.0.1";
        }
        // TODO read the password via input when needed
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(correctPassword);
        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = InputFieldIP.text;
        NetworkManager.Singleton.StartClient();
        MenuPanel.SetActive(false);
    }

    private void ChangeScene()
    {
        NetworkSceneManager.SwitchScene(MainScene);
    }
}
