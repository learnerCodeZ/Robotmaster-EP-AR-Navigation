# Unity + ROS1 Noetic 导航系统开发日志

**日期**: 2026年6月6日（下午）

## 目标

开发 Unity + ROS1 Noetic 导航系统，打通 Unity ↔ ROS 通讯

---

## 完成工作

### 1. 修复 Unity 编译错误

- 恢复 6 个 `.asmdef` 文件（新 GUID）
- 在 `manifest.json` 中添加 `com.unity.modules.jsonserialize` + `com.unity.modules.imageconversion`
- 修复 `Visualizations.Editor.asmdef` 缺失的 Visualizations.Runtime GUID 引用
- 修复 `RosUtils.cs`：`using RosMessageTypes.Std` → `using RosMessageTypes.BuiltinInterfaces`
- 修复 3 个脚本：`RegisterSubscriber<T>()` → `Subscribe<T>()`
- 删除 `Tests/` 文件夹，消除 NUnit 依赖错误

### 2. 文档更新

- `README.md`、`AGENTS.md`、`AGENTS_CN.md` 从 ROS2 Humble 改为 ROS1 Noetic
- 创建 `docs/xavier_setup.md`（Xavier 设置指南）
- 创建 `docs/docker_setup.md`（Docker 环境搭建全攻略）

### 3. 创建 ROS1 桥接包

- `ros1_ws/src/unity_bridge/`
  - `unity_bridge/car_pose_subscriber.py`
  - `CMakeLists.txt`、`package.xml`、`setup.py`
  - `launch/bridge.launch`

### 4. 创建 Unity 场景

- `MainScene.unity`
  - Main Camera
  - Directional Light
  - Ground (Plane)
  - RobotCar (Cube)
    - VirtualCarController
    - RosSubscriber (/cmd_vel)
    - RosOdomSubscriber (/odom)
    - RosPublishCarPose (/car_pose)
  - Trajectory (LineRenderer + TrajectoryRenderer)

### 5. 获取 ROS1 endpoint

- 从 GitHub 下载 `ROS-TCP-Endpoint`（main 分支，rospy 版）
- 放入 `ros1_ws/src/ros_tcp_endpoint/`
- 修复 `bridge.launch` 参数名：`tcp_ip` / `tcp_port`

### 6. 搭建 Docker 环境

- 安装 Docker Desktop for Windows
- 拉取 `ros:noetic-ros-base` 镜像
- 创建 `ros_noetic` 容器（端口映射 10000:10000）
- 容器内安装 `python-is-python3` + `ros-noetic-tf2-msgs`

### 7. 打通通讯

- `catkin_make` 编译 ROS 工作区
- Unity ↔ Docker 内 ROS1 endpoint 成功建立 TCP 连接
- 验证三个 topic：
  - `/cmd_vel` (Twist) — Unity 接收 ✅
  - `/odom` (Odometry) — Unity 接收 ✅
  - `/car_pose` (PoseStamped) — Unity 发布 ✅

---

## 项目结构

```
D:\Unity_Ros\
├── UnityProject\          ← Unity 项目（可打开 Play）
│   ├── Assets\Scripts\    ← 7 个 C# 脚本
│   └── Assets\Scenes\MainScene.unity
├── ROS-TCP-Connector\     ← Unity 包依赖
├── ros1_ws\               ← ROS 工作区
│   └── src\
│       ├── unity_bridge\  ← 桥接包（rospy）
│       └── ros_tcp_endpoint\ ← endpoint 服务器
├── Unity_Ros2\            ← ROS2 Humble 版本（待配置）
└── docs\
    ├── xavier_setup.md
    └── docker_setup.md
```

---

## 尚未完成

- ⏳ `start_ros.bat` 快捷启动脚本
- ⏳ MRTK3 + HoloLens 2 部署
- ⏳ 配置 `Unity_Ros2`（ROS2 Humble 版本）
- ⏳ 场景完善（机器人模型、UI、小地图等）

---

## 关键命令

### 启动 Docker 容器

```powershell
docker start ros_noetic
docker exec -it ros_noetic bash
```

### 启动 ROS1 endpoint

```bash
source /opt/ros/noetic/setup.bash
cd /ros1_ws && source devel/setup.bash
roslaunch unity_bridge bridge.launch tcp_ip:=0.0.0.0 tcp_port:=10000
```

### Unity 连接配置

- ROS IP Address: `localhost`
- ROS Port: `10000`
