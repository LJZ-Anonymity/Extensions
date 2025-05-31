# Quicker 扩展模块开发指南

## 概述

Quicker 应用支持通过扩展模块来增强功能。开发者可以创建自己的扩展模块，并将其放置在扩展目录中，Quicker 将自动加载和初始化这些模块。

## 创建扩展模块

### 步骤 1: 创建一个类库项目

使用 Visual Studio 或其他 .NET 开发工具创建一个类库项目（.NET 8.0 或更高版本）。

### 步骤 2: 添加对 Quicker.Interface 的引用

将 Quicker.Interface.dll 添加为项目引用，或者直接引用包含 IExtensionModule 接口的程序集。

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
在本地 C:\Users\LENOVO\AppData\Roaming\Anonymity\Quicker\Extensions 路径下为模块创建一个新的文件夹

将生成的 .dll 文件复制到刚才创建的文件夹目录中。

## 模块生命周期

1. **加载**: Quicker 启动时，会扫描扩展目录中的所有 .dll 文件。
2. **发现**: 查找实现了 IExtensionModule 接口的类。
3. **依赖解析**: 按照依赖关系排序模块。
4. **初始化**: 调用 Initialize() 方法。
5. **启动**: 调用 Start() 方法。
6. **显示UI**: 如果 HasUI 为 true，调用 ShowWindow() 方法。
7. **停止**: 当应用关闭或模块需要卸载时，调用 Stop() 方法。

## 模块间依赖

如果你的模块依赖于其他模块，可以在 Dependencies 属性中指定依赖模块的名称。Quicker 将确保按正确的顺序初始化模块。

```csharp
public string[] Dependencies => new string[] { "OtherModuleName" };
```

## 最佳实践

1. **提供完整的元数据**: 确保填写正确的模块名称、版本、作者和描述信息。
2. **正确处理异常**: 在模块代码中妥善处理异常，避免影响主应用程序。
3. **资源管理**: 在 Stop() 方法中释放所有资源，包括关闭窗口、停止线程等。
4. **依赖管理**: 明确声明模块的依赖关系，避免循环依赖。
5. **UI设计**: 如果模块有UI界面，确保它与主应用程序的风格一致。

## 示例

请参考 [Examples/SampleModule.cs](https://github.com/Anonymity3314/QuickerExtensions/blob/master/Examples/SampleModule.cs "查看样板文件") 文件，了解如何实现一个简单的扩展模块。

## 常见问题

### Q: 我的模块没有被加载怎么办？
A: 确保 .dll 文件放在正确的目录中，并且包含实现了 IExtensionModule 接口的公共类。

### Q: 如何在模块中访问主应用程序的功能？
A: 可以通过依赖注入或服务定位器模式来访问主应用程序提供的服务。

### Q: 模块可以有多个窗口吗？
A: 是的，ShowWindow() 方法可以显示主窗口，其他窗口可以在需要时创建和显示。

### Q: 如何调试模块？
A: 可以将模块项目添加到主应用程序的解决方案中，并设置调试选项。

## 建议

如果有外部模块，可以在模块文件夹中创建一个文件夹，用于放置外部模块，避免加载扩展模块时报错。

扩展模块的数据库文件应该放在模块文件夹中，避免与主程序文件冲突。

## 联系方式

如有任何问题或建议，请联系我：[查看联系方式](https://github.com/Anonymity3314 "访问作者主页")。