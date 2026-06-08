# MRTK3 安装指南（Unity 2022.3 + HoloLens 2）

## 1. MRTK3 简介

### 什么是 MRTK3

MRTK3（Mixed Reality Toolkit 3）是微软官方的混合现实开发框架，为 Unity 提供 HoloLens 2 和 VR 设备的完整开发工具链。

### 核心功能

| 模块 | 功能 |
|------|------|
| **Input（输入系统）** | 手部追踪、眼动追踪、控制器输入、语音命令 |
| **Spatial Awareness（空间感知）** | 网格扫描、场景理解、空间映射 |
| **UX Components（UI 组件）** | 3D 按钮、滑块、菜单、对话框、工具提示 |
| **Spatial Manipulation（空间操作）** | 物体抓取、缩放、旋转、边界控制 |
| **Solvers（求解器）** | 自动对齐、跟随、吸附、方向约束 |

### 对本项目的作用

在 Robotmaster EP AR Navigation 项目中，MRTK3 提供：

- **HoloLens 2 手部交互**：空气点击（Air Tap）替代鼠标点击来设置导航目标点
- **空间 UI**：在 3D 空间中显示 ROS 连接状态、速度、位置等信息
- **空间定位**：将 AR 内容锚定在真实世界中
- **手势交互**：抓取和操作导航路径可视化

---

## 2. 前置条件

| 条件 | 版本 | 说明 |
|------|------|------|
| Unity Editor | 2022.3.62f1 | 必须使用此版本 |
| UWP Build Support | - | Unity Hub → Installs → Add Modules → Universal Windows Platform Build Support |
| .NET SDK | 6.0+ | Windows 自带或手动安装 |

---

## 3. 完整安装流程（可复现）

### Step 1: 下载 MRTK3 核心包（6 个 .tgz）

从 [MRTK3 GitHub Releases](https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases) 下载以下 6 个包：

```powershell
# 下载目录
$mrtkDir = "D:\Unity_Ros\UnityProject\Packages\mrtk3"
New-Item -ItemType Directory -Path $mrtkDir -Force

# 下载 6 个 MRTK3 包（v3.0.0）
Start-BitsTransfer -Source "https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases/download/v3.0.0/org.mixedrealitytoolkit.core-3.0.0.tgz" -Destination $mrtkDir
Start-BitsTransfer -Source "https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases/download/v3.0.0/org.mixedrealitytoolkit.input-3.0.0.tgz" -Destination $mrtkDir
Start-BitsTransfer -Source "https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases/download/v3.0.0/org.mixedrealitytoolkit.spatialmanipulation-3.0.0.tgz" -Destination $mrtkDir
Start-BitsTransfer -Source "https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases/download/v3.0.0/org.mixedrealitytoolkit.standardassets-3.0.0.tgz" -Destination $mrtkDir
Start-BitsTransfer -Source "https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases/download/v3.0.0/org.mixedrealitytoolkit.uxcomponents-3.0.0.tgz" -Destination $mrtkDir
Start-BitsTransfer -Source "https://github.com/MixedRealityToolkit/MixedRealityToolkit-Unity/releases/download/v3.0.0/org.mixedrealitytoolkit.uxcore-3.0.0.tgz" -Destination $mrtkDir
```

### Step 2: 下载 Graphics Tools（1 个 .tgz）

```powershell
# Graphics Tools v0.7.1（与 MRTK3 兼容的版本）
Start-BitsTransfer -Source "https://github.com/microsoft/MixedReality-GraphicsTools-Unity/releases/download/v0.7.1/com.microsoft.mrtk.graphicstools.unity-0.7.1.tgz" -Destination $mrtkDir
```

> **注意**：MRTK3 要求 Graphics Tools >= 0.5.12，但 GitHub Releases 只有 v0.5.0、v0.7.1、v0.8.1 的预构建 .tgz。使用 v0.7.1 可满足版本要求。

### Step 3: 验证文件完整性

```powershell
# 检查 7 个 .tgz 文件是否完整
Get-ChildItem "$mrtkDir\*.tgz" | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    $isValidGzip = ($bytes[0] -eq 31) -and ($bytes[1] -eq 139)
    Write-Host "$($_.Name) - Size: $([math]::Round($_.Length/1MB,1)) MB, Valid GZip: $isValidGzip"
}
```

最终 `Packages/mrtk3/` 目录应有 7 个文件：

