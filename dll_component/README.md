# Unity Rime输入法集成 - DLL组件

这是Unity Rime中文输入法集成项目的DLL组件部分，将Rime输入法引擎封装成动态链接库，供Unity直接调用。

## 文件结构

```
dll_component/
├── rime_dll.h           # DLL接口头文件
├── rime_dll.cpp         # DLL实现文件
├── test_dll.cpp         # DLL测试程序
├── CMakeLists.txt       # CMake构建配置
├── build.sh             # 构建脚本
├── build/               # 构建输出目录
│   ├── librime_dll.so   # 生成的动态库（Linux）
│   └── test_dll         # 测试程序
└── README.md           # 本文档
```

## 功能特性

- **直接调用**：Unity可以通过P/Invoke直接调用DLL函数，无需进程间通信
- **高性能**：Rime逻辑直接在Unity进程中运行，响应速度快
- **跨平台**：支持Windows、Linux、macOS等平台
- **内存管理**：完善的内存分配和释放机制
- **错误处理**：详细的错误信息和状态反馈
- **模拟实现**：在无法安装librime时提供模拟实现用于测试

## 构建要求

### 系统要求
- Linux: Ubuntu 18.04+ 或其他现代Linux发行版
- Windows: Windows 10+ 和 Visual Studio 2017+
- macOS: macOS 10.14+ 和 Xcode 10+

### 构建工具
- CMake 3.10+
- C++11兼容的编译器（GCC 7+, Clang 5+, MSVC 2017+）

### 依赖库（可选）
- librime-dev（用于真实的Rime引擎集成）

## 构建方法

### Linux/macOS

1. 安装构建依赖：
```bash
# Ubuntu/Debian
sudo apt-get install cmake build-essential

# ArchLinux
sudo pacman -S cmake base-devel

# macOS
brew install cmake
```

2. 运行构建脚本：
```bash
chmod +x build.sh
./build.sh
```

### Windows

1. 安装Visual Studio 2017或更高版本
2. 安装CMake
3. 打开命令提示符，运行：
```cmd
mkdir build
cd build
cmake .. -G "Visual Studio 15 2017 Win64"
cmake --build . --config Release
```

## API接口

### 数据结构

#### RimeCandidate - 候选词结构
```c
typedef struct {
    char text[256];      // 候选词文本
    char comment[256];   // 候选词注释
    int index;           // 候选词索引
} RimeCandidate;
```

#### RimeInputState - 输入状态结构
```c
typedef struct {
    char composition[512];       // 当前输入的拼音
    RimeCandidate* candidates;   // 候选词数组
    int candidate_count;         // 候选词数量
    int page_size;              // 每页候选词数量
    int page_no;                // 当前页码
    int is_last_page;           // 是否为最后一页
} RimeInputState;
```

#### RimeResult - 操作结果结构
```c
typedef struct {
    int success;                // 操作是否成功
    char error_message[512];    // 错误信息
    char selected_text[256];    // 选中的文本
    RimeInputState state;       // 当前输入状态
} RimeResult;
```

### 主要函数

#### 初始化和销毁
```c
// 初始化Rime引擎，返回会话ID
int RimeInitialize(const char* user_data_dir, const char* shared_data_dir);

// 销毁Rime引擎
void RimeDestroy(int session_id);
```

#### 输入处理
```c
// 处理按键输入
void RimeProcessKey(int session_id, int key_code, RimeResult* result);

// 选择候选词
void RimeSelectCandidate(int session_id, int index, RimeResult* result);

// 清空当前输入
void RimeClearComposition(int session_id, RimeResult* result);

// 获取当前输入状态
void RimeGetCurrentState(int session_id, RimeResult* result);
```

#### 内存管理
```c
// 释放结果内存
void RimeFreeResult(RimeResult* result);
```

#### 工具函数
```c
// 获取版本信息
const char* RimeGetVersion();

// 检查Rime引擎是否可用
int RimeIsAvailable();
```

## Unity集成

### 1. 复制DLL文件

将生成的动态库文件复制到Unity项目的Plugins目录：
- Linux: `librime_dll.so`
- Windows: `rime_dll.dll`
- macOS: `librime_dll.dylib`

