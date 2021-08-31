using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seoner;

public class Tilemap : MonoBehaviour
{
    Grid<TileGridObject> grid;
    public TilemapSO TestLevel;
    private void Start()
    {
        LoadLevel(TestLevel);
    }
    public void LoadLevel(TilemapSO levelData)
    {
        grid = levelData.Grid;

        //Generate tilemap
        GenerateTilemap();
    }

    void GenerateTilemap()
    {
        for (int x = 0; x < grid.GetWidth(); x++)
        {
            for (int y = 0; y < grid.GetHeight(); y++)
            {
                Vector3 worldPos = grid.GetCellCenterWorldPosition(x, y);
                TileGridObject tile = grid.GetGridObject(worldPos);
                if(tile.tileScheme !=null)
                    Instantiate(tile.tileScheme.objectPrefab, worldPos, Quaternion.identity, transform);
               // Instantiate();
            }
        }
    }
}
