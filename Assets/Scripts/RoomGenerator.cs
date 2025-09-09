using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomGenerator : MonoBehaviour
{
    [Header("Player Settings")]
    public GameObject playerPrefab;
    private GameObject playerInstance;

    [Header("Dungeon Settings")]
    public int dungeonWidth = 100;
    public int dungeonHeight = 100;
    public int maxLeafSize = 30;

    [Header("Room Prefabs")]
    public List<GameObject> roomPrefabs;

    [Header("Obstacle Settings")]
    public List<GameObject> obstaclePrefabs;
    [Range(0f, 1f)] public float obstacleSpawnChance = 0.5f; // % chance a room spawns one


    [Header("Corridor Tile Prefab")]
    public GameObject floorTilePrefab;

    // mapping of global tile positions -> the Tilemap that contains that wall tile
    private readonly Dictionary<Vector2Int, Tilemap> wallTileToTilemap = new();


    [Header("Enemy Settings")]
    public List<GameObject> enemyPrefabs;
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 5;

    [Header("Special Objects")]
    public GameObject keyPrefab;
    public GameObject chestPrefab;

    private int startingRoomIndex = -1;
    private int keyRoomIndex = -1;


    private List<RectInt> rooms = new();

    private List<RoomController> roomControllers = new List<RoomController>();


    private List<Vector2Int> roomCenters = new();
    private HashSet<Vector2Int> roomTiles = new();
    private HashSet<Vector2Int> wallTiles = new();
// For each room, which corridor wall tiles should be removed on clear
private readonly Dictionary<int, HashSet<Vector2Int>> pendingBreaksPerRoom = new();

// Adjacency list of connected rooms
public Dictionary<int, List<int>> roomConnections = new();

// Corridor paths (full floor path tiles)
public Dictionary<(int, int), List<Vector2Int>> corridorConnections = new();


    void Start()
    {
        GenerateBSPDungeon();

    }

    void GenerateBSPDungeon()
    {
        Leaf root = new Leaf(new RectInt(0, 0, dungeonWidth, dungeonHeight));
        List<Leaf> leaves = new List<Leaf> { root };
        bool didSplit = true;

        while (didSplit)
        {
            didSplit = false;
            foreach (Leaf leaf in new List<Leaf>(leaves))
            {
                if (leaf.left == null && leaf.right == null)
                {
                    if (leaf.area.width > maxLeafSize ||
                        leaf.area.height > maxLeafSize ||
                        Random.value > 0.25f)
                    {
                        if (leaf.Split())
                        {
                            leaves.Add(leaf.left);
                            leaves.Add(leaf.right);
                            didSplit = true;
                        }
                    }
                }
            }
        }

        foreach (Leaf leaf in leaves)
            TryPlaceRoomInLeaf(leaf);

        CollectWallsFromRooms();

        // Connect rooms dynamically
        for (int i = 1; i < rooms.Count; i++)
        {
            ConnectRooms(i - 1, i);
        }

        // After connecting rooms
           SpawnPlayerInBottomLeftRoom();
            SpawnChestAndKey();
            SpawnObstaclesAfterRooms();
    }

void TryPlaceRoomInLeaf(Leaf leaf)
{
    if (leaf.left != null || leaf.right != null) return;

    List<GameObject> shuffled = new(roomPrefabs);
    ShuffleList(shuffled);

    foreach (GameObject prefab in shuffled)
    {
        Vector2 prefabSize = GetRoomSizeInTiles(prefab);
        int roomWidth = (int)prefabSize.x;
        int roomHeight = (int)prefabSize.y;

        if (roomWidth + 2 <= leaf.area.width &&
            roomHeight + 2 <= leaf.area.height)
        {
            int x = leaf.area.x + (leaf.area.width - roomWidth) / 2;
            int y = leaf.area.y + (leaf.area.height - roomHeight) / 2;

            Vector3 spawnPos = new Vector3(x + roomWidth / 2f, y + roomHeight / 2f, 0);
            GameObject roomObj = Instantiate(prefab, spawnPos, Quaternion.identity);

            RectInt roomRect = new RectInt(x, y, roomWidth, roomHeight);
            rooms.Add(roomRect);
            Vector2Int center = new Vector2Int((int)spawnPos.x, (int)spawnPos.y);
            roomCenters.Add(center);

            for (int i = 0; i < roomWidth; i++)
                for (int j = 0; j < roomHeight; j++)
                    roomTiles.Add(new Vector2Int(x + i, y + j));

            // Setup RoomController
            RoomController rc = roomObj.GetComponent<RoomController>();
            if (rc == null) rc = roomObj.AddComponent<RoomController>();

            rc.Initialize(rooms.Count - 1, this); // index = last room added
            roomControllers.Add(rc);              // store for easy lookup

            // Collect wall tilemap
            Tilemap wallTM = null;
            foreach (var tm in roomObj.GetComponentsInChildren<Tilemap>(true))
            {
                if (tm.gameObject.CompareTag("Wall") || tm.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    wallTM = tm;
                    break;
                }
            }
            if (wallTM == null) wallTM = roomObj.GetComponentInChildren<Tilemap>();
            rc.WallTilemap = wallTM;

            // Spawn enemies and register with RoomController
            SpawnEnemiesInRoom(roomRect, rc);

            return;
        }
    }
}

void CollectWallsFromRooms()
{
    wallTiles.Clear();
    wallTileToTilemap.Clear();

    GameObject[] roomObjects = GameObject.FindGameObjectsWithTag("Room");

    foreach (GameObject room in roomObjects)
    {
        // try to find the dedicated wall tilemap first
        Tilemap wallTilemap = null;
        foreach (var tm in room.GetComponentsInChildren<Tilemap>(true))
        {
            if (tm.gameObject.CompareTag("Wall") || tm.gameObject.layer == LayerMask.NameToLayer("Wall"))
            {
                wallTilemap = tm;
                break;
            }
        }
        if (wallTilemap == null) wallTilemap = room.GetComponentInChildren<Tilemap>();
        if (wallTilemap == null) continue;

        Vector3Int roomOrigin = wallTilemap.WorldToCell(room.transform.position);

        foreach (var pos in wallTilemap.cellBounds.allPositionsWithin)
        {
            if (!wallTilemap.HasTile(pos)) continue;

            Vector3Int globalTilePos = pos + roomOrigin;
            var global2 = new Vector2Int(globalTilePos.x, globalTilePos.y);
            wallTiles.Add(global2);

            // map this global tile coordinate to the tilemap that owns it
            // (if multiple tilemaps would claim the same pos, later ones overwrite — unlikely)
            wallTileToTilemap[global2] = wallTilemap;
        }
    }
}

void ConnectRooms(int roomIndexA, int roomIndexB)
    {
        Vector2Int start = roomCenters[roomIndexA];
        Vector2Int end = roomCenters[roomIndexB];

        Debug.Log($"[ConnectRooms] Connecting Room {roomIndexA} center {start} -> Room {roomIndexB} center {end}");
        CreateCorridor(start, end, roomIndexA, roomIndexB);
    }

void CreateCorridor(Vector2Int from, Vector2Int to, int roomIndexA, int roomIndexB)
{
    Debug.Log($"[CreateCorridor] Creating corridor from {from} to {to}");

    AStarPathfinder pathfinder = new AStarPathfinder(wallTiles, dungeonWidth, dungeonHeight);
    List<Vector2Int> path = pathfinder.FindPath(from, to);
    if (path == null || path.Count == 0)
    {
        Debug.LogWarning($"No path found from {from} to {to}");
        return;
    }

    // Save full corridor path for later
    corridorConnections[(roomIndexA, roomIndexB)] = path;

    // Track adjacency
    if (!roomConnections.ContainsKey(roomIndexA))
        roomConnections[roomIndexA] = new List<int>();
    if (!roomConnections.ContainsKey(roomIndexB))
        roomConnections[roomIndexB] = new List<int>();
    if (!roomConnections[roomIndexA].Contains(roomIndexB))
        roomConnections[roomIndexA].Add(roomIndexB);
    if (!roomConnections[roomIndexB].Contains(roomIndexA))
        roomConnections[roomIndexB].Add(roomIndexA);

    // Place floor prefabs for all corridor tiles that are not room interior tiles.
    foreach (Vector2Int pos in path)
    {
        // Place floor prefab for corridor tiles outside rooms
        if (!roomTiles.Contains(pos))
            PlaceFloorTile(pos);
    }

    Debug.Log("[CreateCorridor] Corridor complete");
}



    void PlaceFloorTile(Vector2Int position)
    {
        // Put floor tiles on Z = +1 so rooms render above them
        Vector3 worldPos = new Vector3(position.x + 0.5f, position.y + 0.5f, 1f);
        Instantiate(floorTilePrefab, worldPos, Quaternion.identity);
    }

    Vector2 GetRoomSizeInTiles(GameObject room)
    {
        RoomSize sizeComponent = room.GetComponent<RoomSize>();
        if (sizeComponent != null)
            return new Vector2(sizeComponent.widthInTiles, sizeComponent.heightInTiles);

        Debug.LogWarning($"Room prefab '{room.name}' is missing RoomSize script.");
        return Vector2.zero;
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }



void SpawnPlayerInBottomLeftRoom()
{
    if (playerPrefab == null || roomCenters.Count == 0) return;

    Vector2Int bottomLeft = roomCenters[0];
    int bottomLeftIndex = 0;
    for (int i = 0; i < roomCenters.Count; i++)
    {
        Vector2Int center = roomCenters[i];
        if (center.x <= bottomLeft.x && center.y <= bottomLeft.y)
        {
            bottomLeft = center;
            bottomLeftIndex = i;
        }
    }

    startingRoomIndex = bottomLeftIndex;

    Vector3 spawnPos = new Vector3(bottomLeft.x, bottomLeft.y, 0);
    playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

    Camera.main.GetComponent<CameraFollow>()?.SetTarget(playerInstance.transform);
}



void SpawnEnemiesInRoom(RectInt roomRect, RoomController rc)
{
    if (enemyPrefabs.Count == 0) return;

    int enemyCount = UnityEngine.Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);

    // Shrink room bounds by 2 on each side to avoid walls
    int spawnXMin = roomRect.xMin + 2;
    int spawnXMax = roomRect.xMax - 2;
    int spawnYMin = roomRect.yMin + 2;
    int spawnYMax = roomRect.yMax - 2;

    for (int i = 0; i < enemyCount; i++)
    {
        GameObject prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Count)];

        Vector3 spawnPos = new Vector3(
            UnityEngine.Random.Range(spawnXMin, spawnXMax),
            UnityEngine.Random.Range(spawnYMin, spawnYMax),
            0f
        );

        GameObject enemyObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        BaseEnemy be = enemyObj.GetComponent<BaseEnemy>();

        if (be != null)
        {
            rc.RegisterEnemy(be); // handle room cleared when enemy dies
        }
    }
}


    void SpawnObstacleInRoom(RectInt roomRect, RoomController rc)
    {
        int roomIndex = rc.RoomIndex;

        // Skip obstacle spawning if this room has chest or key
        if (roomIndex == startingRoomIndex || roomIndex == keyRoomIndex)
            return;

        if (obstaclePrefabs.Count == 0) return;
        if (UnityEngine.Random.value > obstacleSpawnChance) return; // roll chance

        GameObject prefab = obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Count)];

        Vector3 spawnPos = new Vector3(
            roomRect.x + roomRect.width / 2f,
            roomRect.y + roomRect.height / 2f,
            0
        );

        GameObject obstacle = Instantiate(prefab, spawnPos, Quaternion.identity);
        obstacle.transform.SetParent(rc.transform, true);

        Debug.Log($"Spawned obstacle {prefab.name} in Room {rc.RoomIndex} at {spawnPos}");
    }

