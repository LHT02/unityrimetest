# Unity Rime输入法集成 - 安装指南

**作者：** Manus AI  
**版本：** 1.0.0  
**日期：** 2025年7月18日

## 概述

本文档提供了Unity Rime中文输入法集成解决方案的详细安装指南，涵盖了不同平台、不同实现方案的安装步骤和配置方法。无论您是初次接触本项目的开发者，还是需要在生产环境中部署的运维人员，都可以在本文档中找到所需的信息。

## 系统要求

### 基本要求

**操作系统支持：**
- Windows 10/11 (x64)
- macOS 10.14+ (Intel/Apple Silicon)
- Ubuntu 18.04+ / CentOS 7+ / 其他主流Linux发行版
- iOS 12+ (仅DLL方案，需额外适配)
- Android 7.0+ (API Level 24+)

**Unity版本要求：**
- Unity 2019.4 LTS 或更高版本
- 推荐使用Unity 2021.3 LTS或Unity 2022.3 LTS
- 支持.NET Standard 2.1或.NET Framework 4.x

**硬件要求：**
- CPU: 双核1.5GHz或更高
- 内存: 4GB RAM (推荐8GB)
- 存储: 至少500MB可用空间
- 网络: 用于Python方案的本地网络通信

### DLL方案额外要求

**开发环境：**
- Windows: Visual Studio 2017或更高版本，包含C++工具集
- macOS: Xcode 10或更高版本，包含命令行工具
- Linux: GCC 7+或Clang 5+，CMake 3.10+

**运行时依赖：**
- Windows: Visual C++ Redistributable 2017或更高版本
- Linux: librime1, librime-dev (可选，用于真实Rime引擎)
- macOS: 无额外依赖

### Python方案额外要求

**Python环境：**
- Python 3.7或更高版本
- pip包管理器
- 网络连接能力

**可选依赖：**
- PyRime库 (用于真实Rime引擎，项目包含模拟实现)
- librime开发库

## 快速安装

### 方案一：DLL集成（推荐用于高性能需求）

#### 步骤1：获取项目文件

```bash
# 克隆项目仓库
git clone https://github.com/your-repo/unity-rime-integration.git
cd unity-rime-integration
```

#### 步骤2：编译DLL组件

**Linux/macOS:**
```bash
cd dll_component

# 安装构建依赖 (Ubuntu)
sudo apt-get update
sudo apt-get install cmake build-essential

# 安装构建依赖 (macOS)
brew install cmake

# 编译DLL
chmod +x build.sh
./build.sh
```

**Windows:**
```cmd
cd dll_component

# 使用Visual Studio命令提示符
mkdir build
cd build
cmake .. -G "Visual Studio 16 2019" -A x64
cmake --build . --config Release
```

#### 步骤3：集成到Unity项目

1. **复制DLL文件到Unity项目：**
   ```bash
   # 创建Plugins目录（如果不存在）
   mkdir -p YourUnityProject/Assets/Plugins
   
   # 复制DLL文件
   # Linux
   cp dll_component/build/librime_dll.so YourUnityProject/Assets/Plugins/
   
   # Windows
   copy dll_component\build\Release\rime_dll.dll YourUnityProject\Assets\Plugins\
   
   # macOS
   cp dll_component/build/librime_dll.dylib YourUnityProject/Assets/Plugins/
   ```

2. **复制Unity脚本：**
   ```bash
   # 创建Scripts目录（如果不存在）
   mkdir -p YourUnityProject/Assets/Scripts/UnityRime
   
   # 复制C#脚本
   cp unity_scripts/*.cs YourUnityProject/Assets/Scripts/UnityRime/
   ```

3. **在Unity中配置：**
   - 打开Unity项目
   - 在场景中创建空GameObject，命名为"RimeInputManager"
   - 添加RimeInputManager组件
   - 设置Implementation Type为"DLL"
   - 配置目标InputField

#### 步骤4：测试安装

