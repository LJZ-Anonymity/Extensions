# Quicker 扩展模块开发指南

[![主项目 Quicker](https://img.shields.io/badge/Main%20Project-Quicker-blue)](https://github.com/LJZ-Anonymity/Quicker)
[![开源协议](https://img.shields.io/badge/License-MIT-blue.svg)](https://github.com/LJZ-Anonymity/Extensions/blob/master/LICENSE)
[![最后更新](https://img.shields.io/github/last-commit/LJZ-Anonymity/Extensions)](https://github.com/LJZ-Anonymity/Extensions/commits)
[![主要语言](https://img.shields.io/github/languages/top/LJZ-Anonymity/Extensions)](https://github.com/LJZ-Anonymity/Extensions)

## 项目简介

本项目为 Quicker 应用的扩展模块开发指南及示例，旨在帮助开发者基于 Quicker 实现更多自定义功能。

**注意：本扩展机制目前为本开源项目独有，非正版 Quicker 官方功能。**

- [Quicker 应用](https://github.com/LJZ-Anonymity/Quicker "查看Quicker项目")
- [使用说明/文档项目](https://github.com/LJZ-Anonymity/Instructions "查看说明文档项目")

## 适用范围

本仓库适用于基于 [LJZ-Anonymity/Quicker](https://github.com/LJZ-Anonymity/Quicker) 项目开发的扩展模块。

与正版 Quicker 软件无关，扩展模块仅适用于本开源项目。

## 开源协议

本项目采用 [MIT License](LICENSE) 协议开源，允许自由使用、修改和分发。

## 版权声明

- 本项目为开源学习项目，所有代码均为作者及社区贡献者独立编写。
- 与正版 Quicker 软件无任何代码关联，扩展机制为本项目自定义实现。
- 如有侵权或不当使用第三方资源，请及时联系作者删除。

## 免责声明

本项目及其扩展模块仅供学习和非商业用途。
因使用本项目或扩展模块造成的任何后果，作者不承担任何责任。

---

## 创建扩展模块

### 步骤 1: 创建一个类库项目

使用 Visual Studio 或其他 .NET 开发工具创建一个类库项目（.NET 8.0 或更高版本）。

### 步骤 2: 添加对 Quicker.exe 的引用

将 Quicker.exe 添加为项目引用，或者直接引用包含 IExtensionModule 接口的程序集。

### 步骤 3: 实现 IExtensionModule 接口

创建一个类，实现 IExtensionModule 接口：

```csharp
using Quicker.Interface;
using System;

namespace YourNamespace
{
    public class YourModule : IExtensionModule
    {
        // 模块元数据
        public string Name => "YourModuleName";
        public string Version => "1.0.0";
        public string Author => "Your Name";
        public string Description => "模块描述";
        
        // 依赖关系
        public string[] Dependencies => new string[0]; // 如果依赖其他模块，在这里指定

        // UI相关
        public bool HasUI => true; // 如果模块有UI界面，设为true
        
        // 生命周期方法
        public void Initialize()
        {
            // 初始化代码
        }
        
        public void Start()
        {
            // 启动模块功能
        }
        
        public void Stop()
        {
            // 停止模块，清理资源
        }
        
        public void ShowWindow()
        {
            // 显示模块的UI界面
            // 如果HasUI为false，此方法不会被调用
        }
    }
}
```

### 步骤 4: 编译项目

编译项目，生成 .dll 文件。

### 步骤 5: 部署模块

在本地 `C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Extensions` 路径下为模块创建一个新的文件夹，将生成的 .dll 文件复制到该文件夹中。

#### 建议

- 外部依赖请放在模块文件夹的子目录中，避免冲突。
- 扩展模块的数据库文件建议放在模块文件夹中，避免与主程序文件冲突。

---

## 模块生命周期

1. **加载**: Quicker 启动时，会扫描扩展目录中的所有 .dll 文件。
2. **发现**: 查找实现了 IExtensionModule 接口的类。
3. **依赖解析**: 按照依赖关系排序模块。
4. **初始化**: 调用 Initialize() 方法。
5. **启动**: 调用 Start() 方法。
6. **显示UI**: 如果 HasUI 为 true，调用 ShowWindow() 方法。
7. **停止**: 当应用关闭或模块需要卸载时，调用 Stop() 方法。

## 模块间依赖

如需依赖其他模块，在 `Dependencies` 属性中指定依赖模块名称。

---

## 最佳实践

1. **提供完整的元数据**：确保填写正确的模块名称、版本、作者和描述信息。
2. **正确处理异常**：在模块代码中妥善处理异常，避免影响主应用程序。
3. **资源管理**：在 Stop() 方法中释放所有资源，包括关闭窗口、停止线程等。
4. **依赖管理**：明确声明模块的依赖关系，避免循环依赖。
5. **UI设计**：如有UI界面，确保风格与主应用程序一致。

---

## 示例

请参考 [Examples/SampleModule.cs](https://github.com/LJZ-Anonymity/Extensions/blob/master/Examples/SampleModule.cs "查看样板文件") 文件，了解如何实现一个简单的扩展模块。

---

## 常见问题

### Q: 我的模块没有被加载怎么办？

A: 确保 .dll 文件放在正确的目录中，并且包含实现了 IExtensionModule 接口的公共类。

### Q: 如何在模块中访问主应用程序的功能？

A: 可以通过依赖注入或服务定位器模式来访问主应用程序提供的服务。

### Q: 模块可以有多个窗口吗？

A: 是的，ShowWindow() 方法可以显示主窗口，其他窗口可以在需要时创建和显示。

### Q: 如何调试模块？

A: 可以将模块项目添加到主应用程序的解决方案中，并设置调试选项。

---

## 联系方式

如有任何问题或建议，请[联系作者](https://github.com/LJZ-Anonymity/Quicker?tab=readme-ov-file#contact "访问作者主页")。