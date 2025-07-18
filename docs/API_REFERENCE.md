# Unity Rime输入法集成 - API参考文档

**作者：** Manus AI  
**版本：** 1.0.0  
**日期：** 2025年7月18日

## 概述

本文档提供了Unity Rime中文输入法集成解决方案的完整API参考，包括所有公共接口、数据结构、事件和配置选项的详细说明。开发者可以通过本文档了解如何在代码中使用各种功能，以及如何扩展和自定义输入法行为。

## 命名空间

所有API都位于`UnityRime`命名空间下：

```csharp
using UnityRime;
```

## 核心类

### RimeInputManager

输入法管理器是整个系统的核心类，负责协调各个组件的工作。

#### 类定义

```csharp
public class RimeInputManager : MonoBehaviour
```

#### 公共属性

| 属性名 | 类型 | 描述 | 默认值 |
|--------|------|------|--------|
| `implementationType` | `RimeImplementationType` | 实现类型（DLL或Python） | `DLL` |
| `targetInputField` | `InputField` | 目标输入框 | `null` |
| `candidatePanel` | `GameObject` | 候选词面板 | `null` |
| `candidateContainer` | `Transform` | 候选词容器 | `null` |
| `candidateButtonPrefab` | `Button` | 候选词按钮预制体 | `null` |
| `compositionText` | `Text` | 组合文本显示 | `null` |
| `statusText` | `Text` | 状态文本显示 | `null` |
| `enableInputMethod` | `bool` | 是否启用输入法 | `true` |
| `toggleKey` | `KeyCode` | 切换键 | `LeftControl` |
| `maxCandidates` | `int` | 最大候选词数量 | `9` |

#### 只读属性

| 属性名 | 类型 | 描述 |
|--------|------|------|
| `IsInputMethodActive` | `bool` | 输入法是否激活 |
| `CurrentInput` | `string` | 当前输入的文本 |

#### 公共方法

##### ToggleInputMethod()

切换输入法激活状态。

```csharp
public void ToggleInputMethod()
```

**示例：**
```csharp
var manager = FindObjectOfType<RimeInputManager>();
manager.ToggleInputMethod();
```

##### SetTargetInputField(InputField)

设置目标输入框。

```csharp
public void SetTargetInputField(InputField inputField)
```

**参数：**
- `inputField` - 目标输入框组件

**示例：**
```csharp
InputField myInputField = GameObject.Find("InputField").GetComponent<InputField>();
manager.SetTargetInputField(myInputField);
```

##### SetInputMethodEnabled(bool)

启用或禁用输入法。

```csharp
public void SetInputMethodEnabled(bool enabled)
```

**参数：**
- `enabled` - 是否启用

**示例：**
```csharp
// 禁用输入法
manager.SetInputMethodEnabled(false);
```

#### 事件

##### OnInputMethodToggled

输入法激活状态改变时触发。

```csharp
public event Action<bool> OnInputMethodToggled;
```

**参数：**
- `bool` - 新的激活状态

**示例：**
```csharp
manager.OnInputMethodToggled += (active) => {
    Debug.Log($"输入法{(active ? "激活" : "关闭")}");
    // 更新UI状态
    statusIcon.color = active ? Color.green : Color.gray;
};
```

##### OnTextInput

文本输入时触发。

```csharp
public event Action<string> OnTextInput;
```

**参数：**
- `string` - 输入的文本

**示例：**
```csharp
manager.OnTextInput += (text) => {
    Debug.Log($"输入了文本: {text}");
    // 记录输入历史
    inputHistory.Add(text);
};
```

### RimeDLLWrapper

DLL包装器类，封装了对Rime DLL的调用。

#### 类定义

```csharp
public class RimeDLLWrapper : IDisposable
```

#### 静态方法

##### GetVersion()

获取Rime DLL版本信息。

```csharp
public static string GetVersion()
```

**返回值：**
- `string` - 版本字符串

