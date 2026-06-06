# Xavier NX ROS1 Noetic 环境搭建指南

Jetson Xavier NX 上需要安装 ROS1 Noetic + TCP 桥接包，以配合 Unity 项目通信。

---

## 1. ROS1 Noetic 安装（如果尚未安装）

```bash
# 设置 ROS Noetic 源
sudo sh -c 'echo "deb http://packages.ros.org/ros/ubuntu focal main" > /etc/apt/sources.list.d/ros-latest.list'

# 添加密钥
sudo apt-key adv --keyserver 'hkp://keyserver.ubuntu.com:80' --recv-key C1CF6E31E6BADE8868B172B4F42ED6FBAB17C654

# 安装 ROS Noetic 桌面完整版
sudo apt update
sudo apt install ros-noetic-desktop-full

# 环境初始化
echo "source /opt/ros/noetic/setup.bash" >> ~/.bashrc
source ~/.bashrc

# 安装 catkin 工具
sudo apt install python3-catkin-tools python3-rosdep
```

---

## 2. 创建 ROS1 工作空间

```bash
mkdir -p ~/ros1_ws/src
cd ~/ros1_ws/src
```

---

## 3. 安装 ROS1 版 ros_tcp_endpoint

ROS-TCP-Connector 仓库为 ROS1 和 ROS2 分别提供了不同的 endpoint 实现。ROS1 版位于仓库的 `endpoints/ROS1/` 目录下。

### 方式 A：从 GitHub 克隆（推荐）

```bash
cd ~/ros1_ws/src

# 克隆 ROS-TCP-Connector 仓库
git clone https://github.com/Unity-Technologies/ROS-TCP-Connector.git

# ROS1 版 endpoint 在 endpoints/ROS1/ 目录下
cp -r ROS-TCP-Connector/endpoints/ROS1/ros_tcp_endpoint .

# 清理不需要的文件
rm -rf ROS-TCP-Connector
```

### 方式 B：手动下载（GitHub 网络不好时）

从 Windows 端手动下载后拷贝到 Xavier：
- 下载地址：https://github.com/Unity-Technologies/ROS-TCP-Connector
- 目录：`endpoints/ROS1/ros_tcp_endpoint/`
- 将整个 `ros_tcp_endpoint/` 文件夹拷贝到 `~/ros1_ws/src/`

---

## 4. 部署 unity_bridge 包

```bash
cd ~/ros1_ws/src

# 从 Windows 项目复制过来（可通过 scp、U盘、git 等方式）
# 文件路径：D:\Unity_Ros\ros1_ws\src\unity_bridge
```

### 在 Windows 上发送到 Xavier

```powershell
# 在 Windows PowerShell 中执行，假设 Xavier IP 为 192.168.1.x
scp -r D:\Unity_Ros\ros1_ws\src\unity_bridge user@192.168.1.x:~/ros1_ws/src/
```

---

## 5. 安装仿真与导航依赖（按需）

```bash
# 必装：消息依赖
sudo apt install ros-noetic-geometry-msgs ros-noetic-nav-msgs ros-noetic-std-msgs ros-noetic-sensor-msgs

# 可选：仿真（TurtleBot3）
sudo apt install ros-noetic-turtlebot3 ros-noetic-turtlebot3-simulations

# 可选：导航栈
sudo apt install ros-noetic-move-base ros-noetic-amcl ros-noetic-map-server

# 可选：SLAM
sudo apt install ros-noetic-slam-toolbox ros-noetic-gmapping ros-noetic-hector-slam
```

---

## 6. 编译工作空间

```bash
cd ~/ros1_ws

# 安装依赖
rosdep install --from-paths src --ignore-src -r -y

# 编译
catkin_make

# 加载环境（每次开新终端都要执行）
source devel/setup.bash
```

---

## 7. 运行 TCP 桥接

```bash
# 终端 1：启动 TCP 桥接（监听 10000 端口）
rosrun ros_tcp_endpoint default_server_endpoint.py
```

**输出确认：**
```
[INFO] [1234567890.123]: [tcp_endpoint] Initializing ROS TCP endpoint server...
[INFO] [1234567890.456]: [tcp_endpoint] Listening on port 10000...
```

---

## 8. 验证通信

### 测试 1：Unity 连接

在 Unity Editor 中：
1. 打开 `MainScene`
2. 菜单 → `Robotics → ROS Settings`
3. 填写 `ROS IP`：Xavier 的 IP（如 `192.168.1.100`）
4. 填写 `ROS Port`：`10000`
5. 点击 Play

**预期：** Console 无报错，Unity 连接到 Xavier。

### 测试 2：手动发消息验证

```bash
# 终端 2：监听 /car_pose（应看到 Unity 发来的位姿）
rostopic echo /car_pose

# 终端 3：发送 /cmd_vel（控制 Unity 中的虚拟小车）
rostopic pub /cmd_vel geometry_msgs/Twist "linear: {x: 0.5, y: 0.0, z: 0.0}, angular: {x: 0.0, y: 0.0, z: 0.0}" -r 10
```

---

## 9. 启动仿真（可选）

```bash
# 终端 4：Gazebo 仿真（如需）
export TURTLEBOT3_MODEL=waffle
roslaunch turtlebot3_gazebo turtlebot3_world.launch

# 终端 5：SLAM（如需）
roslaunch slam_toolbox online_async_launch.py

# 终端 6：导航（如需）
roslaunch move_base move_base.launch
```

---

## 文件结构汇总

```
~/ros1_ws/src/
├── ros_tcp_endpoint/      ← ROS-TCP-Connector endpoints/ROS1
│   └── ...
└── unity_bridge/          ← 自定义桥接包
    ├── package.xml
    ├── setup.py
    ├── launch/bridge.launch
    └── unity_bridge/car_pose_subscriber.py
```

---

## 常见问题

### Q: Unity 连不上 Xavier？

- 确认 Xavier 和 PC 在同一局域网
- 检查 IP 是否正确：`ping 192.168.1.x`
- 检查 10000 端口是否开放：`nc -zv 192.168.1.100 10000`
- 检查 Xavier 防火墙：`sudo ufw status`

### Q: catkin_make 报错找不到包？

```bash
rosdep install --from-paths src --ignore-src -r -y
source devel/setup.bash
```

### Q: /car_pose 收不到消息？

- 检查 Unity Console 是否有 ROS 连接日志
- 确认 Unity 已点击 Play
- 确认 `roscore` 已运行：`rostopic list`