```csharp
// 在Unity脚本中测试
void Start()
{
    var manager = FindObjectOfType<RimeInputManager>();
    if (manager != null)
    {
        Debug.Log("Rime输入法管理器已就绪");
        Debug.Log($"DLL版本: {RimeDLLWrapper.GetVersion()}");
        Debug.Log($"DLL可用性: {RimeDLLWrapper.IsAvailable()}");
    }
}
```

### 方案二：Python集成（推荐用于跨平台项目）

#### 步骤1：准备Python环境

```bash
# 检查Python版本
python3 --version

# 安装虚拟环境（推荐）
python3 -m venv unity_rime_env
source unity_rime_env/bin/activate  # Linux/macOS
# 或
unity_rime_env\Scripts\activate.bat  # Windows
```

#### 步骤2：安装Python依赖

```bash
cd python_component

# 安装基本依赖（项目包含模拟实现，无需额外安装）
# 可选：安装真实PyRime（需要先安装librime）
# pip install pyrime
```

#### 步骤3：启动Python服务器

```bash
# 启动IPC服务器
python3 ipc_server.py

# 或在后台运行
nohup python3 ipc_server.py > rime_server.log 2>&1 &
```

#### 步骤4：集成到Unity项目

1. **复制Unity脚本：**
   ```bash
   mkdir -p YourUnityProject/Assets/Scripts/UnityRime
   cp unity_scripts/*.cs YourUnityProject/Assets/Scripts/UnityRime/
   ```

2. **安装Newtonsoft.Json包：**
   - 在Unity中打开Package Manager
   - 搜索并安装"Newtonsoft Json"包
   - 或通过manifest.json添加依赖

3. **在Unity中配置：**
   - 创建空GameObject，命名为"RimeInputManager"
   - 添加RimeInputManager组件
   - 设置Implementation Type为"Python"
   - 配置服务器地址和端口（默认127.0.0.1:9999）

#### 步骤5：测试连接

```csharp
void Start()
{
    var manager = FindObjectOfType<RimeInputManager>();
    var pythonWrapper = manager.GetComponent<RimePythonWrapper>();
    
    pythonWrapper.OnConnectionChanged += (connected) => {
        Debug.Log($"Python连接状态: {connected}");
    };
}
```

## 详细配置

### DLL方案详细配置

#### 平台特定配置

**Windows平台配置：**

1. **DLL导入设置：**
   在Unity中选择DLL文件，在Inspector中配置：
   - Settings for Any Platform: 取消勾选
   - Settings for Windows: 勾选
   - CPU: x86_64 (或根据目标平台选择)
   - Placeholder: Assets/Plugins/rime_dll.dll

2. **依赖库处理：**
   如果使用真实的librime，需要将相关DLL一起复制：
   ```
   Assets/Plugins/
   ├── rime_dll.dll
   ├── rime.dll
   └── 其他依赖DLL
   ```

3. **代码签名（可选）：**
   对于发布版本，建议对DLL进行代码签名：
   ```cmd
   signtool sign /f certificate.pfx /p password rime_dll.dll
   ```

**macOS平台配置：**

1. **公证处理：**
   macOS Catalina及以后版本需要对dylib进行公证：
   ```bash
   # 签名
   codesign --force --verify --verbose --sign "Developer ID" librime_dll.dylib
   
   # 公证（需要Apple Developer账号）
   xcrun altool --notarize-app --primary-bundle-id "com.yourcompany.rime" \
                --username "your@email.com" --password "@keychain:AC_PASSWORD" \
                --file librime_dll.dylib
   ```

2. **权限配置：**
   在Unity Player Settings中配置必要的权限：
   - Camera Usage Description（如果需要）
   - Microphone Usage Description（如果需要）

**Linux平台配置：**

1. **依赖库安装：**
   ```bash
   # Ubuntu/Debian
   sudo apt-get install librime1 librime-data
   
   # CentOS/RHEL
   sudo yum install librime librime-data
   
   # Arch Linux
   sudo pacman -S librime
   ```

2. **库路径配置：**
   确保系统能找到librime库：
   ```bash
   # 检查库路径
   ldd librime_dll.so
   
   # 如果需要，添加库路径
   export LD_LIBRARY_PATH=/usr/local/lib:$LD_LIBRARY_PATH
   ```

