/**
 * Unity Rime输入法集成 - Python包装器
 * 
 * 作者: Manus AI
 * 版本: 1.0.0
 * 
 * 这个脚本通过TCP Socket与Python进程通信，调用PyRime功能
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

namespace UnityRime
{
    /// <summary>
    /// Python候选词数据结构
    /// </summary>
    [Serializable]
    public class PythonCandidate
    {
        public string text;
        public string comment;
        public int index;
    }

    /// <summary>
    /// Python输入状态数据结构
    /// </summary>
    [Serializable]
    public class PythonInputState
    {
        public string composition;
        public List<PythonCandidate> candidates;
        public int page_size;
        public int page_no;
        public bool is_last_page;
    }

    /// <summary>
    /// Python响应数据结构
    /// </summary>
    [Serializable]
    public class PythonResponse
    {
        public bool success;
        public string error;
        public string selected_text;
        public PythonInputState state;
        public string message;
    }

    /// <summary>
    /// Python请求数据结构
    /// </summary>
    [Serializable]
    public class PythonRequest
    {
        public string command;
        public Dictionary<string, object> @params;
    }

    /// <summary>
    /// Unity友好的Python Rime包装器类
    /// </summary>
    public class RimePythonWrapper : MonoBehaviour
    {
        [Header("连接设置")]
        [SerializeField] private string serverHost = "127.0.0.1";
        [SerializeField] private int serverPort = 9999;
        [SerializeField] private float connectionTimeout = 5.0f;
        [SerializeField] private float heartbeatInterval = 30.0f;

        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private bool isConnected;
        private Coroutine heartbeatCoroutine;

        /// <summary>
        /// 当前输入的拼音
        /// </summary>
        public string CurrentComposition { get; private set; } = "";

        /// <summary>
        /// 当前候选词列表
        /// </summary>
        public List<PythonCandidate> CurrentCandidates { get; private set; } = new List<PythonCandidate>();

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// 连接状态改变事件
        /// </summary>
        public event Action<bool> OnConnectionChanged;

        /// <summary>
        /// 输入状态改变事件
        /// </summary>
        public event Action<string, List<PythonCandidate>> OnInputStateChanged;

        void Start()
        {
            // 自动连接到Python服务器
            StartCoroutine(ConnectToServer());
        }

        void OnDestroy()
        {
            Disconnect();
        }

        /// <summary>
        /// 连接到Python服务器
        /// </summary>
        public IEnumerator ConnectToServer()
        {
            if (isConnected)
            {
                Debug.LogWarning("已经连接到Python服务器");
                yield break;
            }

            Debug.Log($"正在连接到Python服务器 {serverHost}:{serverPort}...");

            try
            {
                tcpClient = new TcpClient();
                
                // 异步连接
                var connectTask = tcpClient.ConnectAsync(serverHost, serverPort);
                float elapsedTime = 0;
                
                while (!connectTask.IsCompleted && elapsedTime < connectionTimeout)
                {
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }

                if (connectTask.IsCompleted && tcpClient.Connected)
                {
                    networkStream = tcpClient.GetStream();
                    isConnected = true;
                    
                    Debug.Log("成功连接到Python服务器");
                    OnConnectionChanged?.Invoke(true);

                    // 启动心跳检测
                    if (heartbeatCoroutine != null)
                    {
                        StopCoroutine(heartbeatCoroutine);
                    }
                    heartbeatCoroutine = StartCoroutine(HeartbeatCoroutine());

                    // 测试连接
                    yield return StartCoroutine(TestConnection());
                }
                else
                {
                    Debug.LogError($"连接Python服务器超时或失败");
                    Disconnect();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"连接Python服务器失败: {e.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (heartbeatCoroutine != null)
            {
                StopCoroutine(heartbeatCoroutine);
                heartbeatCoroutine = null;
            }

            if (networkStream != null)
            {
                networkStream.Close();
                networkStream = null;
            }

            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient = null;
            }

            if (isConnected)
            {
                isConnected = false;
                Debug.Log("已断开与Python服务器的连接");
                OnConnectionChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// 心跳检测协程
        /// </summary>
        private IEnumerator HeartbeatCoroutine()
        {
            while (isConnected)
            {
                yield return new WaitForSeconds(heartbeatInterval);
                
                if (isConnected)
                {
                    yield return StartCoroutine(SendRequestCoroutine("ping", null, (response) => {
                        if (!response.success)
                        {
                            Debug.LogWarning("心跳检测失败，连接可能已断开");
                            Disconnect();
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        private IEnumerator TestConnection()
        {
            yield return StartCoroutine(SendRequestCoroutine("ping", null, (response) => {
                if (response.success)
                {
                    Debug.Log($"Python服务器响应: {response.message}");
                }
                else
                {
                    Debug.LogError("Python服务器测试失败");
                }
            }));
        }

        /// <summary>
        /// 处理按键输入
        /// </summary>
        /// <param name="keyCode">按键码</param>
        /// <param name="callback">回调函数</param>
        public void ProcessKey(int keyCode, Action<bool> callback = null)
        {
            if (!isConnected)
            {
                Debug.LogError("未连接到Python服务器");
                callback?.Invoke(false);
                return;
            }

            var parameters = new Dictionary<string, object> { { "key_code", keyCode } };
            
            StartCoroutine(SendRequestCoroutine("process_key", parameters, (response) => {
                bool success = ProcessResponse(response);
                callback?.Invoke(success);
            }));
        }

        /// <summary>
        /// 选择候选词
        /// </summary>
        /// <param name="index">候选词索引</param>
        /// <param name="callback">回调函数，参数为选中的文本</param>
        public void SelectCandidate(int index, Action<string> callback = null)
        {
            if (!isConnected)
            {
                Debug.LogError("未连接到Python服务器");
                callback?.Invoke(null);
                return;
            }

            var parameters = new Dictionary<string, object> { { "index", index } };
            
            StartCoroutine(SendRequestCoroutine("select_candidate", parameters, (response) => {
                string selectedText = null;
                if (ProcessResponse(response))
                {
                    selectedText = response.selected_text;
                }
                callback?.Invoke(selectedText);
            }));
        }

        /// <summary>
        /// 清空当前输入
        /// </summary>
        /// <param name="callback">回调函数</param>
        public void ClearComposition(Action<bool> callback = null)
        {
            if (!isConnected)
            {
                Debug.LogError("未连接到Python服务器");
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(SendRequestCoroutine("clear_composition", null, (response) => {
                bool success = ProcessResponse(response);
                callback?.Invoke(success);
            }));
        }

        /// <summary>
        /// 获取当前输入状态
        /// </summary>
        /// <param name="callback">回调函数</param>
        public void RefreshState(Action<bool> callback = null)
        {
            if (!isConnected)
            {
                Debug.LogError("未连接到Python服务器");
                callback?.Invoke(false);
                return;
            }

            StartCoroutine(SendRequestCoroutine("get_state", null, (response) => {
                bool success = ProcessResponse(response);
                callback?.Invoke(success);
            }));
        }

        /// <summary>
        /// 发送请求协程
        /// </summary>
        private IEnumerator SendRequestCoroutine(string command, Dictionary<string, object> parameters, Action<PythonResponse> callback)
        {
            if (!isConnected || networkStream == null)
            {
                callback?.Invoke(new PythonResponse { success = false, error = "未连接到服务器" });
                yield break;
            }

            try
            {
                // 构建请求
                var request = new PythonRequest
                {
                    command = command,
                    @params = parameters ?? new Dictionary<string, object>()
                };

                string requestJson = JsonConvert.SerializeObject(request);
                byte[] requestData = Encoding.UTF8.GetBytes(requestJson);

                // 发送消息长度（4字节）
                byte[] lengthData = BitConverter.GetBytes(requestData.Length);
                if (BitConverter.IsLittleEndian)
                {
                    // 确保小端序
                }
                else
                {
                    Array.Reverse(lengthData);
                }

                yield return StartCoroutine(WriteDataCoroutine(lengthData));
                yield return StartCoroutine(WriteDataCoroutine(requestData));

                // 接收响应长度
                byte[] responseLengthData = new byte[4];
                yield return StartCoroutine(ReadDataCoroutine(responseLengthData));

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(responseLengthData);
                }
                int responseLength = BitConverter.ToInt32(responseLengthData, 0);

                // 接收响应数据
                byte[] responseData = new byte[responseLength];
                yield return StartCoroutine(ReadDataCoroutine(responseData));

                string responseJson = Encoding.UTF8.GetString(responseData);
                var response = JsonConvert.DeserializeObject<PythonResponse>(responseJson);

                callback?.Invoke(response);
            }
            catch (Exception e)
            {
                Debug.LogError($"发送请求失败: {e.Message}");
                callback?.Invoke(new PythonResponse { success = false, error = e.Message });
            }
        }

        /// <summary>
        /// 写入数据协程
        /// </summary>
        private IEnumerator WriteDataCoroutine(byte[] data)
        {
            try
            {
                var writeTask = networkStream.WriteAsync(data, 0, data.Length);
                while (!writeTask.IsCompleted)
                {
                    yield return null;
                }
                
                if (writeTask.Exception != null)
                {
                    throw writeTask.Exception;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"写入数据失败: {e.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// 读取数据协程
        /// </summary>
        private IEnumerator ReadDataCoroutine(byte[] buffer)
        {
            try
            {
                int totalRead = 0;
                while (totalRead < buffer.Length)
                {
                    var readTask = networkStream.ReadAsync(buffer, totalRead, buffer.Length - totalRead);
                    while (!readTask.IsCompleted)
                    {
                        yield return null;
                    }
                    
                    if (readTask.Exception != null)
                    {
                        throw readTask.Exception;
                    }
                    
                    int bytesRead = readTask.Result;
                    if (bytesRead == 0)
                    {
                        throw new Exception("连接已断开");
                    }
                    
                    totalRead += bytesRead;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"读取数据失败: {e.Message}");
                Disconnect();
            }
        }

        /// <summary>
        /// 处理Python响应
        /// </summary>
        private bool ProcessResponse(PythonResponse response)
        {
            if (!response.success)
            {
                Debug.LogError($"Python操作失败: {response.error}");
                return false;
            }

            if (response.state != null)
            {
                // 更新当前状态
                CurrentComposition = response.state.composition ?? "";
                CurrentCandidates = response.state.candidates ?? new List<PythonCandidate>();

                // 触发状态改变事件
                OnInputStateChanged?.Invoke(CurrentComposition, CurrentCandidates);
            }

            return true;
        }
    }
}

