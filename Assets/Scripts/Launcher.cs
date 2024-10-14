using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using DevToDev.Analytics;
using Sentry;

public class Launcher : MonoBehaviourPunCallbacks
{
    public event Action OnSearchStart;
    public event Action OnSearchStop;
    private bool isConnectedToMaster = false;
    [SerializeField]
    ObstacleManager obstacleManager;
    /// <summary>
    /// This client's version number. Users are separated from each other by gameVersion (which allows you to make breaking changes).
    /// </summary>
    string gameVersion = "1";
    
    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
    /// </summary>
    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    /// <summary>
    /// MonoBehaviour method called on GameObject by Unity during initialization phase.
    /// </summary>
    void Start()
    {
        DTDAnalyticsConfiguration config = new DTDAnalyticsConfiguration
        {
            LogLevel = DTDLogLevel.Debug,
            TrackingAvailability = DTDTrackingStatus.Enable
        };

#if UNITY_ANDROID
        DTDAnalytics.Initialize("androidAppID", config);
#elif UNITY_IOS
        DTDAnalytics.Initialize("IosAppID", config);
#elif UNITY_WEBGL
        DTDAnalytics.Initialize("WebAppID", config);
#elif UNITY_STANDALONE_WIN
        DTDAnalytics.Initialize("5bc977cf-78f9-070f-bd77-324fa9a625ca", config);
#elif UNITY_STANDALONE_OSX
        DTDAnalytics.Initialize("OsxAppID", config);
#elif UNITY_WSA
        DTDAnalytics.Initialize("UwpAppID", config);
#endif

        DTDAnalytics.StartActivity();

     

        Connect();
    }

    private void OnDestroy()
    {
        DTDAnalytics.StopActivity();
    }

    /// <summary>
    /// Start the connection process.
    /// - If already connected, we attempt joining a random room
    /// - if not yet connected, Connect this application instance to Photon Cloud Network
    /// </summary>
    public void Connect()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;

            DTDAnalytics.CustomEvent("Connect event");
            SentrySdk.CaptureMessage("Connect event");
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("OnPlayerEnteredRoom()");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            obstacleManager.StartGame();
            OnSearchStop.Invoke();
            isConnectedToMaster = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (ObstacleManager.Instance.IsGameActive)
            ObstacleManager.Instance.EndGame(true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster()");
        isConnectedToMaster = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed() PhotonNetwork.CreateRoom()");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 2, CleanupCacheOnLeave = false });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom()");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            obstacleManager.StartGame();
            OnSearchStop();
            isConnectedToMaster = false;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("OnDisconnected() {0}", cause);
        isConnectedToMaster = false;
    }

    public void StartSearch()
    {
        if (!isConnectedToMaster)
            return;
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
        PhotonNetwork.JoinRandomRoom();
        OnSearchStart?.Invoke();
    }
    public void StopSearch()
    {
        if (PhotonNetwork.CurrentRoom != null)
            PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom()");
        OnSearchStop?.Invoke();
    }
}
