using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Transform world;
    public Transform player;

    public Material worldMaterial;

    Vector2Int dimensions;
    Vector2 size;
    Vector3 position;

    int lastSegment;

    Queue<GameObject> chunks;

    int numChunks = 10;

    void Awake()
    {
        dimensions = new Vector2Int(100, 100);
        size = new Vector2(50, 50);
        position = new Vector3(0, 0, 0);

        lastSegment = 0;

        chunks = new Queue<GameObject>();

        for (int i = 0; i < numChunks; i++)
            createNextChunk();
    }

    void Update()
    {
        int currentSegment = Mathf.FloorToInt(player.position.z / size.y);
        if(currentSegment != lastSegment)
        {
            if (chunks.Count > numChunks)
            {
                GameObject oldestChunk = chunks.Dequeue();
                Destroy(oldestChunk);
            }
            createNextChunk();
        }
        lastSegment = currentSegment;
    }

    void createNextChunk()
    {
        GameObject worldChunk = new GameObject();
        chunks.Enqueue(worldChunk);
        worldChunk.transform.parent = world;
        worldChunk.name = "WorldChunk";
        worldChunk.transform.position = position;

        MeshCollider collider = worldChunk.AddComponent<MeshCollider>();

        MeshRenderer renderer = worldChunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = worldMaterial;

        MeshFilter filter = worldChunk.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        position += new Vector3(0, 0, size.y);

        Vector3[] verticies = new Vector3[(dimensions.x + 1) * (dimensions.y + 1)];
        int[] triangles = new int[dimensions.x * dimensions.y * 6];
        Vector2[] uv = new Vector2[verticies.Length];

        //verticies
        for(int i = 0, x = 0; x < dimensions.x + 1; x++)
        {
            for(int z = 0; z < dimensions.y + 1; z++)
            {
                float perlinScale = 3.5f;
                float heightScale = 15f;
                float xpos = position.x + (float)x / dimensions.x * size.x;
                float zpos = position.z + (float)z / dimensions.y * size.y;
                float height = Mathf.PerlinNoise(xpos / dimensions.x * perlinScale, zpos / dimensions.y * perlinScale);
                verticies[i] = new Vector3((float)x / dimensions.x * size.x, height * heightScale + Mathf.Pow(x - dimensions.x / 2, 2) / 250f, (float)z / dimensions.y * size.y);
                uv[i] = new Vector2((float)x / dimensions.x, (float)z / dimensions.y);
                i++;
            }
        }

        //triangles
        for (int i = 0, x = 0; x < dimensions.x; x++)
        {
            for (int z = 0; z < dimensions.y; z++)
            {
                //create square:

                int zRowLength = dimensions.y + 1;
                int selectZRowIndex = x * zRowLength;
                int selectPositionInZRow = selectZRowIndex + z;

                //create triangle 1 of square
                int squareIndex = i * 6;
                triangles[squareIndex] = selectPositionInZRow;
                triangles[squareIndex + 1] = selectPositionInZRow + 1;
                triangles[squareIndex + 2] = selectPositionInZRow + zRowLength + 1;
                
                //create triangle 2 of square
                triangles[squareIndex + 3] = selectPositionInZRow;
                triangles[squareIndex + 4] = selectPositionInZRow + zRowLength + 1;
                triangles[squareIndex + 5] = selectPositionInZRow + zRowLength;

                i++;
            }
        }

        mesh.vertices = verticies;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;
        collider.sharedMesh = mesh;
    }
}