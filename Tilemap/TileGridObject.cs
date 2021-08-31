using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Seoner;

[System.Serializable]
public class TileGridObject 
{
    private Grid<TileGridObject> grid;
    public int x;
    public int y;
    public TileGridObject(Grid<TileGridObject> grid, int x,int y)
    {
        this.x = x;
        this.y = y;
        this.grid = grid;
    }

    public TileSO tileScheme;
}
