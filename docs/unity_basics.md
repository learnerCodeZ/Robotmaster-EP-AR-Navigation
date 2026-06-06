# Unity 基础指南

> 针对本项目（Unity + ROS1 Noetic 导航系统）的完整入门文档

---

## 第一部分：Unity 项目结构详解

### 为什么是这种结构？

Unity 项目不像普通软件那样用 `.sln` / `.csproj` 作为入口，而是用一个**项目文件夹**，里面包含若干预定义子目录。Unity Hub 打开项目时，它识别的是项目根目录（包含 `Assets/` 和 `Packages/` 的文件夹）。

### `D:\Unity_Ros\UnityProject\` 下各文件夹作用

#### Assets/（核心资源目录）

这是唯一需要你手动操作的目录，所有项目资源都在这里：

| 子目录 | 作用 | 当前内容 |
|---|---|---|
| `Assets/Scripts/` | C# 脚本文件（项目逻辑） | 7 个 `.cs` 文件（见下方） |
| `Assets/Scenes/` | 场景文件（.unity） | `MainScene.unity` |
| `Assets/Resources/` | 运行时可加载的资源 | `ROSConnectionPrefab.prefab`、`GeometryCompassSettings.asset` |
| `Assets/Materials/` | 材质文件 | 目前为空 |
| `Assets/Prefabs/` | 预制体（可复用的游戏对象模板） | 目前为空 |

#### Packages/（包管理）

只包含两个文件：

- `manifest.json` — 声明项目依赖的所有包（类似 `package.json`）
- `packages-lock.json` — 锁定实际安装的包版本

当前依赖：
```
com.unity.ugui                         → Unity UI 系统
com.unity.modules.jsonserialize         → JsonUtility（必需）
com.unity.modules.imageconversion       → Texture2D（必需）
com.unity.robotics.ros-tcp-connector    → ROS 通讯包（本地文件引用）
com.unity.robotics.visualizations       → ROS 可视化包（本地文件引用）
```

#### ProjectSettings/（项目全局设置）

20 个 `.asset` 文件，Unity 编辑器自动生成和管理，通常不需要手动编辑：

| 文件 | 作用 |
|---|---|
| `ProjectVersion.txt` | Unity 版本：`2022.3.62f1` |
| `TimeManager.asset` | 时间设置（Fixed Timestep = 0.02s） |
| `DynamicsManager.asset` | 物理引擎设置（重力、碰撞检测等） |
| `InputManager.asset` | 输入按键映射（WASD、Space 等） |
| `QualitySettings.asset` | 渲染质量设置 |
| `TagManager.asset` | 标签和层定义 |
| `EditorBuildSettings.asset` | 构建场景列表 |
| 其他 | 音频、内存、网络等系统配置 |

#### Library/（编译缓存）

Unity 自动生成的缓存，**可安全删除**（删除后 Unity 会自动重建，但首次打开会很慢）：

| 子目录 | 作用 |
|---|---|
| `PackageCache/` | 包缓存（从 registry 下载的包） |
| `ScriptAssemblies/` | 编译后的 C# 程序集 |
| `ShaderCache/` | 着色器编译缓存 |
| `ArtifactDB/` | 构建产物数据库 |

#### Logs/（日志）

只有 `AssetImportWorker` 相关的导入日志，正常情况下很干净。

#### Temp/（临时文件）

Unity 运行时生成的临时数据，关闭 Unity 后通常会自动清理。

#### UserSettings/（用户设置）

编辑器布局、窗口位置等个人偏好设置。

### 为什么 ROS-TCP-Connector 在项目外面？

```
D:\Unity_Ros\
├── ROS-TCP-Connector\        ← 包代码（本地引用）
│   └── com.unity.robotics.ros-tcp-connector\
└── UnityProject\             ← Unity 项目
    └── Packages\manifest.json  → 引用上面的路径
```

`manifest.json` 里的写法：
```json
"com.unity.robotics.ros-tcp-connector": "file:D:/Unity_Ros/ROS-TCP-Connector/com.unity.robotics.ros-tcp-connector"
```

这是**本地文件引用**。放在项目外面的好处是：多个 Unity 项目可以共享同一份 ROS-TCP-Connector 代码（Unity_Ros 和 Unity_Ros2 都引用它）。

### 7 个脚本的分工

