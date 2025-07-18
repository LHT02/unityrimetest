#!/usr/bin/env python3
"""
Unity Rime输入法集成 - 性能基准测试

作者: Manus AI
版本: 1.0.0

这个脚本用于测试不同实现方案的性能差异
"""

import sys
import os
import time
import json
import socket
import subprocess
import statistics
from typing import List, Dict, Any

# 添加项目路径
sys.path.append(os.path.join(os.path.dirname(__file__), '..', 'python_component'))

class PerformanceBenchmark:
    """性能基准测试器"""
    
    def __init__(self):
        self.server_process = None
        self.server_host = "127.0.0.1"
        self.server_port = 9999
        
    def log(self, message: str):
        """记录日志"""
        timestamp = time.strftime("%H:%M:%S")
        print(f"[{timestamp}] {message}")
        
    def start_python_server(self) -> bool:
        """启动Python服务器"""
        try:
            server_script = os.path.join(os.path.dirname(__file__), '..', 'python_component', 'ipc_server.py')
            if not os.path.exists(server_script):
                return False
            
            self.server_process = subprocess.Popen([
                sys.executable, server_script
            ], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
            
            time.sleep(2)
            return self.server_process.poll() is None
            
        except Exception:
            return False
    
    def stop_python_server(self):
        """停止Python服务器"""
        if self.server_process:
            try:
                self.server_process.terminate()
                self.server_process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                self.server_process.kill()
    
    def benchmark_python_latency(self, iterations: int = 1000) -> Dict[str, float]:
        """测试Python模式延迟"""
        self.log(f"测试Python模式延迟 ({iterations} 次迭代)...")
        
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(30)
            sock.connect((self.server_host, self.server_port))
            
            latencies = []
            
            for i in range(iterations):
                start_time = time.perf_counter()
                
                # 发送ping请求
                request = {"command": "ping", "params": {}}
                self._send_request(sock, request)
                self._receive_response(sock)
                
                end_time = time.perf_counter()
                latency = (end_time - start_time) * 1000  # 转换为毫秒
                latencies.append(latency)
                
                if (i + 1) % 100 == 0:
                    self.log(f"  完成 {i + 1}/{iterations} 次测试")
            
            sock.close()
            
            return {
                'min': min(latencies),
                'max': max(latencies),
                'mean': statistics.mean(latencies),
                'median': statistics.median(latencies),
                'stdev': statistics.stdev(latencies) if len(latencies) > 1 else 0
            }
            
        except Exception as e:
            self.log(f"Python延迟测试失败: {e}")
            return {}
    
    def benchmark_python_throughput(self, duration: int = 10) -> float:
        """测试Python模式吞吐量"""
        self.log(f"测试Python模式吞吐量 ({duration} 秒)...")
        
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(30)
            sock.connect((self.server_host, self.server_port))
            
            start_time = time.time()
            end_time = start_time + duration
            request_count = 0
            
            while time.time() < end_time:
                request = {"command": "ping", "params": {}}
                self._send_request(sock, request)
                self._receive_response(sock)
                request_count += 1
                
                if request_count % 100 == 0:
                    elapsed = time.time() - start_time
                    current_rate = request_count / elapsed
                    self.log(f"  当前速率: {current_rate:.1f} 请求/秒")
            
            sock.close()
            
            actual_duration = time.time() - start_time
            throughput = request_count / actual_duration
            
            return throughput
            
        except Exception as e:
            self.log(f"Python吞吐量测试失败: {e}")
            return 0
    
    def benchmark_python_input_processing(self, iterations: int = 100) -> Dict[str, float]:
        """测试Python输入处理性能"""
        self.log(f"测试Python输入处理性能 ({iterations} 次迭代)...")
        
        try:
            sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            sock.settimeout(30)
            sock.connect((self.server_host, self.server_port))
            
            processing_times = []
            test_sequence = [110, 105, 104, 97, 111]  # "nihao"
            
            for i in range(iterations):
                start_time = time.perf_counter()
                
                # 处理完整的输入序列
                for key_code in test_sequence:
                    request = {"command": "process_key", "params": {"key_code": key_code}}
                    self._send_request(sock, request)
                    self._receive_response(sock)
                
                # 选择候选词
                request = {"command": "select_candidate", "params": {"index": 0}}
                self._send_request(sock, request)
                self._receive_response(sock)
                
                end_time = time.perf_counter()
                processing_time = (end_time - start_time) * 1000
                processing_times.append(processing_time)
                
                if (i + 1) % 10 == 0:
                    self.log(f"  完成 {i + 1}/{iterations} 次输入序列")
            
            sock.close()
            
            return {
                'min': min(processing_times),
                'max': max(processing_times),
                'mean': statistics.mean(processing_times),
                'median': statistics.median(processing_times),
                'stdev': statistics.stdev(processing_times) if len(processing_times) > 1 else 0
            }
            
        except Exception as e:
            self.log(f"Python输入处理测试失败: {e}")
            return {}
    
    def benchmark_dll_performance(self, iterations: int = 1000) -> Dict[str, float]:
        """测试DLL性能"""
        self.log(f"测试DLL性能 ({iterations} 次迭代)...")
        
        try:
            dll_build_dir = os.path.join(os.path.dirname(__file__), '..', 'dll_component', 'build')
            test_dll_path = os.path.join(dll_build_dir, 'test_dll')
            
            if not os.path.exists(test_dll_path):
                self.log("DLL测试程序不存在")
                return {}
            
            # 运行DLL性能测试
            # 这里简化处理，实际应该修改test_dll.cpp来支持性能测试
            times = []
            
            for i in range(min(iterations, 10)):  # 限制次数，避免过长时间
                start_time = time.perf_counter()
                
                result = subprocess.run([test_dll_path], 
                                      capture_output=True, 
                                      text=True, 
                                      timeout=10)
                
                end_time = time.perf_counter()
                
                if result.returncode == 0:
                    execution_time = (end_time - start_time) * 1000
                    times.append(execution_time)
                
                if (i + 1) % 5 == 0:
                    self.log(f"  完成 {i + 1}/10 次DLL测试")
            
            if times:
                return {
                    'min': min(times),
                    'max': max(times),
                    'mean': statistics.mean(times),
                    'median': statistics.median(times),
                    'stdev': statistics.stdev(times) if len(times) > 1 else 0
                }
            else:
                return {}
                
        except Exception as e:
            self.log(f"DLL性能测试失败: {e}")
            return {}
    
    def _send_request(self, sock: socket.socket, request: Dict[str, Any]) -> bool:
        """发送请求"""
        try:
            request_json = json.dumps(request)
            request_data = request_json.encode('utf-8')
            
            length_data = len(request_data).to_bytes(4, byteorder='little')
            sock.send(length_data)
            sock.send(request_data)
            
            return True
        except Exception:
            return False
    
    def _receive_response(self, sock: socket.socket) -> Dict[str, Any]:
        """接收响应"""
        try:
            response_length_data = sock.recv(4)
            response_length = int.from_bytes(response_length_data, byteorder='little')
            
            response_data = sock.recv(response_length)
            response_json = response_data.decode('utf-8')
            return json.loads(response_json)
            
        except Exception:
            return {}
    
    def run_benchmark(self):
        """运行完整的基准测试"""
        print("Unity Rime输入法集成 - 性能基准测试")
        print("=" * 60)
        
        # 启动Python服务器
        if not self.start_python_server():
            print("❌ 无法启动Python服务器，跳过Python相关测试")
            python_available = False
        else:
            print("✅ Python服务器启动成功")
            python_available = True
        
        results = {}
        
        # Python性能测试
        if python_available:
            print("\n📊 Python模式性能测试")
            print("-" * 40)
            
            # 延迟测试
            latency_results = self.benchmark_python_latency(1000)
            if latency_results:
                results['python_latency'] = latency_results
                print(f"延迟统计 (1000次):")
                print(f"  最小值: {latency_results['min']:.2f} ms")
                print(f"  最大值: {latency_results['max']:.2f} ms")
                print(f"  平均值: {latency_results['mean']:.2f} ms")
                print(f"  中位数: {latency_results['median']:.2f} ms")
                print(f"  标准差: {latency_results['stdev']:.2f} ms")
            
            # 吞吐量测试
            throughput = self.benchmark_python_throughput(10)
            if throughput > 0:
                results['python_throughput'] = throughput
                print(f"\n吞吐量: {throughput:.1f} 请求/秒")
            
            # 输入处理测试
            input_results = self.benchmark_python_input_processing(50)
            if input_results:
                results['python_input'] = input_results
                print(f"\n输入处理统计 (50次完整序列):")
                print(f"  最小值: {input_results['min']:.2f} ms")
                print(f"  最大值: {input_results['max']:.2f} ms")
                print(f"  平均值: {input_results['mean']:.2f} ms")
                print(f"  中位数: {input_results['median']:.2f} ms")
                print(f"  标准差: {input_results['stdev']:.2f} ms")
        
        # DLL性能测试
        print("\n📊 DLL模式性能测试")
        print("-" * 40)
        
        dll_results = self.benchmark_dll_performance(10)
        if dll_results:
            results['dll_performance'] = dll_results
            print(f"DLL执行统计 (10次):")
            print(f"  最小值: {dll_results['min']:.2f} ms")
            print(f"  最大值: {dll_results['max']:.2f} ms")
            print(f"  平均值: {dll_results['mean']:.2f} ms")
            print(f"  中位数: {dll_results['median']:.2f} ms")
            print(f"  标准差: {dll_results['stdev']:.2f} ms")
        else:
            print("❌ DLL性能测试失败")
        
        # 性能对比
        if python_available and 'python_latency' in results and 'dll_performance' in results:
            print("\n📈 性能对比")
            print("-" * 40)
            
            python_avg = results['python_latency']['mean']
            dll_avg = results['dll_performance']['mean']
            
            if dll_avg > 0:
                speedup = python_avg / dll_avg
                print(f"Python平均延迟: {python_avg:.2f} ms")
                print(f"DLL平均执行时间: {dll_avg:.2f} ms")
                print(f"DLL相对加速比: {speedup:.1f}x")
            
            if 'python_throughput' in results:
                print(f"Python吞吐量: {results['python_throughput']:.1f} 请求/秒")
        
        # 清理
        if python_available:
            self.stop_python_server()
        
        print("\n✅ 基准测试完成")
        
        # 保存结果到文件
        self.save_results(results)
        
        return results
    
    def save_results(self, results: Dict[str, Any]):
        """保存测试结果到文件"""
        try:
            results_file = os.path.join(os.path.dirname(__file__), 'benchmark_results.json')
            
            # 添加时间戳
            results['timestamp'] = time.time()
            results['datetime'] = time.strftime("%Y-%m-%d %H:%M:%S")
            
            with open(results_file, 'w', encoding='utf-8') as f:
                json.dump(results, f, indent=2, ensure_ascii=False)
            
            print(f"\n📄 测试结果已保存到: {results_file}")
            
        except Exception as e:
            print(f"❌ 保存结果失败: {e}")

def main():
    """主函数"""
    benchmark = PerformanceBenchmark()
    benchmark.run_benchmark()

if __name__ == "__main__":
    main()

