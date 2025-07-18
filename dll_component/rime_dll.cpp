/**
 * Unity Rime输入法集成 - DLL实现文件
 * 
 * 作者: Manus AI
 * 版本: 1.0.0
 * 
 * 这个文件实现了Unity可以调用的Rime输入法DLL接口
 */

#include "rime_dll.h"
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <map>
#include <vector>
#include <string>

// 由于在沙盒环境中无法安装librime，我们创建一个模拟实现
// 在实际部署时，应该替换为真正的librime调用

// 模拟的Rime引擎类
class MockRimeEngine {
private:
    std::string composition;
    std::vector<std::pair<std::string, std::string>> candidates;
    std::map<std::string, std::vector<std::string>> mock_dict;
    
public:
    MockRimeEngine() {
        // 初始化模拟词典
        mock_dict["ni"] = {"你", "尼", "泥"};
        mock_dict["hao"] = {"好", "号", "豪"};
        mock_dict["nihao"] = {"你好"};
        mock_dict["shi"] = {"是", "时", "事"};
        mock_dict["jie"] = {"界", "接", "街"};
        mock_dict["shijie"] = {"世界"};
        mock_dict["zhong"] = {"中", "钟", "重"};
        mock_dict["guo"] = {"国", "果", "过"};
        mock_dict["zhongguo"] = {"中国"};
        mock_dict["wo"] = {"我", "握", "卧"};
        mock_dict["ai"] = {"爱", "哀", "挨"};
        mock_dict["woai"] = {"我爱"};
        mock_dict["ni"] = {"你", "尼", "泥"};
    }
    
    bool processKey(int key_code) {
        if (key_code == 65288) { // Backspace
            if (!composition.empty()) {
                composition.pop_back();
                updateCandidates();
            }
            return true;
        } else if (key_code == 65293) { // Enter
            if (!candidates.empty()) {
                return true;
            }
            return false;
        } else if (key_code >= 97 && key_code <= 122) { // a-z
            char ch = static_cast<char>(key_code);
            composition += ch;
            updateCandidates();
            return true;
        }
        return false;
    }
    
    void updateCandidates() {
        candidates.clear();
        if (mock_dict.find(composition) != mock_dict.end()) {
            const auto& words = mock_dict[composition];
            for (const auto& word : words) {
                candidates.push_back({word, "拼音: " + composition});
            }
        }
    }
    
    std::string selectCandidate(int index) {
        if (index >= 0 && index < static_cast<int>(candidates.size())) {
            std::string selected = candidates[index].first;
            composition.clear();
            candidates.clear();
            return selected;
        }
        return "";
    }
    
    void clearComposition() {
        composition.clear();
        candidates.clear();
    }
    
    const std::string& getComposition() const {
        return composition;
    }
    
    const std::vector<std::pair<std::string, std::string>>& getCandidates() const {
        return candidates;
    }
};

// 全局变量
static std::map<int, MockRimeEngine*> sessions;
static int next_session_id = 1;

// 辅助函数
void fillRimeResult(RimeResult* result, bool success, const char* error_msg, 
                   const char* selected_text, MockRimeEngine* engine) {
    if (!result) return;
    
    // 清零结果结构
    memset(result, 0, sizeof(RimeResult));
    
    result->success = success ? 1 : 0;
    
    if (error_msg) {
        strncpy(result->error_message, error_msg, sizeof(result->error_message) - 1);
    }
    
    if (selected_text) {
        strncpy(result->selected_text, selected_text, sizeof(result->selected_text) - 1);
    }
    
    if (engine) {
        // 填充输入状态
        const std::string& comp = engine->getComposition();
        strncpy(result->state.composition, comp.c_str(), sizeof(result->state.composition) - 1);
        
        const auto& candidates = engine->getCandidates();
        result->state.candidate_count = static_cast<int>(candidates.size());
        
        if (result->state.candidate_count > 0) {
            result->state.candidates = static_cast<RimeCandidate*>(
                malloc(sizeof(RimeCandidate) * result->state.candidate_count));
            
            for (int i = 0; i < result->state.candidate_count; ++i) {
                memset(&result->state.candidates[i], 0, sizeof(RimeCandidate));
                strncpy(result->state.candidates[i].text, 
                       candidates[i].first.c_str(), 
                       sizeof(result->state.candidates[i].text) - 1);
                strncpy(result->state.candidates[i].comment, 
                       candidates[i].second.c_str(), 
                       sizeof(result->state.candidates[i].comment) - 1);
                result->state.candidates[i].index = i;
            }
        } else {
            result->state.candidates = nullptr;
        }
        
        result->state.page_size = 5;
        result->state.page_no = 0;
        result->state.is_last_page = 1;
    }
}

// API实现

RIME_API int RimeInitialize(const char* user_data_dir, const char* shared_data_dir) {
    try {
        MockRimeEngine* engine = new MockRimeEngine();
        int session_id = next_session_id++;
        sessions[session_id] = engine;
        return session_id;
    } catch (...) {
        return 0;
    }
}

RIME_API void RimeDestroy(int session_id) {
    auto it = sessions.find(session_id);
    if (it != sessions.end()) {
        delete it->second;
        sessions.erase(it);
    }
}

RIME_API void RimeProcessKey(int session_id, int key_code, RimeResult* result) {
    auto it = sessions.find(session_id);
    if (it == sessions.end()) {
        fillRimeResult(result, false, "Invalid session ID", nullptr, nullptr);
        return;
    }
    
    MockRimeEngine* engine = it->second;
    bool processed = engine->processKey(key_code);
    
    fillRimeResult(result, true, nullptr, nullptr, engine);
}

RIME_API void RimeSelectCandidate(int session_id, int index, RimeResult* result) {
    auto it = sessions.find(session_id);
    if (it == sessions.end()) {
        fillRimeResult(result, false, "Invalid session ID", nullptr, nullptr);
        return;
    }
    
    MockRimeEngine* engine = it->second;
    std::string selected = engine->selectCandidate(index);
    
    fillRimeResult(result, true, nullptr, selected.c_str(), engine);
}

RIME_API void RimeClearComposition(int session_id, RimeResult* result) {
    auto it = sessions.find(session_id);
    if (it == sessions.end()) {
        fillRimeResult(result, false, "Invalid session ID", nullptr, nullptr);
        return;
    }
    
    MockRimeEngine* engine = it->second;
    engine->clearComposition();
    
    fillRimeResult(result, true, nullptr, nullptr, engine);
}

RIME_API void RimeGetCurrentState(int session_id, RimeResult* result) {
    auto it = sessions.find(session_id);
    if (it == sessions.end()) {
        fillRimeResult(result, false, "Invalid session ID", nullptr, nullptr);
        return;
    }
    
    MockRimeEngine* engine = it->second;
    fillRimeResult(result, true, nullptr, nullptr, engine);
}

RIME_API void RimeFreeResult(RimeResult* result) {
    if (result && result->state.candidates) {
        free(result->state.candidates);
        result->state.candidates = nullptr;
        result->state.candidate_count = 0;
    }
}

RIME_API const char* RimeGetVersion() {
    return "Unity Rime DLL v1.0.0 (Mock Implementation)";
}

RIME_API int RimeIsAvailable() {
    return 1; // 模拟实现总是可用
}

