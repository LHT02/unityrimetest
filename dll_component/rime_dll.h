/**
 * Unity Rime输入法集成 - DLL接口头文件
 * 
 * 作者: Manus AI
 * 版本: 1.0.0
 * 
 * 这个头文件定义了Unity可以调用的Rime输入法DLL接口
 */

#ifndef RIME_DLL_H
#define RIME_DLL_H

#ifdef __cplusplus
extern "C" {
#endif

// 导出宏定义
#ifdef _WIN32
    #ifdef RIME_DLL_EXPORTS
        #define RIME_API __declspec(dllexport)
    #else
        #define RIME_API __declspec(dllimport)
    #endif
#else
    #define RIME_API __attribute__((visibility("default")))
#endif

// 数据结构定义

/**
 * 候选词结构
 */
typedef struct {
    char text[256];      // 候选词文本
    char comment[256];   // 候选词注释
    int index;           // 候选词索引
} RimeCandidate;

/**
 * 输入状态结构
 */
typedef struct {
    char composition[512];       // 当前输入的拼音
    RimeCandidate* candidates;   // 候选词数组
    int candidate_count;         // 候选词数量
    int page_size;              // 每页候选词数量
    int page_no;                // 当前页码
    int is_last_page;           // 是否为最后一页
} RimeInputState;

/**
 * 操作结果结构
 */
typedef struct {
    int success;                // 操作是否成功 (1: 成功, 0: 失败)
    char error_message[512];    // 错误信息
    char selected_text[256];    // 选中的文本
    RimeInputState state;       // 当前输入状态
} RimeResult;

// API函数声明

/**
 * 初始化Rime引擎
 * 
 * @param user_data_dir 用户数据目录路径
 * @param shared_data_dir 共享数据目录路径
 * @return 会话ID，失败返回0
 */
RIME_API int RimeInitialize(const char* user_data_dir, const char* shared_data_dir);

/**
 * 销毁Rime引擎
 * 
 * @param session_id 会话ID
 */
RIME_API void RimeDestroy(int session_id);

/**
 * 处理按键输入
 * 
 * @param session_id 会话ID
 * @param key_code 按键码
 * @param result 输出结果
 */
RIME_API void RimeProcessKey(int session_id, int key_code, RimeResult* result);

/**
 * 选择候选词
 * 
 * @param session_id 会话ID
 * @param index 候选词索引
 * @param result 输出结果
 */
RIME_API void RimeSelectCandidate(int session_id, int index, RimeResult* result);

/**
 * 清空当前输入
 * 
 * @param session_id 会话ID
 * @param result 输出结果
 */
RIME_API void RimeClearComposition(int session_id, RimeResult* result);

/**
 * 获取当前输入状态
 * 
 * @param session_id 会话ID
 * @param result 输出结果
 */
RIME_API void RimeGetCurrentState(int session_id, RimeResult* result);

/**
 * 释放结果内存
 * 
 * @param result 要释放的结果
 */
RIME_API void RimeFreeResult(RimeResult* result);

/**
 * 获取版本信息
 * 
 * @return 版本字符串
 */
RIME_API const char* RimeGetVersion();

/**
 * 检查Rime引擎是否可用
 * 
 * @return 1: 可用, 0: 不可用
 */
RIME_API int RimeIsAvailable();

#ifdef __cplusplus
}
#endif

#endif // RIME_DLL_H

