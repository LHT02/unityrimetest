# Unity Rime输入法集成 - Unity脚本

这是Unity Rime中文输入法集成项目的Unity脚本部分，提供了完整的C#脚本来集成Rime输入法到Unity游戏中。

## 文件结构

```
unity_scripts/
├── RimeDLLWrapper.cs        # DLL包装器脚本
├── RimePythonWrapper.cs     # Python包装器脚本
├── RimeInputManager.cs      # 输入法管理器脚本
└── README.md               # 本文档
```

## 脚本说明

### 1. RimeDLLWrapper.cs

这个脚本封装了Rime DLL的调用，提供Unity友好的C#接口。

**主要功能：**
- 封装DLL的P/Invoke调用
- 提供类型安全的C#接口
- 自动内存管理
- 错误处理和日志记录

**主要方法：**
```csharp
// 初始化Rime引擎
bool Initialize(string userDataDir = null, string sharedDataDir = null)

// 处理按键输入
bool ProcessKey(int keyCode)

// 选择候选词
string SelectCandidate(int index)

// 清空当前输入
bool ClearComposition()

// 获取当前输入状态
bool RefreshState()
```

**使用示例：**
```csharp
var wrapper = new RimeDLLWrapper();
if (wrapper.Initialize())
{
    wrapper.ProcessKey(110); // 输入 'n'
    wrapper.ProcessKey(105); // 输入 'i'
    
    if (wrapper.CurrentCandidates.Length > 0)
    {
        string selected = wrapper.SelectCandidate(0);
        Debug.Log($"选中: {selected}");
    }
}
```

### 2. RimePythonWrapper.cs

这个脚本通过TCP Socket与Python进程通信，调用PyRime功能。

**主要功能：**
- TCP Socket通信
- 异步网络操作
- JSON数据序列化
- 连接状态管理
- 心跳检测

**主要方法：**
```csharp
// 连接到Python服务器
IEnumerator ConnectToServer()

// 处理按键输入
void ProcessKey(int keyCode, Action<bool> callback = null)

// 选择候选词
void SelectCandidate(int index, Action<string> callback = null)

// 清空当前输入
void ClearComposition(Action<bool> callback = null)

// 获取当前输入状态
void RefreshState(Action<bool> callback = null)
```

**事件：**
```csharp
// 连接状态改变事件
event Action<bool> OnConnectionChanged;

// 输入状态改变事件
event Action<string, List<PythonCandidate>> OnInputStateChanged;
```

**使用示例：**
```csharp
var wrapper = GetComponent<RimePythonWrapper>();
wrapper.OnConnectionChanged += (connected) => {
    Debug.Log($"连接状态: {connected}");
};

wrapper.ProcessKey(110, (success) => {
    if (success) Debug.Log("按键处理成功");
});
```

### 3. RimeInputManager.cs

这是主要的输入法管理器，统一管理DLL和Python两种实现方式。

**主要功能：**
- 统一的输入法接口
- UI管理（候选词面板、输入框等）
- 按键处理和事件分发
- 输入法状态管理
- 自动切换DLL/Python实现

**配置选项：**
```csharp
[Header("实现类型")]
public RimeImplementationType implementationType = RimeImplementationType.DLL;

[Header("UI组件")]
public InputField targetInputField;
public GameObject candidatePanel;
public Transform candidateContainer;
public Button candidateButtonPrefab;
public Text compositionText;
public Text statusText;

[Header("输入设置")]
public bool enableInputMethod = true;
public KeyCode toggleKey = KeyCode.LeftControl;
public int maxCandidates = 9;
```

**主要方法：**
```csharp
// 切换输入法状态
void ToggleInputMethod()

// 设置目标输入框
void SetTargetInputField(InputField inputField)

// 启用/禁用输入法
void SetInputMethodEnabled(bool enabled)
```

**事件：**
```csharp
// 输入法激活状态改变事件
event Action<bool> OnInputMethodToggled;

// 文本输入事件
event Action<string> OnTextInput;
```

## 使用指南

### 1. 基本设置

1. **导入脚本**：将所有C#脚本复制到Unity项目的Scripts文件夹中。

2. **创建输入法管理器**：
   ```csharp
   // 在场景中创建一个空GameObject
   GameObject rimeManager = new GameObject("RimeInputManager");
   RimeInputManager manager = rimeManager.AddComponent<RimeInputManager>();
   ```

3. **配置组件**：在Inspector中配置RimeInputManager的各项设置。

### 2. DLL模式设置

1. **复制DLL文件**：
   - 将编译好的DLL文件复制到Unity项目的`Assets/Plugins/`目录
   - Linux: `librime_dll.so`
   - Windows: `rime_dll.dll`
   - macOS: `librime_dll.dylib`

2. **配置实现类型**：
   ```csharp
   manager.implementationType = RimeImplementationType.DLL;
   ```

### 3. Python模式设置

1. **启动Python服务器**：
   ```bash
   cd python_component
   python ipc_server.py
   ```

2. **配置实现类型**：
   ```csharp
   manager.implementationType = RimeImplementationType.Python;
   ```

3. **配置连接参数**：
   ```csharp
   RimePythonWrapper pythonWrapper = manager.GetComponent<RimePythonWrapper>();
   pythonWrapper.serverHost = "127.0.0.1";
   pythonWrapper.serverPort = 9999;
   ```

### 4. UI设置

#### 自动UI创建
如果没有指定UI组件，RimeInputManager会自动创建基本的UI：

