# MRTK3 Play Mode 故障排查指南

## 一句话总结

**Standalone（编辑器）不需要 XR 设备加载。UWP（HoloLens 2）需要 OpenXR + 自动加载。两者不冲突但必须分别配置。**

---

## 关键档案（改了哪几个文件）

| 文件 | 作用 |
|------|------|
| `Assets/MRTK.Generated/MRTKProfile.asset` | MRTK3 子系统配置和启动列表 |
| `Assets/XR/XRGeneralSettings.asset` | XR Plug-in Management 按平台的 Loader 配置 |
| `ProjectSettings/EditorBuildSettings.asset` | 构建场景列表、XR 模拟设置引用 |
| `Assets/Editor/ConsoleFix.cs` | 启动时关 Error Pause，设 MRTKProfile.Instance |
| `Assets/MRTK.Generated/MRTKSyntheticHandsConfig.asset` | SyntheticHands 配置（勿删） |
| `Assets/MRTK.Generated/MRTKSettings.asset` | 各平台到 MRTKProfile 的映射 |

---

## 故障汇总

### 1. SyntheticHandsSubsystem 崩溃 / 报错

**现象**: `Configuration could not be retrieved for SyntheticHandsSubsystem` + `NullReferenceException in Provider.Start()`

**原因**: 两个条件同时触发才会崩溃：
- 1) SyntheticHandsSubsystem 在 `loadedSubsystems` 列表中（被 MRTKLifecycleManager 启动）
- 2) `subsystemConfigs` 中没有对应的配置项（或配置 GUID 失效）

**修复**:
- `MRTKProfile.asset` → `loadedSubsystems: []`（禁止 MRTKLifecycleManager 启动它）
- `MRTKProfile.asset` → `subsystemConfigs.entries` 保留 SyntheticHands 条目（`Register()` 能找到 config，不报 error）

**为什么已注册但不启动**: SyntheticHandsSubsystem 会在 domain reload 时自动注册（由 `[MRTKSubsystem]` 属性触发），这是正常的。只要 `loadedSubsystems` 为空，它就不会被创建/启动，只是"存在"而已。

---

### 2. XR Simulation 编辑器崩溃

**现象**:
```
NullReferenceException at SimulationEditorUtilities.CheckIsSimulationSubsystemEnabled()
```

**原因**: 移除了 Standalone 的 XR Simulation Loader 后，AR Foundation 编辑器代码在检测时崩溃。

**修复**: `XRGeneralSettings.asset` → Standalone Providers 的 `m_Loaders` **必须保留** Simulation Loader，即使 `m_AutomaticLoading` 和 `m_AutomaticRunning` 设为 0。

```
Standalone Providers:
  m_AutomaticLoading: 0    ← Loader 在列表中但不自动加载
  m_AutomaticRunning: 0
  m_Loaders:
    - Simulation Loader     ← 必须保留，否则 AR Foundation 编辑器代码崩
```

---

### 3. Error Pause 阻止 Play

**现象**: 按 Play 后编辑器暂停（黄色暂停条），实际是 `Debug.LogError` 触发了 Error Pause。

**原因**: `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 在 `[InitializeOnLoad]` 的 `delayCall` **之前** 执行。所以 `ConsoleFix` 如果只在 delayCall 里设 Error Pause，Register() 运行时还没生效。

**修复**: `ConsoleFix.cs` **构造函数直接** 设 EditorPref，不通过 delayCall。

```csharp
static ConsoleFix()
{
    EditorPrefs.SetBool("DeveloperConsoleErrorPause", false);
    // 同时设 MRTKProfile.Instance
    MRTKProfile.Instance = AssetDatabase.LoadAssetAtPath<MRTKProfile>(...);
}
```

---

### 4. StopSubsystems without initialized manager

**现象**:
```
Call to StopSubsystems without an initialized manager
```

**原因**: 正常。Standalone 没有启动任何 XR Loader（AutomaticLoading: 0），但 XR Manager 被初始化了（InitManagerOnStart: 1）。退出 Play 时 Manager 想停子系统但没东西可停。

**处理**: 忽略，无害。

---

### 5. 连不上 ROS（由于目标计算机积极拒绝）

**现象**: `Connection to localhost:10000 failed`

**原因**: Docker Desktop 没启动 / ros_noetic 容器没跑 / 端口映射不对。

**诊断**:
```
# 检查 Docker 是否在运行
docker ps

# 检查容器状态
docker ps -a --filter name=ros_noetic

# 如果容器不存在，先创建
docker run -it --name ros_noetic -v D:\Unity_Ros\ros1_ws:/ros1_ws -p 10000:10000 ros:noetic-ros-base

# 如果 workspace 没编译
docker exec ros_noetic bash -c "cd /ros1_ws && catkin_make"

# 启动
start_ros.bat
```

---

## 平台区别（Standalone vs UWP）

| | Standalone（编辑器 Play） | UWP（HoloLens 2） |
|:--|:------------------------:|:-----------------:|
| **XR Loader** | Simulation Loader (auto=0) | OpenXR Loader (auto=1) |
| **手部追踪** | MRTKInputSimulator（模拟） | OpenXRHandsSubsystem（真实） |
| **SyntheticHands** | 注册但不启动 | 注册但不启动 |
| **MRTKProfile** | key:1 (Standalone) | key:14 (UWP) |
| **自动加载** | 关闭 | 开启 |

**核心区别**: Editor 用 Simulation Loader 但禁止自动加载（只是为了不崩 AR Foundation）。HoloLens 2 用 OpenXR 且必须自动加载。两者配置互不干扰，通过 XR Plug-in Management 按平台分别设置。

---

## 什么不能改

- ❌ **不能删 Standalone 的 Simulation Loader** — AR Foundation 编辑器代码会崩
- ❌ **不能删 `MRTKProfile.asset` 的 `subsystemConfigs` SyntheticHands 条目** — 否则 Register() 报 LogError
- ❌ **不能把 `loadedSubsystems` 设回包含 SyntheticHands** — Provider.Start() 会 NullReferenceException
- ✅ **Safe: `MRTKProfileSetup.cs` 可以删/禁用** — 它的功能已经被 ConsoleFix 覆盖
- ✅ **Safe: `MRTKInitializer.cs` 可以删** — 不再需要
- ✅ **Safe: `Resources/MRTKProfile.asset` 可以删** — 与 MRTK.Generated/ 的冲突
- ✅ **Safe: `MRTK3AutoFix.EnableXRSimulationLoader()` 可以删掉** — 禁止脚本手动启用 Simulation Loader

---

## 调试口诀

```
Error Pause 在构造设
Standalone Loader 不能删
Synthetic 不启动但要有 config
UWP 用 OpenXR 自动加载
Play 退出报 StopSubsystems -> 忽略
```