**示例：**
```csharp
string version = RimeDLLWrapper.GetVersion();
Debug.Log($"Rime DLL版本: {version}");
```

##### IsAvailable()

检查Rime DLL是否可用。

```csharp
public static bool IsAvailable()
```

**返回值：**
- `bool` - 是否可用

**示例：**
```csharp
if (RimeDLLWrapper.IsAvailable())
{
    Debug.Log("Rime DLL可用");
}
else
{
    Debug.LogError("Rime DLL不可用");
}
```

#### 实例属性

| 属性名 | 类型 | 描述 |
|--------|------|------|
| `CurrentComposition` | `string` | 当前输入的拼音 |
| `CurrentCandidates` | `RimeCandidate[]` | 当前候选词列表 |
| `IsInitialized` | `bool` | 是否已初始化 |

#### 实例方法

##### Initialize(string, string)

初始化Rime引擎。

```csharp
public bool Initialize(string userDataDir = null, string sharedDataDir = null)
```

**参数：**
- `userDataDir` - 用户数据目录路径（可选）
- `sharedDataDir` - 共享数据目录路径（可选）

**返回值：**
- `bool` - 是否初始化成功

**示例：**
```csharp
var wrapper = new RimeDLLWrapper();
if (wrapper.Initialize())
{
    Debug.Log("Rime引擎初始化成功");
}
```

##### ProcessKey(int)

处理按键输入。

```csharp
public bool ProcessKey(int keyCode)
```

**参数：**
- `keyCode` - 按键码

**返回值：**
- `bool` - 是否处理成功

**示例：**
```csharp
// 处理字母'a'的输入
bool success = wrapper.ProcessKey(97);
if (success)
{
    Debug.Log($"当前输入: {wrapper.CurrentComposition}");
    Debug.Log($"候选词数量: {wrapper.CurrentCandidates.Length}");
}
```

##### SelectCandidate(int)

选择候选词。

```csharp
public string SelectCandidate(int index)
```

**参数：**
- `index` - 候选词索引

**返回值：**
- `string` - 选中的文本，失败返回null

**示例：**
```csharp
// 选择第一个候选词
string selectedText = wrapper.SelectCandidate(0);
if (!string.IsNullOrEmpty(selectedText))
{
    Debug.Log($"选中: {selectedText}");
}
```

##### ClearComposition()

清空当前输入。

```csharp
public bool ClearComposition()
```

**返回值：**
- `bool` - 是否清空成功

**示例：**
```csharp
wrapper.ClearComposition();
```

##### RefreshState()

刷新当前输入状态。

```csharp
public bool RefreshState()
```

**返回值：**
- `bool` - 是否刷新成功

**示例：**
```csharp
wrapper.RefreshState();
Debug.Log($"当前状态: {wrapper.CurrentComposition}");
```

### RimePythonWrapper

Python包装器类，通过TCP Socket与Python进程通信。

#### 类定义

```csharp
public class RimePythonWrapper : MonoBehaviour
```

#### 配置属性

| 属性名 | 类型 | 描述 | 默认值 |
|--------|------|------|--------|
| `serverHost` | `string` | 服务器地址 | `"127.0.0.1"` |
| `serverPort` | `int` | 服务器端口 | `9999` |
| `connectionTimeout` | `float` | 连接超时时间（秒） | `5.0f` |
| `heartbeatInterval` | `float` | 心跳间隔（秒） | `30.0f` |

#### 只读属性

| 属性名 | 类型 | 描述 |
|--------|------|------|
| `CurrentComposition` | `string` | 当前输入的拼音 |
| `CurrentCandidates` | `List<PythonCandidate>` | 当前候选词列表 |
| `IsConnected` | `bool` | 是否已连接 |

#### 方法

##### ConnectToServer()

连接到Python服务器。

```csharp
public IEnumerator ConnectToServer()
```

**返回值：**
- `IEnumerator` - 协程

**示例：**
```csharp
StartCoroutine(pythonWrapper.ConnectToServer());
```

