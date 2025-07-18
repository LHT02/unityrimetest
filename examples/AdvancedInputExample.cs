using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Unity Rime输入法集成 - 高级输入示例
/// 
/// 这个示例展示了Rime输入法的高级功能，包括：
/// - 自定义候选词面板
/// - 输入历史记录
/// - 多输入框支持
/// - 主题切换
/// - 性能监控
/// 
/// 作者: Manus AI
/// 版本: 1.0.0
/// </summary>
public class AdvancedInputExample : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private InputField[] inputFields;
    [SerializeField] private Text statusText;
    [SerializeField] private Text performanceText;
    [SerializeField] private Button themeToggleButton;
    [SerializeField] private Button historyButton;
    [SerializeField] private Dropdown inputFieldSelector;
    
    [Header("候选词面板")]
    [SerializeField] private GameObject candidatePanel;
    [SerializeField] private Transform candidateContainer;
    [SerializeField] private Button candidateButtonPrefab;
    
    [Header("输入法设置")]
    [SerializeField] private RimeImplementationType implementationType = RimeImplementationType.DLL;
    [SerializeField] private int maxHistoryEntries = 100;
    
    [Header("主题设置")]
    [SerializeField] private RimeTheme[] themes;
    
    private RimeInputManager rimeManager;
    private List<string> inputHistory = new List<string>();
    private int currentInputFieldIndex = 0;
    private int currentThemeIndex = 0;
    private PerformanceMonitor performanceMonitor;
    private List<Button> candidateButtons = new List<Button>();
    
    void Start()
    {
        InitializeInputMethod();
        SetupUI();
        SetupPerformanceMonitoring();
        ApplyTheme(currentThemeIndex);
    }
    
    /// <summary>
    /// 初始化输入法
    /// </summary>
    private void InitializeInputMethod()
    {
        // 创建输入法管理器
        GameObject rimeManagerObject = new GameObject("RimeInputManager");
        rimeManager = rimeManagerObject.AddComponent<RimeInputManager>();
        
        // 配置输入法
        rimeManager.implementationType = implementationType;
        rimeManager.candidatePanel = candidatePanel;
        rimeManager.candidateContainer = candidateContainer;
        rimeManager.candidateButtonPrefab = candidateButtonPrefab;
        
        // 设置默认输入框
        if (inputFields.Length > 0)
        {
            rimeManager.SetTargetInputField(inputFields[0]);
        }
        
        // 订阅事件
        rimeManager.OnInputMethodToggled += OnInputMethodToggled;
        rimeManager.OnTextInput += OnTextInput;
        
        Debug.Log($"高级输入法初始化完成，类型: {implementationType}");
    }
    
    /// <summary>
    /// 设置UI界面
    /// </summary>
    private void SetupUI()
    {
        // 设置主题切换按钮
        if (themeToggleButton != null)
        {
            themeToggleButton.onClick.AddListener(ToggleTheme);
        }
        
        // 设置历史记录按钮
        if (historyButton != null)
        {
            historyButton.onClick.AddListener(ShowInputHistory);
        }
        
        // 设置输入框选择器
        if (inputFieldSelector != null)
        {
            var options = new List<Dropdown.OptionData>();
            for (int i = 0; i < inputFields.Length; i++)
            {
                options.Add(new Dropdown.OptionData($"输入框 {i + 1}"));
            }
            inputFieldSelector.options = options;
            inputFieldSelector.onValueChanged.AddListener(OnInputFieldChanged);
        }
        
        // 为每个输入框添加焦点事件
        for (int i = 0; i < inputFields.Length; i++)
        {
            int index = i; // 闭包变量
            inputFields[i].onSelect.AddListener((text) => {
                SwitchToInputField(index);
            });
        }
    }
    
    /// <summary>
    /// 设置性能监控
    /// </summary>
    private void SetupPerformanceMonitoring()
    {
        performanceMonitor = gameObject.AddComponent<PerformanceMonitor>();
        performanceMonitor.enableMonitoring = true;
        performanceMonitor.updateInterval = 1.0f;
    }
    
    /// <summary>
    /// 输入法状态改变事件处理
    /// </summary>
    /// <param name="isActive">是否激活</param>
    private void OnInputMethodToggled(bool isActive)
    {
        UpdateStatusText();
        
        // 更新候选词面板显示
        if (candidatePanel != null)
        {
            candidatePanel.SetActive(isActive);
        }
        
        // 高亮当前输入框
        HighlightCurrentInputField(isActive);
    }
    
    /// <summary>
    /// 文本输入事件处理
    /// </summary>
    /// <param name="inputText">输入的文本</param>
    private void OnTextInput(string inputText)
    {
        // 添加到历史记录
        AddToHistory(inputText);
        
        // 记录性能数据
        performanceMonitor?.RecordOperation(Time.deltaTime * 1000);
        
        Debug.Log($"输入了文本: {inputText}");
    }
    
    /// <summary>
    /// 添加到输入历史
    /// </summary>
    /// <param name="text">输入的文本</param>
    private void AddToHistory(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        // 避免重复添加相同的文本
        if (inputHistory.Count > 0 && inputHistory.Last() == text) return;
        
        inputHistory.Add(text);
        
        // 限制历史记录数量
        if (inputHistory.Count > maxHistoryEntries)
        {
            inputHistory.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 显示输入历史
    /// </summary>
    private void ShowInputHistory()
    {
        if (inputHistory.Count == 0)
        {
            Debug.Log("没有输入历史记录");
            return;
        }
        
        Debug.Log("=== 输入历史记录 ===");
        for (int i = inputHistory.Count - 1; i >= 0 && i >= inputHistory.Count - 10; i--)
        {
            Debug.Log($"{inputHistory.Count - i}: {inputHistory[i]}");
        }
    }
    
    /// <summary>
    /// 切换主题
    /// </summary>
    private void ToggleTheme()
    {
        if (themes.Length == 0) return;
        
        currentThemeIndex = (currentThemeIndex + 1) % themes.Length;
        ApplyTheme(currentThemeIndex);
        
        Debug.Log($"切换到主题: {currentThemeIndex + 1}");
    }
    
    /// <summary>
    /// 应用主题
    /// </summary>
    /// <param name="themeIndex">主题索引</param>
    private void ApplyTheme(int themeIndex)
    {
        if (themeIndex < 0 || themeIndex >= themes.Length) return;
        
        var theme = themes[themeIndex];
        
        // 应用主题到候选词面板
        if (candidatePanel != null)
        {
            var panelImage = candidatePanel.GetComponent<Image>();
            if (panelImage != null)
            {
                panelImage.color = theme.backgroundColor;
            }
        }
        
        // 应用主题到状态文本
        if (statusText != null)
        {
            statusText.color = theme.textColor;
            if (theme.font != null)
            {
                statusText.font = theme.font;
            }
            statusText.fontSize = theme.fontSize;
        }
        
        // 应用主题到候选词按钮
        UpdateCandidateButtonTheme(theme);
    }
    
    /// <summary>
    /// 更新候选词按钮主题
    /// </summary>
    /// <param name="theme">主题</param>
    private void UpdateCandidateButtonTheme(RimeTheme theme)
    {
        foreach (var button in candidateButtons)
        {
            if (button != null)
            {
                var buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = theme.backgroundColor;
                }
                
                var buttonText = button.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.color = theme.textColor;
                    if (theme.font != null)
                    {
                        buttonText.font = theme.font;
                    }
                    buttonText.fontSize = theme.fontSize;
                }
            }
        }
    }
    
    /// <summary>
    /// 输入框选择改变事件
    /// </summary>
    /// <param name="index">新的输入框索引</param>
    private void OnInputFieldChanged(int index)
    {
        SwitchToInputField(index);
    }
    
    /// <summary>
    /// 切换到指定输入框
    /// </summary>
    /// <param name="index">输入框索引</param>
    private void SwitchToInputField(int index)
    {
        if (index < 0 || index >= inputFields.Length) return;
        
        currentInputFieldIndex = index;
        rimeManager.SetTargetInputField(inputFields[index]);
        
        // 更新UI显示
        HighlightCurrentInputField(rimeManager.IsInputMethodActive);
        UpdateStatusText();
        
        Debug.Log($"切换到输入框 {index + 1}");
    }
    
    /// <summary>
    /// 高亮当前输入框
    /// </summary>
    /// <param name="highlight">是否高亮</param>
    private void HighlightCurrentInputField(bool highlight)
    {
        // 重置所有输入框样式
        for (int i = 0; i < inputFields.Length; i++)
        {
            var colors = inputFields[i].colors;
            colors.normalColor = Color.white;
            inputFields[i].colors = colors;
        }
        
        // 高亮当前输入框
        if (highlight && currentInputFieldIndex < inputFields.Length)
        {
            var colors = inputFields[currentInputFieldIndex].colors;
            colors.normalColor = Color.green;
            inputFields[currentInputFieldIndex].colors = colors;
        }
    }
    
    /// <summary>
    /// 更新状态文本
    /// </summary>
    private void UpdateStatusText()
    {
        if (statusText != null)
        {
            string status = $"输入法: {(rimeManager.IsInputMethodActive ? "激活" : "关闭")}\n";
            status += $"当前输入框: {currentInputFieldIndex + 1}\n";
            status += $"实现类型: {implementationType}\n";
            status += $"历史记录: {inputHistory.Count} 条";
            
            statusText.text = status;
        }
    }
    
    /// <summary>
    /// 更新性能文本
    /// </summary>
    private void UpdatePerformanceText()
    {
        if (performanceText != null && performanceMonitor != null)
        {
            string perfText = $"FPS: {performanceMonitor.AverageFrameRate:F1}\n";
            perfText += $"处理时间: {performanceMonitor.AverageProcessingTime:F2}ms";
            
            performanceText.text = perfText;
        }
    }
    
    void Update()
    {
        // 更新性能显示
        UpdatePerformanceText();
        
        // 快捷键处理
        HandleShortcuts();
    }
    
    /// <summary>
    /// 处理快捷键
    /// </summary>
    private void HandleShortcuts()
    {
        // F2: 切换主题
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleTheme();
        }
        
        // F3: 显示历史记录
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ShowInputHistory();
        }
        
        // F4: 切换输入框
        if (Input.GetKeyDown(KeyCode.F4))
        {
            int nextIndex = (currentInputFieldIndex + 1) % inputFields.Length;
            SwitchToInputField(nextIndex);
        }
        
        // F5: 显示详细调试信息
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ShowDetailedDebugInfo();
        }
    }
    
    /// <summary>
    /// 显示详细调试信息
    /// </summary>
    private void ShowDetailedDebugInfo()
    {
        Debug.Log("=== 高级输入法调试信息 ===");
        Debug.Log($"实现类型: {rimeManager.implementationType}");
        Debug.Log($"输入法状态: {(rimeManager.IsInputMethodActive ? "激活" : "关闭")}");
        Debug.Log($"当前输入框: {currentInputFieldIndex + 1}/{inputFields.Length}");
        Debug.Log($"当前主题: {currentThemeIndex + 1}/{themes.Length}");
        Debug.Log($"历史记录数量: {inputHistory.Count}");
        Debug.Log($"平均FPS: {performanceMonitor.AverageFrameRate:F1}");
        Debug.Log($"平均处理时间: {performanceMonitor.AverageProcessingTime:F2}ms");
        
        // 显示最近的输入历史
        if (inputHistory.Count > 0)
        {
            Debug.Log("最近5条输入记录:");
            for (int i = inputHistory.Count - 1; i >= 0 && i >= inputHistory.Count - 5; i--)
            {
                Debug.Log($"  {inputHistory.Count - i}: {inputHistory[i]}");
            }
        }
    }
    
    void OnDestroy()
    {
        // 清理事件订阅
        if (rimeManager != null)
        {
            rimeManager.OnInputMethodToggled -= OnInputMethodToggled;
            rimeManager.OnTextInput -= OnTextInput;
        }
    }
}

/// <summary>
/// Rime主题数据结构
/// </summary>
[System.Serializable]
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

/// <summary>
/// 性能监控组件
/// </summary>
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
        }
    }
    
    public void RecordOperation(float processingTime)
    {
        if (!enableMonitoring) return;
        
        totalProcessingTime += processingTime;
        operationCount++;
    }
}

