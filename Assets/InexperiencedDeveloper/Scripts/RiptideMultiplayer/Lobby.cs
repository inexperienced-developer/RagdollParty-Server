using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InexperiencedDeveloper.Multiplayer.Riptide.ServerDev
{
    public class Lobby
    {
        public GameObject LobbyUI;

        public string LobbyName;
        public ushort LobbyId;
        public ushort MaxPlayers = 8;

        public NetPlayer Host;
        public List<NetPlayer> Players;
        public List<NetPlayer> PlayersInGame;

        public Lobby(NetPlayer host, string lobbyName)
        {
            Host = host;
            LobbyId = Host.Id;
            LobbyName = (string.IsNullOrEmpty(lobbyName) ? host.Username : lobbyName);
            Players = new();
            PlayersInGame = new();
        }

        public Lobby(NetPlayer host, string lobbyName, ushort maxPlayers)
        {
            Host = host;
            LobbyId = Host.Id;
            LobbyName = (string.IsNullOrEmpty(lobbyName) ? host.Username : lobbyName);
            MaxPlayers = maxPlayers;
            Players = new();
            PlayersInGame = new();
        }
    }
}


