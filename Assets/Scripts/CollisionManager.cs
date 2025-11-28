using UnityEngine;
using System.Collections.Generic;

public class CollisionManager : MonoBehaviour
{
    [Header("Collision Settings")]
    [Tooltip("Distance at which particles start to collide")]
    public float collisionRadius = 0.5f;
    [Tooltip("How strongly particles push each other apart (higher = stronger)")]
    public float collisionStiffness = 2.5f;
    [Tooltip("Velocity damping during collision (0 = no damping, 1 = full stop)")]
    public float collisionDamping = 0.2f;
    [Tooltip("Number of collision resolution iterations per frame (more = more accurate but slower)")]
    public int collisionIterations = 3;

    [Header("Spatial Hashing")]
    public float cellSize = 0.6f;
    [Header("References")]
    public List<JellySimulation> jellies = new List<JellySimulation>();

    // TODO: jelly-cloth collision is too slow, disabled for now
    // maybe optimize this later or find a better approach
    [Header("Cloth Collision")]
    [Tooltip("Enable collision between jellies and cloth")]
    public bool enableClothCollision = false;  // disabled - performance issues

    [Tooltip("List of cloth simulations to collide with")]
    public List<ClothSimulation> cloths = new List<ClothSimulation>();
    [Tooltip("How much jelly bounces off cloth (higher = more bounce)")]
    public float clothBounceMultiplier = 2.0f;
    [Tooltip("How much jelly pushes cloth down")]
    public float clothImpactMultiplier = 1.5f;  

    [Header("Performance")]
    public bool enableCollision = true;
    public int maxParticlePairs = 20000;

    [Header("Debug")]
    public bool drawDebugGrid = false;
    public bool showStats = false;
    public bool drawCollisionPoints = false;

    private Dictionary<Vector3Int, List<Particle>> spatialHash;
    private List<Particle> allParticles;
    private List<Particle> clothParticles;
    private int collisionCount = 0;
    private int particleCount = 0;
    private int clothCollisionCount = 0;

    void Start()
    {
        allParticles = new List<Particle>();
        clothParticles = new List<Particle>();
        spatialHash = new Dictionary<Vector3Int, List<Particle>>();
    }

    void FixedUpdate()
    {
        if (enableCollision)
        {
            DetectAndResolveCollisions();
        }
    }

    public void DetectAndResolveCollisions()
    {
        CollectAllParticles();

        if (allParticles.Count == 0) return;

        BuildSpatialHash();

        for (int iter = 0; iter < collisionIterations; iter++)
        {
            ResolveCollisions();
            if (enableClothCollision)
            {
                ResolveClothCollisions();
            }
        }

        if (showStats && Time.frameCount % 60 == 0)
        {
            Debug.Log($"CollisionManager: {jellies.Count} jellies, {particleCount} particles, {collisionCount} collisions/frame");
        }
    }

    void CollectAllParticles()
    {
        allParticles.Clear();
        particleCount = 0;

        // Remove null jellies
        for (int i = jellies.Count - 1; i >= 0; i--)
        {
            if (jellies[i] == null)
            {
                jellies.RemoveAt(i);
            }
        }

        // Collect all particles
        foreach (var jelly in jellies)
        {
            if (jelly != null && jelly.GetParticles() != null)
            {
                var particles = jelly.GetParticles();
                allParticles.AddRange(particles);
                particleCount += particles.Count;
            }
        }

        // TODO: cloth collision code - keeping this in case we want to revisit it later
        for (int i = cloths.Count - 1; i >= 0; i--)
        {
            if (cloths[i] == null)
            {
                cloths.RemoveAt(i);
            }
        }

        foreach (var cloth in cloths)
        {
            if (cloth != null && cloth.GetParticles() != null)
            {
                var particles = cloth.GetParticles();
                clothParticles.AddRange(particles);
            }
        }
    }

    void BuildSpatialHash()
    {
        // Clear previous hash
        foreach (var list in spatialHash.Values)
        {
            list.Clear();
        }

        // Add all particles to spatial hash
        foreach (var particle in allParticles)
        {
            Vector3Int cell = GetCellIndex(particle.position);

            if (!spatialHash.ContainsKey(cell))
            {
                spatialHash[cell] = new List<Particle>();
            }

            spatialHash[cell].Add(particle);
        }

        // TODO: cloth particles spatial hash - part of disabled cloth collision system
        foreach (var particle in clothParticles)
        {
            Vector3Int cell = GetCellIndex(particle.position);

            if (!spatialHash.ContainsKey(cell))
            {
                spatialHash[cell] = new List<Particle>();
            }

            spatialHash[cell].Add(particle);
        }
    }

    Vector3Int GetCellIndex(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }

