using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.Tilemaps;
using static TileData;
using static UnityEditor.PlayerSettings;

public class TileManager : MonoBehaviour
{
    [SerializeField]
    private Tilemap map;

    [SerializeField]
    private List<TileData> tileDatas;
    private Dictionary<TileBase, TileData> dataFromTiles;
    private Dictionary<Vector3Int, Tile> saveData;


    [SerializeField]
    private GameObject currentRootPosition;
    private Vector2 startPosition; 

    [Header("PaintingTiles")]
    [SerializeField]
    Tile tileToPaint;
    [SerializeField]
    Tile[] tilesToPaint; 

    [SerializeField]
    Tilemap paintedTilemap;
    string currentDir = "down";
    string newDir; 

    [Header("Win Conditions")]
    [SerializeField]
    Sprite transparencySprite;
    public int TotalTiles;
    public int TilesPressed = 1;
    bool won;

    [Header("Nutrients Lol")]
    [SerializeField]
    Tilemap NutrientsTilemap;
    [SerializeField]
    Tile nutrientTile;

    [Header("Effects")]
    bool HasLevelKey;
    [SerializeField]
    Tilemap EffectTilemap;
    [SerializeField]
    GameObject keyUI;
    bool pushing;
    bool lockTileCheck;

    bool stallStuckCheck;

    [Header("New level")]
    [SerializeField]
    int currentLevel = 1;
    [SerializeField]
    GameObject[] levels;
    public Camera MainCamera;

    [Header("Misc")]
    MainManager mainManagerScript;

