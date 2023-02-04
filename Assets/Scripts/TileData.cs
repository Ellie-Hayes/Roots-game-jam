using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class TileData : ScriptableObject
{
    public TileBase[] tiles;
    public bool wall;

    public enum EffectType { key, keyLock, teleporter, pushPad, none };
    public EffectType type;

    public enum PushType { allDir, left, right, up, down, none };
    public PushType pushType;

}
