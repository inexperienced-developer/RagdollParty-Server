using InexperiencedDeveloper.Core;
using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ServerDev
{
    public enum ServerToClientCommand : ushort
    {
        SyncTicks = 1,
        PlayerConnected,
        PlayerJoined,
        PopulateLobbyList,
        PlayerStart,
        PlayerSpawned,
        PlayerMove,
        SyncPosition,
    }

    public enum ClientToServerRequest : ushort
    {
        ConnectRequest = 1,
        HostRequest,
        JoinRequest,
        StartRequest,
        SpawnRequest,
        MoveRequest,
    }

    public class NetworkManager : Singleton<NetworkManager>
    {
        public Server Server { get; private set; }
        public ushort CurrentTick { get; private set; } = 0;
        [SerializeField] private ushort port;
        [SerializeField] private ushort maxClientCount;

        private void Start()
        {
            RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
            Server = new Server();
            Server.Start(port, maxClientCount);
            Server.ClientDisconnected += OnPlayerDisconnect;
        }

        private void FixedUpdate()
        {
            Server.Tick();
            if (CurrentTick % 300 == 0)
                SendSync();
            CurrentTick++;
        }

        private void OnApplicationQuit()
        {
            Server.Stop();
        }

        private void OnPlayerDisconnect(object sender, ClientDisconnectedEventArgs e)
        {
            Destroy(NetPlayerManager.NetPlayers[e.Id].gameObject);
        }

        #region ServerToClient Message Sender
        private void SendSync()
        {
            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientCommand.SyncTicks);
            msg.AddUShort(CurrentTick);
            Server.SendToAll(msg);
        }
        #endregion
    }
}