#### 高级配置选项

**内存管理配置：**

```csharp
// 在RimeDLLWrapper中配置内存管理
public class RimeDLLWrapper : IDisposable
{
    private const int MAX_CANDIDATES = 20;
    private const int BUFFER_SIZE = 1024;
    
    // 配置内存池大小
    private static readonly ObjectPool<RimeResult> resultPool = 
        new ObjectPool<RimeResult>(() => new RimeResult(), 10);
}
```

**性能优化配置：**

```csharp
// 配置更新频率
public class RimeInputManager : MonoBehaviour
{
    [Header("性能设置")]
    [SerializeField] private float updateInterval = 0.016f; // 60 FPS
    [SerializeField] private int maxCandidatesPerFrame = 5;
    [SerializeField] private bool enableAsyncProcessing = true;
}
```

### Python方案详细配置

#### 服务器配置

**网络配置：**

```python
# 在ipc_server.py中配置网络参数
class IPCServer:
    def __init__(self, host="127.0.0.1", port=9999):
        self.host = host
        self.port = port
        self.max_connections = 10
        self.timeout = 30
        self.buffer_size = 4096
```

**日志配置：**

```python
import logging

# 配置日志级别和格式
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('rime_server.log'),
        logging.StreamHandler()
    ]
)
```

**安全配置：**

```python
# 配置访问控制
ALLOWED_HOSTS = ['127.0.0.1', 'localhost']
MAX_REQUEST_SIZE = 1024 * 1024  # 1MB
RATE_LIMIT = 1000  # 每秒最大请求数
```

#### Unity客户端配置

**连接配置：**

```csharp
[Header("连接设置")]
[SerializeField] private string serverHost = "127.0.0.1";
[SerializeField] private int serverPort = 9999;
[SerializeField] private float connectionTimeout = 5.0f;
[SerializeField] private float heartbeatInterval = 30.0f;
[SerializeField] private int maxRetryAttempts = 3;
```

**数据序列化配置：**

```csharp
// 配置JSON序列化设置
private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    DefaultValueHandling = DefaultValueHandling.Ignore,
    Formatting = Formatting.None
};
```

## 高级安装选项

### 容器化部署

#### Docker部署Python组件

1. **创建Dockerfile：**
   ```dockerfile
   FROM python:3.9-slim
   
   WORKDIR /app
   COPY python_component/ .
   
   # 安装依赖
   RUN apt-get update && apt-get install -y \
       librime1 librime-dev pkg-config \
       && rm -rf /var/lib/apt/lists/*
   
   # 可选：安装PyRime
   # RUN pip install pyrime
   
   EXPOSE 9999
   CMD ["python", "ipc_server.py"]
   ```

2. **构建和运行容器：**
   ```bash
   # 构建镜像
   docker build -t unity-rime-python .
   
   # 运行容器
   docker run -d -p 9999:9999 --name rime-server unity-rime-python
   ```

3. **使用Docker Compose：**
   ```yaml
   version: '3.8'
   services:
     rime-server:
       build: .
       ports:
         - "9999:9999"
       restart: unless-stopped
       volumes:
         - ./logs:/app/logs
   ```

#### Kubernetes部署

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: unity-rime-server
spec:
  replicas: 2
  selector:
    matchLabels:
      app: unity-rime-server
  template:
    metadata:
      labels:
        app: unity-rime-server
    spec:
      containers:
      - name: rime-server
        image: unity-rime-python:latest
        ports:
        - containerPort: 9999
        resources:
          requests:
            memory: "128Mi"
            cpu: "100m"
          limits:
            memory: "512Mi"
            cpu: "500m"
---
apiVersion: v1
kind: Service
metadata:
  name: unity-rime-service
spec:
  selector:
    app: unity-rime-server
  ports:
  - port: 9999
    targetPort: 9999
  type: LoadBalancer
```

### 集群部署

#### 负载均衡配置

对于高并发场景，可以部署多个Python服务器实例：

```python
# load_balancer.py
import random
from typing import List

