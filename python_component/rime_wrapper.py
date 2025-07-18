#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Rime输入法引擎的Python包装器
用于Unity游戏中的中文输入法集成

作者: Manus AI
版本: 1.0.0
"""

import os
import sys
import json
import logging
from typing import List, Dict, Optional, Any
from dataclasses import dataclass, asdict

# 配置日志
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('rime_wrapper.log'),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger(__name__)

@dataclass
class CandidateWord:
    """候选词数据结构"""
    text: str
    comment: str = ""
    index: int = 0

@dataclass
class InputState:
    """输入状态数据结构"""
    composition: str = ""  # 当前输入的拼音
    candidates: List[CandidateWord] = None
    page_size: int = 5
    page_no: int = 0
    is_last_page: bool = True
    
    def __post_init__(self):
        if self.candidates is None:
            self.candidates = []

class RimeWrapper:
    """Rime输入法引擎包装器"""
    
    def __init__(self, user_data_dir: str = None, shared_data_dir: str = None):
        """
        初始化Rime引擎
        
        Args:
            user_data_dir: 用户数据目录
            shared_data_dir: 共享数据目录
        """
        self.user_data_dir = user_data_dir or os.path.expanduser("~/.config/rime")
        self.shared_data_dir = shared_data_dir or "/usr/share/rime-data"
        self.session_id = None
        self.is_initialized = False
        
        # 尝试导入pyrime
        try:
            import pyrime
            self.pyrime = pyrime
            logger.info("PyRime模块导入成功")
        except ImportError as e:
            logger.error(f"PyRime模块导入失败: {e}")
            # 创建一个模拟的pyrime模块用于测试
            self.pyrime = self._create_mock_pyrime()
            logger.warning("使用模拟PyRime模块进行测试")
        
        self._initialize_rime()
    
    def _create_mock_pyrime(self):
        """创建模拟的PyRime模块用于测试"""
        class MockRime:
            def __init__(self):
                self.composition = ""
                self.candidates = []
                self.mock_dict = {
                    "ni": ["你", "尼", "泥"],
                    "hao": ["好", "号", "豪"],
                    "nihao": ["你好"],
                    "shi": ["是", "时", "事"],
                    "jie": ["界", "接", "街"],
                    "shijie": ["世界"],
                    "zhong": ["中", "钟", "重"],
                    "guo": ["国", "果", "过"],
                    "zhongguo": ["中国"]
                }
            
            def process_key(self, key_code):
                if key_code == 65288:  # Backspace
                    if self.composition:
                        self.composition = self.composition[:-1]
                        self._update_candidates()
                    return True
                elif key_code == 65293:  # Enter
                    if self.candidates:
                        return self.candidates[0]
                    return None
                elif 97 <= key_code <= 122:  # a-z
                    char = chr(key_code)
                    self.composition += char
                    self._update_candidates()
                    return True
                return False
            
            def _update_candidates(self):
                self.candidates = []
                if self.composition in self.mock_dict:
                    for i, word in enumerate(self.mock_dict[self.composition]):
                        self.candidates.append({
                            'text': word,
                            'comment': f"拼音: {self.composition}",
                            'index': i
                        })
            
            def get_candidates(self):
                return self.candidates
            
            def get_composition(self):
                return self.composition
            
            def select_candidate(self, index):
                if 0 <= index < len(self.candidates):
                    selected = self.candidates[index]['text']
                    self.composition = ""
                    self.candidates = []
                    return selected
                return None
            
            def clear_composition(self):
                self.composition = ""
                self.candidates = []
        
        class MockPyRime:
            def __init__(self):
                self.rime = MockRime()
            
            def create_session(self):
                return 1
            
            def destroy_session(self, session_id):
                pass
            
            def process_key(self, session_id, key_code):
                return self.rime.process_key(key_code)
            
            def get_context(self, session_id):
                return {
                    'composition': {
                        'preedit': self.rime.get_composition()
                    },
                    'menu': {
                        'candidates': self.rime.get_candidates(),
                        'page_size': 5,
                        'page_no': 0,
                        'is_last_page': True
                    }
                }
            
            def select_candidate(self, session_id, index):
                return self.rime.select_candidate(index)
            
            def clear_composition(self, session_id):
                self.rime.clear_composition()
        
        return MockPyRime()
    
    def _initialize_rime(self):
        """初始化Rime引擎"""
        try:
            # 确保数据目录存在
            os.makedirs(self.user_data_dir, exist_ok=True)
            
            # 创建会话
            self.session_id = self.pyrime.create_session()
            if self.session_id:
                self.is_initialized = True
                logger.info(f"Rime引擎初始化成功，会话ID: {self.session_id}")
            else:
                logger.error("Rime引擎初始化失败：无法创建会话")
        except Exception as e:
            logger.error(f"Rime引擎初始化失败: {e}")
    
    def process_key(self, key_code: int) -> Dict[str, Any]:
        """
        处理按键输入
        
        Args:
            key_code: 按键码
            
        Returns:
            包含处理结果的字典
        """
        if not self.is_initialized:
            return {"error": "Rime引擎未初始化"}
        
        try:
            # 处理按键
            result = self.pyrime.process_key(self.session_id, key_code)
            
            # 获取当前状态
            context = self.pyrime.get_context(self.session_id)
            
            # 构建返回结果
            input_state = self._build_input_state(context)
            
            return {
                "success": True,
                "processed": bool(result),
                "state": asdict(input_state)
            }
        except Exception as e:
            logger.error(f"处理按键失败: {e}")
            return {"error": str(e)}
    
    def select_candidate(self, index: int) -> Dict[str, Any]:
        """
        选择候选词
        
        Args:
            index: 候选词索引
            
        Returns:
            包含选择结果的字典
        """
        if not self.is_initialized:
            return {"error": "Rime引擎未初始化"}
        
        try:
            # 选择候选词
            selected_text = self.pyrime.select_candidate(self.session_id, index)
            
            # 获取更新后的状态
            context = self.pyrime.get_context(self.session_id)
            input_state = self._build_input_state(context)
            
            return {
                "success": True,
                "selected_text": selected_text,
                "state": asdict(input_state)
            }
        except Exception as e:
            logger.error(f"选择候选词失败: {e}")
            return {"error": str(e)}
    
    def clear_composition(self) -> Dict[str, Any]:
        """
        清空当前输入
        
        Returns:
            包含清空结果的字典
        """
        if not self.is_initialized:
            return {"error": "Rime引擎未初始化"}
        
        try:
            self.pyrime.clear_composition(self.session_id)
            
            return {
                "success": True,
                "state": asdict(InputState())
            }
        except Exception as e:
            logger.error(f"清空输入失败: {e}")
            return {"error": str(e)}
    
    def get_current_state(self) -> Dict[str, Any]:
        """
        获取当前输入状态
        
        Returns:
            包含当前状态的字典
        """
        if not self.is_initialized:
            return {"error": "Rime引擎未初始化"}
        
        try:
            context = self.pyrime.get_context(self.session_id)
            input_state = self._build_input_state(context)
            
            return {
                "success": True,
                "state": asdict(input_state)
            }
        except Exception as e:
            logger.error(f"获取状态失败: {e}")
            return {"error": str(e)}
    
    def _build_input_state(self, context: Dict) -> InputState:
        """
        从Rime上下文构建输入状态
        
        Args:
            context: Rime上下文
            
        Returns:
            InputState对象
        """
        composition = ""
        candidates = []
        page_size = 5
        page_no = 0
        is_last_page = True
        
        if context:
            # 获取当前输入的拼音
            if 'composition' in context and context['composition']:
                composition = context['composition'].get('preedit', '')
            
            # 获取候选词列表
            if 'menu' in context and context['menu']:
                menu = context['menu']
                if 'candidates' in menu:
                    for i, candidate in enumerate(menu['candidates']):
                        if isinstance(candidate, dict):
                            candidates.append(CandidateWord(
                                text=candidate.get('text', ''),
                                comment=candidate.get('comment', ''),
                                index=i
                            ))
                        else:
                            # 处理简单字符串格式的候选词
                            candidates.append(CandidateWord(
                                text=str(candidate),
                                comment='',
                                index=i
                            ))
                
                page_size = menu.get('page_size', 5)
                page_no = menu.get('page_no', 0)
                is_last_page = menu.get('is_last_page', True)
        
        return InputState(
            composition=composition,
            candidates=candidates,
            page_size=page_size,
            page_no=page_no,
            is_last_page=is_last_page
        )
    
    def __del__(self):
        """析构函数，清理资源"""
        if self.is_initialized and self.session_id:
            try:
                self.pyrime.destroy_session(self.session_id)
                logger.info("Rime会话已销毁")
            except Exception as e:
                logger.error(f"销毁Rime会话失败: {e}")

def main():
    """主函数，用于测试"""
    logger.info("启动Rime包装器测试")
    
    # 创建Rime包装器实例
    rime = RimeWrapper()
    
    if not rime.is_initialized:
        logger.error("Rime引擎初始化失败")
        return
    
    # 测试输入
    test_inputs = [
        (110, 'n'),  # n
        (105, 'i'),  # i
        (104, 'h'),  # h
        (97, 'a'),   # a
        (111, 'o'),  # o
    ]
    
    logger.info("开始测试输入...")
    for key_code, char in test_inputs:
        logger.info(f"输入字符: {char} (键码: {key_code})")
        result = rime.process_key(key_code)
        logger.info(f"处理结果: {json.dumps(result, ensure_ascii=False, indent=2)}")
    
    # 测试选择候选词
    logger.info("测试选择第一个候选词...")
    result = rime.select_candidate(0)
    logger.info(f"选择结果: {json.dumps(result, ensure_ascii=False, indent=2)}")

if __name__ == "__main__":
    main()