### 2. C#接口声明

在Unity中创建C#脚本来声明DLL接口：

```csharp
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct RimeCandidate
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string text;
    
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string comment;
    
    public int index;
}

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

public class RimeDLL
{
    [DllImport("rime_dll")]
    public static extern int RimeInitialize(string user_data_dir, string shared_data_dir);
    
    [DllImport("rime_dll")]
    public static extern void RimeDestroy(int session_id);
    
    [DllImport("rime_dll")]
    public static extern void RimeProcessKey(int session_id, int key_code, out RimeResult result);
    
    [DllImport("rime_dll")]
    public static extern void RimeSelectCandidate(int session_id, int index, out RimeResult result);
    
    [DllImport("rime_dll")]
    public static extern void RimeClearComposition(int session_id, out RimeResult result);
    
    [DllImport("rime_dll")]
    public static extern void RimeGetCurrentState(int session_id, out RimeResult result);
    
    [DllImport("rime_dll")]
    public static extern void RimeFreeResult(ref RimeResult result);
    
    [DllImport("rime_dll")]
    public static extern IntPtr RimeGetVersion();
    
    [DllImport("rime_dll")]
    public static extern int RimeIsAvailable();
}
```

### 3. 使用示例

```csharp
public class RimeInputManager : MonoBehaviour
{
    private int sessionId;
    
    void Start()
    {
        // 初始化Rime引擎
        sessionId = RimeDLL.RimeInitialize(null, null);
        if (sessionId == 0)
        {
            Debug.LogError("Rime引擎初始化失败");
        }
    }
    
    void Update()
    {
        // 处理键盘输入
        foreach (char c in Input.inputString)
        {
            if (c >= 'a' && c <= 'z')
            {
                RimeResult result;
                RimeDLL.RimeProcessKey(sessionId, (int)c, out result);
                
                if (result.success == 1)
                {
                    // 更新UI显示候选词
                    UpdateCandidateUI(result);
                }
                
                RimeDLL.RimeFreeResult(ref result);
            }
        }
    }
    
    void OnDestroy()
    {
        if (sessionId != 0)
        {
            RimeDLL.RimeDestroy(sessionId);
        }
    }
}
```

## 测试

运行测试程序来验证DLL功能：

```bash
cd build
./test_dll
```

测试程序会模拟输入"nihao"并选择候选词，验证所有主要功能。

## 性能优化

- **内存池**：可以实现内存池来减少频繁的内存分配
- **缓存机制**：对常用候选词进行缓存
- **异步处理**：对于复杂的输入法逻辑可以考虑异步处理

## 故障排除

### 1. DLL加载失败
- 检查DLL文件是否在正确的路径
- 确保DLL的架构（x86/x64）与Unity匹配
- 检查依赖库是否正确安装

### 2. 函数调用失败
- 验证函数签名是否正确
- 检查参数类型和内存布局
- 确保正确释放内存

### 3. 编译错误
- 检查编译器版本和C++标准支持
- 确保所有依赖库正确安装
- 查看详细的编译错误信息

## 扩展开发

### 集成真实的librime

要集成真实的librime而不是模拟实现：

1. 安装librime开发包：
```bash
# Ubuntu
sudo apt-get install librime-dev

# ArchLinux
sudo pacman -S librime

# macOS
brew install librime
```

2. 修改CMakeLists.txt，取消注释librime相关部分：
```cmake
find_package(PkgConfig REQUIRED)
pkg_check_modules(RIME REQUIRED rime-1)
target_link_libraries(UnityRimeDLL ${RIME_LIBRARIES})
target_include_directories(UnityRimeDLL PRIVATE ${RIME_INCLUDE_DIRS})
```

3. 修改rime_dll.cpp，替换MockRimeEngine为真实的librime调用。

### 添加新功能

要添加新的功能，需要：
1. 在rime_dll.h中声明新的函数
2. 在rime_dll.cpp中实现函数
3. 在Unity的C#接口中添加对应的DllImport声明
4. 更新测试程序验证新功能

## 许可证

本项目采用开源许可证，具体许可证信息请参考项目根目录的LICENSE文件。