| 脚本 | 挂载对象 | 作用 |
|---|---|---|
| `VirtualCarController.cs` | RobotCar | 接收速度指令，驱动小车移动和旋转 |
| `RosSubscriber.cs` | RobotCar | 订阅 `/cmd_vel`（Twist），调用 SetVelocity |
| `RosOdomSubscriber.cs` | RobotCar | 订阅 `/odom`（Odometry），更新小车位置 |
| `RosPublishCarPose.cs` | RobotCar | 每 0.1s 发布 `/car_pose`（PoseStamped） |
| `RosUtils.cs` | 无（静态类） | ROS ↔ Unity 坐标系转换 + ROS 时间戳工具 |
| `TrajectoryRenderer.cs` | Trajectory | 每 0.2s 采样位置，用 LineRenderer 画轨迹线 |
| `MapDisplay.cs` | 无（在 UI Canvas 上） | 订阅 `/map`（OccupancyGrid），绘制地图纹理 |

---

## 第二部分：Unity 常用操作

### 1. 首次打开项目（预计 2-5 分钟）

```
Unity Hub → Open → 选择 D:\Unity_Ros\UnityProject → 等待编译完成
```

- 右下角显示编译进度条（第一次会很慢，需要编译所有脚本 + 导入资源）
- 编译完成后 Console 会出现 3 条黄色警告（关于 MRTK 和 Visual Studio 可以忽略）
- 红色错误需要修复

### 2. 编辑器界面布局

Unity 编辑器由多个面板组成：

```
┌──────────────────────────────────────────────────────┐
│  菜单栏（File Edit View GameObject...）               │
├──────────────────┬──────────────┬────────────────────┤
│  Hierarchy       │  Scene       │  Inspector         │
│  （场景层级）      │  （3D 视图）  │  （属性面板）       │
│                  │              │                    │
│  - Main Camera   │              │  [选中对象的详细属性]│
│  - RobotCar      │              │                    │
│  - Ground        │              │                    │
│                  │              │                    │
├──────────────────┼──────────────┤                    │
│  Project         │  Console     │                    │
│  （项目文件）      │  （日志输出）  │                    │
├──────────────────┴──────────────┴────────────────────┤
│  [▶ Play]  [⏸ Pause]  [⏭ Step]                      │
└──────────────────────────────────────────────────────┘
```

| 面板 | 作用 | 常用操作 |
|---|---|---|
| **Hierarchy** | 列出当前场景所有对象 | 双击聚焦、右键新建对象、拖动排列父子关系 |
| **Scene** | 3D 场景视图，观察和编辑场景 | 右键旋转视角、中键平移、滚轮缩放、选中后 F 键聚焦 |
| **Inspector** | 显示选中对象的所有属性 | 修改数值、添加组件、启用/禁用组件 |
| **Project** | 浏览 Assets 目录 | 双击打开文件夹、拖拽资源到场景 |
| **Console** | 查看日志、警告、错误 | 双击错误跳转到对应代码行、Clear 清空 |

### 3. 场景导航（Scene 视图）

| 操作 | 方法 |
|---|---|
| 旋转视角 | 右键 + 拖动 |
| 平移视角 | 中键 + 拖动 |
| 缩放 | 滚轮 |
| 聚焦选中对象 | 按 `F` 键 |
| 移动视角 | WASD（配合右键） |
| 上下视角 | `Q` / `E` |

### 4. GameObject 操作

选中场景中的对象后，使用顶部工具栏或快捷键：

| 工具 | 快捷键 | 作用 |
|---|---|---|
| Move | `W` | 移动对象（拖动 XYZ 轴） |
| Rotate | `E` | 旋转对象 |
| Scale | `R` | 缩放对象 |
| Rect | `T` | 2D 变换 |
| Transform | `Y` | 综合变换 |

### 5. Play 模式（运行）

顶部中间的 ▶ 按钮：

| 按钮 | 快捷键 | 作用 |
|---|---|---|
| ▶ Play | `Ctrl + P` | 启动运行模式 |
| ⏸ Pause | `Ctrl + Shift + P` | 暂停所有时间（包括 Time.time） |
| ⏭ Step | `Ctrl + Alt + P` | 逐帧前进（调试用） |

**时间概念：**

Play 模式下，Unity 按实时运行（默认 60fps），每个脚本按特定顺序执行：

```
每帧执行（60次/秒）：
  Update()      → 普通逻辑（移动、输入检测、计时器）
  LateUpdate()  → 跟随相机等（在所有 Update 之后）

每物理帧执行（50次/秒）：
  FixedUpdate()  → 物理相关逻辑（刚体移动、碰撞检测）

时间属性：
  Time.time              → 从 Play 开始的累计秒数
  Time.deltaTime         → 上一帧到这一帧的秒数（如 0.016s）
  Time.fixedDeltaTime    → 物理帧固定步长（默认 0.02s = 50Hz）
  Time.fixedTime         → 物理帧累计步数
```

