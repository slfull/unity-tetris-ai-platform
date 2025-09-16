using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class TetrisNetworkManager : NetworkManager
{
    private int connectedPlayer = 0;
    public GameObject boardPrefab;
    public Transform leftBoardSpawn;
    public Transform rightBoardSpawn;
    public override Transform GetStartPosition()
    {
        return numPlayers == 0 ? leftBoardSpawn : rightBoardSpawn;
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        Transform startPos = GetStartPosition();
        GameObject player = startPos != null
            ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
            : Instantiate(playerPrefab);
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
        NetworkServer.AddPlayerForConnection(conn, player);
        GameObject boardObj = Instantiate(boardPrefab, startPos.position, startPos.rotation);
        NetworkServer.Spawn(boardObj, conn);
        connectedPlayer++;
        if(connectedPlayer == 2)
        {
            StartGame();
        }
    }
    [Server]
    public bool StartGame()
    {
        if(connectedPlayer != 2)
        {
            return false;
        }
        Debug.Log("兩位玩家已加入，開始遊戲");
        foreach(var board in FindObjectsByType<BoardMultiplayer>(FindObjectsSortMode.None))
        {
            board.isGameStart = true;
            board.StartGameOnServer();
        }
        return true;
    }
}
