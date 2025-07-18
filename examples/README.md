# Unity Rime输入法集成 - 示例项目

**作者：** Manus AI  
**版本：** 1.0.0  
**日期：** 2025年7月18日

## 概述

本目录包含了Unity Rime中文输入法集成解决方案的示例项目，展示了如何在实际Unity项目中使用和集成Rime输入法功能。这些示例从基础用法到高级功能，为开发者提供了完整的参考实现。

## 示例列表

### 1. BasicInputExample.cs - 基础输入示例

这是最简单的集成示例，展示了Rime输入法的核心功能。

**功能特性：**
- 输入法的初始化和配置
- 激活/关闭输入法
- 基本的拼音输入和候选词选择
- 简单的UI反馈
- 事件处理和调试信息

**适用场景：**
- 初次接触Rime输入法集成的开发者
- 需要快速集成基本中文输入功能的项目
- 学习输入法API的基础用法

**使用方法：**
1. 将脚本添加到场景中的GameObject上
2. 在Inspector中配置UI组件引用
3. 选择实现类型（DLL或Python）
4. 运行场景并测试输入功能

### 2. AdvancedInputExample.cs - 高级输入示例

这是一个功能完整的高级示例，展示了输入法的扩展功能和最佳实践。

**功能特性：**
- 多输入框支持和切换
- 自定义候选词面板
- 输入历史记录管理
- 主题系统和UI定制
- 性能监控和优化
- 丰富的快捷键支持
- 详细的调试信息

**适用场景：**
- 需要复杂输入法功能的项目
- 多输入框的表单应用
- 需要自定义UI风格的游戏
- 对性能有严格要求的应用

**使用方法：**
1. 将脚本添加到场景中的GameObject上
2. 配置多个InputField组件
3. 设置候选词面板和按钮预制体
4. 配置主题数组
5. 运行并体验高级功能

## 快速开始

### 环境准备

在运行示例之前，请确保已经完成以下准备工作：

1. **Unity版本：** Unity 2019.4 LTS或更高版本
2. **项目设置：** 确保项目支持.NET Standard 2.1
3. **依赖组件：** 根据选择的实现方案准备相应组件

### DLL方案准备

```bash
# 编译DLL组件
cd ../dll_component
./build.sh

# 复制DLL到Unity项目
cp build/librime_dll.so /path/to/your/unity/project/Assets/Plugins/
```

### Python方案准备

```bash
# 启动Python服务器
cd ../python_component
python3 ipc_server.py
```

### 创建测试场景

1. **创建新场景：**
   - File → New Scene
   - 保存为"RimeInputTest"

2. **添加UI组件：**
   ```
   Canvas
   ├── InputField (目标输入框)
   ├── StatusText (状态显示)
   ├── ToggleButton (切换按钮)
   └── InstructionText (说明文本)
   ```

3. **配置示例脚本：**
   - 创建空GameObject，命名为"InputExample"
   - 添加BasicInputExample或AdvancedInputExample组件
   - 在Inspector中配置UI组件引用

4. **运行测试：**
   - 点击Play按钮
   - 按照界面说明测试输入功能

## 详细使用指南

### BasicInputExample 使用指南

#### 脚本配置

```csharp
[Header("UI组件")]
[SerializeField] private InputField targetInputField;     // 目标输入框
[SerializeField] private Text statusText;                // 状态文本
[SerializeField] private Text instructionText;           // 说明文本
[SerializeField] private Button toggleButton;            // 切换按钮

[Header("输入法设置")]
[SerializeField] private RimeImplementationType implementationType = RimeImplementationType.DLL;
```

#### 操作说明

| 操作 | 说明 |
|------|------|
| 点击切换按钮 | 激活/关闭输入法 |
| Left Ctrl键 | 快捷键切换输入法 |
| 输入拼音 | 在激活状态下输入a-z字母 |
| 数字键1-9 | 选择对应的候选词 |
| Escape键 | 清空当前输入 |
| Backspace键 | 删除最后一个字符 |
| F1键 | 显示调试信息 |

#### 事件处理

```csharp
// 输入法状态改变
rimeManager.OnInputMethodToggled += (isActive) => {
    // 更新UI状态
    UpdateStatusText(isActive);
    
    // 改变输入框样式
    ChangeInputFieldStyle(isActive);
};

// 文本输入
rimeManager.OnTextInput += (inputText) => {
    // 记录输入历史
    SaveInputHistory(inputText);
    
    // 触发自定义逻辑
    OnTextInputCustomLogic(inputText);
};
```

### AdvancedInputExample 使用指南

#### 高级配置

```csharp
[Header("候选词面板")]
[SerializeField] private GameObject candidatePanel;       // 候选词面板
[SerializeField] private Transform candidateContainer;    // 候选词容器
[SerializeField] private Button candidateButtonPrefab;   // 候选词按钮预制体

[Header("主题设置")]
[SerializeField] private RimeTheme[] themes;             // 主题数组
```

