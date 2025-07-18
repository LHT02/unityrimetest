/**
 * Unity Rime输入法集成 - DLL测试程序
 * 
 * 作者: Manus AI
 * 版本: 1.0.0
 * 
 * 这个程序用于测试Rime DLL的功能
 */

#include "rime_dll.h"
#include <iostream>
#include <string>
#include <vector>
#include <cstring>

void printResult(const RimeResult& result) {
    std::cout << "操作结果: " << (result.success ? "成功" : "失败") << std::endl;
    
    if (!result.success) {
        std::cout << "错误信息: " << result.error_message << std::endl;
        return;
    }
    
    if (strlen(result.selected_text) > 0) {
        std::cout << "选中文本: " << result.selected_text << std::endl;
    }
    
    std::cout << "当前输入: " << result.state.composition << std::endl;
    std::cout << "候选词数量: " << result.state.candidate_count << std::endl;
    
    for (int i = 0; i < result.state.candidate_count; ++i) {
        std::cout << "  " << i << ": " << result.state.candidates[i].text 
                  << " (" << result.state.candidates[i].comment << ")" << std::endl;
    }
    
    std::cout << "页面信息: " << result.state.page_no + 1 << "/" 
              << (result.state.is_last_page ? "最后页" : "有更多页") << std::endl;
    std::cout << "---" << std::endl;
}

int main() {
    std::cout << "Unity Rime DLL 测试程序" << std::endl;
    std::cout << "版本: " << RimeGetVersion() << std::endl;
    std::cout << "可用性: " << (RimeIsAvailable() ? "是" : "否") << std::endl;
    std::cout << std::endl;
    
    // 初始化Rime引擎
    int session_id = RimeInitialize(nullptr, nullptr);
    if (session_id == 0) {
        std::cerr << "初始化Rime引擎失败" << std::endl;
        return 1;
    }
    
    std::cout << "Rime引擎初始化成功，会话ID: " << session_id << std::endl;
    std::cout << std::endl;
    
    // 测试输入 "nihao"
    std::vector<int> test_keys = {110, 105, 104, 97, 111}; // n, i, h, a, o
    std::vector<char> test_chars = {'n', 'i', 'h', 'a', 'o'};
    
    RimeResult result;
    
    std::cout << "测试输入 'nihao':" << std::endl;
    for (size_t i = 0; i < test_keys.size(); ++i) {
        std::cout << "输入字符: " << test_chars[i] << " (键码: " << test_keys[i] << ")" << std::endl;
        
        RimeProcessKey(session_id, test_keys[i], &result);
        printResult(result);
        RimeFreeResult(&result);
    }
    
    // 测试选择候选词
    std::cout << "选择第一个候选词:" << std::endl;
    RimeSelectCandidate(session_id, 0, &result);
    printResult(result);
    RimeFreeResult(&result);
    
    // 测试清空输入
    std::cout << "测试新的输入 'wo':" << std::endl;
    std::vector<int> test_keys2 = {119, 111}; // w, o
    std::vector<char> test_chars2 = {'w', 'o'};
    
    for (size_t i = 0; i < test_keys2.size(); ++i) {
        std::cout << "输入字符: " << test_chars2[i] << " (键码: " << test_keys2[i] << ")" << std::endl;
        
        RimeProcessKey(session_id, test_keys2[i], &result);
        printResult(result);
        RimeFreeResult(&result);
    }
    
    // 测试清空输入
    std::cout << "清空当前输入:" << std::endl;
    RimeClearComposition(session_id, &result);
    printResult(result);
    RimeFreeResult(&result);
    
    // 测试获取当前状态
    std::cout << "获取当前状态:" << std::endl;
    RimeGetCurrentState(session_id, &result);
    printResult(result);
    RimeFreeResult(&result);
    
    // 销毁Rime引擎
    RimeDestroy(session_id);
    std::cout << "Rime引擎已销毁" << std::endl;
    
    return 0;
}

