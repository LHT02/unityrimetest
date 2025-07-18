# Unity Rime输入法集成 - Python组件

这是Unity Rime中文输入法集成项目的Python组件部分，负责与Rime输入法引擎交互，并通过TCP Socket与Unity进行通信。

## 文件结构

```
python_component/
├── rime_wrapper.py      # Rime输入法引擎包装器
├── ipc_server.py        # IPC服务器，处理与Unity的通信
├── requirements.txt     # Python依赖列表
└── README.md           # 本文档
```

## 功能特性

- **Rime引擎封装**：提供简洁的Python API来操作Rime输入法引擎
- **进程间通信**：通过TCP Socket与Unity进行实时通信
- **候选词管理**：支持获取和选择候选词
- **输入状态跟踪**：实时跟踪输入状态和拼音组合
- **错误处理**：完善的错误处理和日志记录
- **模拟模式**：在无法安装PyRime时提供模拟实现用于测试

## 安装依赖

### 1. 安装librime

#### Ubuntu/Debian
```bash
sudo apt-get update
sudo apt-get install librime-dev librime1 pkg-config
```

#### ArchLinux
```bash
sudo pacman -S librime pkg-config
```

#### macOS
```bash
brew tap tonyfettes/homebrew-rime
brew install librime pkg-config
```

#### Windows
需要手动编译librime或使用预编译版本。

### 2. 安装PyRime（可选）

```bash
pip install pyrime
```

注意：如果无法安装PyRime，代码中包含了模拟实现，可以用于测试和开发。

## 使用方法

### 启动IPC服务器

```bash
python ipc_server.py
```

服务器将在 `127.0.0.1:9999` 上监听Unity的连接。

### 测试客户端

```bash
python ipc_server.py test
```

这将启动一个测试客户端，模拟Unity与服务器的交互。

### 直接测试Rime包装器

```bash
python rime_wrapper.py
```

这将直接测试Rime包装器的功能。

## API接口

### IPC通信协议

客户端（Unity）与服务器（Python）之间通过JSON消息进行通信：

#### 请求格式
```json
{
    "command": "命令名称",
    "params": {
        "参数名": "参数值"
    }
}
```

#### 响应格式
```json
{
    "success": true,
    "data": "响应数据",
    "error": "错误信息（如果有）"
}
```

### 支持的命令

#### 1. process_key - 处理按键
```json
{
    "command": "process_key",
    "params": {
        "key_code": 110
    }
}
```

#### 2. select_candidate - 选择候选词
```json
{
    "command": "select_candidate",
    "params": {
        "index": 0
    }
}
```

#### 3. clear_composition - 清空输入
```json
{
    "command": "clear_composition",
    "params": {}
}
```

#### 4. get_state - 获取当前状态
```json
{
    "command": "get_state",
    "params": {}
}
```

#### 5. ping - 心跳检测
```json
{
    "command": "ping",
    "params": {}
}
```

### 输入状态数据结构

```json
{
    "composition": "nihao",
    "candidates": [
        {
            "text": "你好",
            "comment": "拼音: nihao",
            "index": 0
        }
    ],
    "page_size": 5,
    "page_no": 0,
    "is_last_page": true
}
```

## 配置选项

### Rime配置

Rime的配置文件通常位于：
- Linux: `~/.config/rime/`
- macOS: `~/Library/Rime/`
- Windows: `%APPDATA%\\Rime\\`

### 服务器配置

可以通过修改 `ipc_server.py` 中的参数来配置服务器：

```python
server = IPCServer(host="127.0.0.1", port=9999)
```

## 日志记录

程序会生成以下日志文件：
- `rime_wrapper.log` - Rime包装器日志
- `ipc_server.log` - IPC服务器日志

日志级别可以通过修改代码中的 `logging.basicConfig` 来调整。

## 故障排除

### 1. PyRime导入失败
如果无法导入PyRime，程序会自动使用模拟实现。模拟实现包含基本的拼音输入功能，可以用于测试。

### 2. 连接失败
检查防火墙设置，确保端口9999没有被占用。

### 3. Rime初始化失败
确保librime正确安装，并且Rime配置文件存在。

## 开发说明

### 扩展功能
要添加新的命令，需要：
1. 在 `RimeWrapper` 类中添加相应的方法
2. 在 `IPCServer._process_request` 方法中添加命令处理逻辑

### 性能优化
- 可以考虑使用异步I/O来提高并发性能
- 对于大量候选词，可以实现分页机制
- 可以添加缓存机制来减少重复计算

## 许可证

本项目采用开源许可证，具体许可证信息请参考项目根目录的LICENSE文件。

