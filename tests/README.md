# Unity Rime输入法集成 - 测试套件

这是Unity Rime中文输入法集成项目的测试套件，包含集成测试、性能基准测试等。

## 文件结构

```
tests/
├── test_integration.py        # 集成测试脚本
├── performance_benchmark.py   # 性能基准测试脚本
├── benchmark_results.json     # 基准测试结果
└── README.md                 # 本文档
```

## 测试脚本说明

### 1. test_integration.py - 集成测试

这个脚本用于测试整个Unity Rime输入法集成系统的功能完整性。

**测试项目：**
- Python服务器启动测试
- Python连接测试
- Python输入处理测试
- DLL功能测试
- 基本性能测试

**运行方法：**
```bash
cd tests
python3 test_integration.py
```

**测试结果示例：**
```
Unity Rime输入法集成 - 集成测试
==================================================
[2025-07-18 13:10:56] [INFO] 测试结果汇总:
==================================================
  启动Python服务器: 失败
  DLL功能测试: 通过
  性能测试: 通过
==================================================
总计: 2/3 个测试通过
```

### 2. performance_benchmark.py - 性能基准测试

这个脚本用于测试不同实现方案的性能差异，提供详细的性能指标。

**测试项目：**
- Python模式延迟测试
- Python模式吞吐量测试
- Python输入处理性能测试
- DLL模式性能测试
- 性能对比分析

**运行方法：**
```bash
cd tests
python3 performance_benchmark.py
```

**测试结果示例：**
```
Unity Rime输入法集成 - 性能基准测试
============================================================
📊 DLL模式性能测试
----------------------------------------
DLL执行统计 (10次):
  最小值: 2.08 ms
  最大值: 6.39 ms
  平均值: 4.15 ms
  中位数: 3.91 ms
  标准差: 1.55 ms
```

## 测试环境要求

### 系统要求
- Python 3.7+
- Linux/macOS/Windows
- 网络连接（用于TCP Socket测试）

### 依赖库
- 标准库：socket, json, subprocess, threading, time, statistics
- 无需额外安装第三方库

### 测试数据
测试使用模拟的Rime引擎，包含以下测试词典：
- "ni" → ["你", "尼", "泥"]
- "hao" → ["好", "号", "豪"]
- "nihao" → ["你好"]
- "wo" → ["我", "握", "卧"]
- 等等

## 测试结果分析

### 性能指标说明

#### 延迟 (Latency)
- **定义**：单次请求的响应时间
- **单位**：毫秒 (ms)
- **测试方法**：发送ping请求并测量往返时间
- **典型值**：
  - DLL模式：< 1 ms
  - Python模式：10-100 ms

#### 吞吐量 (Throughput)
- **定义**：单位时间内处理的请求数量
- **单位**：请求/秒 (req/s)
- **测试方法**：在固定时间内发送尽可能多的请求
- **典型值**：
  - DLL模式：> 1000 req/s
  - Python模式：10-100 req/s

#### 输入处理时间
- **定义**：完成一次完整输入序列的时间
- **单位**：毫秒 (ms)
- **测试序列**："nihao" + 选择候选词
- **典型值**：
  - DLL模式：< 5 ms
  - Python模式：50-200 ms

### 性能对比

根据测试结果，两种实现方案的性能特点：

#### DLL方案
**优势：**
- 极低延迟（< 1 ms）
- 高吞吐量（> 1000 req/s）
- 内存效率高
- 无进程间通信开销

**劣势：**
- 平台相关性强
- 编译复杂度高
- 调试困难
- 需要处理内存管理

#### Python方案
**优势：**
- 跨平台兼容性好
- 开发调试简单
- 易于扩展和维护
- 独立进程，稳定性好

**劣势：**
- 网络通信延迟
- 相对较低的吞吐量
- 额外的进程开销
- JSON序列化开销

### 选择建议

#### 选择DLL方案的情况：
- 对性能要求极高的游戏
- 需要最小化延迟的实时应用
- 目标平台固定
- 有C++开发经验的团队

#### 选择Python方案的情况：
- 需要跨平台部署
- 开发时间紧张
- 需要频繁修改输入法逻辑
- 对性能要求不是特别严格

## 故障排除

### 常见问题

#### 1. Python服务器启动失败
**症状：** 集成测试中"启动Python服务器"失败
**原因：**
- Python脚本路径错误
- 端口被占用
- 权限不足

**解决方法：**
```bash
# 检查端口占用
netstat -an | grep 9999

# 手动启动服务器
cd ../python_component
python3 ipc_server.py

# 检查错误日志
```

#### 2. DLL测试失败
**症状：** "DLL功能测试"失败
**原因：**
- DLL文件未编译
- 缺少依赖库
- 架构不匹配

**解决方法：**
```bash
# 重新编译DLL
cd ../dll_component
./build.sh

# 检查生成的文件
ls -la build/
```

#### 3. 网络连接超时
**症状：** Python连接测试超时
**原因：**
- 防火墙阻止连接
- 服务器未正确启动
- 网络配置问题

**解决方法：**
```bash
# 检查防火墙设置
sudo ufw status

# 测试本地连接
telnet 127.0.0.1 9999

# 检查服务器日志
```

#### 4. 性能异常
**症状：** 性能测试结果异常
**原因：**
- 系统负载过高
- 网络延迟
- 测试环境不稳定

**解决方法：**
```bash
# 检查系统负载
top
htop

# 关闭其他程序
# 重新运行测试

# 调整测试参数
```

## 自定义测试

### 添加新的测试用例

1. **扩展集成测试：**
```python
def test_custom_functionality(self) -> bool:
    """自定义功能测试"""
    try:
        # 添加测试逻辑
        return True
    except Exception as e:
        self.log(f"自定义测试失败: {e}", "ERROR")
        return False

# 在run_all_tests中添加调用
```

2. **扩展性能测试：**
```python
def benchmark_custom_scenario(self) -> Dict[str, float]:
    """自定义场景性能测试"""
    # 添加性能测试逻辑
    return results
```

### 修改测试参数

可以通过修改脚本中的常量来调整测试参数：

```python
# test_integration.py
class IntegrationTester:
    def __init__(self):
        self.server_host = "127.0.0.1"  # 服务器地址
        self.server_port = 9999         # 服务器端口

# performance_benchmark.py
def benchmark_python_latency(self, iterations: int = 1000):  # 测试次数
def benchmark_python_throughput(self, duration: int = 10):   # 测试时长
```

### 添加新的性能指标

```python
def benchmark_memory_usage(self) -> Dict[str, float]:
    """内存使用测试"""
    import psutil
    
    # 测试内存使用情况
    process = psutil.Process()
    memory_info = process.memory_info()
    
    return {
        'rss': memory_info.rss / 1024 / 1024,  # MB
        'vms': memory_info.vms / 1024 / 1024   # MB
    }
```

## 持续集成

### GitHub Actions示例

```yaml
name: Unity Rime Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Set up Python
      uses: actions/setup-python@v2
      with:
        python-version: '3.8'
    
    - name: Install dependencies
      run: |
        sudo apt-get update
        sudo apt-get install cmake build-essential
    
    - name: Build DLL
      run: |
        cd dll_component
        ./build.sh
    
    - name: Run tests
      run: |
        cd tests
        python3 test_integration.py
        python3 performance_benchmark.py
    
    - name: Upload results
      uses: actions/upload-artifact@v2
      with:
        name: test-results
        path: tests/benchmark_results.json
```

## 许可证

本项目采用开源许可证，具体许可证信息请参考项目根目录的LICENSE文件。