#### 多输入框管理

```csharp
// 切换输入框
private void SwitchToInputField(int index)
{
    currentInputFieldIndex = index;
    rimeManager.SetTargetInputField(inputFields[index]);
    HighlightCurrentInputField(true);
}

// 输入框焦点事件
for (int i = 0; i < inputFields.Length; i++)
{
    int index = i;
    inputFields[i].onSelect.AddListener((text) => {
        SwitchToInputField(index);
    });
}
```

#### 主题系统

```csharp
[System.Serializable]
public class RimeTheme
{
    public Color backgroundColor = Color.white;
    public Color textColor = Color.black;
    public Color highlightColor = Color.blue;
    public Font font;
    public int fontSize = 14;
}

// 应用主题
private void ApplyTheme(int themeIndex)
{
    var theme = themes[themeIndex];
    
    // 更新UI组件样式
    UpdateUIWithTheme(theme);
}
```

#### 性能监控

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    public float AverageFrameRate { get; private set; }
    public float AverageProcessingTime { get; private set; }
    
    public void RecordOperation(float processingTime)
    {
        // 记录操作性能数据
    }
}
```

#### 快捷键系统

| 快捷键 | 功能 |
|--------|------|
| F2 | 切换主题 |
| F3 | 显示输入历史 |
| F4 | 切换输入框 |
| F5 | 显示详细调试信息 |

## 自定义扩展

### 创建自定义候选词渲染器

```csharp
public class CustomCandidateRenderer : MonoBehaviour
{
    [SerializeField] private GameObject candidatePrefab;
    [SerializeField] private Transform container;
    
    private List<GameObject> candidateObjects = new List<GameObject>();
    
    public void RenderCandidates(List<CandidateData> candidates)
    {
        // 清空现有候选词
        ClearCandidates();
        
        // 创建新的候选词UI
        foreach (var candidate in candidates)
        {
            CreateCandidateUI(candidate);
        }
    }
    
    private void CreateCandidateUI(CandidateData candidate)
    {
        var candidateObj = Instantiate(candidatePrefab, container);
        
        // 配置候选词显示
        var text = candidateObj.GetComponentInChildren<Text>();
        text.text = $"{candidate.index + 1}.{candidate.text}";
        
        // 添加点击事件
        var button = candidateObj.GetComponent<Button>();
        button.onClick.AddListener(() => {
            SelectCandidate(candidate.index);
        });
        
        candidateObjects.Add(candidateObj);
    }
    
    private void ClearCandidates()
    {
        foreach (var obj in candidateObjects)
        {
            Destroy(obj);
        }
        candidateObjects.Clear();
    }
}
```

### 实现输入历史管理器

```csharp
public class InputHistoryManager : MonoBehaviour
{
    [SerializeField] private int maxHistoryEntries = 100;
    [SerializeField] private string historyFilePath = "input_history.json";
    
    private List<InputHistoryEntry> history = new List<InputHistoryEntry>();
    
    [System.Serializable]
    public class InputHistoryEntry
    {
        public string text;
        public string timestamp;
        public string inputField;
    }
    
    public void AddEntry(string text, string inputFieldName)
    {
        var entry = new InputHistoryEntry
        {
            text = text,
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            inputField = inputFieldName
        };
        
        history.Add(entry);
        
        // 限制历史记录数量
        if (history.Count > maxHistoryEntries)
        {
            history.RemoveAt(0);
        }
        
        // 保存到文件
        SaveHistory();
    }
    
    public List<InputHistoryEntry> GetRecentEntries(int count = 10)
    {
        int startIndex = Mathf.Max(0, history.Count - count);
        return history.GetRange(startIndex, history.Count - startIndex);
    }
    
    private void SaveHistory()
    {
        try
        {
            string json = JsonUtility.ToJson(new SerializableList<InputHistoryEntry>(history));
            System.IO.File.WriteAllText(historyFilePath, json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"保存历史记录失败: {ex.Message}");
        }
    }
    
    private void LoadHistory()
    {
        try
        {
            if (System.IO.File.Exists(historyFilePath))
            {
                string json = System.IO.File.ReadAllText(historyFilePath);
                var list = JsonUtility.FromJson<SerializableList<InputHistoryEntry>>(json);
                history = list.items;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"加载历史记录失败: {ex.Message}");
        }
    }
    
    void Start()
    {
        LoadHistory();
    }
}

[System.Serializable]
public class SerializableList<T>
{
    public List<T> items;
    
    public SerializableList(List<T> items)
    {
        this.items = items;
    }
}
```

### 创建输入法插件系统

```csharp
public interface IRimePlugin
{
    string Name { get; }
    string Version { get; }
    
