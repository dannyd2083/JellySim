# Jelly Simulation - Physics-Based Soft Body Simulation

A Unity project demonstrating real-time physics simulation of soft bodies (jelly cubes) and cloth using mass-spring systems. The project features interactive controls, wind simulation, and collision detection.

## Prerequisites

- **Unity Version**: 2022.3.62f3 or compatible
- **Platform**: Windows
- **Unity Modules Required**:
  - Universal Windows Platform (if building for UWP)
  - Standard Unity modules (already included in default installation)

## Setup Instructions

### 1. Install Unity

1. Download and install **Unity Hub** from [unity.com](https://unity.com/download)
2. Install **Unity Editor version 2022.3.62f3** (or a compatible 2022.3.x version)
   - Open Unity Hub
   - Go to "Installs" tab
   - Click "Install Editor"
   - Select version 2022.3.62f3 (LTS)

### 2. Open the Project

1. Open Unity Hub
2. Click "Open" or "Add" button
3. Navigate to the project folder: `JellySimulation`
4. Select the folder and click "Open"
5. Unity will import the project (this may take a few minutes on first load)

### 3. Verify Setup

Once the project opens:
- Check that there are no errors in the Console window
- The Project window should show the `Assets` folder structure
- The Hierarchy should display the current scene contents

## How to Run

### Running in the Unity Editor (Recommended for Testing)

1. Open one of the scenes from the `Assets/Scenes` folder:
   - **ClothScene.unity** - Cloth simulation only
   - **JellyScene.unity** - Jelly cube simulation with spawner

## Available Scenes

### 1. ClothScene
- Demonstrates cloth physics simulation
- Features a hanging cloth
- Includes wind effects and gravity

### 2. JellyScene
- Interactive jelly cube spawning system
- Click on surfaces to spawn soft-body jelly cubes
- Jellies bounce, deform, and interact with the environment

## Controls

### Camera Controls
- **Right Mouse Button + Drag**: Rotate camera around target
- **Middle Mouse Button + Drag**: Pan camera
- **Mouse Scroll Wheel**: Zoom in/out
- **W/A/S/D Keys**: Pan camera horizontally
- **Q/E Keys**: Pan camera vertically (down/up)

### Jelly Spawning (JellyScene & ClothJellyScene)
- **Left Mouse Click** on a surface: Spawn a jelly cube
- Jellies auto-despawn after 10 seconds (configurable)
- Maximum of 15 jellies at once (configurable)

### Mode Switching (ClothJellyScene)
- **Left Mouse Click** on empty space: Camera control mode
- **Left Mouse Click** on wind source object: Wind source control mode
  - When wind source is selected, it highlights in yellow
  - You can then manipulate the wind source properties

## Features

### Physics Simulation
- **Mass-Spring System**: Particles connected by springs with configurable stiffness and damping
- **Semi-Implicit Euler Integration**: Stable numerical integration for real-time simulation
- **Collision Detection**:
  - Ground collision with restitution (bounciness)
  - Environment collision (walls, obstacles)
  - Jelly-to-jelly collision (in ClothJellyScene)
- **Soft Body Dynamics**: Realistic deformation and recovery

### Jelly Simulation
- **3D Particle Grid**: Configurable grid size (default: 5x5x5)
- **Spring Network**: Structural, shear, and body diagonal springs for volumetric behavior
- **Mesh Rendering**: Real-time deformable mesh visualization
- **Auto-Despawn**: Automatic cleanup after configurable lifetime

### Cloth Simulation
- **2D Particle Grid**: Configurable resolution (default: 30x30)
- **Hanging Cloth Mode**: Pins top row of particles
- **Trampoline Mode**: Pins four corners for horizontal layout
- **Wind Simulation**:
  - Global wind with turbulence (Perlin noise)
  - Local wind sources
  - Configurable strength and direction

### Wind System
- **WindSource Component**: Directional wind affecting nearby particles
- **Global Wind**: Uniform wind with procedural turbulence
- **Force Application**: Wind forces applied based on particle position

## Project Structure

```
JellySimulation/
├── Assets/
│   ├── Scenes/
│   │   ├── ClothScene.unity
│   │   ├── JellyScene.unity
│   │   └── ClothJellyScene.unity
│   ├── Scripts/
│   │   ├── JellySimulation.cs       # Main jelly physics
│   │   ├── ClothSimulation.cs       # Main cloth physics
│   │   ├── Particle.cs              # Particle data structure
│   │   ├── Spring.cs                # Spring force calculation
│   │   ├── JellyMeshRenderer.cs     # Mesh visualization
│   │   ├── JellySpawner.cs          # Interactive spawning system
│   │   ├── CollisionManager.cs      # Jelly-jelly collision
│   │   ├── WindSource.cs            # Wind force generator
│   │   ├── OrbitCamera.cs           # Camera controller
│   │   ├── InputManager.cs          # Input handling
│   │   └── SceneSwitcher.cs         # Scene management
│   └── TextMesh Pro/                # UI text rendering
├── Packages/
│   └── manifest.json                # Unity package dependencies
├── ProjectSettings/                 # Unity project configuration
└── README.md                        # This file
```

## Key Scripts Explained

### Core Physics
- **Particle.cs**: Stores position, velocity, force, mass, and fixed state
- **Spring.cs**: Computes spring forces using Hooke's Law with damping
- **JellySimulation.cs**: Main simulation loop, force accumulation, integration
- **ClothSimulation.cs**: Similar to jelly but for 2D cloth grid

### Rendering
- **JellyMeshRenderer.cs**: Generates and updates deformable mesh from particle positions

### Interaction
- **JellySpawner.cs**: Handles mouse input for spawning jellies
- **InputManager.cs**: Switches between camera and wind source control
- **OrbitCamera.cs**: Orbit camera with pan and zoom

### Systems
- **CollisionManager.cs**: Manages inter-jelly collisions
- **WindSource.cs**: Generates wind forces at particle positions

## Configuration Tips

### Adjusting Jelly Properties (Inspector)
- **Grid Size** (gridSizeX/Y/Z): Number of particles per axis (higher = more detail, slower)
- **Spacing**: Distance between particles (affects size)
- **Stiffness**: Spring stiffness (higher = firmer jelly)
- **Damping**: Energy loss (higher = less bouncy)
- **Particle Mass**: Heavier = falls faster
- **Restitution**: Bounciness on collision (0-1)

### Adjusting Cloth Properties (Inspector)
- **Grid Width/Height**: Cloth resolution
- **Is Trampoline Mode**: Enable for horizontal cloth with corner pins
- **Is Horizontal Layout**: XZ plane vs XY plane orientation
- **Use Wind Source**: Toggle wind effects

### Performance Optimization
- Reduce grid size (fewer particles)
- Decrease substeps (less stable but faster)
- Disable particle/spring visualization
- Reduce max jellies in spawner

## Troubleshooting

### Scene doesn't load
- Ensure you're using Unity 2022.3.x
- Check Console for errors
- Try reimporting the project (Assets > Reimport All)

### Physics looks unstable
- Increase substeps in simulation scripts
- Decrease stiffness value
- Reduce time step

### Low framerate
- Reduce number of particles (grid size)
- Decrease number of spawned jellies
- Disable debug visualization (particles/springs)

### Jellies fall through floor
- Increase collision skin width
- Check ground Y value matches floor position
- Ensure collision layers are set correctly

## Credits

This project was developed as a demonstration of real-time physics simulation techniques using mass-spring systems and numerical integration methods.

## Contact for Grading

If you encounter any issues running this project, please contact the student for assistance or clarification.