class RimeServerPool:
    def __init__(self, servers: List[tuple]):
        self.servers = servers  # [(host, port), ...]
        self.current = 0
    
    def get_server(self) -> tuple:
        # 轮询策略
        server = self.servers[self.current]
        self.current = (self.current + 1) % len(self.servers)
        return server
    
    def get_random_server(self) -> tuple:
        # 随机策略
        return random.choice(self.servers)
```

在Unity中配置服务器池：

```csharp
public class RimeServerPool : MonoBehaviour
{
    [System.Serializable]
    public struct ServerInfo
    {
        public string host;
        public int port;
        public bool isActive;
    }
    
    [SerializeField] private ServerInfo[] servers;
    private int currentServerIndex = 0;
    
    public ServerInfo GetNextServer()
    {
        // 实现负载均衡逻辑
        for (int i = 0; i < servers.Length; i++)
        {
            var server = servers[currentServerIndex];
            currentServerIndex = (currentServerIndex + 1) % servers.Length;
            
            if (server.isActive)
                return server;
        }
        
        throw new System.Exception("没有可用的服务器");
    }
}
```

### 移动平台特殊配置

#### iOS平台配置

1. **静态库编译：**
   ```bash
   # 编译iOS静态库
   cd dll_component
   mkdir build-ios
   cd build-ios
   
   cmake .. -G Xcode \
     -DCMAKE_TOOLCHAIN_FILE=../ios.toolchain.cmake \
     -DPLATFORM=OS64 \
     -DDEPLOYMENT_TARGET=12.0
   
   xcodebuild -project UnityRimeDLL.xcodeproj -configuration Release
   ```

2. **Unity iOS设置：**
   - Player Settings > iOS > Configuration: Release
   - Player Settings > iOS > Target minimum iOS Version: 12.0
   - 添加静态库到Xcode项目

#### Android平台配置

1. **编译Android库：**
   ```bash
   # 安装Android NDK
   export ANDROID_NDK=/path/to/android-ndk
   
   # 编译不同架构
   for ARCH in arm64-v8a armeabi-v7a x86_64; do
     mkdir build-android-$ARCH
     cd build-android-$ARCH
     
     cmake .. \
       -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake \
       -DANDROID_ABI=$ARCH \
       -DANDROID_PLATFORM=android-24
     
     make -j4
     cd ..
   done
   ```

2. **Unity Android设置：**
   ```
   Assets/Plugins/Android/
   ├── arm64-v8a/
   │   └── librime_dll.so
   ├── armeabi-v7a/
   │   └── librime_dll.so
   └── x86_64/
       └── librime_dll.so
   ```

## 故障排除

### 安装问题诊断

#### 常见错误及解决方案

**错误1：DLL加载失败**
```
DllNotFoundException: Unable to load DLL 'rime_dll'
```

解决方案：
1. 检查DLL文件是否在正确路径
2. 确认DLL架构与Unity项目匹配
3. 检查依赖库是否完整
4. 使用Dependency Walker检查依赖关系

**错误2：Python连接超时**
```
TimeoutError: Connection timed out
```

解决方案：
1. 确认Python服务器正在运行
2. 检查防火墙设置
3. 验证端口是否被占用
4. 测试网络连通性

**错误3：编译错误**
```
CMake Error: Could not find CMAKE_CXX_COMPILER
```

解决方案：
1. 安装完整的编译工具链
2. 设置正确的环境变量
3. 检查CMake版本兼容性

#### 诊断工具

**系统信息收集脚本：**

```bash
#!/bin/bash
# diagnose.sh - 系统诊断脚本

echo "=== Unity Rime 诊断报告 ==="
echo "时间: $(date)"
echo "系统: $(uname -a)"
echo

echo "=== Python环境 ==="
python3 --version
pip3 --version
echo

echo "=== 编译工具 ==="
gcc --version 2>/dev/null || echo "GCC未安装"
cmake --version 2>/dev/null || echo "CMake未安装"
echo

echo "=== 网络连接 ==="
netstat -an | grep 9999 || echo "端口9999未监听"
echo