| 文件 | 大小 |
|------|------|
| `org.mixedrealitytoolkit.core-3.0.0.tgz` | ~0.1 MB |
| `org.mixedrealitytoolkit.input-3.0.0.tgz` | ~2.7 MB |
| `org.mixedrealitytoolkit.spatialmanipulation-3.0.0.tgz` | ~0.2 MB |
| `org.mixedrealitytoolkit.standardassets-3.0.0.tgz` | ~4 MB |
| `org.mixedrealitytoolkit.uxcomponents-3.0.0.tgz` | ~0.1 MB |
| `org.mixedrealitytoolkit.uxcore-3.0.0.tgz` | ~0.1 MB |
| `com.microsoft.mrtk.graphicstools.unity-0.7.1.tgz` | ~16 MB |

### Step 4: 更新 manifest.json

打开 `Packages/manifest.json`，在 `dependencies` 中添加以下内容：

```json
{
  "dependencies": {
    "com.unity.inputsystem": "1.14.0",
    "com.unity.robotics.ros-tcp-connector": "file:D:/Unity_Ros/ROS-TCP-Connector/com.unity.robotics.ros-tcp-connector",
    "com.unity.robotics.visualizations": "file:D:/Unity_Ros/ROS-TCP-Connector/com.unity.robotics.visualizations",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.ugui": "1.0.0",
    "com.unity.xr.arfoundation": "5.0.5",
    "com.unity.xr.core-utils": "2.1.0",
    "com.unity.xr.interaction.toolkit": "2.2.0",
    "com.unity.xr.management": "4.2.1",
    "com.unity.xr.openxr": "1.8.2",
    "com.unity.modules.animation": "1.0.0",
    "com.unity.modules.imageconversion": "1.0.0",
    "com.unity.modules.jsonserialize": "1.0.0",
    "com.unity.modules.physics": "1.0.0",
    "com.microsoft.mrtk.graphicstools.unity": "file:mrtk3/com.microsoft.mrtk.graphicstools.unity-0.7.1.tgz",
    "org.mixedrealitytoolkit.core": "file:mrtk3/org.mixedrealitytoolkit.core-3.0.0.tgz",
    "org.mixedrealitytoolkit.input": "file:mrtk3/org.mixedrealitytoolkit.input-3.0.0.tgz",
    "org.mixedrealitytoolkit.uxcomponents": "file:mrtk3/org.mixedrealitytoolkit.uxcomponents-3.0.0.tgz",
    "org.mixedrealitytoolkit.standardassets": "file:mrtk3/org.mixedrealitytoolkit.standardassets-3.0.0.tgz",
    "org.mixedrealitytoolkit.spatialmanipulation": "file:mrtk3/org.mixedrealitytoolkit.spatialmanipulation-3.0.0.tgz",
    "org.mixedrealitytoolkit.uxcore": "file:mrtk3/org.mixedrealitytoolkit.uxcore-3.0.0.tgz"
  },
  "scopedRegistries": []
}
```

> **关键**：`file:` 路径是相对于 `Packages/` 目录的，所以写 `file:mrtk3/...` 而不是 `file:Packages/mrtk3/...`。

### Step 5: 清理缓存

```powershell
# 删除 packages-lock.json 和 Library 文件夹
Remove-Item "D:\Unity_Ros\UnityProject\Packages\packages-lock.json" -Force
Remove-Item "D:\Unity_Ros\UnityProject\Library" -Recurse -Force
```

### Step 6: 重新打开 Unity

1. 关闭 Unity（如果已打开）
2. 重新打开项目
3. Unity 会自动解析所有包（首次需要几分钟联网下载 Unity XR 包）
4. 等待编译完成

### Step 7: 创建 MRTK Profile（自动）

在 `Assets/Editor/` 目录下创建 `MRTKProfileSetup.cs`，用于自动创建 MRTK 配置：