##### Disconnect()

断开与服务器的连接。

```csharp
public void Disconnect()
```

**示例：**
```csharp
pythonWrapper.Disconnect();
```

##### ProcessKey(int, Action<bool>)

处理按键输入（异步）。

```csharp
public void ProcessKey(int keyCode, Action<bool> callback = null)
```

**参数：**
- `keyCode` - 按键码
- `callback` - 完成回调

**示例：**
```csharp
pythonWrapper.ProcessKey(97, (success) => {
    if (success)
    {
        Debug.Log("按键处理成功");
    }
});
```

##### SelectCandidate(int, Action<string>)

选择候选词（异步）。

```csharp
public void SelectCandidate(int index, Action<string> callback = null)
```

**参数：**
- `index` - 候选词索引
- `callback` - 完成回调，参数为选中的文本

**示例：**
```csharp
pythonWrapper.SelectCandidate(0, (selectedText) => {
    if (!string.IsNullOrEmpty(selectedText))
    {
        Debug.Log($"选中: {selectedText}");
    }
});
```

##### ClearComposition(Action<bool>)

清空当前输入（异步）。

```csharp
public void ClearComposition(Action<bool> callback = null)
```

**参数：**
- `callback` - 完成回调

**示例：**
```csharp
pythonWrapper.ClearComposition((success) => {
    Debug.Log($"清空{(success ? "成功" : "失败")}");
});
```

##### RefreshState(Action<bool>)

刷新当前输入状态（异步）。

```csharp
public void RefreshState(Action<bool> callback = null)
```

**参数：**
- `callback` - 完成回调

**示例：**
```csharp
pythonWrapper.RefreshState((success) => {
    if (success)
    {
        Debug.Log($"当前状态: {pythonWrapper.CurrentComposition}");
    }
});
```

#### 事件

##### OnConnectionChanged

连接状态改变时触发。

```csharp
public event Action<bool> OnConnectionChanged;
```

**参数：**
- `bool` - 新的连接状态

**示例：**
```csharp
pythonWrapper.OnConnectionChanged += (connected) => {
    Debug.Log($"连接状态: {connected}");
    if (connected)
    {
        statusText.text = "已连接";
        statusText.color = Color.green;
    }
    else
    {
        statusText.text = "连接断开";
        statusText.color = Color.red;
    }
};
```

##### OnInputStateChanged

输入状态改变时触发。

```csharp
public event Action<string, List<PythonCandidate>> OnInputStateChanged;
```

**参数：**
- `string` - 当前输入的拼音
- `List<PythonCandidate>` - 候选词列表

**示例：**
```csharp
pythonWrapper.OnInputStateChanged += (composition, candidates) => {
    Debug.Log($"输入: {composition}, 候选词: {candidates.Count}个");
    
    // 更新UI显示
    compositionText.text = composition;
    UpdateCandidateButtons(candidates);
};
```

## 数据结构

### RimeCandidate

