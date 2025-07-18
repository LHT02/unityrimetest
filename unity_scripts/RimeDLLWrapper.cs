/**
 * Unity Rime输入法集成 - DLL包装器
 * 
 * 作者: Manus AI
 * 版本: 1.0.0
 * 
 * 这个脚本封装了Rime DLL的调用，提供Unity友好的C#接口
 */

using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityRime
{
    /// <summary>
    /// 候选词结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RimeCandidate
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string text;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string comment;
        
        public int index;
    }

    /// <summary>
    /// 输入状态结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RimeInputState
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string composition;
        
        public IntPtr candidates;
        public int candidate_count;
        public int page_size;
        public int page_no;
        public int is_last_page;
    }

    /// <summary>
    /// 操作结果结构
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct RimeResult
    {
        public int success;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 512)]
        public string error_message;
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string selected_text;
        
        public RimeInputState state;
    }

    /// <summary>
    /// Rime DLL原生接口
    /// </summary>
    public static class RimeDLL
    {
        private const string DLL_NAME = "rime_dll";

        [DllImport(DLL_NAME)]
        public static extern int RimeInitialize(string user_data_dir, string shared_data_dir);
        
        [DllImport(DLL_NAME)]
        public static extern void RimeDestroy(int session_id);
        
        [DllImport(DLL_NAME)]
        public static extern void RimeProcessKey(int session_id, int key_code, out RimeResult result);
        
        [DllImport(DLL_NAME)]
        public static extern void RimeSelectCandidate(int session_id, int index, out RimeResult result);
        
        [DllImport(DLL_NAME)]
        public static extern void RimeClearComposition(int session_id, out RimeResult result);
        
        [DllImport(DLL_NAME)]
        public static extern void RimeGetCurrentState(int session_id, out RimeResult result);
        
        [DllImport(DLL_NAME)]
        public static extern void RimeFreeResult(ref RimeResult result);
        
        [DllImport(DLL_NAME)]
        public static extern IntPtr RimeGetVersion();
        
        [DllImport(DLL_NAME)]
        public static extern int RimeIsAvailable();
    }

    /// <summary>
    /// Unity友好的Rime包装器类
    /// </summary>
    public class RimeDLLWrapper : IDisposable
    {
        private int sessionId;
        private bool isInitialized;
        private bool disposed;

        /// <summary>
        /// 当前输入的拼音
        /// </summary>
        public string CurrentComposition { get; private set; } = "";

        /// <summary>
        /// 当前候选词列表
        /// </summary>
        public RimeCandidate[] CurrentCandidates { get; private set; } = new RimeCandidate[0];

        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// 获取Rime版本信息
        /// </summary>
        public static string GetVersion()
        {
            try
            {
                IntPtr ptr = RimeDLL.RimeGetVersion();
                return Marshal.PtrToStringAnsi(ptr) ?? "Unknown";
            }
            catch (Exception e)
            {
                Debug.LogError($"获取Rime版本失败: {e.Message}");
                return "Error";
            }
        }

        /// <summary>
        /// 检查Rime是否可用
        /// </summary>
        public static bool IsAvailable()
        {
            try
            {
                return RimeDLL.RimeIsAvailable() == 1;
            }
            catch (Exception e)
            {
                Debug.LogError($"检查Rime可用性失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 初始化Rime引擎
        /// </summary>
        /// <param name="userDataDir">用户数据目录</param>
        /// <param name="sharedDataDir">共享数据目录</param>
        /// <returns>是否初始化成功</returns>
        public bool Initialize(string userDataDir = null, string sharedDataDir = null)
        {
            if (isInitialized)
            {
                Debug.LogWarning("Rime引擎已经初始化");
                return true;
            }

            try
            {
                sessionId = RimeDLL.RimeInitialize(userDataDir, sharedDataDir);
                if (sessionId != 0)
                {
                    isInitialized = true;
                    Debug.Log($"Rime引擎初始化成功，会话ID: {sessionId}");
                    return true;
                }
                else
                {
                    Debug.LogError("Rime引擎初始化失败：无法创建会话");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Rime引擎初始化失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 处理按键输入
        /// </summary>
        /// <param name="keyCode">按键码</param>
        /// <returns>是否处理成功</returns>
        public bool ProcessKey(int keyCode)
        {
            if (!isInitialized)
            {
                Debug.LogError("Rime引擎未初始化");
                return false;
            }

            try
            {
                RimeResult result;
                RimeDLL.RimeProcessKey(sessionId, keyCode, out result);

                bool success = ProcessResult(result);
                RimeDLL.RimeFreeResult(ref result);
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"处理按键失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 选择候选词
        /// </summary>
        /// <param name="index">候选词索引</param>
        /// <returns>选中的文本，失败返回null</returns>
        public string SelectCandidate(int index)
        {
            if (!isInitialized)
            {
                Debug.LogError("Rime引擎未初始化");
                return null;
            }

            try
            {
                RimeResult result;
                RimeDLL.RimeSelectCandidate(sessionId, index, out result);

                string selectedText = null;
                if (result.success == 1)
                {
                    selectedText = result.selected_text;
                    ProcessResult(result);
                }
                else
                {
                    Debug.LogError($"选择候选词失败: {result.error_message}");
                }

                RimeDLL.RimeFreeResult(ref result);
                return selectedText;
            }
            catch (Exception e)
            {
                Debug.LogError($"选择候选词失败: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 清空当前输入
        /// </summary>
        /// <returns>是否清空成功</returns>
        public bool ClearComposition()
        {
            if (!isInitialized)
            {
                Debug.LogError("Rime引擎未初始化");
                return false;
            }

            try
            {
                RimeResult result;
                RimeDLL.RimeClearComposition(sessionId, out result);

                bool success = ProcessResult(result);
                RimeDLL.RimeFreeResult(ref result);
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"清空输入失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取当前输入状态
        /// </summary>
        /// <returns>是否获取成功</returns>
        public bool RefreshState()
        {
            if (!isInitialized)
            {
                Debug.LogError("Rime引擎未初始化");
                return false;
            }

            try
            {
                RimeResult result;
                RimeDLL.RimeGetCurrentState(sessionId, out result);

                bool success = ProcessResult(result);
                RimeDLL.RimeFreeResult(ref result);
                
                return success;
            }
            catch (Exception e)
            {
                Debug.LogError($"获取状态失败: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 处理DLL返回的结果
        /// </summary>
        /// <param name="result">DLL返回的结果</param>
        /// <returns>是否处理成功</returns>
        private bool ProcessResult(RimeResult result)
        {
            if (result.success != 1)
            {
                Debug.LogError($"操作失败: {result.error_message}");
                return false;
            }

            // 更新当前状态
            CurrentComposition = result.state.composition ?? "";

            // 更新候选词列表
            if (result.state.candidate_count > 0 && result.state.candidates != IntPtr.Zero)
            {
                CurrentCandidates = new RimeCandidate[result.state.candidate_count];
                int candidateSize = Marshal.SizeOf<RimeCandidate>();
                
                for (int i = 0; i < result.state.candidate_count; i++)
                {
                    IntPtr candidatePtr = new IntPtr(result.state.candidates.ToInt64() + i * candidateSize);
                    CurrentCandidates[i] = Marshal.PtrToStructure<RimeCandidate>(candidatePtr);
                }
            }
            else
            {
                CurrentCandidates = new RimeCandidate[0];
            }

            return true;
        }

        /// <summary>
        /// 销毁Rime引擎
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (isInitialized && sessionId != 0)
                {
                    try
                    {
                        RimeDLL.RimeDestroy(sessionId);
                        Debug.Log("Rime引擎已销毁");
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"销毁Rime引擎失败: {e.Message}");
                    }
                    
                    isInitialized = false;
                    sessionId = 0;
                }

                disposed = true;
            }
        }

        ~RimeDLLWrapper()
        {
            Dispose(false);
        }
    }
}

