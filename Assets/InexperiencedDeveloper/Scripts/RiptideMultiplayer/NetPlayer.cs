using InexperiencedDeveloper.ActiveRagdoll;
using RiptideNetworking;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ServerDev
{
    public class NetPlayer : MonoBehaviour
    {
        public Player Player;
        public ushort Id { get; private set; }
        public string Username { get; private set; }
        public Lobby MyLobby;

        public void Init(ushort id, string username)
        {
            Id = id;
            Username = username;
        }

        private void OnDestroy()
        {
            NetPlayerManager.NetPlayers.Remove(Id);
            print("Removed " + Id);
        }

        public void Spawn(short id = -1)
        {
            if(id < 0)
            {
                SendSpawned();
            }
            else
            {
                SendSpawned((ushort)id);
            }
        }

        #region ServerToClient Message Sender
        //Lobby Data
        public void JoinLobby(ushort lobbyId, string lobbyName)
        {
            MyLobby = LobbyManager.Lobbies[lobbyId];
            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientCommand.PlayerJoined);
            msg.AddUShort(lobbyId);
            msg.AddUShort(Id);
            msg.AddString(lobbyName);
            NetworkManager.Instance.Server.SendToAll(msg);
        }

        public void EnterGame(ushort levelIndex)
        {
            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientCommand.PlayerStart);
            msg.AddUShort(levelIndex);
            NetworkManager.Instance.Server.Send(msg, Id);
        }

        //Spawn Data
        public void PreLobbySpawn()
        {
            NetworkManager.Instance.Server.SendToAll(SetPreLobbyData());
            NetPlayerManager.Instance.SendLobbyDictToClient(Id);
        }

        public void PreLobbySpawn(ushort toClientId)
        {
            NetworkManager.Instance.Server.Send(SetPreLobbyData(), toClientId);
        }

        private Message SetPreLobbyData()
        {
            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientCommand.PlayerConnected);
            msg.AddUShort(Id);
            msg.AddString((string.IsNullOrEmpty(Username) ? "Guest" : Username));
            return msg;
        }

        private void SendSpawned()
        {
            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientCommand.PlayerSpawned);
            NetworkManager.Instance.Server.SendToAll(SetSpawnData(msg));
        }

        private void SendSpawned(ushort toClientId)
        {
            Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientCommand.PlayerSpawned);
            NetworkManager.Instance.Server.Send(SetSpawnData(msg), toClientId);
        }

        private Message SetSpawnData(Message msg)
        {
            print($"Spawning {Id}");
            msg.AddUShort(Id);
            msg.AddString((string.IsNullOrEmpty(Username) ? "Guest" : Username));
            msg.AddVector3(transform.position);
            msg.AddUShort(MyLobby.LobbyId);
            return msg;
        }

        #endregion

        #region ClientToServer Message Handler
        [MessageHandler((ushort)ClientToServerRequest.ConnectRequest)]
        private static void Name(ushort fromClientId, Message msg)
        {
            NetPlayerManager.LobbySearcherSpawn(fromClientId, msg.GetString());
        }
        #endregion
    }
}

