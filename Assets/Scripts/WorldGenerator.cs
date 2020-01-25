using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    public Transform world;
    public Transform player;

    public Material worldMaterial;

    void Start()
    {
        for(int i = 0; i < 10; i++)
            createChunk(new Vector3(0, 0, i * 50));
    }

    void Update()
    {

    }

    void createChunk(Vector3 position)
    {
        GameObject worldChunk = new GameObject();
        worldChunk.transform.parent = world;
        worldChunk.name = "WorldChunk";
        worldChunk.transform.position = position;

        MeshRenderer renderer = worldChunk.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = worldMaterial;

        MeshFilter filter = worldChunk.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();

        Vector2Int dimensions = new Vector2Int(100, 100);
        Vector2 size = new Vector2(50, 50);

        Vector3[] verticies = new Vector3[(dimensions.x + 1) * (dimensions.y + 1)];
        int[] triangles = new int[dimensions.x * dimensions.y * 6];
        Vector2[] uv = new Vector2[verticies.Length];

        //verticies
        for(int i = 0, x = 0; x < dimensions.x + 1; x++)
        {
            for(int z = 0; z < dimensions.y + 1; z++)
            {
                float scale = 3.5f;
                float xpos = position.x + (float)x / dimensions.x * size.x;
                float zpos = position.z + (float)z / dimensions.y * size.y;
                float height = Mathf.PerlinNoise(xpos / dimensions.x * scale, zpos / dimensions.y * scale);
                verticies[i] = new Vector3((float)x / dimensions.x * size.x, height * 5f + Mathf.Pow(x - dimensions.x / 2, 2) / 250f, (float)z / dimensions.y * size.y);
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
    }
}