    private void Awake()
    {
        dataFromTiles = new Dictionary<TileBase, TileData>();
        saveData = new Dictionary<Vector3Int, Tile>();
        foreach (var tileData in tileDatas)
        {
            foreach (var tile in tileData.tiles)
            {
                dataFromTiles.Add(tile, tileData);
            }
        }

        startPosition = currentRootPosition.transform.position;
        SaveLevel();
        GetTileAmountSprite();
        mainManagerScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MainManager>();
       
    }
    void Update()
    {
        

        if (!mainManagerScript.paused)
        {
            CheckInput();

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentRootPosition.transform.position = startPosition;
                currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 0);
                currentDir = "down";
                paintedTilemap.ClearAllTiles();

                TilesPressed = 1;
                LoadLevel();
            }

            
        }

       

    }

    void CheckInput()
    {
        if (pushing) { return; }
        
        if (Input.GetKeyDown(KeyCode.W)) { newDir = "up"; currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 180); MoveRoot(0, 1); }
        else if (Input.GetKeyDown(KeyCode.A)) { newDir = "left"; currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 270); MoveRoot(-1, 0); }
        else if (Input.GetKeyDown(KeyCode.S)) { newDir = "down"; currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 0); MoveRoot(0, -1); }
        else if (Input.GetKeyDown(KeyCode.D)) { newDir = "right"; currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 90); MoveRoot(1, 0);  }
    }

    void MoveRoot(int directionX, int directionY)
    {
        lockTileCheck = false;
        stallStuckCheck = false;
        Vector2 oldPosition = currentRootPosition.transform.position;
        Vector2 newPosition = new Vector2(currentRootPosition.transform.position.x + directionX, currentRootPosition.transform.position.y + directionY);
        Vector3Int gridPosition = map.WorldToCell(newPosition);
        TileBase TileToStepOn = map.GetTile(gridPosition);
        
        if (EffectTilemap.GetTile(gridPosition) != null)
        {
            TileBase EffectTile = EffectTilemap.GetTile(gridPosition);
            if (dataFromTiles[EffectTile].type == TileData.EffectType.keyLock && HasLevelKey)
            {
                EffectTilemap.SetTile(gridPosition, null);
                HasLevelKey = false;
                keyUI.SetActive(false);
            }
            else if (dataFromTiles[EffectTile].type == TileData.EffectType.keyLock && !HasLevelKey)
            {
                lockTileCheck = true;
            }
            
        }

        if (!dataFromTiles[TileToStepOn].wall && !paintedTilemap.HasTile(gridPosition) && !lockTileCheck)
        {
            Vector3Int paintPosition = map.WorldToCell(oldPosition);
            CheckTilePaint(paintPosition);
            TileBase paintedTile = map.GetTile(paintPosition);
            //dataFromTiles[paintedTile].SteppedOn = true;
            
            currentRootPosition.transform.position = newPosition;
            TilesPressed += 1;

            if (TilesPressed >= TotalTiles)
            {
                currentLevel += 1;
                newLevel();
            }

            if (EffectTilemap.GetTile(gridPosition) != null)
            {
                TileBase EffectTile = EffectTilemap.GetTile(gridPosition);
               
                if (dataFromTiles[EffectTile].type == TileData.EffectType.teleporter)
                {
                    stallStuckCheck = true;
                }
            }

            CheckEffect(directionX, directionY);

            bool uhOh = CheckStuck();
            if (uhOh && !stallStuckCheck)
            {
                currentRootPosition.transform.position = startPosition;
                currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 0);
                currentDir = "down";
                paintedTilemap.ClearAllTiles();
                
                TilesPressed = 1;
                LoadLevel();
            }
           
        }
        else
        {
            pushing = false; 
        }

        
    }

    void CheckEffect(int directionX, int directionY)
    {
        Vector3Int gridPosition = EffectTilemap.WorldToCell(currentRootPosition.transform.position);
        Vector2 direction = new Vector2(directionX, directionY);
        if (EffectTilemap.HasTile(gridPosition))
        {
            TileBase CurrentTile = EffectTilemap.GetTile(gridPosition);

            switch (dataFromTiles[CurrentTile].type)
            {
                case TileData.EffectType.key:
                    keyUI.SetActive(true);
                    HasLevelKey = true;
                    EffectTilemap.SetTile(gridPosition, null);
                    break;
                case TileData.EffectType.keyLock:
                    break;
                case TileData.EffectType.teleporter:
                    Sprite tileSprite = EffectTilemap.GetSprite(gridPosition);
                    Debug.Log(tileSprite);
                    findTeleporter(gridPosition, tileSprite);
                    break;
                case TileData.EffectType.pushPad:
                    pushing = false;
                    getPushType(gridPosition, direction);
                    break;
                case TileData.EffectType.none:
                    break;
                default:
                    break;
            }
        }

       
    }

    void getPushType(Vector3Int gridPosition, Vector2 direction)
    {

        pushing = false;
        TileBase tile = EffectTilemap.GetTile(gridPosition);

        switch (dataFromTiles[tile].pushType)
        {
            case TileData.PushType.allDir:
                break;
            case TileData.PushType.left:
                direction = new Vector2(-1, 0);
                break;
            case TileData.PushType.right:
                direction = new Vector2(1, 0);
                break;
            case TileData.PushType.up:
                direction = new Vector2(0, 1);
                break;
            case TileData.PushType.down:
                direction = new Vector2(0, -1);
                break;
            case TileData.PushType.none:
                break;
            default:
                break;
        }
        EffectTilemap.SetTile(gridPosition, null);
        StartCoroutine("push", direction);
    }

    IEnumerator push(Vector2 direction)
    {
        Debug.Log("hi");
        pushing = true;
        while (pushing)
        {
            yield return new WaitForSeconds(0.5f);
            MoveRoot((int)direction.x, (int)direction.y);
        }
    }

    bool CheckStuck()
    {
        if (won) { return false; }
       
        //Up
        Vector2 newPosition = new Vector2(currentRootPosition.transform.position.x, currentRootPosition.transform.position.y + 1);
        Vector3Int gridPosition = map.WorldToCell(newPosition);
        TileBase TileToStepOn = map.GetTile(gridPosition);
        
        if (!paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall && EffectTilemap.GetTile(gridPosition) == null) { return false; }
        else if (EffectTilemap.GetTile(gridPosition) != null)
        {
            TileBase EffectTile = EffectTilemap.GetTile(gridPosition);
            if (dataFromTiles[EffectTile].type == TileData.EffectType.keyLock && HasLevelKey) { return false; }
            if (dataFromTiles[EffectTile].type != TileData.EffectType.keyLock && !paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall) { return false; }
        }



        //Down
        newPosition = new Vector2(currentRootPosition.transform.position.x, currentRootPosition.transform.position.y + -1);
        gridPosition = map.WorldToCell(newPosition);
        TileToStepOn = map.GetTile(gridPosition);
        if (!paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall && EffectTilemap.GetTile(gridPosition) == null) { return false; }
        else if (EffectTilemap.GetTile(gridPosition) != null)
        {
            TileBase EffectTile = EffectTilemap.GetTile(gridPosition);
            if (dataFromTiles[EffectTile].type == TileData.EffectType.keyLock && HasLevelKey) { return false; }
            if (dataFromTiles[EffectTile].type != TileData.EffectType.keyLock && !paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall) { return false; }
        }

        //Left
        newPosition = new Vector2(currentRootPosition.transform.position.x + 1, currentRootPosition.transform.position.y);
        gridPosition = map.WorldToCell(newPosition);
        TileToStepOn = map.GetTile(gridPosition);
        if (!paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall && EffectTilemap.GetTile(gridPosition) == null) { return false; }
        else if (EffectTilemap.GetTile(gridPosition) != null)
        {
            TileBase EffectTile = EffectTilemap.GetTile(gridPosition);
            if (dataFromTiles[EffectTile].type == TileData.EffectType.keyLock && HasLevelKey) { return false; }
            if (dataFromTiles[EffectTile].type != TileData.EffectType.keyLock && !paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall) { return false; }
        }

        //Right
        newPosition = new Vector2(currentRootPosition.transform.position.x - 1, currentRootPosition.transform.position.y);
        gridPosition = map.WorldToCell(newPosition);
        TileToStepOn = map.GetTile(gridPosition);
        if (!paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall && EffectTilemap.GetTile(gridPosition) == null) { return false; }
        else if (EffectTilemap.GetTile(gridPosition) != null)
        {
            TileBase EffectTile = EffectTilemap.GetTile(gridPosition);
            if (dataFromTiles[EffectTile].type == TileData.EffectType.keyLock && HasLevelKey) { return false; }
            if (dataFromTiles[EffectTile].type != TileData.EffectType.keyLock && !paintedTilemap.HasTile(gridPosition) && !dataFromTiles[TileToStepOn].wall) { return false; }
        }

        return true; 


    }

    [ContextMenu("Paint")]
    void TilePaint(Vector3Int positionToPaint, Tile tileToPaint)
    {
        Debug.Log("Paimt!!");
        paintedTilemap.SetTile(positionToPaint, tileToPaint);
    }

    void GetTileAmountSprite()
    {
        TotalTiles = 0;

        // loop through all of the tiles        
        BoundsInt bounds = map.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
     {
            Tile tile = map.GetTile<Tile>(pos);
            if (tile != null)
            {
                if (tile.sprite == transparencySprite)
                {
                    
                }
                else
                {
                    TotalTiles += 1;
                    NutrientsTilemap.SetTile(pos, nutrientTile);
                }
            }
        }

        Debug.Log(TotalTiles);
       
    }

    void findTeleporter(Vector3Int teleporter1Pos, Sprite matchingSprite)
    {
        Debug.Log(teleporter1Pos);
        Vector2 teleporter2Pos;
        BoundsInt bounds = EffectTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = EffectTilemap.GetTile<Tile>(pos);
            if (tile != null)
            {
                Debug.Log("tile not null");
                if (tile.sprite == matchingSprite)
                {
                    Debug.Log(pos);
                    if (pos == teleporter1Pos)
                    {
                       
                    }
                    else
                    {
                        teleporter2Pos = EffectTilemap.CellToWorld(pos);
                       
                        currentRootPosition.transform.position = new Vector2(teleporter2Pos.x + 0.5f, teleporter2Pos.y + 0.5f);
                        Debug.Log("found teleporter");
                        paintedTilemap.SetTile(teleporter1Pos, tileToPaint);
                        TilesPressed += 1;
                    }

                }
                
            }
        }

        bool uhOh = CheckStuck();
        if (uhOh)
        {
            currentRootPosition.transform.position = startPosition;
            paintedTilemap.ClearAllTiles();

            TilesPressed = 1;
            LoadLevel();
        }
    }

    void SaveLevel()
    {
        saveData.Clear();
        BoundsInt bounds = EffectTilemap.cellBounds;
        

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = EffectTilemap.GetTile<Tile>(pos);
            if (tile != null)
            {
                saveData.Add(pos, tile);
            }
        }
    }

    void LoadLevel()
    {
        HasLevelKey = false;
        keyUI.SetActive(false);
        EffectTilemap.ClearAllTiles();
        foreach (KeyValuePair<Vector3Int, Tile> data in saveData)
        {
            Vector3Int gridPos = data.Key;
            Tile tile = data.Value;
            EffectTilemap.SetTile(gridPos, tile);

        }
    }

    void newLevel()
    {
        if (currentLevel >= levels.Length + 1)
        {
            
            mainManagerScript.won = true;
        }
        else
        {
            levels[currentLevel - 2].SetActive(false);
            levels[currentLevel - 1].SetActive(true);
            LevelData levelData = levels[currentLevel - 1].GetComponent<LevelData>();
            map = levelData.dirtMap;
            paintedTilemap = levelData.rootMap;
            NutrientsTilemap = levelData.nutrientMap;
            EffectTilemap = levelData.effectMap;
            MainCamera.orthographicSize = levelData.cameraSize;

            startPosition = levelData.startPos.transform.position;
            currentRootPosition.transform.position = startPosition;
            currentRootPosition.transform.rotation = Quaternion.Euler(0, 0, 0);
            currentDir = "down";

            TilesPressed = 1;
            SaveLevel();
            GetTileAmountSprite();
        }
       

    }

    void CheckTilePaint(Vector3Int pos)
    {

        Debug.Log(currentDir);
        Debug.Log(newDir);
        if ((currentDir == "down" && newDir == "down") || (currentDir == "up" && newDir == "up"))
        {
            TilePaint(pos, tilesToPaint[0]);
        }
        if ((currentDir == "left" && newDir == "left") || (currentDir == "right" && newDir == "right"))
        {
            TilePaint(pos, tilesToPaint[1]);
        }
        if ((currentDir == "up" && newDir == "right") || (currentDir == "left" && newDir == "down"))
        {
            TilePaint(pos, tilesToPaint[2]);
        }
        if ((currentDir == "right" && newDir == "down") || (currentDir == "up" && newDir == "left"))
        {
            TilePaint(pos, tilesToPaint[3]);
        }
        if ((currentDir == "left" && newDir == "up") || (currentDir == "down" && newDir == "right"))
        {
            TilePaint(pos, tilesToPaint[4]);
        }
        if ((currentDir == "right" && newDir == "up") || (currentDir == "down" && newDir == "left"))
        {
            TilePaint(pos, tilesToPaint[5]);
        }

        currentDir = newDir; 
    }
}