在我们项目中：
- `VirtualCarController.FixedUpdate()` → 用 `Time.fixedDeltaTime` 移动物体
- `TrajectoryRenderer.Update()` → 用 `Time.deltaTime` + `sampleInterval` 控制采样
- `RosPublishCarPose.Update()` → 用 `Time.deltaTime` + `publishRate` 控制发布频率

### 6. ROS 配置

菜单栏 → **Robotics → ROS Settings**

| 字段 | 值 | 说明 |
|---|---|---|
| ROS IP Address | `localhost` | 连接本地 Docker 容器 |
| ROS Port | `10000` | TCP 端口 |

保存后会在 `Assets/Resources/` 生成 `ROSConnectionPrefab.prefab`，包含连接配置。

### 7. 查看当前场景

双击 `Assets/Scenes/MainScene.unity` 打开场景。

场景层级（Hierarchy 面板）：
```
MainScene
├── Main Camera         → 跟随拍摄用
├── Directional Light   → 平行光照
├── Ground              → 地面（Plane，10x10 单位）
└── RobotCar            → 小车（Cube）
    ├── [VirtualCarController]    → 移动控制
    ├── [RosSubscriber]           → 接收 /cmd_vel
    ├── [RosOdomSubscriber]       → 接收 /odom
    └── [RosPublishCarPose]       → 发布 /car_pose
    └── Trajectory                → 轨迹渲染子对象
        └── [TrajectoryRenderer]  → LineRenderer 画线
```

### 8. 包管理器

菜单栏 → **Window → Package Manager**

可以查看当前项目安装的所有包。本项目安装了：
- Unity 内置模块（UGUI、JsonSerialize、ImageConversion 等）
- ROS-TCP-Connector（本地引用）
- Visualizations（本地引用）

---

## 第三部分：你应该知道的常识

### 1. Unity 是组件式架构

Unity 不使用传统的 OOP 继承链，而是用**组合**：

```
GameObject（游戏对象）
  ├── Transform（必有）     → 位置、旋转、缩放
  ├── MeshRenderer          → 渲染模型
  ├── Collider              → 碰撞检测
  └── YourScript            → 你写的逻辑
```

**脚本只是组件之一**。没有挂脚本的对象（如 Ground、Directional Light）不会执行任何逻辑。

### 2. 脚本生命周期

```
Awake()          → 对象创建时（只执行一次）
Start()          → 第一帧执行（只执行一次）
Update()         → 每帧执行（60次/秒）
LateUpdate()     → 所有 Update 之后（60次/秒）
FixedUpdate()    → 物理帧执行（50次/秒）
OnDestroy()      → 对象销毁时
```

### 3. 八个脚本的分工

| 脚本 | 职责 |
|---|---|
| `VirtualCarController` | 接收速度指令，驱动小车前进/旋转（物理移动） |
| `RosSubscriber` | 订阅 `/cmd_vel`，把 Twist 数据传给 VirtualCarController |
| `RosOdomSubscriber` | 订阅 `/odom`，更新小车位置（用于外部控制小车位置） |
| `RosPublishCarPose` | 每 0.1s 发布 `/car_pose`，告诉 ROS 小车在哪 |
| `RosUtils` | 静态工具类：坐标系转换 + ROS 时间戳 |
| `TrajectoryRenderer` | 每 0.2s 采样位置，画轨迹线 |
| `MapDisplay` | 订阅 `/map`，把 OccupancyGrid 转为 Texture2D 显示 |

### 4. ROS vs Unity 坐标系

```
ROS:  X 前、Y 左、Z 上（右手坐标系）
Unity: X 右、Y 上、Z 前（左手坐标系）
```

转换由 `RosUtils.cs` 处理：

| 方法 | 用途 |
|---|---|
| `UnityToRosPosition()` | Unity 位置 → ROS 位置（用于 /car_pose） |
| `RosToUnityPosition()` | ROS 位置 → Unity 位置（用于 /odom、/map） |
| `UnityToRosRotation()` | Unity 旋转 → ROS 旋转 |
| `RosToUnityRotation()` | ROS 旋转 → Unity 旋转 |
| `GetRosTimeNow()` | 将 Windows 当前时间转为 ROS TimeMsg |

### 5. TCP 通讯原理

```
Unity (Windows)
    │  ROSConnection 组件
    │  TCP Client → localhost:10000
    ↓
Docker Container
    │  ros_tcp_endpoint (TcpServer)
    │  Python rospy 节点
    ↓
ROS1 Master
    │  roscore
    ↓
ROS Nodes
    （/cmd_vel、/odom、/car_pose）
```

