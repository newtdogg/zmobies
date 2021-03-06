﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class Pathfinding : MonoBehaviour
{
    // Start is called before the first frame update
    private MapGenerator mapGenerator;
    private TileTools tileTools;
    private WorldTile[,] worldTileMap;
    private Map map;
    private Tilemap tilemap;
    public bool debug;
    private PathRequestController requestManager;

    void Start() {
        requestManager = GetComponent<PathRequestController>();
        var mapObject = GameObject.Find("MapGridObject");
        tilemap = mapObject.transform.GetChild(0).gameObject.GetComponent<Tilemap>();
        mapGenerator = mapObject.GetComponent<MapGenerator>();
        map = mapObject.GetComponent<Map>();
        tileTools = GameObject.Find("TileTools").GetComponent<TileTools>();
    }

    // Update is called once per frame
    void Update() {
        if(mapGenerator.mapGenerated) {
            worldTileMap = map.worldTiles;
            // for(var y = 0; y < map.GetLength(0) -1; y++) {
            //     for(var x = 0; x < map.GetLength(1) -1; x++) {
            //         if(map[y,x].walkable) {
            //             tilemap.SetTileFlags(new Vector3Int(x, y, 0), TileFlags.None);
            //             tilemap.SetColor(new Vector3Int(x, y, 0), new Color(0.74f, 0.23f, 0.1f, 1f));
            //         }
            //     }
            // }
        }
    }

    public void startFindPath(Vector2 startingPosition, Vector2 targetPosition) {
        StartCoroutine(findPath(startingPosition, targetPosition));
    }

    public IEnumerator findPath(Vector2 startingPosition, Vector2 targetPosition) {
        Vector2[] waypoints = new Vector2[0];
        bool pathSuccess = false;
       
        var startingTile = worldTileMap[(int)startingPosition.x, (int)startingPosition.y];
        var endTile = worldTileMap[(int)targetPosition.x, (int)targetPosition.y];
        startingTile.parent = startingTile;

        // Debugging
        // var tPos = new Vector3Int((int)targetPosition.x, (int)targetPosition.y, 0);
        // tilemap.SetTileFlags(tPos, TileFlags.None);
        // tilemap.SetColor(tPos, new Color(0.74f, 0.23f, 0.1f, 1f));

        if(startingTile.walkable && endTile.walkable) {
            Heap<WorldTile> openSet = new Heap<WorldTile>(mapGenerator.width * mapGenerator.height);
            HashSet<WorldTile> closedSet = new HashSet<WorldTile>();

            openSet.Add(startingTile);

            while(openSet.Count > 0) {
                var currentWorldTile = openSet.RemoveFirst();
                closedSet.Add(currentWorldTile);

                // NOTE Debugging
                // tileTools.setTileColor(tilemap, currentWorldTile.worldPosition.x, currentWorldTile.worldPosition.y, new Color(0.74f, 0.23f, 0.1f, 0.1f));
                if ((Mathf.Floor(currentWorldTile.worldPosition.x) == Mathf.Floor(targetPosition.x)) &&
                    (Mathf.Floor(currentWorldTile.worldPosition.y) == Mathf.Floor(targetPosition.y))) {
                    pathSuccess = true;
                    break;
                }

                var neighbourTiles = mapGenerator.getNeighbourTiles(currentWorldTile.worldPosition);
                foreach (var neighbour in neighbourTiles) {
                    if(!neighbour.walkable || closedSet.Contains(neighbour)) {
                        continue;
                    }
                    int newMovementCostToNeighbour = currentWorldTile.gCost + getDistance(currentWorldTile, neighbour);
                    if(newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour)) {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = getDistance(neighbour, worldTileMap[(int)targetPosition.x, (int)targetPosition.y]);
                        neighbour.parent = currentWorldTile;
                        if(!openSet.Contains(neighbour)) {
                            openSet.Add(neighbour);
                        } else {
                            openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }
            // pathSuccess = true;
            // waypoints = new Vector2[0];
        }
        yield return null;
        if(pathSuccess) {
            waypoints = retracePath(startingTile, endTile);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess); 
    }

    Vector2[] retracePath(WorldTile startingTile, WorldTile endTile) {
        resetAllTiles();
        List<WorldTile> path = new List<WorldTile>();
// ????????????????
        var currentWorldTile = endTile;
        // while(startingTile != endTile) {
        while((startingTile.worldPosition.x != currentWorldTile.worldPosition.x) ||
              (startingTile.worldPosition.y != currentWorldTile.worldPosition.y)) {
            path.Add(currentWorldTile);

            // DEBUGGING
            // tileTools.setTileColor(tilemap, currentWorldTile.worldPosition.x, currentWorldTile.worldPosition.y, new Color(1f, 1f, 1f, 1f));

            currentWorldTile = currentWorldTile.parent;
        }
        var waypoints = simplifyPath(path);

        // var waypointVectors = new List<Vector2>();
        // for (int i = 0; i < path.Count; i++) {
        //     waypointVectors.Add(path[i].worldPosition);
        // }
        // var waypoints = waypointVectors.ToArray();

        return waypoints;
    }

    Vector2[] simplifyPath (List<WorldTile> path) {
        List<Vector2> waypoints = new List<Vector2>();
        Vector2 directionOld = Vector2.zero;
        for (int i = 1; i < path.Count; i++) {
            Vector2 directionNew = new Vector2(
                path[i-1].worldPosition.x - path[i].worldPosition.x,
                path[i-1].worldPosition.y - path[i].worldPosition.y 
            );
            if(directionNew != directionOld) {
                waypoints.Add(path[i - 1].worldPosition);
            }
            directionOld = directionNew;
        }
        var waypointsArr = waypoints.ToArray();
        Array.Reverse(waypointsArr);
		return waypointsArr;
    }

    public int getDistance(WorldTile a, WorldTile b) {
        var distX = (int)Mathf.Abs(a.worldPosition.x - b.worldPosition.x);
        var distY = (int)Mathf.Abs(a.worldPosition.y - b.worldPosition.y);

        if(distX > distY) {
            return 14 * distY + 10 * (distX - distY);
        } else {
            return 14 * distX + 10 * (distY - distX);
        }
    }


    // DEBUGGING

    public void resetAllTiles() {
        for(var x = 0; x < 60; x++) {
            for(var y = 0; y < 60; y++) {
                tileTools.setTileColor(tilemap, x, y, new Color(0, 0, 0, 0));
            }
        }
    }
}
