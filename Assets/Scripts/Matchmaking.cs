using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using fredfishgames.networking;
using System;
using System.Net;
using UnityEngine.Networking;
using System.Threading;
using Mirror;
using NobleConnect.Mirror;
using UnityEngine.UI;


public class Matchmaking : MonoBehaviour {
    Matchmaker mmkr;
    [Header("Connection info")]
    public string Address = "localhost";
    public int Port = 24713;

    Thread GettingMatchThread;
    public static Matchmaking singleton;
    
    NobleNetworkManager networkManager;

    public bool waitForIp;

    #region EDIT

    void OnGettingStatus()
    {
        Debug.Log("Getting status");
    }

    void OnStartClient() {
        Debug.Log("Start Client");
    }


    void OnStartServer()
    {
        Debug.Log("Start Server");
    }

    void OnClearServer() {

    }
#endregion

    #region getServer
    //Get a server to join
    public void StartClient()
    {
        GettingMatchThread = mmkr.GetServer(GetServerCallback);
        OnStartClient();
    }

    public void GetServerCallback(string address) {

        if (address.Equals("")) {
            Debug.Log("No server found...");
            return;
        }
        Debug.Log($"Server Callback {address}");
        
        string[] pair = address.Split(':');
        
        networkManager.networkAddress = pair[0];
        networkManager.networkPort = ushort.Parse(pair[1]);
        networkManager.StartClient();
        
        
    }

    #endregion
    #region sendServer
    //Get a server to join
    public void StartServer()
    {
        networkManager.StartHost();
        waitForIp = true;
    }


    public void SendServerCallback(string args)
    {
        if (args == "0")
        {
            Debug.Log("Couldn't send server...");
            return;
        }
    }

    #endregion
    #region getStatus
    //Get a server to join
    public void GetStatus()
    {
        GettingMatchThread = mmkr.GetStatus(GetStatusCallback);
        OnGettingStatus();
    }


    public static void GetStatusCallback(string args)
    {
        Debug.Log($"Status result: {args}");
    }
    #endregion
    #region clearServer
    //Get a server to join
    public void ClearServer()
    {
        GettingMatchThread = mmkr.ClearServer(ClearServerCallback);
        OnClearServer();
    }


    public static void ClearServerCallback(string args)
    {
        Debug.Log($"Clear result: {args}");
    }
    // Use this for initialization
    void Start()
    {
        networkManager = (NobleNetworkManager)NetworkManager.singleton;
        mmkr = new Matchmaker(Address, Port);
        singleton = this;
    }
    void Update()
    {
        mmkr.Update();

        if (waitForIp && networkManager.HostEndPoint != null)
        {
            GettingMatchThread = mmkr.SendServer(SendServerCallback, networkManager.HostEndPoint.Address, (ushort)networkManager.HostEndPoint.Port);
            OnStartServer();
            waitForIp = false;
        }

    }
    #endregion
}
