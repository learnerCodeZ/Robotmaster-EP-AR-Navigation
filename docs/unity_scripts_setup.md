# Unity Scripts Setup

本文件记录项目中所有脚本的挂载位置和验证方法。

## 1. 脚本概览

项目共 12 个 C# 脚本，分两类：

| 类型 | 数量 | 说明 |
|------|:----:|------|
| 场景自带 | 5 | 创建场景时已自动挂载 |
| 手动挂载 | 5 | Phase 1-4 新增，需要在 Inspector 中添加 |
| 静态工具 | 1 | `RosUtils` 不需要挂载 |
| 未使用 | 1 | `MapDisplay` 为地图扩展预留，当前不需要 |

## 2. 挂载对照表

### 场景自带（已挂载）

| 脚本 | 挂载对象 | 说明 |
|------|:--------:|------|
| `VirtualCarController` | RobotCar | 控制小车前进/转向 |
| `RosSubscriber` | RobotCar | 订阅 `/cmd_vel` (Twist) |
| `RosOdomSubscriber` | RobotCar | 订阅 `/odom` → 更新位置 |
| `RosPublishCarPose` | CarPosePublisher | 每 0.1s 发布 `/car_pose` |
| `TrajectoryRenderer` | Trajectory | 绘制运动轨迹（已改：颜色渐变 + R键清除） |

### 手动挂载（Phase 1-4）

| 脚本 | 挂载对象 | 创建说明 |
|------|:--------:|----------|
| `NavGoalMarker` | GoalManager（新建空物体） | 点击地面 → 绿色球体 + 发布 `/goal_pose` |
| `StatusUI` | GoalManager（与上面同对象） | 自动生成 Canvas，显示 ROS 连接状态 |
| `SceneVisuals` | SceneVisuals（新建空物体） | 运行时给 RobotCar/Ground 上色 |
| `CarDirectionIndicator` | RobotCar | 车头方向小方块 |
| `GoToGoal` | RobotCar | 点击目标后自动驶向目标 |

### 静态工具

| 脚本 | 说明 |
|------|------|
| `RosUtils` | 坐标转换（Unity↔ROS）+ ROS 时间戳，不需要挂载 |

### 未使用

| 脚本 | 说明 |
|------|------|
| `MapDisplay` | 订阅 `/map` 显示占用栅格，Xavier 部署后启用 |

## 3. 完整设置步骤

### 第一步：打开场景

```
Unity → Assets/Scenes/MainScene.unity → 双击
```

### 第二步：创建对象并挂载脚本

**GoalManager**（挂 2 个脚本）：
1. Hierarchy 右键 → Create Empty → 命名 `GoalManager`
2. Add Component → `NavGoalMarker`
3. Add Component → `StatusUI`

**SceneVisuals**（挂 1 个脚本）：
1. Hierarchy 右键 → Create Empty → 命名 `SceneVisuals`
2. Add Component → `SceneVisuals`

**RobotCar**（追加 2 个脚本）：
1. Hierarchy 选中 `RobotCar`
2. Add Component → `CarDirectionIndicator`
3. Add Component → `GoToGoal`

### 第三步：确认挂载

检查 Inspector 中的组件列表：

| 对象 | 应有组件 |
|------|----------|
| RobotCar | Transform, MeshFilter, MeshRenderer, BoxCollider, VirtualCarController, RosSubscriber, RosOdomSubscriber, **CarDirectionIndicator**, **GoToGoal** |
| GoalManager | Transform, **NavGoalMarker**, **StatusUI** |
| SceneVisuals | Transform, **SceneVisuals** |

### 第四步：启动 ROS（可选）

如果需要 Unity↔ROS 通信：

```bash
# 先运行 Docker 端点
start_ros.bat
```

### 第五步：Play 测试

## 4. 验证方法

| 脚本 | 正确表现 | Console 日志 |
|------|----------|-------------|
| SceneVisuals | RobotCar 变蓝，Ground 变灰绿 | 无 |
| CarDirectionIndicator | RobotCar 前端出现黄色小方块 | 无 |
| StatusUI | 左上角显示 `ROS Connected`（绿）/ `Disconnected`（红） | 无 |
| NavGoalMarker | 点击地面出现绿色球体（大小 0.8） | `[Goal] clicked → ROS(...)` |
| GoToGoal | 小车自动驶向点击位置，到达后停止 | `[GoToGoal] Target set` + `[GoToGoal] dist=...` |
| TrajectoryRenderer | 运动时出现青色渐变轨迹，按 R 清除 | 无 |
| RosPublishCarPose | Play 后持续发布 | `[CarPose] published → ROS(...)` |

## 5. 常见问题

### 点击地面没有球

- 确认 `NavGoalMarker` 已挂到 GoalManager
- 确认 Ground 有 MeshCollider
- 确认 Main Camera 标签为 `MainCamera`

### StatusUI 左上角没有文字

- 确认 `StatusUI` 已挂到 GoalManager
- `StatusUI` 会自动创建 Canvas，不需要手动创建

### 小车不动

- 确认 `GoToGoal` 已挂到 RobotCar
- 确认 Console 有 `[GoToGoal] Target set` 日志
- 确认 `VirtualCarController` 也在 RobotCar 上（`GoToGoal` 通过 `GetComponent` 获取）

### 连接一直显示红色

- 如果 `start_ros.bat` 已运行，且 `[CarPose]` 日志正常，说明连接正常
- StatusUI 通过 `RosPublishCarPose.LastPublishTime` 判断连接状态
- 如果 `CarPose` 正在发布但还是红色，检查是否有多处 `LastPublishTime` 赋值冲突

### AudioListener 警告

- 已从 MainCamera 移除该组件
- 如果仍然出现，忽略即可（不影响功能）
