/**
 * Unity Rime输入法集成 - 输入法管理器
 * 
 * 作者: Manus AI
 * 版本: 1.0.0
 * 
 * 这个脚本管理Rime输入法的整体逻辑，包括UI显示和用户交互
 */

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityRime
{
    /// <summary>
    /// 输入法实现类型
    /// </summary>
    public enum RimeImplementationType
    {
        DLL,        // 使用DLL实现
        Python      // 使用Python进程实现
    }

    /// <summary>
    /// Rime输入法管理器
    /// </summary>
    public class RimeInputManager : MonoBehaviour
    {
        [Header("实现类型")]
        [SerializeField] private RimeImplementationType implementationType = RimeImplementationType.DLL;

        [Header("UI组件")]
        [SerializeField] private InputField targetInputField;
        [SerializeField] private GameObject candidatePanel;
        [SerializeField] private Transform candidateContainer;
        [SerializeField] private Button candidateButtonPrefab;
        [SerializeField] private Text compositionText;
        [SerializeField] private Text statusText;

        [Header("输入设置")]
        [SerializeField] private bool enableInputMethod = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.LeftControl;
        [SerializeField] private int maxCandidates = 9;

        // 内部组件
        private RimeDLLWrapper dllWrapper;
        private RimePythonWrapper pythonWrapper;
        private List<Button> candidateButtons = new List<Button>();
        private bool isInputMethodActive = false;
        private string currentInput = "";

        /// <summary>
        /// 输入法是否激活
        /// </summary>
        public bool IsInputMethodActive => isInputMethodActive;

        /// <summary>
        /// 当前输入的文本
        /// </summary>
        public string CurrentInput => currentInput;

        /// <summary>
        /// 输入法激活状态改变事件
        /// </summary>
        public event Action<bool> OnInputMethodToggled;

        /// <summary>
        /// 文本输入事件
        /// </summary>
        public event Action<string> OnTextInput;

        void Start()
        {
            InitializeComponents();
            InitializeRimeEngine();
            SetupUI();
        }

        void Update()
        {
            HandleInput();
        }

        void OnDestroy()
        {
            CleanupRimeEngine();
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 如果没有指定目标输入框，尝试查找
            if (targetInputField == null)
            {
                targetInputField = FindObjectOfType<InputField>();
            }

            // 创建候选词面板（如果不存在）
            if (candidatePanel == null)
            {
                CreateCandidatePanel();
            }

            // 初始化候选词按钮
            CreateCandidateButtons();

            // 设置初始状态
            SetCandidatePanelVisible(false);
            UpdateStatusText("正在初始化...");
        }

        /// <summary>
        /// 初始化Rime引擎
        /// </summary>
        private void InitializeRimeEngine()
        {
            switch (implementationType)
            {
                case RimeImplementationType.DLL:
                    InitializeDLLWrapper();
                    break;
                case RimeImplementationType.Python:
                    InitializePythonWrapper();
                    break;
            }
        }

        /// <summary>
        /// 初始化DLL包装器
        /// </summary>
        private void InitializeDLLWrapper()
        {
            try
            {
                if (!RimeDLLWrapper.IsAvailable())
                {
                    UpdateStatusText("错误: Rime DLL不可用");
                    return;
                }

                dllWrapper = new RimeDLLWrapper();
                if (dllWrapper.Initialize())
                {
                    UpdateStatusText($"DLL模式已就绪 - {RimeDLLWrapper.GetVersion()}");
                    Debug.Log("Rime DLL初始化成功");
                }
                else
                {
                    UpdateStatusText("错误: DLL初始化失败");
                    Debug.LogError("Rime DLL初始化失败");
                }
            }
            catch (Exception e)
            {
                UpdateStatusText($"错误: {e.Message}");
                Debug.LogError($"Rime DLL初始化异常: {e.Message}");
            }
        }

        /// <summary>
        /// 初始化Python包装器
        /// </summary>
        private void InitializePythonWrapper()
        {
            pythonWrapper = GetComponent<RimePythonWrapper>();
            if (pythonWrapper == null)
            {
                pythonWrapper = gameObject.AddComponent<RimePythonWrapper>();
            }

            // 订阅事件
            pythonWrapper.OnConnectionChanged += OnPythonConnectionChanged;
            pythonWrapper.OnInputStateChanged += OnPythonInputStateChanged;

            UpdateStatusText("正在连接Python服务器...");
        }

        /// <summary>
        /// Python连接状态改变回调
        /// </summary>
        private void OnPythonConnectionChanged(bool connected)
        {
            if (connected)
            {
                UpdateStatusText("Python模式已就绪");
                Debug.Log("Python服务器连接成功");
            }
            else
            {
                UpdateStatusText("错误: Python服务器连接失败");
                Debug.LogError("Python服务器连接失败");
            }
        }

        /// <summary>
        /// Python输入状态改变回调
        /// </summary>
        private void OnPythonInputStateChanged(string composition, List<PythonCandidate> candidates)
        {
            UpdateComposition(composition);
            UpdateCandidatesFromPython(candidates);
        }

        /// <summary>
        /// 清理Rime引擎
        /// </summary>
        private void CleanupRimeEngine()
        {
            if (dllWrapper != null)
            {
                dllWrapper.Dispose();
                dllWrapper = null;
            }

            if (pythonWrapper != null)
            {
                pythonWrapper.OnConnectionChanged -= OnPythonConnectionChanged;
                pythonWrapper.OnInputStateChanged -= OnPythonInputStateChanged;
            }
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            // 设置输入框事件
            if (targetInputField != null)
            {
                targetInputField.onValueChanged.AddListener(OnInputFieldChanged);
            }
        }

        /// <summary>
        /// 创建候选词面板
        /// </summary>
        private void CreateCandidatePanel()
        {
            // 创建候选词面板
            GameObject panelObj = new GameObject("CandidatePanel");
            panelObj.transform.SetParent(transform);
            
            candidatePanel = panelObj;
            
            // 添加Canvas组件（如果需要）
            Canvas canvas = panelObj.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = panelObj.AddComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingOrder = 100;
            }

            // 创建容器
            GameObject containerObj = new GameObject("Container");
            containerObj.transform.SetParent(panelObj.transform);
            candidateContainer = containerObj.transform;

            // 添加布局组件
            HorizontalLayoutGroup layout = containerObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 5;
            layout.padding = new RectOffset(10, 10, 5, 5);

            // 创建组合文本
            GameObject compositionObj = new GameObject("Composition");
            compositionObj.transform.SetParent(panelObj.transform);
            compositionText = compositionObj.AddComponent<Text>();
            compositionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            compositionText.fontSize = 16;
            compositionText.color = Color.blue;

            Debug.Log("候选词面板创建完成");
        }

        /// <summary>
        /// 创建候选词按钮
        /// </summary>
        private void CreateCandidateButtons()
        {
            if (candidateContainer == null) return;

            // 清除现有按钮
            foreach (Button btn in candidateButtons)
            {
                if (btn != null) DestroyImmediate(btn.gameObject);
            }
            candidateButtons.Clear();

            // 创建新按钮
            for (int i = 0; i < maxCandidates; i++)
            {
                Button btn = CreateCandidateButton(i);
                candidateButtons.Add(btn);
            }
        }

        /// <summary>
        /// 创建单个候选词按钮
        /// </summary>
        private Button CreateCandidateButton(int index)
        {
            GameObject btnObj = new GameObject($"Candidate_{index}");
            btnObj.transform.SetParent(candidateContainer);

            Button btn = btnObj.AddComponent<Button>();
            Text btnText = btnObj.AddComponent<Text>();
            
            btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            btnText.fontSize = 14;
            btnText.color = Color.black;
            btnText.alignment = TextAnchor.MiddleCenter;

            // 设置按钮点击事件
            int capturedIndex = index;
            btn.onClick.AddListener(() => SelectCandidate(capturedIndex));

            // 设置RectTransform
            RectTransform rectTransform = btnObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60, 30);

            btnObj.SetActive(false);
            return btn;
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            if (!enableInputMethod) return;

            // 检查切换键
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleInputMethod();
            }

            // 处理输入法激活状态下的按键
            if (isInputMethodActive)
            {
                HandleInputMethodKeys();
            }
        }

        /// <summary>
        /// 处理输入法按键
        /// </summary>
        private void HandleInputMethodKeys()
        {
            foreach (char c in Input.inputString)
            {
                if (c == '\b') // Backspace
                {
                    ProcessKey(8); // ASCII码8是Backspace
                }
                else if (c == '\n' || c == '\r') // Enter
                {
                    ProcessKey(13); // ASCII码13是Enter
                }
                else if (c >= 'a' && c <= 'z')
                {
                    ProcessKey((int)c);
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    ProcessKey((int)char.ToLower(c));
                }
            }

            // 处理数字键选择候选词
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    SelectCandidate(i - 1);
                }
            }

            // ESC键清空输入
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ClearComposition();
            }
        }

        /// <summary>
        /// 切换输入法状态
        /// </summary>
        public void ToggleInputMethod()
        {
            isInputMethodActive = !isInputMethodActive;
            
            if (!isInputMethodActive)
            {
                ClearComposition();
            }

            UpdateStatusText(isInputMethodActive ? "输入法已激活" : "输入法已关闭");
            OnInputMethodToggled?.Invoke(isInputMethodActive);
            
            Debug.Log($"输入法状态: {(isInputMethodActive ? "激活" : "关闭")}");
        }

        /// <summary>
        /// 处理按键
        /// </summary>
        private void ProcessKey(int keyCode)
        {
            switch (implementationType)
            {
                case RimeImplementationType.DLL:
                    ProcessKeyWithDLL(keyCode);
                    break;
                case RimeImplementationType.Python:
                    ProcessKeyWithPython(keyCode);
                    break;
            }
        }

        /// <summary>
        /// 使用DLL处理按键
        /// </summary>
        private void ProcessKeyWithDLL(int keyCode)
        {
            if (dllWrapper == null || !dllWrapper.IsInitialized) return;

            if (dllWrapper.ProcessKey(keyCode))
            {
                UpdateComposition(dllWrapper.CurrentComposition);
                UpdateCandidatesFromDLL(dllWrapper.CurrentCandidates);
            }
        }

        /// <summary>
        /// 使用Python处理按键
        /// </summary>
        private void ProcessKeyWithPython(int keyCode)
        {
            if (pythonWrapper == null || !pythonWrapper.IsConnected) return;

            pythonWrapper.ProcessKey(keyCode, (success) => {
                if (!success)
                {
                    Debug.LogError("Python处理按键失败");
                }
            });
        }

        /// <summary>
        /// 选择候选词
        /// </summary>
        private void SelectCandidate(int index)
        {
            switch (implementationType)
            {
                case RimeImplementationType.DLL:
                    SelectCandidateWithDLL(index);
                    break;
                case RimeImplementationType.Python:
                    SelectCandidateWithPython(index);
                    break;
            }
        }

        /// <summary>
        /// 使用DLL选择候选词
        /// </summary>
        private void SelectCandidateWithDLL(int index)
        {
            if (dllWrapper == null || !dllWrapper.IsInitialized) return;

            string selectedText = dllWrapper.SelectCandidate(index);
            if (!string.IsNullOrEmpty(selectedText))
            {
                InputText(selectedText);
                UpdateComposition(dllWrapper.CurrentComposition);
                UpdateCandidatesFromDLL(dllWrapper.CurrentCandidates);
            }
        }

        /// <summary>
        /// 使用Python选择候选词
        /// </summary>
        private void SelectCandidateWithPython(int index)
        {
            if (pythonWrapper == null || !pythonWrapper.IsConnected) return;

            pythonWrapper.SelectCandidate(index, (selectedText) => {
                if (!string.IsNullOrEmpty(selectedText))
                {
                    InputText(selectedText);
                }
            });
        }

        /// <summary>
        /// 清空输入
        /// </summary>
        private void ClearComposition()
        {
            switch (implementationType)
            {
                case RimeImplementationType.DLL:
                    if (dllWrapper != null && dllWrapper.IsInitialized)
                    {
                        dllWrapper.ClearComposition();
                        UpdateComposition(dllWrapper.CurrentComposition);
                        UpdateCandidatesFromDLL(dllWrapper.CurrentCandidates);
                    }
                    break;
                case RimeImplementationType.Python:
                    if (pythonWrapper != null && pythonWrapper.IsConnected)
                    {
                        pythonWrapper.ClearComposition();
                    }
                    break;
            }
        }

        /// <summary>
        /// 输入文本到目标输入框
        /// </summary>
        private void InputText(string text)
        {
            if (targetInputField != null)
            {
                targetInputField.text += text;
            }

            currentInput += text;
            OnTextInput?.Invoke(text);
            
            Debug.Log($"输入文本: {text}");
        }

        /// <summary>
        /// 更新组合文本显示
        /// </summary>
        private void UpdateComposition(string composition)
        {
            if (compositionText != null)
            {
                compositionText.text = composition;
            }

            bool hasComposition = !string.IsNullOrEmpty(composition);
            SetCandidatePanelVisible(hasComposition);
        }

        /// <summary>
        /// 从DLL更新候选词
        /// </summary>
        private void UpdateCandidatesFromDLL(RimeCandidate[] candidates)
        {
            for (int i = 0; i < candidateButtons.Count; i++)
            {
                Button btn = candidateButtons[i];
                if (i < candidates.Length)
                {
                    Text btnText = btn.GetComponentInChildren<Text>();
                    btnText.text = $"{i + 1}.{candidates[i].text}";
                    btn.gameObject.SetActive(true);
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 从Python更新候选词
        /// </summary>
        private void UpdateCandidatesFromPython(List<PythonCandidate> candidates)
        {
            for (int i = 0; i < candidateButtons.Count; i++)
            {
                Button btn = candidateButtons[i];
                if (i < candidates.Count)
                {
                    Text btnText = btn.GetComponentInChildren<Text>();
                    btnText.text = $"{i + 1}.{candidates[i].text}";
                    btn.gameObject.SetActive(true);
                }
                else
                {
                    btn.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 设置候选词面板可见性
        /// </summary>
        private void SetCandidatePanelVisible(bool visible)
        {
            if (candidatePanel != null)
            {
                candidatePanel.SetActive(visible);
            }
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText(string status)
        {
            if (statusText != null)
            {
                statusText.text = status;
            }
            Debug.Log($"状态: {status}");
        }

        /// <summary>
        /// 输入框内容改变回调
        /// </summary>
        private void OnInputFieldChanged(string value)
        {
            currentInput = value;
        }

        /// <summary>
        /// 设置目标输入框
        /// </summary>
        public void SetTargetInputField(InputField inputField)
        {
            if (targetInputField != null)
            {
                targetInputField.onValueChanged.RemoveListener(OnInputFieldChanged);
            }

            targetInputField = inputField;

            if (targetInputField != null)
            {
                targetInputField.onValueChanged.AddListener(OnInputFieldChanged);
            }
        }

        /// <summary>
        /// 启用/禁用输入法
        /// </summary>
        public void SetInputMethodEnabled(bool enabled)
        {
            enableInputMethod = enabled;
            if (!enabled && isInputMethodActive)
            {
                ToggleInputMethod();
            }
        }
    }
}