候选词数据结构（DLL方案）。

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct RimeCandidate
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string text;        // 候选词文本
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string comment;     // 候选词注释
    
    public int index;          // 候选词索引
}
```

**示例：**
```csharp
foreach (var candidate in wrapper.CurrentCandidates)
{
    Debug.Log($"{candidate.index}: {candidate.text} ({candidate.comment})");
}
```

### PythonCandidate

候选词数据结构（Python方案）。

```csharp
[Serializable]
public class PythonCandidate
{
    public string text;        // 候选词文本
    public string comment;     // 候选词注释
    public int index;          // 候选词索引
}
```

**示例：**
```csharp
foreach (var candidate in pythonWrapper.CurrentCandidates)
{
    Debug.Log($"{candidate.index}: {candidate.text} ({candidate.comment})");
}
```

### RimeInputState

输入状态数据结构（DLL方案）。

```csharp
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct RimeInputState
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
    public string composition;     // 当前输入的拼音
    
    public IntPtr candidates;      // 候选词数组指针
    public int candidate_count;    // 候选词数量
    public int page_size;          // 每页候选词数量
    public int page_no;            // 当前页码
    public int is_last_page;       // 是否为最后一页
}
```

### PythonInputState

输入状态数据结构（Python方案）。

```csharp
[Serializable]
public class PythonInputState
{
    public string composition;                    // 当前输入的拼音
    public List<PythonCandidate> candidates;     // 候选词列表
    public int page_size;                        // 每页候选词数量
    public int page_no;                          // 当前页码
    public bool is_last_page;                    // 是否为最后一页
}
```

## 枚举类型

### RimeImplementationType

实现类型枚举。

```csharp
public enum RimeImplementationType
{
    DLL,        // DLL实现
    Python      // Python实现
}
```

**示例：**
```csharp
manager.implementationType = RimeImplementationType.DLL;
```

## 接口

### IInputMethodEngine

输入法引擎接口，用于扩展自定义引擎。

```csharp
public interface IInputMethodEngine
{
    bool Initialize(string configPath);
    bool ProcessKey(int keyCode);
    string[] GetCandidates();
    string SelectCandidate(int index);
    void ClearComposition();
    string GetComposition();
}
```

**实现示例：**
```csharp
public class CustomInputEngine : IInputMethodEngine
{
    public bool Initialize(string configPath)
    {
        // 初始化自定义引擎
        return true;
    }
    
    public bool ProcessKey(int keyCode)
    {
        // 处理按键输入
        return true;
    }
    
    public string[] GetCandidates()
    {
        // 返回候选词列表
        return new string[] { "候选词1", "候选词2" };
    }
    
    public string SelectCandidate(int index)
    {
        // 选择候选词
        return "选中的文本";
    }
    
    public void ClearComposition()
    {
        // 清空输入
    }
    
    public string GetComposition()
    {
        // 返回当前输入
        return "当前输入";
    }
}
```

### ICandidateRenderer

候选词渲染器接口，用于自定义候选词显示。

```csharp
public interface ICandidateRenderer
{
    void RenderCandidates(List<CandidateData> candidates);
    void ClearCandidates();
    void HighlightCandidate(int index);
}
```

**实现示例：**
```csharp
public class CustomCandidateRenderer : ICandidateRenderer
{
    public void RenderCandidates(List<CandidateData> candidates)
    {
        // 自定义候选词渲染逻辑
        foreach (var candidate in candidates)
        {
            CreateCandidateUI(candidate);
        }
    }
    
    public void ClearCandidates()
    {
        // 清空候选词显示
    }
    
    public void HighlightCandidate(int index)
    {
        // 高亮指定候选词
    }
    
    private void CreateCandidateUI(CandidateData candidate)
    {
        // 创建候选词UI元素
    }
}
```

## 配置类

### RimeConfig

Rime配置类，用于设置各种参数。

```csharp
[Serializable]
public class RimeConfig
{
    [Header("基本设置")]
    public string userDataPath = "";
    public string sharedDataPath = "";
    public string schemaId = "luna_pinyin";
    
    [Header("显示设置")]
    public int maxCandidates = 9;
    public bool showComments = true;
    public bool showPageInfo = true;
    
    [Header("行为设置")]
    public bool autoCommit = false;
    public bool enablePrediction = true;
    public bool enableCompletion = true;
    
    [Header("快捷键设置")]
    public KeyCode[] candidateKeys = {
        KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3,
        KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6,
        KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9
    };
}
```

**使用示例：**
```csharp
public class RimeConfigManager : MonoBehaviour
{
    [SerializeField] private RimeConfig config;
    
    void Start()
    {
        ApplyConfig(config);
    }
    
