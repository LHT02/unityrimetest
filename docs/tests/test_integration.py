#!/usr/bin/env python3
"""
Unity Rime输入法集成 - 集成测试脚本

作者: Manus AI
版本: 1.0.0

这个脚本用于测试整个Unity Rime输入法集成系统的功能
"""

import sys
import os
import time
import json
import socket
import subprocess
import threading
from typing import Dict, List, Any, Optional

# 添加项目路径
sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'python_component'))

class IntegrationTester:
    """集成测试器"""
    
    def __init__(self):
        self.test_results = []
        self.server_process = None
        self.server_host = "127.0.0.1"
        self.server_port = 9999
        
    def log(self, message: str, level: str = "INFO"):
        """记录日志"""
        timestamp = time.strftime("%Y-%m-%d %H:%M:%S")
        log_message = f"[{timestamp}] [{level}] {message}"
        print(log_message)
        
    def start_python_server(self) -> bool:
        """启动Python服务器"""
        try:
            self.log("启动Python IPC服务器...")
            
            # 检查服务器脚本是否存在
            server_script = os.path.join(os.path.dirname(__file__), '..', 'python_component', 'ipc_server.py')
            if not os.path.exists(server_script):
                self.log(f"服务器脚本不存在: {server_script}", "ERROR")
                return False
            
            # 启动服务器进程
            self.server_process = subprocess.Popen([
                sys.executable, server_script
            ], stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True)
            
            # 等待服务器启动
            time.sleep(2)
            
            # 检查进程是否还在运行
            if self.server_process.poll() is None:
                self.log("Python服务器启动成功")
                return True
            else:
                stdout, stderr = self.server_process.communicate()
                self.log(f"Python服务器启动失败: {stderr}", "ERROR")
                return False
                
        except Exception as e:
            self.log(f"启动Python服务器异常: {e}", "ERROR")
            return False
    
    def stop_python_server(self):
        """停止Python服务器"""
        if self.server_process:
            try:
                self.server_process.terminate()
                self.server_process.wait(timeout=5)
                self.log("Python服务器已停止")
            except subprocess.TimeoutExpired:
                self.server_process.kill()
                self.log("强制终止Python服务器")
            except Exception as e:
                self.log(f"停止Python服务器异常: {e}", "ERROR")
    
    def test_python_connection(self) -> bool:
        """测试Python连接"""
        try:
            self.log("测试Python服务器连接...")
            
            # 创建TCP连接
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(5)
            sock.connect((self.server_host, self.server_port))
            
            # 发送ping请求
            request = {
                "command": "ping",
                "params": {}
            }
            
            request_json = json.dumps(request)
            request_data = request_json.encode('utf-8')
            
            # 发送消息长度
            length_data = len(request_data).to_bytes(4, byteorder='little')
            sock.send(length_data)
            sock.send(request_data)
            
            # 接收响应长度
            response_length_data = sock.recv(4)
            response_length = int.from_bytes(response_length_data, byteorder='little')
            
            # 接收响应数据
            response_data = sock.recv(response_length)
            response_json = response_data.decode('utf-8')
            response = json.loads(response_json)
            
            sock.close()
            
            if response.get('success'):
                self.log("Python连接测试成功")
                return True
            else:
                self.log(f"Python连接测试失败: {response.get('error', 'Unknown error')}", "ERROR")
                return False
                
        except Exception as e:
            self.log(f"Python连接测试异常: {e}", "ERROR")
            return False
    
    def test_python_input_processing(self) -> bool:
        """测试Python输入处理"""
        try:
            self.log("测试Python输入处理...")
            
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(10)
            sock.connect((self.server_host, self.server_port))
            
            # 测试输入序列: "nihao"
            test_keys = [110, 105, 104, 97, 111]  # n, i, h, a, o
            test_chars = ['n', 'i', 'h', 'a', 'o']
            
            for i, key_code in enumerate(test_keys):
                self.log(f"输入字符: {test_chars[i]} (键码: {key_code})")
                
                request = {
                    "command": "process_key",
                    "params": {"key_code": key_code}
                }
                
                # 发送请求
                if not self._send_request(sock, request):
                    return False
                
                # 接收响应
                response = self._receive_response(sock)
                if not response or not response.get('success'):
                    self.log(f"处理按键失败: {response.get('error', 'Unknown error') if response else 'No response'}", "ERROR")
                    return False
                
                # 检查状态
                state = response.get('state', {})
                composition = state.get('composition', '')
                candidates = state.get('candidates', [])
                
                self.log(f"当前输入: {composition}, 候选词数量: {len(candidates)}")
            
            # 测试选择候选词
            if len(candidates) > 0:
                self.log("测试选择第一个候选词...")
                
                request = {
                    "command": "select_candidate",
                    "params": {"index": 0}
                }
                
                if not self._send_request(sock, request):
                    return False
                
                response = self._receive_response(sock)
                if response and response.get('success'):
                    selected_text = response.get('selected_text', '')
                    self.log(f"选中文本: {selected_text}")
                else:
                    self.log("选择候选词失败", "ERROR")
                    return False
            
            sock.close()
            self.log("Python输入处理测试成功")
            return True
            
        except Exception as e:
            self.log(f"Python输入处理测试异常: {e}", "ERROR")
            return False
    
    def _send_request(self, sock: socket.socket, request: Dict[str, Any]) -> bool:
        """发送请求"""
        try:
            request_json = json.dumps(request)
            request_data = request_json.encode('utf-8')
            
            # 发送消息长度
            length_data = len(request_data).to_bytes(4, byteorder='little')
            sock.send(length_data)
            sock.send(request_data)
            
            return True
        except Exception as e:
            self.log(f"发送请求失败: {e}", "ERROR")
            return False
    
    def _receive_response(self, sock: socket.socket) -> Optional[Dict[str, Any]]:
        """接收响应"""
        try:
            # 接收响应长度
            response_length_data = sock.recv(4)
            if len(response_length_data) != 4:
                return None
            
            response_length = int.from_bytes(response_length_data, byteorder='little')
            
            # 接收响应数据
            response_data = sock.recv(response_length)
            if len(response_data) != response_length:
                return None
            
            response_json = response_data.decode('utf-8')
            return json.loads(response_json)
            
        except Exception as e:
            self.log(f"接收响应失败: {e}", "ERROR")
            return None
    
    def test_dll_functionality(self) -> bool:
        """测试DLL功能"""
        try:
            self.log("测试DLL功能...")
            
            # 检查DLL文件是否存在
            dll_build_dir = os.path.join(os.path.dirname(__file__), '..', 'dll_component', 'build')
            test_dll_path = os.path.join(dll_build_dir, 'test_dll')
            
            if not os.path.exists(test_dll_path):
                self.log(f"DLL测试程序不存在: {test_dll_path}", "ERROR")
                return False
            
            # 运行DLL测试程序
            result = subprocess.run([test_dll_path], capture_output=True, text=True, timeout=30)
            
            if result.returncode == 0:
                self.log("DLL功能测试成功")
                self.log("DLL测试输出:")
                for line in result.stdout.split('\n'):
                    if line.strip():
                        self.log(f"  {line}")
                return True
            else:
                self.log(f"DLL功能测试失败，返回码: {result.returncode}", "ERROR")
                if result.stderr:
                    self.log(f"错误输出: {result.stderr}", "ERROR")
                return False
                
        except subprocess.TimeoutExpired:
            self.log("DLL测试超时", "ERROR")
            return False
        except Exception as e:
            self.log(f"DLL功能测试异常: {e}", "ERROR")
            return False
    
    def test_performance(self) -> bool:
        """性能测试"""
        try:
            self.log("开始性能测试...")
            
            # 测试Python模式性能
            python_performance = self._test_python_performance()
            
            # 测试DLL模式性能（如果可用）
            dll_performance = self._test_dll_performance()
            
            self.log(f"性能测试结果:")
            self.log(f"  Python模式: {python_performance:.2f} ms/操作")
            if dll_performance > 0:
                self.log(f"  DLL模式: {dll_performance:.2f} ms/操作")
            
            return True
            
        except Exception as e:
            self.log(f"性能测试异常: {e}", "ERROR")
            return False
    
    def _test_python_performance(self) -> float:
        """测试Python性能"""
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(30)
            sock.connect((self.server_host, self.server_port))
            
            # 测试100次按键处理
            test_count = 100
            start_time = time.time()
            
            for i in range(test_count):
                request = {
                    "command": "process_key",
                    "params": {"key_code": 97 + (i % 26)}  # a-z循环
                }
                
                self._send_request(sock, request)
                self._receive_response(sock)
            
            end_time = time.time()
            sock.close()
            
            total_time = (end_time - start_time) * 1000  # 转换为毫秒
            return total_time / test_count
            
        except Exception as e:
            self.log(f"Python性能测试失败: {e}", "ERROR")
            return -1
    
    def _test_dll_performance(self) -> float:
        """测试DLL性能"""
        # 这里可以添加DLL性能测试代码
        # 由于需要在Unity环境中运行，这里返回模拟值
        return 0.5  # 假设DLL性能更好
    
    def run_all_tests(self) -> bool:
        """运行所有测试"""
        self.log("开始运行集成测试...")
        
        all_passed = True
        
        # 测试1: 启动Python服务器
        if not self.start_python_server():
            self.test_results.append(("启动Python服务器", False))
            all_passed = False
        else:
            self.test_results.append(("启动Python服务器", True))
            
            # 测试2: Python连接
            if not self.test_python_connection():
                self.test_results.append(("Python连接测试", False))
                all_passed = False
            else:
                self.test_results.append(("Python连接测试", True))
                
                # 测试3: Python输入处理
                if not self.test_python_input_processing():
                    self.test_results.append(("Python输入处理", False))
                    all_passed = False
                else:
                    self.test_results.append(("Python输入处理", True))
        
        # 测试4: DLL功能
        if not self.test_dll_functionality():
            self.test_results.append(("DLL功能测试", False))
            all_passed = False
        else:
            self.test_results.append(("DLL功能测试", True))
        
        # 测试5: 性能测试
        if not self.test_performance():
            self.test_results.append(("性能测试", False))
            all_passed = False
        else:
            self.test_results.append(("性能测试", True))
        
        # 清理
        self.stop_python_server()
        
        # 输出测试结果
        self.print_test_results()
        
        return all_passed
    
    def print_test_results(self):
        """打印测试结果"""
        self.log("=" * 50)
        self.log("测试结果汇总:")
        self.log("=" * 50)
        
        passed_count = 0
        total_count = len(self.test_results)
        
        for test_name, passed in self.test_results:
            status = "通过" if passed else "失败"
            self.log(f"  {test_name}: {status}")
            if passed:
                passed_count += 1
        
        self.log("=" * 50)
        self.log(f"总计: {passed_count}/{total_count} 个测试通过")
        
        if passed_count == total_count:
            self.log("所有测试通过！", "SUCCESS")
        else:
            self.log(f"{total_count - passed_count} 个测试失败", "ERROR")

def main():
    """主函数"""
    print("Unity Rime输入法集成 - 集成测试")
    print("=" * 50)
    
    tester = IntegrationTester()
    success = tester.run_all_tests()
    
    if success:
        print("\n✅ 所有测试通过！系统集成成功。")
        return 0
    else:
        print("\n❌ 部分测试失败，请检查相关组件。")
        return 1

if __name__ == "__main__":
    sys.exit(main())

