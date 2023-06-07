### Introduction

![replace this with our own gif](https://phillipscorp.com/india/wp-content/uploads/sites/3/2020/07/ur10e.gif)

### Quick Start

* open scene
* run python script
* play scene
* Change Parameters: 
* - In the top left of the screen, toggle on/off the Control Panel
  - Click on the Tabs on the left to see the different categories of parameters available to be changed
  - Enter a value (must be a number)
* check analytics

### Scene Hierarchy

### Spray Painting

- Spray Painting using PaintIn3D asset from Asset Store: ([Paint in 3D | Painting | Unity Asset Store](https://assetstore.unity.com/packages/tools/painting/paint-in-3d-26286))
  
  - Uses a Particle System. The particles are made up of P3DPaintSpheres (from PaintIn3D). One particle applies the colour, and the other applies the finish (more information about this in the following bullet point).

- To be able to paint, the material must have a P3DPaintable component. Then, for each texture. there must be a P3DPaintableTexture assigned to each shader slot. We have materials assigned to the following shader slots:
  
  - _MainTex
  
  - _MetallicGlossMap

- The mesh requires Convex Colliders. 

- The UV map cannot have overlapping coordinates. This can be fixed using PaintIn3D or doing an unwrap in Blender. 

### RobotArm

* URDF downloaded from and converted with
* HybridIK
* setting joints
* setting the constraint to last two joints
* Painting Paths
* Robot Job system

### CV

* Algorithm
* Area Lightning Setup
* Running Python script
* How to update the ground truth images

### Modifying Simulation Parameters

* Control Panel created in Unity's uGUI and C# scripts. The user can change the following parameters:
  * Conveyor Speed
  * Spray Radius
  * Spray Angle
  * Spray Pressure (maps to Particle System's Rate Over Time)
  * Robot Distance from Car
  * Robot Speed
  * Number of Workers
  * Time to Fix Defects

### Data Collection and Analytics

- LSD

- Visualize outputs in another UI
