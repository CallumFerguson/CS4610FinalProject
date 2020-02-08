using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Transform world;
    public Transform player;

    public GameObject obstaclePrefab;

    public Material worldMaterial;

    Vector2Int dimensions;
    Vector2 size;

    Dictionary<Vector2Int, GameObject> chunks;
    int chunkRadius = 5;
    Vector2Int lastChunkPos;

    void Awake()
    {
        dimensions = new Vector2Int(25, 25);
        size = new Vector2(50, 50);
        lastChunkPos = getChunkPos();

        chunks = new Dictionary<Vector2Int, GameObject>();

        createChunks(getChunkPos());

        //createChunkObject(Vector3.zero);
    }

    void Update()
    {
        Vector2Int currentChunkPos = getChunkPos();

        if(currentChunkPos != lastChunkPos)
        {
            createChunks(currentChunkPos);
        }

        lastChunkPos = currentChunkPos;
    }

    Vector2Int getChunkPos()
    {
        return new Vector2Int(Mathf.FloorToInt(player.transform.position.x / size.x), Mathf.FloorToInt(player.transform.position.z / size.y));
    }

    void createChunks(Vector2Int currentChunkPos)
    {
        HashSet<Vector2Int> chunksToRemove = new HashSet<Vector2Int>(chunks.Keys);
        for (int x = currentChunkPos.x - chunkRadius; x < currentChunkPos.x + chunkRadius; x++)
        {
            for (int z = currentChunkPos.y - chunkRadius; z < currentChunkPos.y + chunkRadius; z++)
            {
                Vector2Int chunkPos = new Vector2Int(x, z);
                chunksToRemove.Remove(chunkPos);
                if (!chunks.ContainsKey(chunkPos))
                    chunks.Add(chunkPos, createChunkObject(new Vector3(x * size.x, 0, z * size.y)));
            }
        }

        foreach(var chunkPos in chunksToRemove)
        {
            Destroy(chunks[chunkPos]);
            chunks.Remove(chunkPos);
        }
    }

    GameObject createChunkObject(Vector3 position)
    {
        GameObject worldChunk = new GameObject();
        worldChunk.transform.parent = world;
        worldChunk.name = "WorldChunk";
        worldChunk.transform.position = position;

        MeshCollider collider = worldChunk.AddComponent<MeshCollider>();

        MeshRenderer renderer = worldChunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = worldMaterial;

        MeshFilter filter = worldChunk.AddComponent<MeshFilter>();

        Mesh mesh = createChunkMesh(position);

        filter.sharedMesh = mesh;
        collider.sharedMesh = mesh;

        //place obstacles
        for(int i = 0; i < 20; i++)
        {
            Vector2 perlinPos = new Vector2(position.x + Random.value * size.x, position.z + Random.value * size.y);
            Vector3 obPos = new Vector3(perlinPos.x, getPerlinHeight(perlinPos), perlinPos.y);
            GameObject obstacle = Instantiate(obstaclePrefab, obPos, Quaternion.identity, worldChunk.transform);
            obstacle.name = "obstacle";
        }

        return worldChunk;
    }

    float getPerlinHeight(Vector2 position)
    {
        float x = position.x;
        float z = position.y;

        Vector2 perlinOffset = new Vector2(10000, 10000);
        float perlinScale = 0.035f;
        float heightScale = 5f;

        x += perlinOffset.x;
        z += perlinOffset.y;

        x *= perlinScale;
        z *= perlinScale;

        float height = Mathf.PerlinNoise(x, z);
        height *= heightScale;
        return height;
    }

    Mesh createChunkMesh(Vector3 position)
    {
        Mesh mesh = new Mesh();

        Vector3[] verticies = new Vector3[(dimensions.x + 1) * (dimensions.y + 1)];
        int[] triangles = new int[dimensions.x * dimensions.y * 6];
        Vector2[] uv = new Vector2[verticies.Length];

        //verticies
        for(int i = 0, x = 0; x < dimensions.x + 1; x++)
        {
            for(int z = 0; z < dimensions.y + 1; z++)
            {
                float xpos = position.x + (float)x / dimensions.x * size.x;
                float zpos = position.z + (float)z / dimensions.y * size.y;
                float height = getPerlinHeight(new Vector2(xpos, zpos));
                verticies[i] = new Vector3((float)x / dimensions.x * size.x, height, (float)z / dimensions.y * size.y);
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

        return mesh;
    }
}