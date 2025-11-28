using UnityEngine;
using System.Collections.Generic;

public class ClothSimulation : MonoBehaviour
{
    [Header("Cloth Grid Settings")]
    public int gridWidth = 30;        // Number of particles in X
    public int gridHeight = 30;       // Number of particles in Y
    public float spacing = 0.5f;      // Distance between particles
    public float hangingHeight = 5.0f;

    [Header("Cloth Type")] 
    [Tooltip("Trampoline mode: fix 4 corners instead of top row")]
    public bool isTrampolineMode = false;

    [Tooltip("Horizontal layout (XZ plane) - for trampoline/floor. Vertical (XY plane) - for hanging cloth")]
    public bool isHorizontalLayout = false;

    [Header("Physics Parameters")]
    public float particleMass = 1f;
    public float gravity = -9.8f;
    public float stiffness = 50f;     // Spring stiffness (k)
    public float damping = 0.5f;      // Spring damping
    public float timeStep = 0.01f;    // Simulation timestep (dt)
    public int subSteps = 5;          // Substeps for stability

    [Header("Collision")]
    public float groundY = 0f;        // Ground plane height
    public float restitution = 0.3f;  // Bounciness (kr)

    [Header("Visualization")]
    public bool drawParticles = true;
    public bool drawSprings = true;
    public float particleRadius = 0.1f;
    public Material lineMaterial;
    public bool useMesh = true;
    public Material clothMaterial;


    [Header("Wind")]
    public bool useWindSource = true;            
    public WindSource windSource;               
    [Space]
    public bool useGlobalWind = false;          
    public Vector3 globalWindDirection = new Vector3(1, 0, 0);
    public float globalWindStrength = 5f;
    public float globalWindTurbulence = 2f;
    public float globalWindFrequency = 1f;

    // Internal data
    private List<Particle> particles;
    private List<Spring> springs;
    private List<GameObject> particleObjects;  
    private List<LineRenderer> springLines;
    private Mesh clothMesh;
    private GameObject clothObject;

    void Start()
    {
        InitializeCloth();
    }