    void ApplyConfig(RimeConfig config)
    {
        var manager = FindObjectOfType<RimeInputManager>();
        manager.maxCandidates = config.maxCandidates;
        
        // 应用其他配置...
    }
}
```

## 工具类

### RimeUtils

Rime工具类，提供各种辅助功能。

```csharp
public static class RimeUtils
{
    /// <summary>
    /// 检查字符是否为有效的拼音字符
    /// </summary>
    public static bool IsValidPinyinChar(char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
    
    /// <summary>
    /// 将Unity KeyCode转换为ASCII码
    /// </summary>
    public static int KeyCodeToAscii(KeyCode keyCode)
    {
        if (keyCode >= KeyCode.A && keyCode <= KeyCode.Z)
        {
            return (int)keyCode - (int)KeyCode.A + 97; // 转换为小写
        }
        
        switch (keyCode)
        {
            case KeyCode.Backspace: return 8;
            case KeyCode.Return: return 13;
            case KeyCode.Space: return 32;
            default: return 0;
        }
    }
    
    /// <summary>
    /// 格式化候选词显示文本
    /// </summary>
    public static string FormatCandidateText(int index, string text, string comment)
    {
        if (string.IsNullOrEmpty(comment))
        {
            return $"{index + 1}.{text}";
        }
        else
        {
            return $"{index + 1}.{text} ({comment})";
        }
    }
    
    /// <summary>
    /// 验证服务器地址格式
    /// </summary>
    public static bool IsValidServerAddress(string address)
    {
        if (string.IsNullOrEmpty(address))
            return false;
        
        // 简单的IP地址验证
        string[] parts = address.Split('.');
        if (parts.Length != 4)
            return false;
        
        foreach (string part in parts)
        {
            if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                return false;
        }
        
        return true;
    }
}
```

**使用示例：**
```csharp
// 检查按键是否有效
if (RimeUtils.IsValidPinyinChar(inputChar))
{
    int keyCode = RimeUtils.KeyCodeToAscii(KeyCode.A);
    wrapper.ProcessKey(keyCode);
}

// 格式化候选词显示
string displayText = RimeUtils.FormatCandidateText(0, "你好", "拼音: nihao");
candidateButton.GetComponentInChildren<Text>().text = displayText;
```

### PerformanceMonitor

性能监控类，用于监控输入法性能。

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    [Header("监控设置")]
    public bool enableMonitoring = true;
    public float updateInterval = 1.0f;
    
    private float lastUpdateTime;
    private int frameCount;
    private float totalProcessingTime;
    private int operationCount;
    
    public float AverageFrameRate { get; private set; }
    public float AverageProcessingTime { get; private set; }
    
    void Update()
    {
        if (!enableMonitoring) return;
        
        frameCount++;
        
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            AverageFrameRate = frameCount / updateInterval;
            AverageProcessingTime = operationCount > 0 ? totalProcessingTime / operationCount : 0;
            
            // 重置计数器
            frameCount = 0;
            totalProcessingTime = 0;
            operationCount = 0;
            lastUpdateTime = Time.time;
            
            // 输出性能信息
            Debug.Log($"FPS: {AverageFrameRate:F1}, 平均处理时间: {AverageProcessingTime:F2}ms");
        }
    }
    
    public void RecordOperation(float processingTime)
    {
        if (!enableMonitoring) return;
        
        totalProcessingTime += processingTime;
        operationCount++;
    }
}
```

**使用示例：**
```csharp
public class RimeInputManager : MonoBehaviour
{
    private PerformanceMonitor performanceMonitor;
    
    void Start()
    {
        performanceMonitor = GetComponent<PerformanceMonitor>();
    }
    
    private void ProcessKeyWithMonitoring(int keyCode)
    {
        float startTime = Time.realtimeSinceStartup;
        
        // 处理按键
        bool success = ProcessKey(keyCode);
        
        float processingTime = (Time.realtimeSinceStartup - startTime) * 1000; // 转换为毫秒
        performanceMonitor?.RecordOperation(processingTime);
    }
}
```

## 错误处理

### RimeException

Rime相关异常类。

```csharp
public class RimeException : System.Exception
{
    public RimeErrorCode ErrorCode { get; }
    
    public RimeException(RimeErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
    
    public RimeException(RimeErrorCode errorCode, string message, System.Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
```

### RimeErrorCode

错误代码枚举。

```csharp
public enum RimeErrorCode
{
    None = 0,
    InitializationFailed = 1,
    DllNotFound = 2,
    ConnectionFailed = 3,
    InvalidParameter = 4,
    ProcessingFailed = 5,
    NetworkTimeout = 6,
    UnknownError = 999
}
```

**使用示例：**
```csharp
try
{
    wrapper.Initialize();
}
catch (RimeException ex)
{
    switch (ex.ErrorCode)
    {
        case RimeErrorCode.DllNotFound:
            Debug.LogError("DLL文件未找到，请检查安装");
            break;
        case RimeErrorCode.InitializationFailed:
            Debug.LogError("初始化失败，请检查配置");
            break;
        default:
            Debug.LogError($"未知错误: {ex.Message}");
            break;
    }
}
```

## 扩展点

### 自定义主题

```csharp
[Serializable]
public class RimeTheme
{
    [Header("颜色设置")]
    public Color backgroundColor = Color.white;
    public Color textColor = Color.black;
    public Color highlightColor = Color.blue;
    public Color borderColor = Color.gray;
    
    [Header("字体设置")]
    public Font font;
    public int fontSize = 14;
    public FontStyle fontStyle = FontStyle.Normal;
    
    [Header("布局设置")]
    public Vector2 panelSize = new Vector2(300, 100);
    public Vector2 buttonSize = new Vector2(60, 30);
    public float spacing = 5f;
    public RectOffset padding = new RectOffset(10, 10, 5, 5);
}

public class RimeThemeManager : MonoBehaviour
{
    [SerializeField] private RimeTheme[] themes;
    [SerializeField] private int currentThemeIndex = 0;
    
    public void ApplyTheme(int themeIndex)
    {
        if (themeIndex < 0 || themeIndex >= themes.Length)
            return;
        
        currentThemeIndex = themeIndex;
        var theme = themes[themeIndex];
        
        // 应用主题到UI组件
        ApplyThemeToComponents(theme);
    }
    
    private void ApplyThemeToComponents(RimeTheme theme)
    {
        // 实现主题应用逻辑
    }
}
```

### 插件系统

```csharp
public interface IRimePlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    
    void Initialize(RimeInputManager manager);
    void OnTextInput(string text);
    void OnStateChanged(bool active);
    void OnCandidateSelected(string candidate);
    void Cleanup();
}

public class RimePluginManager : MonoBehaviour
{
    private List<IRimePlugin> plugins = new List<IRimePlugin>();
    private RimeInputManager inputManager;
    
    void Start()
    {
        inputManager = GetComponent<RimeInputManager>();
        LoadPlugins();
    }
    
    public void LoadPlugin(IRimePlugin plugin)
    {
        plugins.Add(plugin);
        plugin.Initialize(inputManager);
        
        Debug.Log($"加载插件: {plugin.Name} v{plugin.Version}");
    }
    
    public void UnloadPlugin(IRimePlugin plugin)
    {
        if (plugins.Remove(plugin))
        {
            plugin.Cleanup();
            Debug.Log($"卸载插件: {plugin.Name}");
        }
    }
    
    private void LoadPlugins()
    {
        // 从指定目录加载插件
        // 实现插件发现和加载逻辑
    }
    
    public void NotifyTextInput(string text)
    {
        foreach (var plugin in plugins)
        {
            try
            {
                plugin.OnTextInput(text);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"插件 {plugin.Name} 处理文本输入时出错: {ex.Message}");
            }
        }
    }
}
```

## 最佳实践

### 内存管理

```csharp
// 使用对象池避免频繁分配
public class CandidateButtonPool : MonoBehaviour
{
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private int poolSize = 20;
    
    private Queue<Button> buttonPool = new Queue<Button>();
    private List<Button> activeButtons = new List<Button>();
    
    void Start()
    {
        // 预创建按钮
        for (int i = 0; i < poolSize; i++)
        {
            var button = Instantiate(buttonPrefab);
            button.gameObject.SetActive(false);
            buttonPool.Enqueue(button);
        }
    }
    
    public Button GetButton()
    {
        Button button;
        if (buttonPool.Count > 0)
        {
            button = buttonPool.Dequeue();
        }
        else
        {
            button = Instantiate(buttonPrefab);
        }
        
        button.gameObject.SetActive(true);
        activeButtons.Add(button);
        return button;
    }
    
    public void ReturnButton(Button button)
    {
        if (activeButtons.Remove(button))
        {
            button.gameObject.SetActive(false);
            buttonPool.Enqueue(button);
        }
    }
    
    public void ReturnAllButtons()
    {
        foreach (var button in activeButtons)
        {
            button.gameObject.SetActive(false);
            buttonPool.Enqueue(button);
        }
        activeButtons.Clear();
    }
}
```

### 异步处理

```csharp
// 使用协程处理耗时操作
public class AsyncRimeProcessor : MonoBehaviour
{
    private Queue<System.Action> operationQueue = new Queue<System.Action>();
    private bool isProcessing = false;
    
    public void QueueOperation(System.Action operation)
    {
        operationQueue.Enqueue(operation);
        
        if (!isProcessing)
        {
            StartCoroutine(ProcessOperations());
        }
    }
    
    private IEnumerator ProcessOperations()
    {
        isProcessing = true;
        
        while (operationQueue.Count > 0)
        {
            var operation = operationQueue.Dequeue();
            
            try
            {
                operation.Invoke();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"操作执行失败: {ex.Message}");
            }
            
            // 每帧最多处理一个操作，避免卡顿
            yield return null;
        }
        
        isProcessing = false;
    }
}
```

## 调试工具

### 调试面板

```csharp
public class RimeDebugPanel : MonoBehaviour
{
    [Header("调试设置")]
    public bool showDebugInfo = true;
    public KeyCode toggleKey = KeyCode.F12;
    
    private RimeInputManager inputManager;
    private bool isVisible = false;
    private Rect windowRect = new Rect(10, 10, 300, 200);
    
    void Start()
    {
        inputManager = FindObjectOfType<RimeInputManager>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo || !isVisible) return;
        
        windowRect = GUILayout.Window(0, windowRect, DrawDebugWindow, "Rime调试信息");
    }
    
    void DrawDebugWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        // 基本信息
        GUILayout.Label($"实现类型: {inputManager.implementationType}");
        GUILayout.Label($"输入法状态: {(inputManager.IsInputMethodActive ? "激活" : "关闭")}");
        GUILayout.Label($"当前输入: {inputManager.CurrentInput}");
        
        // DLL信息
        if (inputManager.implementationType == RimeImplementationType.DLL)
        {
            GUILayout.Label($"DLL可用: {RimeDLLWrapper.IsAvailable()}");
            GUILayout.Label($"DLL版本: {RimeDLLWrapper.GetVersion()}");
        }
        
        // Python信息
        if (inputManager.implementationType == RimeImplementationType.Python)
        {
            var pythonWrapper = inputManager.GetComponent<RimePythonWrapper>();
            if (pythonWrapper != null)
            {
                GUILayout.Label($"Python连接: {pythonWrapper.IsConnected}");
                GUILayout.Label($"服务器: {pythonWrapper.serverHost}:{pythonWrapper.serverPort}");
            }
        }
        
        // 操作按钮
        if (GUILayout.Button("切换输入法"))
        {
            inputManager.ToggleInputMethod();
        }
        
        if (GUILayout.Button("清空输入"))
        {
            // 根据实现类型调用相应的清空方法
        }
        
        GUILayout.EndVertical();
        GUI.DragWindow();
    }
}
```

---

*本API参考文档最后更新于2025年7月18日*

