using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Seoner;
public class TilemapEditor : EditorWindow
{
    public TilePaletteSO Palette;


    private int selectedTileIndex;
    private GameObject previewParentObject;
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private Seoner.Grid<TileGridObject> grid;
    private GameObject[,] previewGOArray;

    string levelName;
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
    string rebuildGridButton = "Rebuild grid";
    bool showTilemapPreview;

    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Tilemap data editor")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(TilemapEditor));
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        if(previewParentObject == null)
        {
            previewParentObject = new GameObject("TilemapPreview");
        }
        if(grid != null)
        {
            //Rebuild tile preview
            for (int x = 0; x < grid.GetWidth(); x++)
            {
                for (int y = 0; y < grid.GetHeight(); y++)
                {
                    TileGridObject t = grid.GetGridObject(x, y);
                    if (t.tileScheme.objectPrefab != null)
                    {
                        CreatePreviewTile(x, y, t.tileScheme.objectPrefab);
                    }
                }
            }
        }
    }
    private void OnDisable()
    {
        if(previewParentObject != null)
        {
            DestroyImmediate(previewParentObject);
        }
    }
    private void OnFocus()
    {
        
    }
    private void OnLostFocus()
    {
        
    }
    void OnGUI()
    {
        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        levelName = EditorGUILayout.TextField("Level name: ", levelName);

        width = EditorGUILayout.IntField("Grid width: ", width);
        height = EditorGUILayout.IntField("Grid height: ", height);
        cellSize = EditorGUILayout.FloatField("Cell size: ", cellSize);
        
        


        //Load palette
        Palette = (TilePaletteSO)EditorGUILayout.ObjectField("Palette: ", Palette, typeof(TilePaletteSO), true);

        if (Palette == null)
            return;
        
        originPosition = EditorGUILayout.Vector3Field("Grid origin position: ", originPosition);

        selectedTileIndex = EditorGUILayout.Popup("Selected Tile: ", selectedTileIndex, GetTilesNames());
        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        showTilemapPreview = EditorGUILayout.Toggle("Show Tilemap Preview", showTilemapPreview);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();

        if(GUILayout.Button("Clean grid"))
        {
            DestroyPreviewTiles();
        }

        if (GUILayout.Button(rebuildGridButton))
        {
            RebuildGrid();
        }
        if(GUILayout.Button("Save tilemap data"))
        {
            TilemapDataCreator.CreateTilemapData(grid, levelName);
        }

        if (showTilemapPreview)
        {
            previewParentObject?.SetActive(true);
        }
        else
        {
            previewParentObject?.SetActive(false);
        }
        

    }
    private void RebuildGrid()
    {
        //DestroyPreviewTiles();
        //Create mockup-grid 
        grid = new Grid<TileGridObject>(width, height, cellSize, originPosition, (Grid<TileGridObject> g, int x, int y) => new TileGridObject(g, x, y));
        previewGOArray = new GameObject[grid.GetWidth(), grid.GetHeight()];
    }

    private void DestroyPreviewTiles()
    {
        for (int i = 0; i < previewParentObject.transform.childCount; i++)
        {
            DestroyImmediate(previewParentObject.transform.GetChild(i).gameObject);
        }
    }

    private string[] GetTilesNames()
    {
        string[] result = new string[Palette.tiles.Count];

        for (int i = 0; i < Palette.tiles.Count; i++)
        {
            result[i] = Palette.tiles[i].name;
        }

        return result;
    }

    private void OnSceneGUI(SceneView sceneView)
    {

        if(Event.current.type == EventType.MouseDown)
        {
            if (grid == null)
            {
                Debug.LogWarning("Rebuild grid first before placing tiles!");
                return;
            }
            if(Palette == null)
            {
                return;
            }

            Vector3 mousePosition = Event.current.mousePosition;
            mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;
            mousePosition = sceneView.camera.ScreenToWorldPoint(mousePosition);
            //mousePosition.y = -mousePosition.y;
            TileGridObject selectedTile = grid.GetGridObject(mousePosition);
            Debug.Log("mouse pos: x: " + mousePosition.x + " y: " + mousePosition.y);
            if (selectedTile != null)
            {
                Debug.Log("selected tile: X: " + selectedTile.x + " Y: " + selectedTile.y);
                //Vector3 worldPos = new Vector3(selectedTile.x, selectedTile.y) + new Vector3(cellSize, cellSize) * 0.5f;
                //
                //Vector3 worldPos = grid.GetCellCenterWorldPosition(selectedTile.x, selectedTile.y);
                //grid.GetCellCenterWorldPosition(selectedTile.x,selectedTile.y);
                //if tile already exist, clean it and replace
                if (selectedTile.tileScheme != null)
                {
                    //destroy last tile
                    DestroyImmediate(previewGOArray[selectedTile.x,selectedTile.y]);
                }
                TileGridObject newTile = new TileGridObject(grid, selectedTile.x, selectedTile.y);
                CreatePreviewTile(selectedTile.x, selectedTile.y, Palette.tiles[selectedTileIndex].objectPrefab);
                newTile.tileScheme = Palette.tiles[selectedTileIndex];
                Debug.Log("Tile {"+selectedTile.x+","+selectedTile.y+"} set to: " + newTile.tileScheme.name);
                //Palette.tiles[selectedTileIndex];
                grid.SetGridObject(selectedTile.x, selectedTile.y, newTile);
                Debug.Log("x: " + selectedTile.x + " y: " + selectedTile.y);

            }
        }
    }
    private void CreatePreviewTile(int x, int y, GameObject objectPrefab)
    {
        Vector3 worldPos = grid.GetCellCenterWorldPosition(x, y);
        previewGOArray[x, y] = Instantiate(objectPrefab, worldPos, Quaternion.identity, previewParentObject.transform);
    }

}

public static class TilemapDataCreator
{
    public static void CreateTilemapData(Grid<TileGridObject> grid, string assetName)
    {
        TilemapSO tilemapData = ScriptableObject.CreateInstance<TilemapSO>();
        tilemapData.Grid = (Grid<TileGridObject>)grid.Clone();
        AssetDatabase.CreateAsset(tilemapData, "Assets/Data/Levels/"+assetName+".asset");
        AssetDatabase.SaveAssets();
    }
}
