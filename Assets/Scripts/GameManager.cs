using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public Player playerPrefab;
    public GameObject spawnPoints;

    private int spawnIndex = 0;
    private List<Vector3> availiableSpawns = new List<Vector3>();

    public void Awake()
    {
        RefreshSpawns();
    }

    public override void OnNetworkSpawn()
    {
        if (IsHost)
        {
            SpawnPlayers();
        }
    }

    private void RefreshSpawns()
    {
        Transform[] allPoints = spawnPoints.GetComponentsInChildren<Transform>();
        availiableSpawns.Clear();
        foreach (Transform point in allPoints)
        {
            if (point != spawnPoints.transform)
            {
                availiableSpawns.Add(point.localPosition);
            }
        }
    }

    public Vector3 GetNextSpawnLocation()
    {
        var newPosition = availiableSpawns[spawnIndex];
        newPosition.y = 1.5f;
        spawnIndex += 1;
        if (spawnIndex > availiableSpawns.Count - 1)
        {
            spawnIndex = 0;
        }
        return newPosition;
    }
    private void SpawnPlayers()
    {
        foreach (PlayerInfo info in GameData.Instance.allPlayers)
        {
            SpawnPlayer(info);
        }
    }

    private void SpawnPlayer(PlayerInfo info)
    {
        Player playerSpawn = Instantiate(
            playerPrefab, 
            GetNextSpawnLocation(), 
            Quaternion.identity);

        playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
        playerSpawn.PlayerColor.Value = info.color;
        players.Add(playerSpawn);
        playerSpawn.Score.OnValueChanged += HostOnPlayerScoreChanged;
    }

    private void GameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER");
        Player winner = null;

        foreach (Player player in players)
        {
            player.ProcessInput.Value = false;
            if(player.Score.Value >= maxScore)
            {
                winner = player;
            }
        }

        foreach (Player player in players)
        {
            if (player.Score.Value >= maxScore)
            {
                player.gameObject.transform.LookAt(winner.transform);
            }
        }

        var bullets = GameObject.FindGameObjectsWithTag("Bullet");
        for (var i = 0; i < bullets.Length; i++)
        {
            Destroy(bullets[i]);
        }
    }

    //----------------------
    // Events
    //----------------------

    private void HostOnPlayerScoreChanged(int previous, int current)
    {
        if(current >= maxScore && !isGameOver)
        {
            GameOver();
        }
    }

    private void HostOnClientDisconnect(ulong clientId)
    {
        NetworkObject nObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        Player pObject = nObject.GetComponent<Player>();
        PlayerSettings.Remove(pObject);
        Destroy(pObject);
    }

    private void HostOnClientConnected(ulong clientId)
    {
        int playerIndex = GameData.Instance.FindPlayerIndex(clientId);
        if(playerIndex != -1)
        {
            PlayerInfo newPlayerInfo = GameData.Instance.allPlayers[playerIndex];
            SpawnPlayer(newPlayerInfo);
        }
    }
}