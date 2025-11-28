using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class JellySpawner : MonoBehaviour
{
    [Header("Stage Setup")]
    [Tooltip("The stage collider where jellies will spawn")]
    public Collider stageCollider;

    [Tooltip("Height above click point where jellies spawn")]
    public float spawnHeightAboveStage = 5f;

    [Tooltip("Layer mask for clickable surfaces")]
    public LayerMask spawnRaycastMask = -1;

    [Header("Spawn Mode")]  
    [Tooltip("If true, spawn on ANY surface hit by raycast (not just stageCollider)")]
    public bool spawnOnAnySurface = false;

    [Header("Collision Layers")]
    [Tooltip("Layer mask for PHYSICS")]
    public LayerMask environmentCollisionMask = -1;


    [Header("Spawning Settings")]
    [Tooltip("Maximum number of jellies allowed at once")]
    public int maxJellies = 15;
    public float spawnCooldown = 0.3f;

    [Header("Jelly Configuration")]
    [Tooltip("Random materials for jellies")]
    public Material[] jellyMaterials;

    public Vector3Int gridSize = new Vector3Int(4, 4, 4);
    public float spacing = 0.45f;
    public float particleMass = 0.4f;
    public float stiffness = 500f;
    public float damping = 5f;

    [Header("Jelly Lifetime")]
    public bool enableAutoDespawn = true;
    public float jellyLifetime = 10f;

    [Header("Collision System")]
    [Tooltip("Reference to CollisionManager for jelly-to-jelly collisions")]
    public CollisionManager collisionManager;

    [Header("Cleanup")]
    [Tooltip("Destroy jellies that fall below this Y position")]
    public float despawnY = -10f;

    [Tooltip("How often to check for out-of-bounds jellies (seconds)")]
    public float checkInterval = 1f;

    [Header("Input Options")]
    public bool clickToSpawn = true;
    public bool autoSpawn = false;
    public float autoSpawnInterval = 1f;

    [Header("Debug")]
    public bool showDebugGizmos = true;
    public bool showCooldownMessage = false;

    // Private variables
    private List<JellySimulation> activeJellies = new List<JellySimulation>();
    private float nextAutoSpawnTime;
    private float nextCleanupTime;
    private int jellyCounter = 0;
    private Camera mainCamera;
    private float lastSpawnTime = -999f;

    void Start()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("JellySpawner: No main camera found!");
        }

        if (stageCollider == null && !spawnOnAnySurface)
        {
            Debug.LogWarning("JellySpawner: No stage collider assigned!");
        }

        if (jellyMaterials == null || jellyMaterials.Length == 0)
        {
            Debug.LogWarning("JellySpawner: No jelly materials assigned!");
        }

        nextAutoSpawnTime = Time.time + autoSpawnInterval;
        nextCleanupTime = Time.time + checkInterval;
    }

    void Update()
    {
        HandleInput();

        if (autoSpawn && Time.time >= nextAutoSpawnTime)
        {
            SpawnJellyAtRandomPosition();
            nextAutoSpawnTime = Time.time + autoSpawnInterval;
        }

        if (Time.time >= nextCleanupTime)
        {
            CleanupJellies();
            nextCleanupTime = Time.time + checkInterval;
        }
    }

    void HandleInput()
    {
        if (!clickToSpawn || mainCamera == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            // Ignore clicks on UI elements
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            float timeSinceLastSpawn = Time.time - lastSpawnTime;

            if (timeSinceLastSpawn < spawnCooldown)
            {
                if (showCooldownMessage)
                {
                    float remainingCooldown = spawnCooldown - timeSinceLastSpawn;
                    Debug.Log($"JellySpawner: Cooldown active. Wait {remainingCooldown:F1}s");
                }
                return; 
            }
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 1000f, spawnRaycastMask))
            {
                bool canSpawn = false;

                if (spawnOnAnySurface)
                {
                    canSpawn = true;
                }

                else if (stageCollider != null)
                {
                    canSpawn = (hit.collider == stageCollider);
                }

                if (canSpawn)
                {
                    Vector3 spawnPosition = hit.point + Vector3.up * spawnHeightAboveStage;
                    SpawnJelly(spawnPosition);
                    lastSpawnTime = Time.time;
                }
                else if (showCooldownMessage) 
                {
                    Debug.Log($"JellySpawner: Clicked on {hit.collider.name}, but only {stageCollider?.name} is allowed");
                }
            }
        }
    }

    void SpawnJellyAtRandomPosition()
    {
        if (stageCollider == null) return;

        float timeSinceLastSpawn = Time.time - lastSpawnTime;
        if (timeSinceLastSpawn < spawnCooldown)
        {
            return;
        }
        Bounds bounds = stageCollider.bounds;
        Vector3 randomPos = new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            bounds.max.y + spawnHeightAboveStage,
            Random.Range(bounds.min.z, bounds.max.z)
        );

        SpawnJelly(randomPos);
        lastSpawnTime = Time.time;
    }

    void SpawnJelly(Vector3 position)
    {
        if (activeJellies.Count >= maxJellies)
        {
            Debug.Log($"JellySpawner: Max jellies reached ({maxJellies})");
            return;
        }

        // Create GameObject
        GameObject jellyObj = new GameObject($"Jelly_{jellyCounter++}");
        jellyObj.transform.position = position;

        JellySimulation jelly = jellyObj.AddComponent<JellySimulation>();

        // Configure physics parameters
        jelly.gridSizeX = gridSize.x;
        jelly.gridSizeY = gridSize.y;
        jelly.gridSizeZ = gridSize.z;
        jelly.spacing = spacing;
        jelly.particleMass = particleMass;
        jelly.stiffness = stiffness;
        jelly.damping = damping;
        jelly.useMesh = true;
        jelly.drawParticles = false;
        jelly.drawSprings = false;

        // Configure lifetime 
        jelly.autoDespawn = enableAutoDespawn;
        jelly.despawnAfterSeconds = jellyLifetime;

        // Configure environment collision
        jelly.useEnvironmentCollision = true;
        jelly.collisionLayers = environmentCollisionMask;

        // Assign random material
        if (jellyMaterials != null && jellyMaterials.Length > 0)
        {
            Material randomMat = jellyMaterials[Random.Range(0, jellyMaterials.Length)];
            jelly.particleMaterial = randomMat;
        }


        activeJellies.Add(jelly);

        // Register with collision manager
        if (collisionManager != null)
        {
            collisionManager.jellies.Add(jelly);
        }
    }

    void CleanupJellies()
    {
        for (int i = activeJellies.Count - 1; i >= 0; i--)
        {
            JellySimulation jelly = activeJellies[i];

            if (jelly == null)
            {
                activeJellies.RemoveAt(i);
                continue;
            }

            // Destroy jellies that fell off the stage
            if (jelly.transform.position.y < despawnY)
            {
                if (collisionManager != null)
                {
                    collisionManager.jellies.Remove(jelly);
                }

                Destroy(jelly.gameObject);
                activeJellies.RemoveAt(i);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        if (stageCollider != null)
        {
            Bounds bounds = stageCollider.bounds;

            // Spawn height plane
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Vector3 spawnPlaneCenter = bounds.center + Vector3.up * spawnHeightAboveStage;
            Vector3 spawnPlaneSize = new Vector3(bounds.size.x, 0.1f, bounds.size.z);
            Gizmos.DrawCube(spawnPlaneCenter, spawnPlaneSize);

            // Stage outline
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        // Despawn line
        Gizmos.color = Color.red;
        float lineSize = 20f;
        Vector3 despawnCenter = Vector3.up * despawnY;
        Gizmos.DrawLine(
            despawnCenter + new Vector3(-lineSize, 0, -lineSize),
            despawnCenter + new Vector3(lineSize, 0, lineSize)
        );
        Gizmos.DrawLine(
            despawnCenter + new Vector3(-lineSize, 0, lineSize),
            despawnCenter + new Vector3(lineSize, 0, -lineSize)
        );
    }

    public void ClearAllJellies()
    {
        for (int i = activeJellies.Count - 1; i >= 0; i--)
        {
            if (activeJellies[i] != null)
            {
                if (collisionManager != null)
                {
                    collisionManager.jellies.Remove(activeJellies[i]);
                }
                Destroy(activeJellies[i].gameObject);
            }
        }
        activeJellies.Clear();
    }
}