    void ResolveCollisions()
    {
        collisionCount = 0;
        HashSet<(Particle, Particle)> checkedPairs = new HashSet<(Particle, Particle)>();
        int pairCount = 0;

        foreach (var particle in allParticles)
        {
            if (particle == null) continue;

            Vector3Int cell = GetCellIndex(particle.position);

            // Check neighboring cells (3x3x3 grid)
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighborCell = cell + new Vector3Int(x, y, z);

                        if (!spatialHash.ContainsKey(neighborCell)) continue;

                        foreach (var other in spatialHash[neighborCell])
                        {
                            if (other == null || particle == other) continue;

                            // Ensure we only check each pair once
                            var pair = particle.GetHashCode() < other.GetHashCode()
                                ? (particle, other)
                                : (other, particle);

                            if (checkedPairs.Contains(pair)) continue;
                            checkedPairs.Add(pair);

                            pairCount++;
                            if (pairCount > maxParticlePairs) return;

                            // Handle collision
                            HandleParticlePairCollision(particle, other);
                        }
                    }
                }
            }
        }
    }

    // TODO: disabled cloth collision - was causing fps drops
    // keeping the code here in case we want to optimize and re-enable later
    void ResolveClothCollisions()
    {
        clothCollisionCount = 0;

        foreach (var jellyParticle in allParticles)
        {
            if (jellyParticle == null || jellyParticle.isFixed) continue;

            Vector3Int cell = GetCellIndex(jellyParticle.position);

            // check nearby cloth particles
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int z = -1; z <= 1; z++)
                    {
                        Vector3Int neighborCell = cell + new Vector3Int(x, y, z);

                        if (!spatialHash.ContainsKey(neighborCell)) continue;

                        foreach (var clothParticle in spatialHash[neighborCell])
                        {
                            if (clothParticle == null) continue;

                     
                            if (!clothParticles.Contains(clothParticle)) continue;

                            HandleClothJellyCollision(jellyParticle, clothParticle);
                        }
                    }
                }
            }
        }
    }


    // TODO: part of disabled cloth collision system
    void HandleClothJellyCollision(Particle jellyParticle, Particle clothParticle)
    {
        Vector3 delta = jellyParticle.position - clothParticle.position;
        float distance = delta.magnitude;

        if (distance < collisionRadius && distance > 0.0001f)
        {
            clothCollisionCount++;

            Vector3 normal = delta.normalized;
            float overlap = collisionRadius - distance;

            
            float halfOverlap = overlap * 0.5f;
            jellyParticle.position += normal * halfOverlap;

            if (!clothParticle.isFixed)
            {
                clothParticle.position -= normal * halfOverlap;
            }

          
            Vector3 relativeVelocity = jellyParticle.velocity - clothParticle.velocity;
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

            if (velocityAlongNormal < 0)
            {
                float impulseMagnitude = -(1f + collisionDamping) * velocityAlongNormal;
                impulseMagnitude /= (1f / jellyParticle.mass + 1f / clothParticle.mass);

                Vector3 impulse = normal * impulseMagnitude;

               
                jellyParticle.velocity -= impulse / jellyParticle.mass * clothBounceMultiplier;

                
                if (!clothParticle.isFixed)
                {
                    clothParticle.velocity += impulse / clothParticle.mass * clothImpactMultiplier;
                }
            }
        }
    }

    void HandleParticlePairCollision(Particle p1, Particle p2)
    {
        Vector3 delta = p2.position - p1.position;
        float distance = delta.magnitude;

        // Check if particles are colliding
        if (distance < collisionRadius && distance > 0.0001f)
        {
            collisionCount++;

            Vector3 direction = delta / distance;
            float overlap = collisionRadius - distance;

            // Position correction (separate particles)
            float totalMass = p1.mass + p2.mass;
            float ratio1 = p2.mass / totalMass;  // How much p1 moves
            float ratio2 = p1.mass / totalMass;  // How much p2 moves

            // Apply position correction with stiffness multiplier
            float separationForce = overlap * collisionStiffness;

            if (!p1.isFixed)
            {
                p1.position -= direction * separationForce * ratio1;
            }
            if (!p2.isFixed)
            {
                p2.position += direction * separationForce * ratio2;
            }

            // Velocity correction (bounce/damping)
            Vector3 relativeVelocity = p2.velocity - p1.velocity;
            float velocityAlongNormal = Vector3.Dot(relativeVelocity, direction);

            // Only apply impulse if particles are moving towards each other
            if (velocityAlongNormal < 0)
            {
                // Calculate impulse
                float impulseMagnitude = -(1f + collisionDamping) * velocityAlongNormal;
                impulseMagnitude /= (1f / p1.mass + 1f / p2.mass);

                Vector3 impulse = direction * impulseMagnitude;

                if (!p1.isFixed)
                {
                    p1.velocity -= impulse / p1.mass;
                }
                if (!p2.isFixed)
                {
                    p2.velocity += impulse / p2.mass;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (spatialHash == null) return;

        // Draw spatial grid
        if (drawDebugGrid)
        {
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            foreach (var cell in spatialHash.Keys)
            {
                Vector3 center = new Vector3(
                    cell.x * cellSize + cellSize * 0.5f,
                    cell.y * cellSize + cellSize * 0.5f,
                    cell.z * cellSize + cellSize * 0.5f
                );
                Gizmos.DrawWireCube(center, Vector3.one * cellSize);
            }
        }

        // Draw collision spheres
        if (drawCollisionPoints && allParticles != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            foreach (var particle in allParticles)
            {
                if (particle != null)
                {
                    Gizmos.DrawWireSphere(particle.position, collisionRadius);
                }
            }
        }
    }
}