    void InitializeCloth()
    {
        particles = new List<Particle>();
        springs = new List<Spring>();
        particleObjects = new List<GameObject>();  
        springLines = new List<LineRenderer>();   

        // Create particle grid
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 pos;

                if (isHorizontalLayout)
                {
                   
                    pos = new Vector3(
                        x * spacing - (gridWidth - 1) * spacing * 0.5f,
                        hangingHeight,
                        y * spacing - (gridHeight - 1) * spacing * 0.5f
                    );
                }

                else
                {
              
                    pos = new Vector3(x * spacing, hangingHeight - y * spacing, 0f);
                }

                // Fix top row particles (pinned)
                bool isFixed;
                if (isTrampolineMode)
                {
                    
                    bool isCorner = (x == 0 || x == gridWidth - 1) &&
                                   (y == 0 || y == gridHeight - 1);
                    isFixed = isCorner;
                }
                else
                {
 
                    isFixed = (y == 0);
                }

                Particle p = new Particle(pos, particleMass, isFixed);
                particles.Add(p);

                if (drawParticles)
                {
                    GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    sphere.transform.position = pos;
                    sphere.transform.localScale = Vector3.one * particleRadius * 2;
                    sphere.GetComponent<Collider>().enabled = false; 

                    
                    Renderer rend = sphere.GetComponent<Renderer>();
                    rend.material.color = isFixed ? Color.red : Color.yellow;

                    particleObjects.Add(sphere);
                }
            }
        }

        // Create springs
        CreateSprings();

        if (drawSprings)
        {
            foreach (var spring in springs)
            {
                GameObject lineObj = new GameObject("SpringLine");
                lineObj.transform.parent = transform;

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.positionCount = 2;
                lr.startWidth = 0.02f;
                lr.endWidth = 0.02f;
                lr.material = lineMaterial;
                lr.startColor = Color.cyan;
                lr.endColor = Color.cyan;

                springLines.Add(lr);
            }
        }
        Debug.Log($"Created {particles.Count} particles and {springs.Count} springs");

        if (useMesh)
        {
            CreateClothMesh();
        }
    }

    void CreateSprings()
    {
        // Helper to get particle index
        int GetIndex(int x, int y) => y * gridWidth + x;

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int idx = GetIndex(x, y);

                // Stretch springs (horizontal and vertical)
                if (x < gridWidth - 1) // Right
                {
                    springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y)], stiffness, damping));
                }
                if (y < gridHeight - 1) // Down
                {
                    springs.Add(new Spring(particles[idx], particles[GetIndex(x, y + 1)], stiffness, damping));
                }

                // Shear springs (diagonal)
                if (x < gridWidth - 1 && y < gridHeight - 1)
                {
                    springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y + 1)], stiffness, damping));
                }
                if (x > 0 && y < gridHeight - 1)
                {
                    springs.Add(new Spring(particles[idx], particles[GetIndex(x - 1, y + 1)], stiffness, damping));
                }

                // Bend springs (skip one vertex)
                if (x < gridWidth - 2) // Right skip
                {
                    springs.Add(new Spring(particles[idx], particles[GetIndex(x + 2, y)], stiffness, damping));
                }
                if (y < gridHeight - 2) // Down skip
                {
                    springs.Add(new Spring(particles[idx], particles[GetIndex(x, y + 2)], stiffness, damping));
                }
            }
        }
    }

    void FixedUpdate()
    {
        float dt = timeStep / subSteps;

        for (int i = 0; i < subSteps; i++)
        {
            SimulationStep(dt);
        }
    }

    void Update()
    {
        UpdateVisualization();
    }

    void UpdateVisualization()
    {
        // Update particle sphere positions
        if (drawParticles && particleObjects.Count > 0)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particleObjects[i].transform.position = particles[i].position;
            }
        }

        // Update spring line positions
        if (drawSprings && springLines.Count > 0)
        {
            for (int i = 0; i < springs.Count; i++)
            {
                springLines[i].SetPosition(0, springs[i].particleA.position);
                springLines[i].SetPosition(1, springs[i].particleB.position);
            }
        }

        if (useMesh && clothMesh != null)
        {
            Vector3[] vertices = clothMesh.vertices;
            for (int i = 0; i < particles.Count; i++)
            {
                vertices[i] = particles[i].position;
            }
            clothMesh.vertices = vertices;
            clothMesh.RecalculateNormals();  // Update lighting
            clothMesh.RecalculateBounds();
        }
    }

    void SimulationStep(float dt)
    {
        // Clear forces
        foreach (var p in particles)
        {
            p.ClearForce();
        }

        // Apply gravity
        foreach (var p in particles)
        {
            if (!p.isFixed)
            {
                p.AddForce(new Vector3(0, gravity * p.mass, 0));
            }
        }


        if (useWindSource && windSource != null)
        {
            // Use local wind source
            foreach (var p in particles)
            {
                if (!p.isFixed)
                {
                    Vector3 windForce = windSource.GetWindForceAt(p.position);
                    p.AddForce(windForce * p.mass);
                }
            }
        }
        else if (useGlobalWind)
        {
            // Use global wind with turbulence
            float time = Time.time;
            foreach (var p in particles)
            {
                if (!p.isFixed)
                {
                    Vector3 wind = globalWindDirection.normalized * globalWindStrength;

                    float noiseX = Mathf.PerlinNoise(time * globalWindFrequency, p.position.y * 0.1f);
                    float noiseY = Mathf.PerlinNoise(p.position.x * 0.1f, time * globalWindFrequency);
                    Vector3 turbulence = new Vector3(noiseX - 0.5f, noiseY - 0.5f, 0) * globalWindTurbulence;

                    p.AddForce((wind + turbulence) * p.mass);
                }
            }
        }



        // Apply spring forces
        foreach (var spring in springs)
        {
            spring.ApplyForce();
        }

        // Semi-Implicit Euler Integration (Slide 25, 31)
        foreach (var p in particles)
        {
            if (!p.isFixed)
            {
                // v(t+1) = v(t) + dt * a(t)
                p.velocity += (p.force / p.mass) * dt;

                // x(t+1) = x(t) + dt * v(t+1)  uses NEW velocity
                p.position += p.velocity * dt;
            }
        }

        // Handle ground collision (Slide 37)
        foreach (var p in particles)
        {
            if (p.position.y < groundY)
            {
                p.position.y = groundY;
                p.velocity.y = -p.velocity.y * restitution; // Bounce
                p.velocity.x *= 0.9f; // Friction
                p.velocity.z *= 0.9f;
            }
        }
    }


    void CreateClothMesh()
    {
        // Create GameObject for cloth
        clothObject = new GameObject("ClothMesh");
        clothObject.transform.parent = transform;

        MeshFilter meshFilter = clothObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = clothObject.AddComponent<MeshRenderer>();

        meshRenderer.material = clothMaterial;

        // Create mesh
        clothMesh = new Mesh();
        clothMesh.name = "ClothMesh";

        // Generate vertices 
        Vector3[] vertices = new Vector3[particles.Count];
        for (int i = 0; i < particles.Count; i++)
        {
            vertices[i] = particles[i].position;
        }

        // Generate triangles
        List<int> triangles = new List<int>();

        for (int y = 0; y < gridHeight - 1; y++)
        {
            for (int x = 0; x < gridWidth - 1; x++)
            {
                int topLeft = y * gridWidth + x;
                int topRight = topLeft + 1;
                int bottomLeft = (y + 1) * gridWidth + x;
                int bottomRight = bottomLeft + 1;

                // First triangle 
                triangles.Add(topLeft);
                triangles.Add(topRight);
                triangles.Add(bottomLeft);

                // Second triangle (top-right, bottom-left, bottom-right)
                triangles.Add(topRight);
                triangles.Add(bottomRight);
                triangles.Add(bottomLeft);
            }
        }

        // Generate UVs (for textures)
        Vector2[] uvs = new Vector2[particles.Count];
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int idx = y * gridWidth + x;
                uvs[idx] = new Vector2((float)x / (gridWidth - 1), (float)y / (gridHeight - 1));
            }
        }

        clothMesh.vertices = vertices;
        clothMesh.triangles = triangles.ToArray();
        clothMesh.uv = uvs;
        clothMesh.RecalculateNormals();  // Important for lighting!
        clothMesh.RecalculateBounds();

        meshFilter.mesh = clothMesh;

        Debug.Log($"Created mesh with {vertices.Length} vertices and {triangles.Count / 3} triangles");
    }


    // TODO: used for cloth-jelly collision (currently disabled due to performance)
    public List<Particle> GetParticles()
    {
        return particles;
    }


    void OnDestroy()
    {
        foreach (var obj in particleObjects)
        {
            if (obj != null) Destroy(obj);
        }

        foreach (var line in springLines)
        {
            if (line != null) Destroy(line.gameObject);
        }

        if (clothObject != null) Destroy(clothObject);
        if (clothMesh != null) Destroy(clothMesh);
    }

    void OnDrawGizmos()
    {
        if (particles == null) return;

        // Draw springs
        if (drawSprings)
        {
            Gizmos.color = Color.cyan;
            foreach (var spring in springs)
            {
                Gizmos.DrawLine(spring.particleA.position, spring.particleB.position);
            }
        }

        // Draw particles
        if (drawParticles)
        {
            foreach (var p in particles)
            {
                Gizmos.color = p.isFixed ? Color.red : Color.yellow;
                Gizmos.DrawSphere(p.position, particleRadius);
            }
        }
    }
}