using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity Rime输入法集成 - 基础输入示例
/// 
/// 这个示例展示了如何在Unity项目中集成和使用Rime中文输入法的基本功能。
/// 包括输入法的初始化、激活/关闭、文本输入和候选词选择等核心功能。
/// 
/// 作者: Manus AI
/// 版本: 1.0.0
/// </summary>
public class BasicInputExample : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private InputField targetInputField;
    [SerializeField] private Text statusText;
    [SerializeField] private Text instructionText;
    [SerializeField] private Button toggleButton;
    
    [Header("输入法设置")]
    [SerializeField] private RimeImplementationType implementationType = RimeImplementationType.DLL;
    
    private RimeInputManager rimeManager;
    
    void Start()
    {
        InitializeInputMethod();
        SetupUI();
        ShowInstructions();
    }
    
    /// <summary>
    /// 初始化输入法
    /// </summary>
    private void InitializeInputMethod()
    {
        // 创建输入法管理器GameObject
        GameObject rimeManagerObject = new GameObject("RimeInputManager");
        rimeManager = rimeManagerObject.AddComponent<RimeInputManager>();
        
        // 配置输入法类型
        rimeManager.implementationType = implementationType;
        
        // 设置目标输入框
        if (targetInputField != null)
        {
            rimeManager.SetTargetInputField(targetInputField);
        }
        
        // 订阅事件
        rimeManager.OnInputMethodToggled += OnInputMethodToggled;
        rimeManager.OnTextInput += OnTextInput;
        
        Debug.Log($"输入法初始化完成，类型: {implementationType}");
    }
    
    /// <summary>
    /// 设置UI界面
    /// </summary>
    private void SetupUI()
    {
        // 设置切换按钮
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(() => {
                rimeManager.ToggleInputMethod();
            });
        }
        
        // 设置初始状态文本
        UpdateStatusText(false);
    }
    
    /// <summary>
    /// 显示使用说明
    /// </summary>
    private void ShowInstructions()
    {
        if (instructionText != null)
        {
            instructionText.text = 
                "使用说明:\n" +
                "1. 点击切换按钮或按Left Ctrl键激活输入法\n" +
                "2. 在输入框中输入拼音字母\n" +
                "3. 使用数字键1-9选择候选词\n" +
                "4. 按Escape键清空当前输入\n" +
                "5. 按Backspace键删除字符";
        }
    }
    
    /// <summary>
    /// 输入法状态改变事件处理
    /// </summary>
    /// <param name="isActive">是否激活</param>
    private void OnInputMethodToggled(bool isActive)
    {
        UpdateStatusText(isActive);
        
        Debug.Log($"输入法{(isActive ? "激活" : "关闭")}");
        
        // 可以在这里添加视觉反馈，如改变输入框边框颜色
        if (targetInputField != null)
        {
            var colors = targetInputField.colors;
            colors.normalColor = isActive ? Color.green : Color.white;
            targetInputField.colors = colors;
        }
    }
    
    /// <summary>
    /// 文本输入事件处理
    /// </summary>
    /// <param name="inputText">输入的文本</param>
    private void OnTextInput(string inputText)
    {
        Debug.Log($"输入了文本: {inputText}");
        
        // 可以在这里添加输入历史记录、自动保存等功能
        // 例如：SaveInputHistory(inputText);
    }
    
    /// <summary>
    /// 更新状态文本显示
    /// </summary>
    /// <param name="isActive">输入法是否激活</param>
    private void UpdateStatusText(bool isActive)
    {
        if (statusText != null)
        {
            statusText.text = $"输入法状态: {(isActive ? "激活" : "关闭")}";
            statusText.color = isActive ? Color.green : Color.gray;
        }
    }
    
    void Update()
    {
        // 监听快捷键
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ShowDebugInfo();
        }
    }
    
    /// <summary>
    /// 显示调试信息
    /// </summary>
    private void ShowDebugInfo()
    {
        if (rimeManager != null)
        {
            Debug.Log("=== Rime输入法调试信息 ===");
            Debug.Log($"实现类型: {rimeManager.implementationType}");
            Debug.Log($"输入法状态: {(rimeManager.IsInputMethodActive ? "激活" : "关闭")}");
            Debug.Log($"当前输入: {rimeManager.CurrentInput}");
            Debug.Log($"目标输入框: {(rimeManager.targetInputField != null ? "已设置" : "未设置")}");
            
            // 根据实现类型显示特定信息
            if (rimeManager.implementationType == RimeImplementationType.DLL)
            {
                Debug.Log($"DLL可用性: {RimeDLLWrapper.IsAvailable()}");
                Debug.Log($"DLL版本: {RimeDLLWrapper.GetVersion()}");
            }
            else if (rimeManager.implementationType == RimeImplementationType.Python)
            {
                var pythonWrapper = rimeManager.GetComponent<RimePythonWrapper>();
                if (pythonWrapper != null)
                {
                    Debug.Log($"Python连接状态: {pythonWrapper.IsConnected}");
                    Debug.Log($"服务器地址: {pythonWrapper.serverHost}:{pythonWrapper.serverPort}");
                }
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

