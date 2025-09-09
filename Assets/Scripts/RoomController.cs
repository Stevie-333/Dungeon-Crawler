using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomController : MonoBehaviour
{
    [Header("Room Info")]
    public RectInt Bounds;             // Room rectangle in grid space
    public int RoomIndex;              // Index in rooms list
    public Tilemap WallTilemap;        // Assign in code during room spawn

    public event Action<int> OnCleared; // Event fired when room cleared

    private RoomGenerator roomGenerator;    // Reference to RoomGenerator
    private List<BaseEnemy> enemies = new List<BaseEnemy>();
    private bool isCleared = false;

    [Header("Door / Corridor Visual")]
    [SerializeField] private TileBase doorwayTile;
    // Assign a corridor/doorway-looking tile in the Inspector.
    // If left null, we’ll erase the wall tile visually instead.

    /// <summary>
    /// Initialize the room controller with its index and generator reference.
    /// Must be called right after room prefab is instantiated.
    /// </summary>
    public void Initialize(int index, RoomGenerator generator)
    {
        RoomIndex = index;
        roomGenerator = generator;

        // Attempt to automatically find the wall tilemap if not already assigned
        if (WallTilemap == null)
        {
            Tilemap tm = null;
            foreach (var childTM in GetComponentsInChildren<Tilemap>(true))
            {
                if (childTM.gameObject.CompareTag("Wall") ||
                    childTM.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    tm = childTM;
                    break;
                }
            }

            if (tm == null) tm = GetComponentInChildren<Tilemap>();
            WallTilemap = tm;
        }
        
         OpenAllCorridorExits();
    }

    /// <summary>
    /// Register an enemy in this room. Automatically subscribes to its death event.
    /// </summary>
    public void RegisterEnemy(BaseEnemy enemy)
    {
        if (enemy == null) return;
        enemies.Add(enemy);
        enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void HandleEnemyDied(BaseEnemy enemy)
    {
        enemies.Remove(enemy);

        if (enemies.Count == 0 && !isCleared)
        {
            isCleared = true;
            Debug.Log($"Room {RoomIndex} cleared!");
            OpenAllCorridorExits();
            OnCleared?.Invoke(RoomIndex);
        }
    }

    /// <summary>
    /// Opens all corridor exits connected to this room and the other rooms
    /// by disabling collisions on corridor tiles.
    /// </summary>
    private void OpenAllCorridorExits()
    {
        if (roomGenerator == null) return;

        var connectedRooms = roomGenerator.GetConnectedRooms(RoomIndex);

        foreach (int connectedRoomIndex in connectedRooms)
        {
            var corridorTiles = roomGenerator.GetCorridorTiles(RoomIndex, connectedRoomIndex);
            if (corridorTiles == null) continue;

            // 🔹 For each tile in the corridor, check every room that contains it
            foreach (var tilePos in corridorTiles)
            {
                for (int i = 0; i < roomGenerator.RoomCount; i++)
                {
                    RectInt roomBounds = roomGenerator.GetRoomBounds(i);

                    if (roomBounds.Contains(tilePos))
                    {
                        RoomController rc = roomGenerator.GetRoomController(i);
                        if (rc != null && rc.WallTilemap != null)
                        {
                            DisableWallCollisionAndReplace(rc.WallTilemap, tilePos);
                        }
                    }
                }
            }
        }
    AudioManager.Instance.PlayDoorOpen();

}

    

    /// <summary>
    /// Disables collision at a single tile position in the given wall tilemap.
    /// Replaces it visually with a corridor tile.
    /// </summary>
private void DisableWallCollisionAndReplace(Tilemap tilemap, Vector2Int worldTilePos)
{
    if (tilemap == null) return;

    Vector3 worldCenter = new Vector3(worldTilePos.x + 0.5f, worldTilePos.y + 0.5f, 0);
    Vector3Int localCell = tilemap.WorldToCell(worldCenter);

    if (!tilemap.HasTile(localCell)) return;

    // Open the main door cell
    if (doorwayTile != null)
    {
        tilemap.SetTile(localCell, doorwayTile);
        tilemap.SetColliderType(localCell, Tile.ColliderType.None);
    }
    else
    {
        tilemap.SetTile(localCell, null);
        tilemap.SetColliderType(localCell, Tile.ColliderType.None);
    }

    // Also clear collision one tile above (keep visible)
    Vector3Int aboveCell = localCell + Vector3Int.up;
    if (tilemap.HasTile(aboveCell))
    {
        tilemap.SetColliderType(aboveCell, Tile.ColliderType.None);
    }

    var tmc = tilemap.GetComponent<TilemapCollider2D>();
    if (tmc != null) tmc.ProcessTilemapChanges();

    Debug.Log($"Opened corridor at {localCell} and cleared collision above.");
}

}
