# Docker Desktop + ROS1 Noetic 环境搭建

## 1. 安装 Docker Desktop

1. 下载 [Docker Desktop for Windows](https://www.docker.com/products/docker-desktop/)
2. 安装后启动
3. 进入 **Settings → General** → 勾选 **"Use WSL 2 based engine"**
4. 重启 Docker Desktop，右下角显示绿色 Running 即可

### 验证安装

```powershell
docker --version
docker ps
```

---

## 2. 配置镜像加速（中国用户）

Docker Desktop → Settings → Docker Engine → 添加 `registry-mirrors`：

```json
{
  "registry-mirrors": [
    "https://docker.m.daocloud.io",
    "https://dockerproxy.com",
    "https://mirror.ccs.tencentyun.com"
  ]
}
```

点击 **Apply & Restart**。

---

## 3. 拉取 ROS1 Noetic 镜像

```powershell
docker pull ros:noetic-ros-base
```

常用镜像对比：

| 镜像 | 大小 | 说明 |
|---|---|---|
| `ros:noetic-ros-base` | ~600MB | 最小化，无 GUI，适合开发 |
| `ros:noetic-desktop-full` | ~2GB | 完整版含 rviz/gazebo，需要时再拉 |

---

## 4. 创建并启动容器

```powershell
docker run -it --name ros_noetic -v D:\Unity_Ros\ros1_ws:/ros1_ws -p 10000:10000 ros:noetic-ros-base
```

参数说明：

| 参数 | 作用 |
|---|---|
| `-it` | 交互模式 + 分配终端，可输入命令 |
| `--name ros_noetic` | 容器命名为 `ros_noetic` |
| `-v D:\Unity_Ros\ros1_ws:/ros1_ws` | 将 Windows 的 `ros1_ws` 挂载到容器内 `/ros1_ws` |
| `-p 10000:10000` | 将容器内 10000 端口映射到 Windows 的 10000 端口 |

> `-v` 挂载后，Windows 上修改的代码会**实时同步**到容器内，无需复制。

---

## 5. 容器内首次配置

进入容器后依次执行：

```bash
# 安装 Python 工具链（解决 python3 → python 问题）
apt update
apt install -y python-is-python3 python3-catkin-tools python3-pip
pip3 install rospkg

# 编译 ROS 工作区
cd /ros1_ws
catkin_make
source devel/setup.bash

# 启动 ROS1 endpoint
roslaunch unity_bridge bridge.launch tcp_ip:=0.0.0.0 tcp_port:=10000
```

看到以下日志即成功：

```
[INFO]: Starting server on 0.0.0.0:10000
```

此时 Unity 可以通过 `localhost:10000` 连接到容器内的 ROS。

---

## 6. 日常使用命令

### 启动已停止的容器

```powershell
docker start ros_noetic
docker attach ros_noetic
```

### 后台启动（不进入交互终端）

```powershell
docker start ros_noetic
```

### 进入正在运行的容器

```powershell
docker exec -it ros_noetic bash
```

### 查看容器列表

```powershell
docker ps -a
```

### 查看容器日志

```powershell
docker logs ros_noetic
docker logs -f ros_noetic  # 实时跟踪
```

### 停止容器

```powershell
docker stop ros_noetic
```

### 删除并重建容器

```powershell
docker rm -f ros_noetic
```

然后重新执行 `docker run` 创建新容器。

---

## 7. 文件挂载原理

```
Windows D:\Unity_Ros\ros1_ws\  ←→  Container /ros1_ws/
```

| Windows 路径 | 容器内路径 | 说明 |
|---|---|---|
| `D:\Unity_Ros\ros1_ws\src\` | `/ros1_ws/src/` | 源码目录，Windows 上编辑 |
| 编译产物 `build/`、`devel/` | 容器内 `/ros1_ws/build/`、`devel/` | 只在容器内生成 |

> 在 Windows 上用 VSCode 编辑代码，容器内编译运行，两者同步。

---

## 8. 端口映射原理

```
Unity (Windows)
    ↕ localhost:10000
Docker Desktop (端口映射)
    ↕ 容器内 0.0.0.0:10000
ROS1 TCP Endpoint
```

Unity 的 ROS Settings 中填写：
- **ROS IP Address**: `localhost`
- **ROS Port**: `10000`

---

## 9. 未来扩展：ROS2 Humble 共存

```powershell
docker pull ros:humble-ros-base

docker run -it --name ros2_humble -v D:\Unity_Ros2:/ros2_ws -p 10000:10000 ros:humble-ros-base
```

两个容器完全隔离，互不干扰：

| 容器 | ROS 版本 | 用途 |
|---|---|---|
| `ros_noetic` | ROS1 Noetic | 真实 Xavier 机器人通信 |
| `ros2_humble` | ROS2 Humble | 仿真/硬件开发 |

---

## 10. 常见问题

### Q: `apt update` 很慢？

```bash
sed -i 's/archive.ubuntu.com/mirrors.aliyun.com/g' /etc/apt/sources.list
apt update
```

### Q: `catkin_make` 报找不到某个包？

```bash
apt install ros-noetic-<package_name>
# 例如：
apt install ros-noetic-geometry-msgs
apt install ros-noetic-nav-msgs
```

### Q: Unity 连接不上？

1. 确认容器正在运行：`docker ps`
2. 确认端口映射：`docker port ros_noetic`
3. 测试连通性：在容器内 `curl localhost:10000` 或在 Windows 用浏览器访问 `http://localhost:10000`
4. 如果 `localhost` 不行，用容器 IP：`docker inspect ros_noetic | findstr IPAddress`

### Q: 想用 VSCode 编辑容器内代码？

安装 [Dev Containers](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) 插件，可以打开挂载的容器代码目录。

---

## 附录：Docker 常用命令速查

```powershell
# 镜像管理
docker images                     # 列出本地镜像
docker rmi <image>                # 删除镜像

# 容器管理
docker ps                         # 查看运行中的容器
docker ps -a                      # 查看所有容器（含停止的）
docker rm <container>             # 删除容器
docker rm -f <container>          # 强制删除

# 容器交互
docker exec -it <container> bash  # 进入容器
docker attach <container>         # 附加到容器主进程
docker logs <container>           # 查看日志

# 网络
docker port <container>           # 查看端口映射
docker inspect <container>        # 查看容器详细信息
```