- Unity 通过 `ROSConnection.GetOrCreateInstance()` 连接到 `localhost:10000`
- `ros_tcp_endpoint` 接收消息，转发给 ROS 节点
- ROS 节点的消息也通过这个 TCP 连接发回 Unity

### 6. Topic 流程

Unity 启动时自动完成：

```
1. ROSConnection.GetOrCreateInstance()
   → 加载 ROSConnectionPrefab（如果有的话）
   → 连接到 localhost:10000

2. 调用 Subscribe<T>(topic, callback)
   → Unity 向 endpoint 发送注册请求
   → endpoint 在 ROS 端创建订阅者
   → Docker 日志显示: RegisterSubscriber(/cmd_vel, Twist) OK

3. ROS 端发消息
   → endpoint 通过 TCP 转发给 Unity
   → Unity 调用你的 callback 函数
   → Docker 日志无变化（因为是 Unity 接收）

4. 调用 RegisterPublisher<T>(topic) + Publish<T>(topic, msg)
   → Unity 每 0.1s 发一次 /car_pose
   → endpoint 在 ROS 端创建发布者并转发消息
```

### 7. ROS1 vs ROS2

| 对比项 | ROS1 (Noetic) | ROS2 (Humble) |
|---|---|---|
| 版本 | 最终版 2023.5 停止维护 | 持续更新中 |
| 运行方式 | 需要 roscore | 无需中心节点 |
| 语言 | rospy (Python) | rclpy (Python) |
| 构建工具 | catkin_make | colcon build |
| 通讯 | TCP（自定义） | DDS（标准协议） |
| 包管理 | apt (ROS 官方) | apt + rosdep |

当前项目用 ROS1，因为：
- Xavier NX 已装好 ROS1 Noetic
- ROS1 的 TCP 通讯协议与 ROS-TCP-Connector 兼容
- `Unity_Ros2` 项目会切换到 ROS2 Humble

### 8. 为什么用 Docker

Windows 不能原生跑 ROS1。三种方案对比：

| 方案 | 优点 | 缺点 |
|---|---|---|
| WSL2 + 原生 ROS1 | 性能最好 | 容易出版本兼容问题 |
| Docker Desktop | 隔离好、快、易恢复 | 需要学一点 Docker 命令 |
| 虚拟机 | 完整 Linux 环境 | 资源占用大、慢 |

Docker 的核心概念：

```
镜像（Image）    → 只读的系统模板（ros:noetic-ros-base）
容器（Container）→ 镜像的运行实例（ros_noetic）
卷（Volume）     → 挂载 Windows 文件夹（-v D:\Unity_Ros\ros1_ws:/ros1_ws）
端口映射         → 容器的 10000 端口映射到 Windows 的 localhost:10000
```

### 9. 文件修改后需重新编译

| 修改类型 | 处理方式 |
|---|---|
| C# 脚本（`.cs`） | Unity 自动重新编译（几秒） |
| 场景文件（`.unity`） | 无需编译，立即生效 |
| 材质/纹理 | 无需编译，立即生效 |
| ROS Python 代码 | 容器内重新运行 launch 即可（不需要编译） |
| ROS 包结构变更 | 需要在容器内重新 `catkin_make` |

### 10. Unity 与 ROS 的时间关系

| 概念 | Unity | ROS1 |
|---|---|---|
| 当前时间 | `Time.time`（Play 开始后秒数，从 0 开始） | `rospy.Time.now()`（从 1970 年 Unix 纪元） |
| 时间戳消息 | 无内置时间戳类型 | `HeaderMsg.stamp` = `TimeMsg{sec, nanosec}` |
| 帧间隔 | `Time.deltaTime`（两帧之间秒数） | 无帧概念（事件驱动） |
| 固定步长 | `Time.fixedDeltaTime`（默认 0.02s = 50Hz） | `rospy.Rate(10)`（10Hz 循环） |
| 暂停 | Play 模式 Pause 按钮 | `rospy.spin()` 无暂停（需要手动控制） |

- ROS 的 `Header.stamp` 包含消息产生的时间戳
- Unity 的 `Time.time` 在 Play 开始时为 0，与 ROS 时间无关
- `RosUtils.GetRosTimeNow()` 将 Windows UTC 时间转为 ROS `TimeMsg`（用于 /car_pose 的 Header.stamp）
- **Real-time ≠ Game-time**：Play 模式下 Unity 按实时运行，帧率影响 `Update()` 调用次数，但不影响 `FixedUpdate()` 的固定步长
