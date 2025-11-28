using UnityEngine;
using System.Collections.Generic;

public class JellySimulation : MonoBehaviour
{
    [Header("Jelly Cube Settings")]
    public int gridSizeX = 5;         
    public int gridSizeY = 5;        
    public int gridSizeZ = 5;        
    public float spacing = 0.5f;      

    [Header("Physics Parameters")]
    public float particleMass = 1f;
    public float gravity = -9.8f;
    public float stiffness = 100f;    
    public float damping = 1f;
    public float timeStep = 0.01f;
    public int subSteps = 10;

    [Header("Collision - Ground")]
    public float groundY = 0f;
    public float restitution = 0.5f;

    [Header("Collision - Environment")]
    [Tooltip("Enable collision with walls and other colliders")]
    public bool useEnvironmentCollision = true;

    [Tooltip("Layers that jelly particles will collide with")]
    public LayerMask collisionLayers = -1;
    public float collisionSkinWidth = 0.02f;
    public float surfaceFriction = 0.1f;

    [Header("Lifetime")]
    [Tooltip("Automatically destroy this jelly after X seconds")]
    public bool autoDespawn = true;
    public float despawnAfterSeconds = 10f;

    [Header("Wind")]
    public bool useWindSource = false;
    public WindSource windSource;

    [Header("Visualization")]
    public bool useMesh = true;
    public bool drawParticles = false;
    public bool drawSprings = false;
    public float particleRadius = 0.1f;
    public Material particleMaterial;

    // Internal data
    private List<Particle> particles;
    private List<Spring> springs;
    private List<GameObject> particleObjects;
    private float spawnTime;
    private CollisionManager collisionManager;

    void Start()
    {
        spawnTime = Time.time;
        collisionManager = FindObjectOfType<CollisionManager>();
        InitializeJelly();
    }

