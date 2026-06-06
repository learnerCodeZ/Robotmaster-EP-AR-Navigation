# AGENTS

Instructions for AI coding agents working on this project.

## Project Context

This is a Robotmaster EP mobile robot AR navigation system using HoloLens 2. The robot runs ROS1 Noetic on a Jetson Xavier NX with an RPLIDAR A2. Unity communicates with ROS1 via TCP (ROS-TCP-Connector). HoloLens 2 provides the AR interface.

## Key Conventions

- **No git repository** — do not run git commands
- **Unity project** is in `UnityProject/` (created with Unity 2022.3 LTS)
- **ROS1 workspace** is in `ros1_ws/` (runs on Jetson Xavier NX, Ubuntu 20.04)
- Shell: use bash syntax, not Windows CMD
- Path separator: `/` not `\`

## Development Principle

**Simulation first, hardware later.** All logic should be validated in the Unity Editor + Gazebo simulation before building to HoloLens 2 or connecting to the real robot.

## When Writing Unity C# Scripts

- Use `Unity.Robotics.ROSTCPConnector` for all ROS1 communication
- ROS1 → Unity coordinate transform: `(-y, z, x)`
- ROS1 → Unity rotation: Z-up quaternion to Y-up quaternion
- HoloLens 2 is ARM64 UWP — avoid unsupported .NET APIs, JIT-only code, and Windows-only namespaces
- MRTK3 for all XR interaction (hand ray, air tap, near/far interaction)
- For network calls, handle UWP sandbox restrictions (use `InternetClient` + `PrivateNetworkClientServer` capabilities)

## When Writing ROS1 Code

- Workspace: `ros1_ws/`, target: ROS1 Noetic, OS: Ubuntu 20.04
- Package naming uses lowercase with underscores
- Use `catkin_make` or `catkin build` for building
- `ros_tcp_endpoint` is the bridge between ROS1 and Unity (clone from Unity Robotics Hub endpoints/ROS1)

## When Configuring HoloLens 2

- Build target: UWP, ARM64
- Minimum platform: 10.0.10240.0
- Required capabilities: Spatial Perception, InternetClient, InternetClientServer, PrivateNetworkClientServer, Microphone
- Deploy via USB (fastest for iteration) or WiFi

## Common Pitfalls

- UWP apps cannot connect to localhost — always use the actual IP of the ROS1 machine
- HoloLens 2 WiFi latency is ~5-20ms on local network, acceptable for teleop but account for it in UI feedback
- ROS-TCP-Connector message serialization can bottleneck at high-frequency topics like `/scan` — throttle to 5-10Hz if needed
