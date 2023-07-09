using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class TerrainGenerator : MonoBehaviour
{
    [Range(2,10)] public int nPower;
    public float terrainHeight;
    public float roughness;

    public Renderer textureRender;

    public bool autoUpdate;


    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMap()
    {
        Texture2D texture = TextureFromHeightMap(DiamondSquare());
        textureRender.sharedMaterial.mainTexture = texture;
        textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public float[,] DiamondSquare()
    {
        int gridSize = (int)(Mathf.Pow(2, nPower) + 1);
        float _roughness = roughness;
        float[,] terrainGrid = new float[gridSize, gridSize];

        // Set initial corner heights
        terrainGrid[0, 0] = Random.Range(0f, terrainHeight);
        terrainGrid[0, gridSize - 1] = Random.Range(0f, terrainHeight);
        terrainGrid[gridSize - 1, 0] = Random.Range(0f, terrainHeight);
        terrainGrid[gridSize - 1, gridSize - 1] = Random.Range(0f, terrainHeight);

        int stepSize = gridSize - 1;

        while (stepSize > 1)
        {
            int halfStep = stepSize / 2;

            // Diamond step
            for (int row = 0; row < gridSize - 1; row += stepSize)
            {
                for (int col = 0; col < gridSize - 1; col += stepSize)
                {
                    int centerX = row + halfStep;
                    int centerY = col + halfStep;

                    int left = (col - halfStep + gridSize) % gridSize;
                    int right = (col + halfStep) % gridSize;
                    int top = (row - halfStep + gridSize) % gridSize;
                    int bottom = (row + halfStep) % gridSize;

                    float averageHeight = (terrainGrid[row, col] + terrainGrid[row, right] +
                                           terrainGrid[bottom, col] + terrainGrid[bottom, right]) / 4f;

                    float randomOffset = Random.Range(-_roughness * halfStep, _roughness * halfStep);
                    terrainGrid[centerX, centerY] = averageHeight + randomOffset;
                }
            }

            // Square step
            for (int row = 0; row < gridSize; row += halfStep)
            {
                for (int col = 0; col < gridSize; col += halfStep)
                {
                    int left = (col - halfStep + gridSize) % gridSize;
                    int right = (col + halfStep) % gridSize;
                    int top = (row - halfStep + gridSize) % gridSize;
                    int bottom = (row + halfStep) % gridSize;

                    float averageHeight = (terrainGrid[row, left] + terrainGrid[row, right] +
                                           terrainGrid[top, col] + terrainGrid[bottom, col]) / 4f;

                    float randomOffset = Random.Range(-_roughness * halfStep, _roughness * halfStep);
                    terrainGrid[row, col] = averageHeight + randomOffset;
                }
            }

            stepSize = halfStep;
            _roughness *= 0.5f;
        }

        return terrainGrid;
    }
}

[CustomEditor(typeof(TerrainGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator mapGenerator = (TerrainGenerator)target;

        if (DrawDefaultInspector())
        {
            if (mapGenerator.autoUpdate)
                mapGenerator.GenerateMap();
        }

        if (GUILayout.Button("Generate"))
        {
            mapGenerator.GenerateMap();
        }

    }
}