    void InitializeJelly()
    {
        particles = new List<Particle>();
        springs = new List<Spring>();
        particleObjects = new List<GameObject>();

        Vector3 centerOffset = new Vector3(
            -(gridSizeX - 1) * spacing * 0.5f,
            -(gridSizeY - 1) * spacing * 0.5f,
            -(gridSizeZ - 1) * spacing * 0.5f
        );

        // Create 3D particle grid
        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    Vector3 localOffset = new Vector3(
                        x * spacing,
                        y * spacing,
                        z * spacing
                     );

                    Vector3 worldPos = transform.position + centerOffset + localOffset;

                    Particle p = new Particle(worldPos, particleMass, false);
                    particles.Add(p);

                    // Visualization
                    if (drawParticles)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.position = worldPos;
                        sphere.transform.localScale = Vector3.one * particleRadius * 2;
                        sphere.GetComponent<Collider>().enabled = false;

                        if (particleMaterial != null)
                        {
                            sphere.GetComponent<Renderer>().material = particleMaterial;
                        }

                        particleObjects.Add(sphere);
                    }
                }
            }
        }

        // Create springs
        CreateSprings();

        Debug.Log($"Created Jelly: {particles.Count} particles, {springs.Count} springs");

        if (useMesh)
        {
            InitializeMeshRenderer();
        }
    }

    void CreateSprings()
    {
        int GetIndex(int x, int y, int z)
        {
            return z * (gridSizeX * gridSizeY) + y * gridSizeX + x;
        }

        for (int z = 0; z < gridSizeZ; z++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    int idx = GetIndex(x, y, z);

                    // Structural springs 

                    // +X direction
                    if (x < gridSizeX - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y, z)], stiffness, damping));
                    }

                    // +Y direction
                    if (y < gridSizeY - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x, y + 1, z)], stiffness, damping));
                    }

                    // +Z direction
                    if (z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x, y, z + 1)], stiffness, damping));
                    }

                    // Shear springs (12 face diagonals per cube)

                    // XY plane diagonals
                    if (x < gridSizeX - 1 && y < gridSizeY - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y + 1, z)], stiffness, damping));
                    }
                    if (x > 0 && y < gridSizeY - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x - 1, y + 1, z)], stiffness, damping));
                    }

                    // XZ plane diagonals
                    if (x < gridSizeX - 1 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y, z + 1)], stiffness, damping));
                    }
                    if (x > 0 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x - 1, y, z + 1)], stiffness, damping));
                    }

                    // YZ plane diagonals
                    if (y < gridSizeY - 1 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x, y + 1, z + 1)], stiffness, damping));
                    }
                    if (y > 0 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x, y - 1, z + 1)], stiffness, damping));
                    }

                    // Body diagonals (4 space diagonals per cube)
                    if (x < gridSizeX - 1 && y < gridSizeY - 1 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y + 1, z + 1)], stiffness, damping));
                    }
                    if (x > 0 && y < gridSizeY - 1 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x - 1, y + 1, z + 1)], stiffness, damping));
                    }
                    if (x < gridSizeX - 1 && y > 0 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x + 1, y - 1, z + 1)], stiffness, damping));
                    }
                    if (x > 0 && y > 0 && z < gridSizeZ - 1)
                    {
                        springs.Add(new Spring(particles[idx], particles[GetIndex(x - 1, y - 1, z + 1)], stiffness, damping));
                    }
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
        if (autoDespawn && Time.time - spawnTime > despawnAfterSeconds)
        {
            DestroyJelly();
            return;
        }
        UpdateVisualization();
    }

    void SimulationStep(float dt)
    {
        // 1. Clear forces
        foreach (var p in particles)
        {
            p.ClearForce();
        }

        // 2. Apply gravity
        foreach (var p in particles)
        {
            p.AddForce(new Vector3(0, gravity * p.mass, 0));
        }

        // 3. Apply wind (optional)
        if (useWindSource && windSource != null)
        {
            foreach (var p in particles)
            {
                Vector3 windForce = windSource.GetWindForceAt(p.position);
                p.AddForce(windForce * p.mass);
            }
        }

        // 4. Apply spring forces
        foreach (var spring in springs)
        {
            spring.ApplyForce();
        }

        // 5. Semi-Implicit Euler Integration
        foreach (var p in particles)
        {
            p.velocity += (p.force / p.mass) * dt;
            p.position += p.velocity * dt;
        }

        HandleCollisions();
    }

    void HandleCollisions()
    {
        foreach (var p in particles)
        {
            if (p.isFixed) continue;

            // Ground collision (simple Y-check)
            HandleGroundCollision(p);

            // Environment collision (walls, obstacles)
            if (useEnvironmentCollision)
            {
                HandleEnvironmentCollision(p);
            }
        }

    }

    void HandleGroundCollision(Particle p)
    {
        float collisionY = groundY + particleRadius;

        if (p.position.y < collisionY)
        {
            // Position correction
            p.position.y = collisionY;

            // Velocity reflection with restitution
            if (p.velocity.y < 0)
            {
                p.velocity.y = -p.velocity.y * restitution;
            }

            // Friction
            p.velocity.x *= (1f - surfaceFriction);
            p.velocity.z *= (1f - surfaceFriction);
        }
    }


    void HandleEnvironmentCollision(Particle p)
    {
        // Use sphere overlap to detect nearby colliders
        Collider[] hitColliders = Physics.OverlapSphere(
            p.position,
            particleRadius + collisionSkinWidth,
            collisionLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider col in hitColliders)
        {
            // Skip our own mesh or children
            if (col.transform == transform || col.transform.IsChildOf(transform))
                continue;

            // Find closest point on collider surface
            Vector3 closestPoint = col.ClosestPoint(p.position);
            Vector3 delta = p.position - closestPoint;
            float distance = delta.magnitude;

            // If particle is penetrating or too close
            if (distance < particleRadius + collisionSkinWidth)
            {
                if (distance > 0.0001f)
                {
                    Vector3 normal = delta.normalized;
                    float penetration = (particleRadius + collisionSkinWidth) - distance;

                    // Position correction - push particle out
                    p.position += normal * penetration;

                    // Velocity correction - bounce and friction
                    float velocityAlongNormal = Vector3.Dot(p.velocity, normal);

                    if (velocityAlongNormal < 0)
                    {
                        // Reflect velocity along normal (bounce)
                        p.velocity -= normal * velocityAlongNormal * (1f + restitution);

                        // Apply friction to tangential velocity
                        Vector3 tangentialVelocity = p.velocity - normal * Vector3.Dot(p.velocity, normal);
                        p.velocity -= tangentialVelocity * surfaceFriction;
                    }
                }
                else
                {
                    // Emergency separation - distance is too small
                    p.position += Vector3.up * (particleRadius + collisionSkinWidth);
                    p.velocity *= 0.5f;
                }
            }
        }
    }

    void UpdateVisualization()
    {
        if (drawParticles && particleObjects.Count > 0)
        {
            for (int i = 0; i < particles.Count; i++)
            {
                particleObjects[i].transform.position = particles[i].position;
            }
        }
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
        if (drawParticles && particleObjects.Count == 0)
        {
            Gizmos.color = Color.yellow;
            foreach (var p in particles)
            {
                Gizmos.DrawSphere(p.position, particleRadius);
            }
        }
    }


    // used by CollisionManager for jelly-jelly collision
    // (also was used for cloth collision but that's disabled now)
    public List<Particle> GetParticles()
    {
        return particles;
    }


    void InitializeMeshRenderer()
    {
        JellyMeshRenderer meshRenderer = GetComponent<JellyMeshRenderer>();

        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<JellyMeshRenderer>();
        }

        meshRenderer.jellySimulation = this;

        if (meshRenderer.jellyMaterial == null && particleMaterial != null)
        {
            meshRenderer.jellyMaterial = particleMaterial;
        }

        meshRenderer.ForceInitialize();
    }


    void DestroyJelly()
    {
        // Unregister from collision manager
        if (collisionManager != null)
        {
            collisionManager.jellies.Remove(this);
        }

        // Clean up visualization objects
        foreach (var obj in particleObjects)
        {
            if (obj != null) Destroy(obj);
        }

        Debug.Log($"{gameObject.name}: Lifetime expired ({despawnAfterSeconds}s), destroying...");

        // Destroy the entire GameObject
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        foreach (var obj in particleObjects)
        {
            if (obj != null) Destroy(obj);
        }
    }
}