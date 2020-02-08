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
        worldChunk.name = "WorldChunk";
        worldChunk.transform.position = position;
        worldChunk.transform.parent = world;

        GameObject bottom = createChunkMeshObject(position, false);
        bottom.transform.parent = worldChunk.transform;
        GameObject top = createChunkMeshObject(position + new Vector3(0, 10f, 0), true);
        top.transform.parent = worldChunk.transform;

        //place obstacles
        for (int i = 0; i < 20; i++)
        {
            Vector2 perlinPos = new Vector2(position.x + Random.value * size.x, position.z + Random.value * size.y);
            Vector3 obPos = new Vector3(perlinPos.x, getPerlinHeight(perlinPos), perlinPos.y);
            GameObject obstacle = Instantiate(obstaclePrefab, obPos, Quaternion.identity, worldChunk.transform);
            //obstacle.transform.eulerAngles = new Vector3(Random.value * 90f - 45f, Random.value * 360f, Random.value * 90f - 45f);
            obstacle.name = "obstacle";

            RaycastHit hit;
            if (Physics.Raycast(obstacle.transform.position + new Vector3(0, 5, 0), Vector3.down, out hit, Mathf.Infinity))
            {
                obstacle.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                obstacle.transform.eulerAngles += new Vector3(Random.value * 60 - 30f, 0, Random.value * 60f - 30f);
            }
        }

        return worldChunk;
    }

    GameObject createChunkMeshObject(Vector3 position, bool flip)
    {
        GameObject chunkMeshObject = new GameObject();
        chunkMeshObject.name = "WorldChunk";
        chunkMeshObject.transform.position = position;

        MeshCollider collider = chunkMeshObject.AddComponent<MeshCollider>();

        MeshRenderer renderer = chunkMeshObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = worldMaterial;

        MeshFilter filter = chunkMeshObject.AddComponent<MeshFilter>();

        Mesh mesh = createChunkMesh(position, flip);

        filter.sharedMesh = mesh;
        collider.sharedMesh = mesh;

        return chunkMeshObject;
    }

    float getPerlinHeight(Vector2 position)
    {
        return getPerlinHeight(position, 0.035f, 5f) + getPerlinHeight(position, 0.005f, 75f);
    }

    float getPerlinHeight(Vector2 position, float perlinScale, float heightScale)
    {
        float x = position.x;
        float z = position.y;

        Vector2 perlinOffset = new Vector2(10000, 10000);

        x += perlinOffset.x;
        z += perlinOffset.y;

        x *= perlinScale;
        z *= perlinScale;

        float height = Mathf.PerlinNoise(x, z);
        height *= heightScale;
        return height;
    }

    Mesh createChunkMesh(Vector3 position, bool flip)
    {
        Mesh mesh = new Mesh();

        Vector3[] verticies = new Vector3[(dimensions.x + 1) * (dimensions.y + 1)];
        int[] triangles = new int[dimensions.x * dimensions.y * 6];
        Vector2[] uv = new Vector2[verticies.Length];

        //verticies
        for (int i = 0, x = 0; x < dimensions.x + 1; x++)
        {
            for (int z = 0; z < dimensions.y + 1; z++)
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
                triangles[squareIndex + 1 + (!flip ? 0 : 1)] = selectPositionInZRow + 1;
                triangles[squareIndex + 2 - (!flip ? 0 : 1)] = selectPositionInZRow + zRowLength + 1;

                //create triangle 2 of square
                triangles[squareIndex + 3] = selectPositionInZRow;
                triangles[squareIndex + 4 + (!flip ? 0 : 1)] = selectPositionInZRow + zRowLength + 1;
                triangles[squareIndex + 5 - (!flip ? 0 : 1)] = selectPositionInZRow + zRowLength;

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