echo "=== 文件权限 ==="
ls -la dll_component/build/ 2>/dev/null || echo "DLL构建目录不存在"
ls -la python_component/ 2>/dev/null || echo "Python组件目录不存在"
```

**Unity诊断脚本：**

```csharp
public class RimeDiagnostics : MonoBehaviour
{
    [ContextMenu("运行诊断")]
    public void RunDiagnostics()
    {
        Debug.Log("=== Unity Rime 诊断 ===");
        
        // 检查DLL可用性
        try
        {
            bool available = RimeDLLWrapper.IsAvailable();
            string version = RimeDLLWrapper.GetVersion();
            Debug.Log($"DLL可用: {available}, 版本: {version}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DLL检查失败: {e.Message}");
        }
        
        // 检查Python连接
        var pythonWrapper = FindObjectOfType<RimePythonWrapper>();
        if (pythonWrapper != null)
        {
            Debug.Log($"Python连接状态: {pythonWrapper.IsConnected}");
        }
        
        // 检查Unity版本
        Debug.Log($"Unity版本: {Application.unityVersion}");
        Debug.Log($"平台: {Application.platform}");
    }
}
```

### 性能优化

#### 内存优化

```csharp
// 对象池实现
public class ObjectPool<T> where T : class, new()
{
    private readonly Queue<T> objects = new Queue<T>();
    private readonly Func<T> createFunc;
    private readonly Action<T> resetAction;
    
    public ObjectPool(Func<T> createFunc, Action<T> resetAction = null)
    {
        this.createFunc = createFunc;
        this.resetAction = resetAction;
    }
    
    public T Get()
    {
        if (objects.Count > 0)
        {
            return objects.Dequeue();
        }
        return createFunc();
    }
    
    public void Return(T obj)
    {
        resetAction?.Invoke(obj);
        objects.Enqueue(obj);
    }
}
```

#### 网络优化

```python
# 连接池实现
import queue
import threading
from contextlib import contextmanager

class ConnectionPool:
    def __init__(self, max_connections=10):
        self.max_connections = max_connections
        self.pool = queue.Queue(maxsize=max_connections)
        self.lock = threading.Lock()
    
    @contextmanager
    def get_connection(self):
        try:
            conn = self.pool.get_nowait()
        except queue.Empty:
            conn = self.create_connection()
        
        try:
            yield conn
        finally:
            try:
                self.pool.put_nowait(conn)
            except queue.Full:
                conn.close()
```

## 验证安装

### 功能验证清单

完成安装后，请按照以下清单验证功能：

**基本功能验证：**
- [ ] 输入法管理器成功初始化
- [ ] 可以切换输入法开关状态
- [ ] 能够输入拼音字母
- [ ] 候选词正确显示
- [ ] 可以选择候选词
- [ ] 文本正确输出到目标输入框

**DLL方案验证：**
- [ ] DLL文件成功加载
- [ ] 版本信息正确显示
- [ ] 性能测试通过（延迟<10ms）
- [ ] 内存使用正常

**Python方案验证：**
- [ ] Python服务器成功启动
- [ ] Unity客户端成功连接
- [ ] 网络通信正常
- [ ] 心跳检测工作

**UI功能验证：**
- [ ] 候选词面板正确显示
- [ ] 输入状态实时更新
- [ ] 按键响应正常
- [ ] 界面布局适配不同分辨率

### 自动化测试

运行项目提供的测试套件：

```bash
# 运行集成测试
cd tests
python3 test_integration.py

# 运行性能测试
python3 performance_benchmark.py

# 检查测试结果
cat benchmark_results.json
```

预期的测试结果应该显示所有核心功能测试通过，性能指标在合理范围内。

## 下一步

安装完成后，建议阅读以下文档：

1. **用户指南** - 了解如何使用输入法的各项功能
2. **开发者文档** - 学习如何自定义和扩展功能
3. **API参考** - 查看详细的API文档
4. **最佳实践** - 了解性能优化和安全建议

如果在安装过程中遇到问题，请参考故障排除部分或联系技术支持。

---

*本安装指南最后更新于2025年7月18日*

