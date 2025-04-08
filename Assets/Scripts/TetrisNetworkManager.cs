using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TetrisNetworkManager : NetworkManager
{
    public Transform leftBoardSpawn;
    public Transform rightBoardSpawn;
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform start = numPlayers == 0 ? leftBoardSpawn : rightBoardSpawn;
        GameObject player = Instantiate(playerPrefab, start.position, start.rotation);
        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
