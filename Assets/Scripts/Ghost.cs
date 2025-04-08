using UnityEngine;
using UnityEngine.Tilemaps;
using Mirror;
using System.Collections.Generic;

public class Ghost : NetworkBehaviour
{
    public Tile tile;
    public Board mainBoard;
    public Piece trackingPiece;

    public Tilemap tilemap { get; private set; }
    public Vector3Int[] cells { get; private set; }
    public Vector3Int position { get; private set; }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        cells = new Vector3Int[4];
    }

    private void LateUpdate()
    {
        if(isLocalPlayer)
        {
            Clear();
            Copy();
            Drop();
            Set();
        }
    }

    //[Command(requiresAuthority = false)]
    private void Clear()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
        //RpcClear();
    }

    //[Command(requiresAuthority = false)]
    private void Copy()
    {
        for (int i = 0; i < cells.Length; i++) {
            cells[i] = trackingPiece.cells[i];
        }
        //RpcCopy();
    }

    //[Command(requiresAuthority = false)]
    private void Drop()
    {
        Vector3Int position = trackingPiece.position;

        int current = position.y;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        mainBoard.Clear(trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (mainBoard.IsValidPosition(trackingPiece, position)) {
                this.position = position;
            } else {
                break;
            }
        }

        mainBoard.Set(trackingPiece);
        //RpcDrop();
    }

    //[Command(requiresAuthority = false)]
    private void Set()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
            positions.Add(tilePosition);
        }
        //RpcSet();
    }
    /**
    [ClientRpc]
    private void RpcClear()
    {
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    [ClientRpc]
    private void RpcCopy()
    {
        for (int i = 0; i < cells.Length; i++) {
            cells[i] = trackingPiece.cells[i];
        }
    }

    [ClientRpc]
    private void RpcDrop()
    {
        Vector3Int position = trackingPiece.position;

        int current = position.y;
        int bottom = -mainBoard.boardSize.y / 2 - 1;

        mainBoard.Clear(trackingPiece);

        for (int row = current; row >= bottom; row--)
        {
            position.y = row;

            if (mainBoard.IsValidPosition(trackingPiece, position)) {
                this.position = position;
            } else {
                break;
            }
        }

        mainBoard.Set(trackingPiece);
    }
    [ClientRpc]
    private void RpcSet()
    {
        List<Vector3Int> positions = new List<Vector3Int>();
        for (int i = 0; i < cells.Length; i++)
        {
            Vector3Int tilePosition = cells[i] + position;
            tilemap.SetTile(tilePosition, tile);
            positions.Add(tilePosition);
        }
    }
    **/
}
