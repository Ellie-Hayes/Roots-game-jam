using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelData : MonoBehaviour
{
    public Tilemap rootMap;
    public Tilemap dirtMap;
    public Tilemap nutrientMap;
    public Tilemap effectMap;

    public GameObject startPos;
    public int cameraSize;

}
