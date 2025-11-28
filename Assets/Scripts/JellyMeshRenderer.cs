using UnityEngine;
using System.Collections.Generic;

public class JellyMeshRenderer : MonoBehaviour
{
    [Header("References")]
    public JellySimulation jellySimulation;
    public Material jellyMaterial;

    [Header("Mesh Settings")]
    public bool smoothNormals = true;

    // Internal
    private GameObject meshObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh jellyMesh;

    private List<Particle> particles;
    private int gridX, gridY, gridZ;

    public void ForceInitialize()
    {
        if (jellySimulation == null)
        {
            Debug.LogError("JellyMeshRenderer: jellySimulation is NULL!");
            return;
        }

        CreateMeshObject();
        GenerateMesh();
    }

    void CreateMeshObject()
    {
        meshObject = new GameObject("JellyMesh");
        meshObject.transform.parent = transform;
        meshObject.transform.localPosition = Vector3.zero;

        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer = meshObject.AddComponent<MeshRenderer>();

        if (jellyMaterial != null)
        {
            meshRenderer.material = jellyMaterial;
        }

        jellyMesh = new Mesh();
        jellyMesh.name = "JellyMesh";
        meshFilter.mesh = jellyMesh;
    }

    void GenerateMesh()
    {

        if (jellySimulation == null)
        {
            Debug.LogError("JellyMeshRenderer: jellySimulation is NULL!");
            return;
        }

        particles = jellySimulation.GetParticles();

        if (particles == null)
        {
            Debug.LogError("JellyMeshRenderer: GetParticles() returned NULL!");
            return;
        }

        if (particles.Count == 0)
        {
            Debug.LogError("JellyMeshRenderer: particles list is empty!");
            return;
        }

        particles = jellySimulation.GetParticles();
        gridX = jellySimulation.gridSizeX;
        gridY = jellySimulation.gridSizeY;
        gridZ = jellySimulation.gridSizeZ;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Generate 6 faces of the cube

        // Front face (Z = 0)
        AddFace(vertices, triangles, uvs, 0, 0, 0, 1, 0, 0, 0, 1, 0);

        // Back face (Z = max)
        AddFace(vertices, triangles, uvs, 0, 0, gridZ - 1, 1, 0, 0, 0, 1, 0);

        // Left face (X = 0)
        AddFace(vertices, triangles, uvs, 0, 0, 0, 0, 0, 1, 0, 1, 0);

        // Right face (X = max)
        AddFace(vertices, triangles, uvs, gridX - 1, 0, 0, 0, 0, 1, 0, 1, 0);

        // Bottom face (Y = 0)
        AddFace(vertices, triangles, uvs, 0, 0, 0, 1, 0, 0, 0, 0, 1);

        // Top face (Y = max)
        AddFace(vertices, triangles, uvs, 0, gridY - 1, 0, 1, 0, 0, 0, 0, 1);

        jellyMesh.vertices = vertices.ToArray();
        jellyMesh.triangles = triangles.ToArray();
        jellyMesh.uv = uvs.ToArray();
        jellyMesh.RecalculateNormals();
        jellyMesh.RecalculateBounds();

        Debug.Log($"Generated Jelly Mesh: {vertices.Count} vertices, {triangles.Count / 3} triangles");
    }

    void AddFace(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs,
                 int startX, int startY, int startZ,
                 int dirX, int dirY, int dirZ,
                 int perpX, int perpY, int perpZ)
    {
        int gridU = (dirX != 0) ? gridX : (dirZ != 0) ? gridZ : gridY;
        int gridV = (perpX != 0) ? gridX : (perpZ != 0) ? gridZ : gridY;

        int baseVertexIndex = vertices.Count;

        // Add vertices for this face
        for (int v = 0; v < gridV; v++)
        {
            for (int u = 0; u < gridU; u++)
            {
                int x = startX + u * dirX + v * perpX;
                int y = startY + u * dirY + v * perpY;
                int z = startZ + u * dirZ + v * perpZ;

                int particleIndex = GetParticleIndex(x, y, z);
                Vector3 worldPos = particles[particleIndex].position;
                Vector3 localPos = transform.InverseTransformPoint(worldPos);
                vertices.Add(localPos);

                // UV coordinates
                uvs.Add(new Vector2((float)u / (gridU - 1), (float)v / (gridV - 1)));
            }
        }

        // Add triangles for this face
        for (int v = 0; v < gridV - 1; v++)
        {
            for (int u = 0; u < gridU - 1; u++)
            {
                int i0 = baseVertexIndex + v * gridU + u;
                int i1 = i0 + 1;
                int i2 = i0 + gridU;
                int i3 = i2 + 1;

                // Triangle 1
                triangles.Add(i0);
                triangles.Add(i2);
                triangles.Add(i1);

                // Triangle 2
                triangles.Add(i1);
                triangles.Add(i2);
                triangles.Add(i3);
            }
        }
    }

    int GetParticleIndex(int x, int y, int z)
    {
        return z * (gridX * gridY) + y * gridX + x;
    }

    void Update()
    {
        UpdateMesh();
    }

    void UpdateMesh()
    {
       
        if (jellyMesh == null) return;

        if (jellySimulation != null)
        {
            particles = jellySimulation.GetParticles();
        }

        if (particles == null || particles.Count == 0) return;

        Vector3[] vertices = jellyMesh.vertices;

        int vertexIndex = 0;

        // Update all 6 faces
        UpdateFaceVertices(ref vertexIndex, vertices, 0, 0, 0, 1, 0, 0, 0, 1, 0);
        UpdateFaceVertices(ref vertexIndex, vertices, 0, 0, gridZ - 1, 1, 0, 0, 0, 1, 0);
        UpdateFaceVertices(ref vertexIndex, vertices, 0, 0, 0, 0, 0, 1, 0, 1, 0);
        UpdateFaceVertices(ref vertexIndex, vertices, gridX - 1, 0, 0, 0, 0, 1, 0, 1, 0);
        UpdateFaceVertices(ref vertexIndex, vertices, 0, 0, 0, 1, 0, 0, 0, 0, 1);
        UpdateFaceVertices(ref vertexIndex, vertices, 0, gridY - 1, 0, 1, 0, 0, 0, 0, 1);

        jellyMesh.vertices = vertices;

        if (smoothNormals)
        {
            jellyMesh.RecalculateNormals();
        }

        jellyMesh.RecalculateBounds();
    }

    void UpdateFaceVertices(ref int vertexIndex, Vector3[] vertices,
                           int startX, int startY, int startZ,
                           int dirX, int dirY, int dirZ,
                           int perpX, int perpY, int perpZ)
    {
        int gridU = (dirX != 0) ? gridX : (dirZ != 0) ? gridZ : gridY;
        int gridV = (perpX != 0) ? gridX : (perpZ != 0) ? gridZ : gridY;

        for (int v = 0; v < gridV; v++)
        {
            for (int u = 0; u < gridU; u++)
            {
                int x = startX + u * dirX + v * perpX;
                int y = startY + u * dirY + v * perpY;
                int z = startZ + u * dirZ + v * perpZ;

                int particleIndex = GetParticleIndex(x, y, z);
                Vector3 worldPos = particles[particleIndex].position;
                Vector3 localPos = transform.InverseTransformPoint(worldPos);
                vertices[vertexIndex] = localPos;
                vertexIndex++;
            }
        }
    }

    void OnDestroy()
    {
        if (meshObject != null)
        {
            Destroy(meshObject);
        }
        if (jellyMesh != null)
        {
            Destroy(jellyMesh);
        }
    }
}