using InexperiencedDeveloper.Core;
using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ServerDev
{
    public class LobbyManager : Singleton<LobbyManager>
    {
        public static Dictionary<ushort, Lobby> Lobbies = new();

        [SerializeField] private GameObject lobbyPrefab;
        [SerializeField] private Transform lobbyUIParent;
        private static GameObject lobby_prefab;
        private static Transform lobby_ui_parent;

        private void OnEnable()
        {
            lobby_prefab = lobbyPrefab;
            lobby_ui_parent = lobbyUIParent;
        }

        public static void JoinLobby(ushort lobbyId, ushort playerId)
        {
            if (Lobbies.ContainsKey(lobbyId))
            {
                Lobby lobby = Lobbies[lobbyId];
                NetPlayer player = NetPlayerManager.GetPlayerById(playerId);
                lobby.Players.Add(player);
                player.JoinLobby(lobby.LobbyId, lobby.LobbyName);
            }
            else
            {
                Debug.LogError($"Lobby {lobbyId} doesn't exist");
            }
        }

        private static void CreateLobby(ushort playerId, string lobbyName)
        {
            if (!Lobbies.ContainsKey(playerId))
            {
                NetPlayer host = NetPlayerManager.GetPlayerById(playerId);
                if (host == null)
                {
                    Debug.LogError("Host doesn't exist");
                    return;
                }
                Lobby lobby = new Lobby(host, lobbyName);
                lobby.LobbyUI = Instantiate(lobby_prefab, lobby_ui_parent);
                lobby.LobbyUI.SetActive(false);
                Lobbies.Add(playerId, lobby);
                JoinLobby(host.Id, host.Id);
            }
            else
            {
                Debug.LogError($"Lobby {playerId} already exists as {Lobbies[playerId].LobbyName}");
            }
        }

        private static void StartLobby(ushort lobbyId, ushort levelIndex)
        {
            //Add level selection
            foreach(var player in Lobbies[lobbyId].Players)
            {
                player.EnterGame(levelIndex);
            }
        }

        #region ServerToClient Message Handler
        [MessageHandler((ushort)ClientToServerRequest.HostRequest)]
        private static void CreateNewLobby(ushort fromClientId, Message msg)
        {
            ushort playerId = msg.GetUShort();
            string lobbyName = msg.GetString();
            print("Lobby creating");
            CreateLobby(playerId, lobbyName);
        }

        [MessageHandler((ushort)ClientToServerRequest.JoinRequest)]
        private static void ClientJoinLobby(ushort fromClientId, Message msg)
        {
            ushort playerId = msg.GetUShort();
            ushort lobbyId = msg.GetUShort();

            JoinLobby(lobbyId, playerId);
            print("Joining lobby");
        }

        [MessageHandler((ushort)ClientToServerRequest.StartRequest)]
        private static void HandleStartRequest(ushort fromClientId, Message msg)
        {
            ushort lobbyId = msg.GetUShort();
            ushort levelIndex = msg.GetUShort();

            StartLobby(lobbyId, levelIndex);
        }

        #endregion
    }

}