```csharp
// 自动创建候选词面板
private void CreateCandidatePanel()

// 自动创建候选词按钮
private void CreateCandidateButtons()
```

#### 手动UI配置
也可以手动创建和配置UI组件：

1. **创建候选词面板**：
   ```csharp
   // 创建Canvas
   GameObject canvasObj = new GameObject("CandidateCanvas");
   Canvas canvas = canvasObj.AddComponent<Canvas>();
   canvas.renderMode = RenderMode.ScreenSpaceOverlay;
   
   // 创建候选词面板
   GameObject panelObj = new GameObject("CandidatePanel");
   panelObj.transform.SetParent(canvasObj.transform);
   
   // 配置到RimeInputManager
   manager.candidatePanel = panelObj;
   ```

2. **创建输入框**：
   ```csharp
   GameObject inputObj = new GameObject("InputField");
   InputField inputField = inputObj.AddComponent<InputField>();
   
   // 配置到RimeInputManager
   manager.targetInputField = inputField;
   ```

### 5. 事件处理

```csharp
// 监听输入法状态改变
manager.OnInputMethodToggled += (active) => {
    Debug.Log($"输入法{(active ? "激活" : "关闭")}");
};

// 监听文本输入
manager.OnTextInput += (text) => {
    Debug.Log($"输入了文本: {text}");
};
```

### 6. 按键映射

默认按键映射：
- `Left Ctrl`: 切换输入法开关
- `a-z`: 输入拼音
- `1-9`: 选择候选词
- `Backspace`: 删除字符
- `Enter`: 确认输入
- `Escape`: 清空当前输入

可以通过修改`HandleInputMethodKeys()`方法来自定义按键映射。

## 高级用法

### 1. 自定义UI样式

```csharp
// 自定义候选词按钮样式
private Button CreateStyledCandidateButton(int index)
{
    GameObject btnObj = new GameObject($"Candidate_{index}");
    Button btn = btnObj.AddComponent<Button>();
    
    // 添加背景图片
    Image bgImage = btnObj.AddComponent<Image>();
    bgImage.sprite = candidateButtonSprite;
    
    // 设置颜色
    ColorBlock colors = btn.colors;
    colors.normalColor = Color.white;
    colors.highlightedColor = Color.yellow;
    btn.colors = colors;
    
    return btn;
}
```

### 2. 多输入框支持

```csharp
public class MultiInputFieldManager : MonoBehaviour
{
    private RimeInputManager rimeManager;
    private InputField currentInputField;
    
    void Start()
    {
        rimeManager = FindObjectOfType<RimeInputManager>();
    }
    
    public void SwitchToInputField(InputField inputField)
    {
        currentInputField = inputField;
        rimeManager.SetTargetInputField(inputField);
    }
}
```

### 3. 自定义输入处理

```csharp
public class CustomInputHandler : MonoBehaviour
{
    private RimeInputManager rimeManager;
    
    void Start()
    {
        rimeManager = FindObjectOfType<RimeInputManager>();
        rimeManager.OnTextInput += HandleCustomInput;
    }
    
    private void HandleCustomInput(string text)
    {
        // 自定义文本处理逻辑
        ProcessInputText(text);
    }
}
```

## 性能优化

### 1. 对象池
对于频繁创建/销毁的UI元素，使用对象池：

```csharp
public class CandidateButtonPool : MonoBehaviour
{
    private Queue<Button> buttonPool = new Queue<Button>();
    
    public Button GetButton()
    {
        if (buttonPool.Count > 0)
        {
            return buttonPool.Dequeue();
        }
        else
        {
            return CreateNewButton();
        }
    }
    
    public void ReturnButton(Button button)
    {
        button.gameObject.SetActive(false);
        buttonPool.Enqueue(button);
    }
}
```

### 2. 异步处理
对于耗时操作，使用协程或异步方法：

```csharp
private IEnumerator ProcessKeyAsync(int keyCode)
{
    yield return new WaitForEndOfFrame();
    
    // 在下一帧处理按键
    ProcessKey(keyCode);
}
```

## 故障排除

### 1. DLL加载失败
- 检查DLL文件是否在正确路径
- 确保DLL架构与Unity匹配
- 查看Unity Console的错误信息

### 2. Python连接失败
- 确保Python服务器正在运行
- 检查IP地址和端口配置
- 查看防火墙设置

### 3. UI显示问题
- 检查Canvas设置
- 确保UI组件正确配置
- 查看RectTransform设置

### 4. 输入无响应
- 检查输入法是否激活
- 确认目标输入框设置
- 查看按键映射配置

## 扩展开发

### 1. 添加新的输入法引擎
```csharp
public enum RimeImplementationType
{
    DLL,
    Python,
    WebAPI,    // 新增Web API实现
    Custom     // 新增自定义实现
}
```

### 2. 自定义候选词显示
```csharp
public interface ICandidateRenderer
{
    void RenderCandidates(List<CandidateData> candidates);
}

public class CustomCandidateRenderer : ICandidateRenderer
{
    public void RenderCandidates(List<CandidateData> candidates)
    {
        // 自定义渲染逻辑
    }
}
```

### 3. 插件系统
```csharp
public interface IRimePlugin
{
    void Initialize(RimeInputManager manager);
    void OnTextInput(string text);
    void OnStateChanged(bool active);
}
```

## 许可证

本项目采用开源许可证，具体许可证信息请参考项目根目录的LICENSE文件。

