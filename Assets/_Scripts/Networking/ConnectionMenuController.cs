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

public class ConnectionMenuController : MonoBehaviour
{
    public bool StartDirectlyAsServer;
    public TMP_InputField InputFieldIP;
    public Button ButtonServer;
    public Button ButtonHost;
    public string MainScene = "MainXR";

    private string correctPassword = "password31";
    private string ipToConnect;

    private void Start()
    {
#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
        StartDirectlyAsServer = true;
#endif

        if (StartDirectlyAsServer)
            Server();

        ButtonServer.onClick.AddListener(Server);
        ButtonHost.onClick.AddListener(() => Host());

        // Just for testing
        //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = false;
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("ApprovalCheck");
        bool approve = false;
        // if the connection date is correct then approve
        string password = System.Text.Encoding.ASCII.GetString(connectionData);
        if (password == correctPassword)
        {
            // we can join
            approve = true;
        }

        //ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("13069875965682951367");

        callback(false, null, approve, new Vector3(0, 0, 0), Quaternion.identity);
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

    #region ClientConnect
    public void StartClient(string ip)
    {
        ipToConnect = ip;
        Client();
    }

    public void Client()
    {
        if (!string.IsNullOrEmpty(InputFieldIP.text))
        {
            ipToConnect = InputFieldIP.text;
        }
        // TODO read the password via input when needed
        //NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(correctPassword);

        NetworkManager.Singleton.GetComponent<UNetTransport>().ConnectAddress = ipToConnect;
        NetworkManager.Singleton.StartClient();
    }
    #endregion

    private void ChangeScene()
    {
        NetworkSceneManager.SwitchScene(MainScene);
    }
}
