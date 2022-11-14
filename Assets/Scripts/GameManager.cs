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
        foreach (PlayerInfo pi in GameData.Instance.allPlayers)
        {
            Player playerSpawn = Instantiate(playerPrefab, GetNextSpawnLocation(), Quaternion.identity);
            playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(pi.clientId);
            playerSpawn.PlayerColor.Value = pi.color;
        }
    }
}