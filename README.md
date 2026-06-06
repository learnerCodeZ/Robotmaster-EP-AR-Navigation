# Robotmaster EP + HoloLens 2 AR 导航系统

使用 HoloLens 2 操控搭载 RPLIDAR A2 的 Robotmaster EP 小车进行自主导航，并在 AR 视野中实时绘制行驶轨迹。

## 硬件

| 设备 | 角色 |
|------|------|
| Robotmaster EP | 下位机（运动控制） |
| RPLIDAR A2 | 激光雷达（建图/定位） |
| Jetson Xavier NX | 上位机（ROS1 Noetic） |
| HoloLens 2 | AR 交互终端 |

## 软件栈

- **OS**: Ubuntu 20.04 (Jetson Xavier NX)
- **ROS1**: Noetic Ninjemys
- **仿真**: Gazebo Classic 11
- **导航**: move_base + SLAM Toolbox
- **Unity**: 2022.3 LTS + ROS-TCP-Connector + MRTK3
- **部署**: HoloLens 2 (ARM64, UWP)

## 架构

```
[HoloLens 2 / Unity] ←── WiFi ──→ [Jetson Xavier NX]
       │                                 │
   MRTK3 UI                        ros_tcp_endpoint
       │                                 │
  ROS-TCP-Connector ←── TCP:10000 ──→ ROS1 Noetic
                                           │
                                    ┌──────┴──────┐
                                    │             │
                              move_base + SLAM    /cmd_vel → EP
                                    │
                              /odom, /scan, /map
```

## 目录结构

```
D:/Unity_Ros/
├── UnityProject/          # Unity 工程
├── ros1_ws/               # ROS1 工作空间
│   └── src/
│       ├── unity_bridge/      # 自定义桥接包
│       └── ros_tcp_endpoint/  # ROS1 TCP 服务器
├── ROS-TCP-Connector/     # Unity 端 ROS 通信包（本地引用）
├── start_ros.bat          # 一键启动 Docker 容器
├── Unity_Ros2/            # ROS2 Humble 版本（待配置）
└── docs/                  # 文档
    ├── docker_setup.md        # Docker 环境搭建指南
    ├── unity_basics.md        # Unity 基础指南
    └── xavier_setup.md        # Xavier 设置指南
```

## 本地开发（Windows Docker）

在 Xavier 之前，先在 Windows 上用 Docker 打通 Unity ↔ ROS1 通讯。

### 第 1 步：启动 ROS 环境

双击 `D:\Unity_Ros\start_ros.bat`

等待出现：
```
Starting server on 0.0.0.0:10000
```

### 第 2 步：启动 Unity

打开 Unity Hub → 打开 `UnityProject`

菜单栏 → **Robotics → ROS Settings** → 确认配置：
- ROS IP Address: `localhost`
- ROS Port: `10000`

打开 `Assets/Scenes/MainScene.unity` → 点击 **Play**

Console 应显示连接日志。

### 第 3 步：测试通讯

新开一个 PowerShell，进入容器：
```powershell
docker exec -it ros_noetic bash
```

容器内：
```bash
source /opt/ros/noetic/setup.bash
rostopic pub -1 /odom nav_msgs/Odometry "{header: {frame_id: odom}, child_frame_id: base_link, pose: {pose: {position: {x: 1.0, y: 2.0, z: 0.0}}}}"
```

Unity Console 出现 `[Odom] pos(1.00, 2.00)...` 即成功。

### 第 4 步：关闭

- Unity：点击 **Play** 退出运行
- ROS：bat 窗口按 **Ctrl + C** 停掉 endpoint
- 可以关闭所有窗口

## 快速开始

### 1. Xavier 端 — 启动仿真

```bash
# 终端 1: Gazebo
export TURTLEBOT3_MODEL=waffle
roslaunch turtlebot3_gazebo turtlebot3_world.launch

# 终端 2: SLAM
roslaunch slam_toolbox online_async_launch.py

# 终端 3: Navigation
roslaunch move_base move_base.launch

# 终端 4: TCP 桥接
rosrun ros_tcp_endpoint default_server_endpoint.py
```

### 2. PC 端 — Unity Editor

1. 打开 `UnityProject`，加载 `MainScene`
2. 确认 ROS IP 指向 Xavier（默认 `192.168.1.x:10000`）
3. 点击 Play 运行

### 3. HoloLens 2 — 部署

1. File → Build Settings → UWP → ARM64
2. Build → 用 VS 打开 .sln
3. 选择 Release / ARM64 / Device → Ctrl+F5

## 通信 Topics

| Topic | 类型 | 方向 | 用途 |
|-------|------|------|------|
| `/cmd_vel` | Twist | Unity → ROS1 | 速度控制 |
| `/odom` | Odometry | ROS1 → Unity | 里程计位姿 |
| `/scan` | LaserScan | ROS1 → Unity | 激光雷达 |
| `/map` | OccupancyGrid | ROS1 → Unity | 占据栅格地图 |
| `/goal_pose` | PoseStamped | Unity → ROS1 | 导航目标点 |
| `/trajectory` | Path | ROS1 → Unity | 规划路径 / 行驶轨迹 |

## 开发路线

1. **环境搭建** — 安装 Unity + MRTK3 + ROS1 依赖
2. **通信验证** — PC Unity Editor ↔ Xavier ROS1 收发消息
3. **仿真闭环** — Gazebo 虚拟小车 + Unity 操控 + 轨迹绘制
4. **HoloLens 移植** — Build 到 HoloLens 2，适配手势交互
5. **实车对接** — 连接 Robotmaster EP + RPLIDAR A2

## 坐标系转换

```
ROS1 (右手): X-前 Y-左 Z-上
Unity (左手): X-右 Y-上 Z-前

转换: Unity(pos) = (-y, z, x)
```
