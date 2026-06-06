# Manual Unity Editor Installation Guide

适用于从有网络的电脑下载 Unity Editor 并手动安装到本机。

## 适用场景
Unity Hub 国内下载失败时使用此方案。

## 前提
- Unity Hub 已安装（`/usr/bin/unityhub`）
- 目标版本：**Unity 6000.3.16f1 LTS**（changeset: `a56f230f6470`）

---

## Step 1: 在有网络的电脑上下载

下载 Linux 版 Unity Editor（~3.9 GB tar.xz）：

```
https://download.unity3d.com/download_unity/a56f230f6470/LinuxEditorInstaller/Unity-6000.3.16f1.tar.xz
```

建议用支持断点续传的工具：
```bash
# aria2c（推荐，多线程）
aria2c -x 4 -s 4 --continue \
  "https://download.unity3d.com/download_unity/a56f230f6470/LinuxEditorInstaller/Unity-6000.3.16f1.tar.xz"

# 或 wget
wget -c "https://download.unity3d.com/download_unity/a56f230f6470/LinuxEditorInstaller/Unity-6000.3.16f1.tar.xz"
```

> 如果在国内下载失败，需要开启代理/VPN（全局+TUN模式，节点选择美国/日本/澳大利亚）。

## Step 2: 传输到本机

将下载好的 `Unity-6000.3.16f1.tar.xz` 文件传输到本机：
- **U盘**：拷贝到本机任意目录
- **SCP**：`scp user@remote:/path/to/Unity-6000.3.16f1.tar.xz /tmp/`
- **其他方式**：微信/QQ/网盘等

## Step 3: 询问用户安装路径

**必须先向用户确认安装目标路径**，不要使用默认路径。

示例问题：
> "Unity Editor 安装到哪个目录？（例如：/home/xxx/Unity/Hub/Editor/6000.3.16f1）"

等待用户输入后再继续。

## Step 4: 创建目录并解压

```bash
mkdir -p "<用户指定的路径>"
tar -xf /path/to/Unity-6000.3.16f1.tar.xz -C "<用户指定的路径>"
```

解压后目录结构示例：
```
<用户指定的路径>/
├── Editor/
│   ├── Unity
│   ├── UnityEditor
│   └── ...
├── MonoBleedingEdge/
├── PlaybackEngines/
└── ...
```

## Step 5: 验证安装

```bash
ls "<用户指定的路径>/Editor/Unity"
```

如果有输出且文件可执行，说明安装成功。

## Step 6: 注册到 Unity Hub

Unity Hub 会自动检测 `~/Unity/Hub/Editor/` 下的编辑器。

如果用户选择了非标准路径，需要手动添加：
1. 打开 Unity Hub
2. Preferences → Editor
3. 点击 **Browse** 添加 `<用户指定的路径>/Editor/Unity`

## Step 7: 打开项目验证

在 Unity Hub 中打开项目：
```
/media/xuhao/AA9448619448325D/Unity_Ros/UnityProject/
```

Unity 会提示升级项目版本，确认即可。

---

## 常见问题

### Q: 解压后 Unity Hub 检测不到编辑器？
A: 检查目录结构是否正确。核心要求是 `<路径>/Editor/Unity` 存在。如果用了非标准路径，手动在 Unity Hub 中添加。

### Q: 打开项目时报错？
A: Unity 6000.3 会升级 2022 LTS 项目，这是正常的。如果 ROS-TCP-Connector 报错，需要在 Package Manager 中重新导入。

### Q: 文件传输很慢？
A: ~3.9GB 的文件，USB 3.0 约 2-3 分钟，SCP 局域网约 5-10 分钟。


# docker
Docker Desktop 已经在后台运行（你之前成功 pull 和 run 了容器）。
要进入容器终端，重新打开 PowerShell 执行：
docker exec -it ros_noetic bash
然后进入后 source ROS 环境 + 启动 launch：
```bash
source /opt/ros/noetic/setup.bash
cd /ros1_ws && source devel/setup.bash
roslaunch unity_bridge bridge.launch tcp_ip:=0.0.0.0 tcp_port:=10000
```
如果需要两个终端：
- 终端 1：运行 endpoint（上面那条命令）
- 终端 2：新开 PowerShell → docker exec -it ros_noetic bash → 发测试数据
快速验证容器是否在运行
docker ps
如果没看到 ros_noetic，先启动它：
docker start -ai ros_noetic
然后 source + roslaunch。

## 平常关闭
顺序无所谓，推荐：
 1. 容器内按 Ctrl + C 停掉 roslaunch
 2. 退出容器（如果 attach 了）
exit
 3. 停止容器
docker stop ros_noetic
 4. 关掉 Docker Desktop（也可以不关，放后台无所谓）
也可以直接关掉 PowerShell 窗口和 Docker Desktop，但 推荐手动 stop 容器，避免下次启动时报异常。
重新打开
 1. 打开 Docker Desktop（如果关掉了）
 2. 启动容器
docker start ros_noetic

3. 进入容器
docker exec -it ros_noetic bash

4. 启动 endpoint
```bash
source /opt/ros/noetic/setup.bash
cd /ros1_ws && source devel/setup.bash
roslaunch unity_bridge bridge.launch tcp_ip:=0.0.0.0 tcp_port:=10000
```