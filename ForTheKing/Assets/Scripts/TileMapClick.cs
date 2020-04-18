using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapClick : MonoBehaviour
{

    public Tilemap tilemap;

    private void OnMouseDown()
    {
        Vector3Int tilePos = tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        GameObject.Find("BattleManager").GetComponent<BattleUI>().onTileClick(tilePos.x, tilePos.y);
    }

}
