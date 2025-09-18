# Quicker扩展开发示例

## 扩展接口资源

`Quicker\Extend\IExtensionModule.cs`

## UI资源

`Quicker\Resources\Styles\`

## 扩展示例

### 带右键菜单的扩展示例
- `SampleContextMenuModule.cs` - 扩展模块，支持右键菜单功能
- `SampleContextMenu.xaml` - 扩展菜单的UI设计
- `SampleContextMenu.xaml.cs` - 扩展菜单的逻辑实现
- `SampleContextMenu.csproj` - 项目文件，包含必要的依赖和配置

这个示例展示了如何创建一个完整的扩展，包括：
- 扩展模块的基本实现
- 右键菜单的创建
- 图标嵌入
- 完整的生命周期管理
- 项目配置和依赖管理

### 扩展菜单开发要点

1. **模块配置**：
   - 设置 `HasContextMenu = true` 启用右键菜单
   - 实现 `IconData` 属性提供扩展图标
   - 设置 `HasUI = false`（如果不需要主窗口）

2. **菜单类要求**：
   - 继承自 `BaseMenuWindow`
   - 构造函数参数：`(int buttonId, string tableName, Window sourceWindow)`
   - 重写 `OnWindowLoaded` 方法设置位置
   - 重写 `HandleDeactivated` 方法处理失焦关闭

3. **XAML配置**：
   - 根元素使用 `<local:BaseMenuWindow>`
   - 设置 `xmlns:local="clr-namespace:Quicker.Windows.Menus;assembly=Quicker"`
   - 配置窗口样式：`WindowStyle="None"`, `AllowsTransparency="True"` 等

4. **功能特性**：
   - 自动淡入淡出动画
   - 失焦自动关闭
   - 智能焦点管理（防止MainWindow意外关闭）
   - 自动置顶显示
   - 自动定位到鼠标附近

## 快速开始

1. **复制示例文件**：将 `SampleContextMenu*` 文件复制到你的扩展项目中
2. **重命名文件**：将文件名改为你的扩展名称
3. **修改命名空间**：更新代码中的命名空间
4. **自定义内容**：
   - 修改 `SampleContextMenuModule.cs` 中的模块信息
   - 更新 `SampleContextMenu.xaml` 中的UI设计
   - 在 `SampleContextMenu.xaml.cs` 中添加你的业务逻辑
5. **添加图标**：在项目文件中取消注释图标嵌入部分，并添加你的图标文件
6. **构建项目**：使用 `dotnet build` 构建扩展DLL
7. **测试扩展**：在Quicker中加载生成的DLL文件进行测试