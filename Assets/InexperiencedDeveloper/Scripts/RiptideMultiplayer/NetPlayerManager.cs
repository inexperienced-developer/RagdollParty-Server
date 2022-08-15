using InexperiencedDeveloper.ActiveRagdoll;
using InexperiencedDeveloper.Core;
using InexperiencedDeveloper.Core.Controls;
using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InexperiencedDeveloper.Multiplayer.Riptide.ServerDev
{
    public class NetPlayerManager : Singleton<NetPlayerManager>
    {
        public static Dictionary<ushort, NetPlayer> NetPlayers = new();

        [SerializeField] private GameObject playerPrefab;
        private static GameObject player_prefab;

        private void OnEnable()
        {
            player_prefab = playerPrefab;
        }

        public static NetPlayer GetPlayerById(ushort id)
        {
            if (NetPlayers.ContainsKey(id))
            {
                return NetPlayers[id];
            }
            else
            {
                Debug.LogError($"No player with ID: {id}");
                return null;
            }
        }

        public static void LobbySearcherSpawn(ushort id, string username)
        {
            foreach (NetPlayer otherPlayer in NetPlayers.Values)
                otherPlayer.PreLobbySpawn(id);

            NetPlayer player = new GameObject().AddComponent<NetPlayer>();
            player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)}";
            player.Init(id, username);
            NetPlayers.Add(id, player);
            player.PreLobbySpawn();
        }

        public static void Spawn(ushort lobbyId, ushort playerId)
        {
            Lobby lobby = LobbyManager.Lobbies[lobbyId];
            foreach (NetPlayer otherPlayer in lobby.PlayersInGame)
                otherPlayer.Spawn((short)playerId);

            NetPlayer netPlayer = Instantiate(player_prefab, new Vector3(Random.Range(0f, 2f), 1, Random.Range(0f, 2f)), Quaternion.identity).GetComponent<NetPlayer>();
            NetPlayer playerData = GetPlayerById(playerId);
            netPlayer.name = $"Player {playerId} ({(string.IsNullOrEmpty(playerData.Username) ? "Guest" : playerData.Username)}";
            netPlayer.Init(playerId, playerData.Username);
            netPlayer.Player = RagdollSetup(netPlayer);
            netPlayer.MyLobby = lobby;
            lobby.PlayersInGame.Add(netPlayer);
            NetPlayers[playerId] = netPlayer;
            foreach (NetPlayer otherPlayer in lobby.PlayersInGame)
                netPlayer.Spawn((short)otherPlayer.Id);
        }

        private static ActiveRagdoll.Player RagdollSetup(NetPlayer netPlayer)
        {
            Ragdoll ragdoll = netPlayer.GetComponentInChildren<Ragdoll>();
            ActiveRagdoll.Player player = netPlayer.gameObject.AddComponent<ActiveRagdoll.Player>();
            GroundManager groundManager = netPlayer.gameObject.AddComponent<GroundManager>();
            PlayerControls controls = netPlayer.gameObject.AddComponent<PlayerControls>();
            RagdollMovement movement = netPlayer.gameObject.AddComponent<RagdollMovement>();
            player.Init();
            return player;
        }

        private static void SendInputToPlayer(ushort playerId, Vector2 movement, Vector2 lookDir, bool jump)
        {
            NetPlayer netPlayer = NetPlayers[playerId];
            Vector3 walkDir = new Vector3(movement.x, 0, movement.y);
            netPlayer.Player.Controls.ReceiveInputs(walkDir, lookDir, jump);

            List<NetPlayer> playersInGame = netPlayer.MyLobby.PlayersInGame;
            PlayerControls controls = netPlayer.Player.Controls;
            for (int i = 0; i < playersInGame.Count; i++)
            {
                if (playersInGame[i] == netPlayer) continue;
                SendToClients(playerId, movement, lookDir, jump, playersInGame[i].Id);
            }
        }

        #region ServerToClient Message Sender
        public void SendLobbyDictToClient(ushort toPlayerId)
        {
            foreach (var player in NetPlayers.Values)
            {
                Lobby lobby = player.MyLobby;
                if(lobby != null)
                {
                    Message msg = Message.Create(MessageSendMode.reliable, (ushort)ServerToClientCommand.PopulateLobbyList);
                    ushort lobbyId = lobby.LobbyId;
                    string lobbyName = lobby.LobbyName;

                    msg.AddUShort(lobbyId);
                    msg.AddString(lobbyName);
                    msg.AddUShort(player.Id);

                    NetworkManager.Instance.Server.Send(msg, toPlayerId);
                }
                else
                {
                    Debug.LogWarning($"Player {player.Username} does not have a lobby");
                }

            }
        }


        private static void SendToClients(ushort playerId, Vector2 walkDir, Vector2 camAngles, bool jump, ushort toPlayerId)
        {
            Message msg = Message.Create(MessageSendMode.unreliable, (ushort)ServerToClientCommand.PlayerMove);
            msg.AddUShort(playerId);
            msg.AddVector2(walkDir);
            msg.AddVector2(camAngles);
            msg.AddBool(jump);

            NetworkManager.Instance.Server.Send(msg, toPlayerId);
        }



        #endregion

        #region ClientToServer Message Handler
        [MessageHandler((ushort)ClientToServerRequest.SpawnRequest)]
        private static void ClientSpawn(ushort fromClientId, Message msg)
        {
            ushort lobbyId = msg.GetUShort();

            Spawn(lobbyId, fromClientId);
        }

        [MessageHandler((ushort)ClientToServerRequest.MoveRequest)]
        private static void ReceiveInput(ushort fromPlayerId, Message msg)
        {
            Vector2 movement = msg.GetVector2();
            Vector2 lookDir = msg.GetVector2();
            bool jump = msg.GetBool();

            SendInputToPlayer(fromPlayerId, movement, lookDir, jump);
        }

        #endregion

    }
}

