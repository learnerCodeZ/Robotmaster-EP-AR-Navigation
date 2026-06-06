# AGENTS

面向在此项目上工作的 AI 编码助手的说明。

## 项目背景

这是一个基于 Robotmaster EP 移动机器人、使用 HoloLens 2 的 AR 导航系统。机器人在 Jetson Xavier NX 上运行 ROS1 Noetic，搭载 RPLIDAR A2。Unity 通过 TCP（ROS-TCP-Connector）与 ROS1 通信。HoloLens 2 提供 AR 界面。

## 关键约定

- **无 git 仓库** — 不要运行 git 命令
- **Unity 项目**位于 `UnityProject/`（使用 Unity 2022.3 LTS 创建）
- **ROS1 工作空间**位于 `ros1_ws/`（在 Jetson Xavier NX，Ubuntu 20.04 上运行）
- Shell：使用 bash 语法，而非 Windows CMD
- 路径分隔符：使用 `/` 而非 `\`

## 开发原则

**仿真优先，硬件随后。** 所有逻辑应先通过 Unity Editor + Gazebo 仿真验证，然后再构建到 HoloLens 2 或连接真实机器人。

## 编写 Unity C# 脚本时

- 使用 `Unity.Robotics.ROSTCPConnector` 进行所有 ROS1 通信
- ROS1 → Unity 坐标变换：`(-y, z, x)`
- ROS1 → Unity 旋转变换：Z-up 四元数到 Y-up 四元数
- HoloLens 2 是 ARM64 UWP — 避免使用不支持的 .NET API、仅 JIT 代码以及仅 Windows 的命名空间
- 使用 MRTK3 进行所有 XR 交互（手部射线、空中点击、近/远交互）
- 网络调用时，处理 UWP 沙箱限制（使用 `InternetClient` + `PrivateNetworkClientServer` 功能）

## 编写 ROS1 代码时

- 工作空间：`ros1_ws/`，目标：ROS1 Noetic，操作系统：Ubuntu 20.04
- 包命名使用小写字母和下划线
- 使用 `catkin_make` 或 `catkin build` 构建
- `ros_tcp_endpoint` 是 ROS1 和 Unity 之间的桥梁（从 Unity Robotics Hub endpoints/ROS1 克隆）

## 配置 HoloLens 2 时

- 构建目标：UWP，ARM64
- 最低平台：10.0.10240.0
- 必需功能：Spatial Perception、InternetClient、InternetClientServer、PrivateNetworkClientServer、Microphone
- 通过 USB（迭代速度最快）或 WiFi 部署

## 常见陷阱

- UWP 应用无法连接 localhost — 始终使用 ROS1 机器的实际 IP
- HoloLens 2 在局域网上的 WiFi 延迟约为 5-20ms，对遥控操作可接受，但需在 UI 反馈中加以考虑
- ROS-TCP-Connector 的消息序列化在高频率主题（如 `/scan`）上可能成为瓶颈 — 必要时限频至 5-10Hz