    void Initialize(RimeInputManager manager);
    void OnTextInput(string text);
    void OnStateChanged(bool active);
    void Cleanup();
}

public class AutoCompletePlugin : IRimePlugin
{
    public string Name => "自动补全插件";
    public string Version => "1.0.0";
    
    private RimeInputManager manager;
    private List<string> commonPhrases;
    
    public void Initialize(RimeInputManager manager)
    {
        this.manager = manager;
        LoadCommonPhrases();
    }
    
    public void OnTextInput(string text)
    {
        // 实现自动补全逻辑
        var suggestions = GetAutoCompleteSuggestions(text);
        ShowSuggestions(suggestions);
    }
    
    public void OnStateChanged(bool active)
    {
        // 处理状态变化
    }
    
    public void Cleanup()
    {
        // 清理资源
    }
    
    private void LoadCommonPhrases()
    {
        commonPhrases = new List<string>
        {
            "你好", "谢谢", "再见", "请问", "不客气"
            // 加载更多常用短语
        };
    }
    
    private List<string> GetAutoCompleteSuggestions(string input)
    {
        return commonPhrases
            .Where(phrase => phrase.Contains(input))
            .Take(5)
            .ToList();
    }
    
    private void ShowSuggestions(List<string> suggestions)
    {
        // 显示自动补全建议
    }
}
```

## 性能优化建议

### 内存优化

```csharp
// 使用对象池管理候选词按钮
public class CandidateButtonPool : MonoBehaviour
{
    private Queue<Button> buttonPool = new Queue<Button>();
    private List<Button> activeButtons = new List<Button>();
    
    public Button GetButton()
    {
        if (buttonPool.Count > 0)
        {
            var button = buttonPool.Dequeue();
            button.gameObject.SetActive(true);
            activeButtons.Add(button);
            return button;
        }
        
        return CreateNewButton();
    }
    
    public void ReturnButton(Button button)
    {
        if (activeButtons.Remove(button))
        {
            button.gameObject.SetActive(false);
            buttonPool.Enqueue(button);
        }
    }
}
```

### 渲染优化

```csharp
// 使用Canvas Group控制候选词面板显示
public class OptimizedCandidatePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void SetVisible(bool visible)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
```

## 故障排除

### 常见问题

#### 1. 示例脚本编译错误

**问题：** 缺少命名空间或类型定义
**解决：** 确保已正确复制所有Unity脚本文件

#### 2. UI组件引用丢失

**问题：** Inspector中的UI组件引用为空
**解决：** 重新拖拽UI组件到对应的字段

#### 3. 输入法无响应

**问题：** 按键输入没有反应
**解决：** 检查输入法是否已激活，目标输入框是否正确设置

#### 4. 性能问题

**问题：** 输入延迟或卡顿
**解决：** 启用性能监控，检查是否有性能瓶颈

### 调试技巧

```csharp
// 添加详细的调试日志
public class RimeDebugLogger : MonoBehaviour
{
    [SerializeField] private bool enableDebugLog = true;
    
    public void LogInputEvent(string eventType, string details)
    {
        if (!enableDebugLog) return;
        
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss.fff");
        Debug.Log($"[{timestamp}] Rime {eventType}: {details}");
    }
}

// 在示例脚本中使用
private RimeDebugLogger debugLogger;

void Start()
{
    debugLogger = gameObject.AddComponent<RimeDebugLogger>();
}

private void OnTextInput(string text)
{
    debugLogger.LogInputEvent("TextInput", $"输入文本: {text}");
}
```

## 扩展资源

### 预制体模板

建议创建以下预制体以便重复使用：

1. **RimeInputManager.prefab** - 配置好的输入法管理器
2. **CandidatePanel.prefab** - 候选词面板模板
3. **CandidateButton.prefab** - 候选词按钮模板
4. **InputFieldWithRime.prefab** - 集成了Rime的输入框

### 脚本模板

```csharp
// RimeInputTemplate.cs - 基础模板
public class RimeInputTemplate : MonoBehaviour
{
    [Header("基本配置")]
    [SerializeField] private RimeImplementationType implementationType;
    [SerializeField] private InputField targetInputField;
    
    private RimeInputManager rimeManager;
    
    protected virtual void Start()
    {
        InitializeRime();
        SetupEvents();
    }
    
    protected virtual void InitializeRime()
    {
        // 初始化输入法
    }
    
    protected virtual void SetupEvents()
    {
        // 设置事件处理
    }
    
    protected virtual void OnTextInput(string text)
    {
        // 处理文本输入
    }
    
    protected virtual void OnInputMethodToggled(bool active)
    {
        // 处理状态切换
    }
}
```

## 许可证

这些示例项目采用与主项目相同的MIT许可证，可以自由使用、修改和分发。

---

*本示例文档最后更新于2025年7月18日*