```csharp
using UnityEditor;
using UnityEngine;
using MixedReality.Toolkit;
using MixedReality.Toolkit.Editor;
using MixedReality.Toolkit.Input;
using MixedReality.Toolkit.Subsystems;
using System;
using System.Collections;
using System.Reflection;

[InitializeOnLoad]
public static class MRTKProfileSetup
{
    static MRTKProfileSetup()
    {
        EditorApplication.delayCall += EnsureMRTKProfile;
    }

    static void EnsureMRTKProfile()
    {
        string profilePath = "Assets/MRTK.Generated/MRTKProfile.asset";
        string configPath = "Assets/MRTK.Generated/MRTKSyntheticHandsConfig.asset";
        string settingsPath = "Assets/MRTK.Generated/MRTKSettings.asset";
        string folder = "Assets/MRTK.Generated";

        if (!AssetDatabase.IsValidFolder(folder))
        {
            AssetDatabase.CreateFolder("Assets", "MRTK.Generated");
        }

        // Create SyntheticHandsConfig
        SyntheticHandsConfig synthConfig = AssetDatabase.LoadAssetAtPath<SyntheticHandsConfig>(configPath);
        if (synthConfig == null)
        {
            synthConfig = ScriptableObject.CreateInstance<SyntheticHandsConfig>();
            AssetDatabase.CreateAsset(synthConfig, configPath);
        }

        // Create MRTKProfile
        MRTKProfile profile = AssetDatabase.LoadAssetAtPath<MRTKProfile>(profilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<MRTKProfile>();
            AssetDatabase.CreateAsset(profile, profilePath);
        }

        // Register SyntheticHandsConfig via reflection
        var field = typeof(MRTKProfile).GetField("subsystemConfigs", BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
        {
            var dict = field.GetValue(profile) as IDictionary;
            if (dict != null)
            {
                var subsystemType = new SystemType(typeof(SyntheticHandsSubsystem));
                if (!dict.Contains(subsystemType))
                {
                    dict[subsystemType] = synthConfig;
                    EditorUtility.SetDirty(profile);
                }
            }
        }

        AssetDatabase.SaveAssets();

        // Register profile in MRTKSettings
        MRTKSettings settings = AssetDatabase.LoadAssetAtPath<MRTKSettings>(settingsPath);
        if (settings == null)
        {
            settings = ScriptableObject.CreateInstance<MRTKSettings>();
            AssetDatabase.CreateAsset(settings, settingsPath);
        }

        settings.SetProfileForBuildTarget(BuildTargetGroup.Standalone, profile);
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();

        MRTKProfile.Instance = profile;
        Debug.Log("[MRTKProfileSetup] MRTK Profile fully configured.");
    }
}
```

---

## 4. 踩坑记录与解决方案

### 坑 1：Mixed Reality Feature Tool 返回 401 错误

**现象**：运行 MixedRealityFeatureTool，添加 Unity Package Manager 后报 `401 Unauthorized`

**原因**：微软认证服务器对中国 IP 限制

**解决**：放弃 Feature Tool，从 GitHub Releases 手动下载 .tgz 文件

---

### 坑 2：scopedRegistry 添加后返回 401

**现象**：在 `manifest.json` 中添加 MRTK scoped registry 后，Unity 解析时报 401

**原因**：同上，微软 NuGet 源对中国 IP 限制

**解决**：不使用 scopedRegistry，全部改为本地 `file:` 引用

---

### 坑 3：`file:Packages/mrtk3/...` 双重路径

**现象**：Unity 报错 `The file [D:\...\Packages\Packages\mrtk3\...\package.json] cannot be found`

**原因**：Unity 的 `file:` 协议路径是**相对于 `Packages/` 目录**的，写 `file:Packages/mrtk3/...` 会变成 `Packages/Packages/mrtk3/...`

**解决**：改为 `file:mrtk3/...`（不带 `Packages/` 前缀）

---

### 坑 4：Graphics Tools v0.5.12 没有预构建 .tgz

**现象**：MRTK3 要求 `com.microsoft.mrtk.graphicstools.unity >= 0.5.12`，但 GitHub Releases 只有 v0.5.0 和 v0.7.1

**原因**：Graphics Tools 仓库只有部分版本有预构建 .tgz

**解决**：使用 v0.7.1（版本 >= 0.5.12，与 MRTK3 兼容）

---

### 坑 5：`standardassets` 下载不完整（zlib 截断）

**现象**：Unity 报 `zlib: unexpected end of file`

**原因**：`Invoke-WebRequest` 下载大文件超时，文件被截断

**解决**：改用 `Start-BitsTransfer`（Windows BITS 服务）下载，并验证 GZip 头

```powershell
# 推荐的下载方式
Start-BitsTransfer -Source "<URL>" -Destination "<PATH>"
```

---

### 坑 6：Graphics Tools v0.7.1 编译错误（CS1069）

**现象**：`IAnimationWindowPreview` 找不到，`NotKeyable` 找不到

**原因**：Graphics Tools v0.7.1 引用了 `UnityEngine.AnimationModule`，但 `com.unity.modules.animation` 未在 manifest 中声明

**解决**：在 `manifest.json` 中添加 `"com.unity.modules.animation": "1.0.0"`

---

