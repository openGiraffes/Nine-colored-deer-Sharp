# Nine-colored-deer-Sharp (九色鹿Sharp)

## 项目简介

**Nine-colored-deer-Sharp** 是一个基于 WPF 开发的 KaiOS 手机助手工具，通过 ADB (Android Debug Bridge) 连接和管理 KaiOS 设备，提供设备控制、文件管理、应用管理、应用商店等功能。

## 项目信息

- **项目类型**: WPF 桌面应用程序
- **目标框架**: .NET Framework 4.6.2
- **开发语言**: C#
- **UI框架**: WPF + HandyControl
- **主要功能**: KaiOS 设备管理和应用安装

## 核心功能

### 1. 设备连接与管理
- 通过 ADB 连接 KaiOS 设备
- 自动检测和选择设备
- 显示设备信息（型号、CPU、系统版本、序列号等）
- 设备重启和关机功能

### 2. 屏幕控制
- 实时屏幕截图
- 自动刷新屏幕（可开启/关闭）
- 虚拟按键模拟（支持多种 KaiOS 设备型号，包括 Nokia 8110 等）
- 支持按键按下/释放事件

### 3. 文件管理
- 浏览设备文件系统
- 文件上传到设备
- 文件下载到本地
- 文件/文件夹删除
- 文件重命名
- 修改文件权限
- 创建新目录
- 快速跳转到常用目录（SD卡、USB存储等）

### 4. 应用管理
- 查看已安装应用列表
- 查看运行中的应用
- 安装应用（支持 .webapp 和 .zip 格式）
- 卸载应用
- 启动应用
- 关闭应用

### 5. 应用商店
- **官方商店**: 支持 KaiOS 官方应用商店
- **OpenGiraffes 商店**: 第三方应用源
- **Banana Hackers 商店**: 第三方应用源
- 应用搜索（本地和在线搜索）
- 应用详情查看
- 应用下载和安装
- 分页显示应用列表

## 项目结构

```
src/
├── adb/                    # ADB 工具文件
│   ├── adb.exe
│   ├── AdbWinApi.dll
│   └── AdbWinUsbApi.dll
├── Beans/                  # 数据模型类
│   ├── BHAppItem.cs        # Banana Hackers 应用项
│   ├── FileItem.cs         # 文件项
│   ├── IStonItem.cs        # 商店应用接口
│   ├── KaiosAppItem.cs     # KaiOS 应用项
│   ├── KaiosStoneItem.cs   # 商店应用项
│   └── KaistonDetailItem.cs # 应用详情项
├── Converts/               # 转换器
│   └── ImageCacheConverter.cs # 图片缓存转换器
├── Helper/                 # 辅助类
│   ├── FileReceiver.cs     # 文件接收器
│   ├── kaiosHelper.cs      # KaiOS 核心辅助类
│   ├── OutPutReveiver.cs   # 输出接收器
│   ├── ProcessViewer.cs   # 进度查看器
│   └── ZipHelper.cs        # ZIP 压缩辅助
├── utils/                  # 工具类
│   ├── AnimationHelper.cs  # 动画辅助
│   ├── DialogUtil.cs       # 对话框工具
│   └── KaiSton.cs          # KaiOS 商店 API 工具
├── Properties/             # 项目属性
├── MainWindow.xaml         # 主窗口界面
├── MainWindow.xaml.cs      # 主窗口逻辑
├── App.xaml                # 应用程序定义
├── App.xaml.cs             # 应用程序逻辑
└── packages.config          # NuGet 包配置
```

## 技术栈

### 核心依赖

- **AdvancedSharpAdbClient** (2.5.8) - ADB 客户端库，用于与 Android/KaiOS 设备通信
- **HandyControl** (3.5.1) - WPF UI 控件库，提供现代化界面组件
- **Newtonsoft.Json** (13.0.3) - JSON 序列化/反序列化
- **EasyHttp** (1.7.0) - HTTP 客户端库
- **SharpZipLib** (1.4.2) - ZIP 文件处理
- **Microsoft.AppCenter** (5.0.3) - 应用分析和崩溃报告

### 其他依赖

- **PropertyChanged.Fody** (4.1.0) - 属性变更通知注入
- **Fody** (6.8.0) - IL 代码编织工具
- **HawkNet** (1.4.4.0) - HTTP 认证库
- 各种 .NET Framework 兼容性包和 Polyfill

## 主要类说明

### kaiosHelper
核心辅助类，负责：
- ADB 设备连接和管理
- 屏幕截图获取
- 应用安装/卸载/启动/关闭
- 文件上传/下载
- 设备信息获取
- Shell 命令执行

### MainWindow
主窗口类，包含：
- UI 事件处理
- 文件管理界面逻辑
- 应用管理界面逻辑
- 应用商店界面逻辑
- 虚拟按键模拟
- 屏幕刷新控制

### KaiSton
KaiOS 商店 API 工具类，负责：
- 与 KaiOS 官方 API 通信
- 获取应用列表和详情
- 下载应用包
- API 认证和密钥管理

## 功能特性

### 设备支持
- 支持多种 KaiOS 设备型号
- 特别优化了 Nokia 8110 的按键映射
- 自动检测设备型号并应用相应的按键映射

### 文件操作
- 支持文件和文件夹操作
- 支持符号链接处理
- 文件权限管理（chmod）
- 安全的文件删除确认

### 应用安装
- 支持 `.webapp` 和 `.zip` 格式
- 自动验证安装包格式
- 安装进度显示
- 安装后自动刷新应用列表

### 应用商店
- 多源支持（官方、OpenGiraffes、Banana Hackers）
- 本地缓存应用列表（减少网络请求）
- 分页显示（每页20个应用）
- 应用搜索功能
- 应用详情查看
- 一键下载和安装

## 使用说明

### 环境要求
- Windows 操作系统
- .NET Framework 4.6.2 或更高版本
- 已启用 USB 调试的 KaiOS 设备
- ADB 驱动已安装

### 基本使用流程
1. 启动应用程序
2. 通过 USB 连接 KaiOS 设备
3. 点击设备信息区域刷新设备连接
4. 使用各个功能模块进行设备管理

### 功能模块
- **屏幕控制**: 查看设备屏幕，进行截图和按键模拟
- **文件管理**: 浏览、上传、下载设备文件
- **应用管理**: 管理设备上安装的应用
- **应用商店**: 浏览和安装应用商店中的应用

## 开发信息

### 项目配置
- **目标平台**: AnyCPU (支持 x86)
- **语言版本**: C# 7.3
- **输出类型**: Windows 应用程序 (WinExe)

### 构建要求
- Visual Studio 2017 或更高版本
- .NET Framework 4.6.2 SDK
- NuGet 包管理器

### 异常处理
- 集成了 Microsoft AppCenter 进行崩溃报告
- 全局异常处理机制
- UI 线程和后台线程异常捕获

## 注意事项

1. **设备连接**: 确保设备已启用 USB 调试模式
2. **权限要求**: 某些文件操作可能需要 root 权限
3. **网络连接**: 应用商店功能需要网络连接
4. **文件安全**: 删除文件操作会进行确认，但仍需谨慎操作

## 许可证

本项目许可证信息请查看项目仓库中的 LICENSE 文件。

## 更新日志

项目版本信息可通过应用程序标题栏查看（格式：KaiOS手机助手 vX.X.X）。

---

**注意**: 本项目为 KaiOS 设备管理工具，使用前请确保已了解相关风险，并遵守设备使用规范。

