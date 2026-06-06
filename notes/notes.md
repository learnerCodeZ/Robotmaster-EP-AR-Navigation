- Unity Editor的作用

是一个游戏/实时3D引擎编辑器，简单说：是运行虚拟小车、显示地图、绘制轨迹的桌面程序

- unityhub的作用是什么，editor的作用是什么

Unity Hub 是管理工具（像游戏启动器），负责：
安装/卸载/切换多个 Unity Editor 版本
 管理 Unity 项目列表
管理许可证（个人版/专业版）

Unity Editor 是真正的开发工具，负责：
打开 .unity 项目
编辑场景、写 C# 脚本、搭建 UI
编译构建到目标平台（Windows / HoloLens / Android 等）
简单说：**Hub 是"管家"，Editor 是"干活的"**。要开发这个 ROS + AR 项目，两个都要装。

- 什么是MRTK

这是 Unity 开发中常用的工具包

 MRTK (Mixed Reality Toolkit)
是什么：
MRTK 是微软官方提供的一个开源项目，专门用来快速开发 HoloLens 这类混合现实头显应用。它给 Unity 项目直接提供了一整套开箱即用的交互模块，比如：

手势识别（捏合、点击、抓取、手部射线）

眼动追踪

空间映射（扫描真实环境，让虚拟物体能放在桌子或地面上）

优化的 3D UI 控件（按钮、滑块、对话框，都可以直接在空中用手点击）

语音命令