### 坑 7：XR Interaction Toolkit 2.3.0 NullReferenceException

**现象**：点 Play 时弹出 `NullReferenceException: XRInteractionProjectValidation`

**原因**：XRI 2.3.0 的编辑器验证代码存在 bug，静态构造函数中触发 NullReferenceException

**解决**：将 XRI 从 `2.3.0` 降级到 `2.2.0`

```json
"com.unity.xr.interaction.toolkit": "2.2.0"
```

---

### 坑 8：MRTK Profile could not be retrieved

**现象**：Play 模式下报 `MRTK Profile could not be retrieved`

**原因**：MRTK3 需要一个 `MRTKProfile` ScriptableObject 来配置子系统，但项目中没有创建

**解决**：
1. 创建 `MRTKProfile` 资产 + `SyntheticHandsConfig` 资产
2. 通过反射将 Config 注册到 Profile 的 `subsystemConfigs` 字典
3. 在 `MRTKSettings` 中关联 Profile

具体代码见 Step 7 的 `MRTKProfileSetup.cs`

---

### 坑 9：Git URL 下载失败（RPC failed / early EOF）

**现象**：Unity 通过 Git URL 拉取 Graphics Tools 时报 `error: RPC failed; curl 56 Recv failure`

**原因**：GitHub 大文件通过 Git 协议传输不稳定

**解决**：改用 `Start-BitsTransfer` 直接下载 .tgz，然后用 `file:` 本地引用

---

## 5. 验证清单

安装完成后，逐项检查：

| # | 检查项 | 方法 | 预期结果 |
|---|--------|------|----------|
| 1 | Package Manager 无报错 | `Window → Package Manager → In Project` | 所有 13 个包显示正常（7 MRTK3 + 6 Unity XR） |
| 2 | 编译无错误 | 项目加载后 Console 无红色错误 | `error CS` 不出现 |
| 3 | Play 模式可进入 | 点击 Play 按钮 | 场景正常运行 |
| 4 | MRTK Profile 已配置 | Console 显示 `[MRTKProfileSetup] MRTK Profile fully configured.` | Profile 和 Config 资产存在于 `Assets/MRTK.Generated/` |
| 5 | 场景对象完整 | Hierarchy 面板 | Main Camera、Directional Light、Ground、RobotCar、Trajectory、GoalManager 均存在 |
| 6 | ROS TCP 连接 | Play 模式下 Console | StatusUI 显示连接状态（Docker 运行时为绿色） |
| 7 | 导航功能正常 | 点击地面 | 绿色球出现，小车移动，轨迹线显示 |

---

## 6. 常见问题

### Q: 为什么不用 Unity Package Manager 的 Git URL？
A: 中国网络环境下 GitHub Git 协议不稳定，大文件传输经常断连。本地 .tgz 引用最可靠。

### Q: 为什么 XRI 要降级到 2.2.0？
A: XRI 2.3.0 的 `XRInteractionProjectValidation` 编辑器脚本存在 NullReferenceException bug，会在进入 Play 模式时弹出错误。2.2.0 没有此问题。

### Q: Graphics Tools 版本不匹配怎么办？
A: MRTK3 要求 >= 0.5.12，但 v0.7.1 向后兼容，可以安全使用。v0.8.x 也兼容但可能引入新的 API 变化。

### Q: MRTK Profile 可以手动创建吗？
A: 可以。通过 `Window → Mixed Reality → Toolkit` 打开 MRTK 配置向导，点击 `Add to Scene and Configure`。但自动化脚本更可靠。

### Q: 如何在 HoloLens 2 上测试？
A: 需要先配置 UWP ARM64 平台（见 `hololens2_setup.md`），然后 Build 并部署到设备。

---

## 7. 相关文件

| 文件 | 路径 | 说明 |
|------|------|------|
| manifest.json | `Packages/manifest.json` | 包依赖配置 |
| MRTK3 .tgz 包 | `Packages/mrtk3/` | 7 个本地包文件 |
| MRTKProfileSetup.cs | `Assets/Editor/MRTKProfileSetup.cs` | 自动创建 MRTK Profile |
| MRTKSettings.asset | `Assets/MRTK.Generated/MRTKSettings.asset` | MRTK 设置 |
| MRTKProfile.asset | `Assets/MRTK.Generated/MRTKProfile.asset` | MRTK Profile 配置 |
| SyntheticHandsConfig.asset | `Assets/MRTK.Generated/MRTKSyntheticHandsConfig.asset` | 手部模拟配置 |

---

*最后更新：2026-06-08*