void SpawnObstaclesAfterRooms()
{
    for (int i = 0; i < rooms.Count; i++)
    {
        if (i == startingRoomIndex || i == keyRoomIndex)
            continue; // skip special rooms

        RectInt rect = rooms[i];
        RoomController rc = GetRoomController(i);
        SpawnObstacleInRoom(rect, rc);
    }
}



    void SpawnChestAndKey()
{
    if (chestPrefab == null || keyPrefab == null) return;

    // --- Spawn chest in starting room ---
    RoomController startRC = GetRoomController(startingRoomIndex);
    RectInt startRoomRect = rooms[startingRoomIndex];

    Vector3 chestPos = new Vector3(
        startRoomRect.x + startRoomRect.width / 2f,
        startRoomRect.y + startRoomRect.height / 2f,
        0
    );

    GameObject chest = Instantiate(chestPrefab, chestPos, Quaternion.identity);
    chest.transform.SetParent(startRC.transform, true);

    // --- Find furthest reachable room for the key ---
    keyRoomIndex = FindFurthestReachableRoom(startingRoomIndex);

    // --- Spawn key ---
    RoomController keyRC = GetRoomController(keyRoomIndex);
    RectInt keyRoomRect = rooms[keyRoomIndex];

    Vector3 keyPos = new Vector3(
        keyRoomRect.x + keyRoomRect.width / 2f,
        keyRoomRect.y + keyRoomRect.height / 2f,
        0
    );

    GameObject key = Instantiate(keyPrefab, keyPos, Quaternion.identity);
    key.transform.SetParent(keyRC.transform, true);

    Debug.Log($"Spawned chest in Room {startingRoomIndex} and key in Room {keyRoomIndex} (furthest reachable)");
}


    public List<Vector2Int> GetCorridorTiles(int roomA, int roomB)
{
    if (corridorConnections.TryGetValue((roomA, roomB), out var tiles))
        return tiles;
    if (corridorConnections.TryGetValue((roomB, roomA), out tiles))
        return tiles;
    return null;
}

    public List<int> GetConnectedRooms(int roomIndex)
    {
        if (roomConnections.TryGetValue(roomIndex, out var connected))
            return connected;
        return new List<int>();
    }

public RoomController GetRoomController(int roomIndex)
{
    if (roomIndex >= 0 && roomIndex < roomControllers.Count)
        return roomControllers[roomIndex];
    return null;
}

int FindFurthestReachableRoom(int startIndex)
{
    Queue<int> queue = new Queue<int>();
    Dictionary<int, int> distance = new Dictionary<int, int>();

    queue.Enqueue(startIndex);
    distance[startIndex] = 0;

    int furthestRoom = startIndex;
    int maxDist = 0;

    while (queue.Count > 0)
    {
        int current = queue.Dequeue();
        int dist = distance[current];

        if (dist > maxDist)
        {
            maxDist = dist;
            furthestRoom = current;
        }

        if (roomConnections.TryGetValue(current, out var neighbors))
        {
            foreach (int neighbor in neighbors)
            {
                if (!distance.ContainsKey(neighbor))
                {
                    distance[neighbor] = dist + 1;
                    queue.Enqueue(neighbor);
                }
            }
        }
    }

    return furthestRoom;
}

public int RoomCount => rooms.Count;

    public RectInt GetRoomBounds(int index)
    {
        if (index >= 0 && index < rooms.Count)
            return rooms[index];
        return new RectInt();
    }

}
