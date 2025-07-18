# Unity Rime输入法集成 - 部署指南

**作者：** Manus AI  
**版本：** 1.0.0  
**日期：** 2025年7月18日

## 概述

本文档提供了Unity Rime中文输入法集成解决方案在不同环境下的详细部署指南，包括开发环境、测试环境和生产环境的部署策略。无论您是要在本地开发、团队协作，还是发布到各个平台，都可以在本文档中找到相应的部署方案。

## 部署架构

### 整体部署架构

Unity Rime输入法集成解决方案支持多种部署架构，可以根据项目需求和技术约束选择最适合的方案。

**单机部署架构：**
```
Unity应用
├── DLL组件 (librime_dll.so/dll/dylib)
└── 配置文件
```

**分布式部署架构：**
```
Unity应用 ←→ Python服务器
                ├── Rime引擎
                ├── 词典数据
                └── 配置文件
```

**混合部署架构：**
```
Unity应用
├── DLL组件 (主要)
└── Python服务器 (备用/扩展)
```

### 部署环境分类

#### 开发环境 (Development)
- 目标：快速开发和调试
- 特点：完整的开发工具链，详细的日志输出
- 性能要求：中等
- 稳定性要求：低

#### 测试环境 (Testing)
- 目标：功能验证和性能测试
- 特点：接近生产环境的配置，自动化测试
- 性能要求：高
- 稳定性要求：中等

#### 生产环境 (Production)
- 目标：最终用户使用
- 特点：高性能、高稳定性、安全性
- 性能要求：最高
- 稳定性要求：最高

## 开发环境部署

### 本地开发环境

#### Windows开发环境

**系统要求：**
- Windows 10/11 (x64)
- Visual Studio 2017或更高版本
- Unity 2019.4 LTS或更高版本
- Git for Windows

**部署步骤：**

1. **克隆项目仓库：**
   ```cmd
   git clone https://github.com/your-repo/unity-rime-integration.git
   cd unity-rime-integration
   ```

2. **编译DLL组件：**
   ```cmd
   cd dll_component
   mkdir build
   cd build
   cmake .. -G "Visual Studio 16 2019" -A x64
   cmake --build . --config Debug
   ```

3. **设置Python环境：**
   ```cmd
   python -m venv venv
   venv\Scripts\activate
   cd ..\python_component
   pip install -r requirements.txt
   ```

4. **配置Unity项目：**
   ```cmd
   # 创建Unity项目目录结构
   mkdir UnityProject\Assets\Plugins
   mkdir UnityProject\Assets\Scripts\UnityRime
   
   # 复制文件
   copy dll_component\build\Debug\rime_dll.dll UnityProject\Assets\Plugins\
   copy unity_scripts\*.cs UnityProject\Assets\Scripts\UnityRime\
   ```

5. **启动开发服务：**
   ```cmd
   # 启动Python服务器（新终端）
   cd python_component
   python ipc_server.py
   
   # 启动Unity编辑器
   "C:\Program Files\Unity\Hub\Editor\2021.3.0f1\Editor\Unity.exe" -projectPath UnityProject
   ```

#### macOS开发环境

**系统要求：**
- macOS 10.14或更高版本
- Xcode 10或更高版本
- Unity 2019.4 LTS或更高版本
- Homebrew包管理器

**部署步骤：**

1. **安装依赖工具：**
   ```bash
   # 安装Homebrew（如果未安装）
   /bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"
   
   # 安装开发工具
   brew install cmake git python3
   xcode-select --install
   ```

2. **克隆和编译：**
   ```bash
   git clone https://github.com/your-repo/unity-rime-integration.git
   cd unity-rime-integration
   
   # 编译DLL
   cd dll_component
   chmod +x build.sh
   ./build.sh
   
   # 设置Python环境
   cd ../python_component
   python3 -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt
   ```

3. **配置Unity项目：**
   ```bash
   # 创建Unity项目结构
   mkdir -p UnityProject/Assets/{Plugins,Scripts/UnityRime}
   
   # 复制文件
   cp dll_component/build/librime_dll.dylib UnityProject/Assets/Plugins/
   cp unity_scripts/*.cs UnityProject/Assets/Scripts/UnityRime/
   ```

#### Linux开发环境

**系统要求：**
- Ubuntu 18.04+或其他主流发行版
- GCC 7+或Clang 5+
- CMake 3.10+
- Unity 2019.4 LTS或更高版本

**部署步骤：**

1. **安装系统依赖：**
   ```bash
   # Ubuntu/Debian
   sudo apt-get update
   sudo apt-get install -y build-essential cmake git python3 python3-pip python3-venv
   
   # 可选：安装真实的librime
   sudo apt-get install -y librime1 librime-dev
   
   # CentOS/RHEL
   sudo yum groupinstall -y "Development Tools"
   sudo yum install -y cmake git python3 python3-pip
   ```

2. **编译和配置：**
   ```bash
   git clone https://github.com/your-repo/unity-rime-integration.git
   cd unity-rime-integration
   
   # 编译DLL
   cd dll_component
   chmod +x build.sh
   ./build.sh
   
   # Python环境
   cd ../python_component
   python3 -m venv venv
   source venv/bin/activate
   pip install -r requirements.txt
   
   # Unity项目配置
   mkdir -p UnityProject/Assets/{Plugins,Scripts/UnityRime}
   cp dll_component/build/librime_dll.so UnityProject/Assets/Plugins/
   cp unity_scripts/*.cs UnityProject/Assets/Scripts/UnityRime/
   ```

### 团队开发环境

#### 版本控制配置

**Git配置文件 (.gitignore)：**
```gitignore
# Unity生成文件
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]ser[Ss]ettings/

# 编译输出
dll_component/build/
python_component/__pycache__/
python_component/*.pyc
python_component/venv/

# 配置文件
*.log
*.tmp
.DS_Store
Thumbs.db

# IDE文件
.vscode/
.idea/
*.suo
*.user
*.userosscache
*.sln.docstates
```

**Git LFS配置 (.gitattributes)：**
```gitattributes
# Unity文件
*.unity filter=lfs diff=lfs merge=lfs -text
*.prefab filter=lfs diff=lfs merge=lfs -text
*.asset filter=lfs diff=lfs merge=lfs -text
*.mat filter=lfs diff=lfs merge=lfs -text

# 二进制文件
*.dll filter=lfs diff=lfs merge=lfs -text
*.so filter=lfs diff=lfs merge=lfs -text
*.dylib filter=lfs diff=lfs merge=lfs -text
*.exe filter=lfs diff=lfs merge=lfs -text

# 媒体文件
*.png filter=lfs diff=lfs merge=lfs -text
*.jpg filter=lfs diff=lfs merge=lfs -text
*.wav filter=lfs diff=lfs merge=lfs -text
*.mp3 filter=lfs diff=lfs merge=lfs -text
```

#### 开发工作流

**分支策略：**
```
main (生产分支)
├── develop (开发分支)
│   ├── feature/dll-optimization
│   ├── feature/python-async
│   └── feature/ui-enhancement
├── release/v1.0.0 (发布分支)
└── hotfix/critical-bug (热修复分支)
```

**CI/CD配置 (.github/workflows/ci.yml)：**
```yaml
name: Unity Rime CI

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-dll:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Install dependencies (Ubuntu)
      if: matrix.os == 'ubuntu-latest'
      run: |
        sudo apt-get update
        sudo apt-get install -y cmake build-essential
    
    - name: Install dependencies (macOS)
      if: matrix.os == 'macos-latest'
      run: |
        brew install cmake
    
    - name: Build DLL
      run: |
        cd dll_component
        chmod +x build.sh
        ./build.sh
    
    - name: Upload artifacts
      uses: actions/upload-artifact@v2
      with:
        name: dll-${{ matrix.os }}
        path: dll_component/build/
  
  test-python:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Set up Python
      uses: actions/setup-python@v2
      with:
        python-version: '3.8'
    
    - name: Install dependencies
      run: |
        cd python_component
        pip install -r requirements.txt
    
    - name: Run tests
      run: |
        cd tests
        python test_integration.py
        python performance_benchmark.py
  
  unity-build:
    runs-on: ubuntu-latest
    needs: [build-dll]
    
    steps:
    - uses: actions/checkout@v2
    
    - name: Download DLL artifacts
      uses: actions/download-artifact@v2
      with:
        name: dll-ubuntu-latest
        path: UnityProject/Assets/Plugins/
    
    - name: Unity Build
      uses: game-ci/unity-builder@v2
      env:
        UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
      with:
        projectPath: UnityProject
        targetPlatform: StandaloneLinux64
    
    - name: Upload build
      uses: actions/upload-artifact@v2
      with:
        name: unity-build
        path: build/
```

## 测试环境部署

### 自动化测试环境

#### Docker容器化部署

**Dockerfile (Python组件)：**
```dockerfile
FROM python:3.9-slim

WORKDIR /app

# 安装系统依赖
RUN apt-get update && apt-get install -y \
    librime1 \
    librime-dev \
    pkg-config \
    && rm -rf /var/lib/apt/lists/*

# 复制Python组件
COPY python_component/ .

# 安装Python依赖
RUN pip install --no-cache-dir -r requirements.txt

# 可选：安装真实PyRime
# RUN pip install pyrime

# 暴露端口
EXPOSE 9999

# 健康检查
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD python -c "import socket; s=socket.socket(); s.connect(('localhost', 9999)); s.close()" || exit 1

# 启动服务
CMD ["python", "ipc_server.py"]
```

**Docker Compose配置：**
```yaml
version: '3.8'

services:
  rime-server:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "9999:9999"
    environment:
      - LOG_LEVEL=INFO
      - MAX_CONNECTIONS=50
    volumes:
      - ./logs:/app/logs
      - ./config:/app/config
    restart: unless-stopped
    networks:
      - rime-network
  
  rime-test:
    build:
      context: .
      dockerfile: Dockerfile.test
    depends_on:
      - rime-server
    environment:
      - RIME_SERVER_HOST=rime-server
      - RIME_SERVER_PORT=9999
    volumes:
      - ./tests:/app/tests
      - ./test-results:/app/results
    networks:
      - rime-network
    command: ["python", "-m", "pytest", "tests/", "-v", "--junitxml=results/test-results.xml"]

networks:
  rime-network:
    driver: bridge

volumes:
  logs:
  test-results:
```

#### 测试自动化脚本

**测试部署脚本 (deploy-test.sh)：**
```bash
#!/bin/bash

set -e

echo "开始测试环境部署..."

# 环境变量
TEST_ENV=${TEST_ENV:-staging}
DOCKER_REGISTRY=${DOCKER_REGISTRY:-localhost:5000}
VERSION=${VERSION:-latest}

# 构建镜像
echo "构建Docker镜像..."
docker build -t ${DOCKER_REGISTRY}/unity-rime-python:${VERSION} .

# 推送镜像（如果有注册表）
if [ "$DOCKER_REGISTRY" != "localhost:5000" ]; then
    echo "推送镜像到注册表..."
    docker push ${DOCKER_REGISTRY}/unity-rime-python:${VERSION}
fi

# 部署到测试环境
echo "部署到测试环境..."
docker-compose -f docker-compose.test.yml down
docker-compose -f docker-compose.test.yml up -d

# 等待服务启动
echo "等待服务启动..."
sleep 30

# 运行健康检查
echo "运行健康检查..."
docker-compose -f docker-compose.test.yml exec rime-server python -c "
import socket
import sys
try:
    s = socket.socket()
    s.connect(('localhost', 9999))
    s.close()
    print('服务健康检查通过')
except:
    print('服务健康检查失败')
    sys.exit(1)
"

# 运行自动化测试
echo "运行自动化测试..."
docker-compose -f docker-compose.test.yml run --rm rime-test

# 收集测试结果
echo "收集测试结果..."
docker cp $(docker-compose -f docker-compose.test.yml ps -q rime-test):/app/results ./test-results/

echo "测试环境部署完成！"
```

### 性能测试环境

#### 负载测试配置

**负载测试脚本 (load_test.py)：**
```python
import asyncio
import aiohttp
import json
import time
import statistics
from concurrent.futures import ThreadPoolExecutor
import argparse

class RimeLoadTester:
    def __init__(self, host="localhost", port=9999, concurrent_users=10, test_duration=60):
        self.host = host
        self.port = port
        self.concurrent_users = concurrent_users
        self.test_duration = test_duration
        self.results = []
    
    async def simulate_user_session(self, session_id):
        """模拟用户输入会话"""
        test_sequences = [
            [110, 105, 104, 97, 111],  # "nihao"
            [119, 111, 104, 101, 110],  # "wohen"
            [120, 105, 101, 120, 105, 101],  # "xiexie"
        ]
        
        start_time = time.time()
        operation_count = 0
        errors = 0
        
        while time.time() - start_time < self.test_duration:
            try:
                # 选择随机测试序列
                import random
                sequence = random.choice(test_sequences)
                
                # 模拟完整的输入过程
                session_start = time.time()
                
                for key_code in sequence:
                    await self.send_process_key(key_code)
                    operation_count += 1
                
                # 选择候选词
                await self.send_select_candidate(0)
                operation_count += 1
                
                session_duration = time.time() - session_start
                self.results.append({
                    'session_id': session_id,
                    'duration': session_duration,
                    'operations': len(sequence) + 1,
                    'timestamp': time.time()
                })
                
                # 短暂休息
                await asyncio.sleep(0.1)
                
            except Exception as e:
                errors += 1
                print(f"用户 {session_id} 发生错误: {e}")
        
        print(f"用户 {session_id} 完成: {operation_count} 操作, {errors} 错误")
    
    async def send_process_key(self, key_code):
        """发送按键处理请求"""
        # 这里应该实现实际的网络请求
        # 为了示例，我们使用模拟延迟
        await asyncio.sleep(0.01)  # 模拟网络延迟
    
    async def send_select_candidate(self, index):
        """发送候选词选择请求"""
        await asyncio.sleep(0.01)  # 模拟网络延迟
    
    async def run_load_test(self):
        """运行负载测试"""
        print(f"开始负载测试: {self.concurrent_users} 并发用户, {self.test_duration} 秒")
        
        start_time = time.time()
        
        # 创建并发用户任务
        tasks = []
        for i in range(self.concurrent_users):
            task = asyncio.create_task(self.simulate_user_session(i))
            tasks.append(task)
        
        # 等待所有任务完成
        await asyncio.gather(*tasks)
        
        end_time = time.time()
        total_duration = end_time - start_time
        
        # 分析结果
        self.analyze_results(total_duration)
    
    def analyze_results(self, total_duration):
        """分析测试结果"""
        if not self.results:
            print("没有收集到测试结果")
            return
        
        durations = [r['duration'] for r in self.results]
        total_operations = sum(r['operations'] for r in self.results)
        
        print("\n=== 负载测试结果 ===")
        print(f"总测试时间: {total_duration:.2f} 秒")
        print(f"并发用户数: {self.concurrent_users}")
        print(f"总会话数: {len(self.results)}")
        print(f"总操作数: {total_operations}")
        print(f"平均吞吐量: {total_operations / total_duration:.2f} 操作/秒")
        print(f"平均会话时长: {statistics.mean(durations):.3f} 秒")
        print(f"会话时长中位数: {statistics.median(durations):.3f} 秒")
        print(f"最短会话时长: {min(durations):.3f} 秒")
        print(f"最长会话时长: {max(durations):.3f} 秒")
        
        if len(durations) > 1:
            print(f"会话时长标准差: {statistics.stdev(durations):.3f} 秒")

async def main():
    parser = argparse.ArgumentParser(description='Rime输入法负载测试')
    parser.add_argument('--host', default='localhost', help='服务器地址')
    parser.add_argument('--port', type=int, default=9999, help='服务器端口')
    parser.add_argument('--users', type=int, default=10, help='并发用户数')
    parser.add_argument('--duration', type=int, default=60, help='测试时长（秒）')
    
    args = parser.parse_args()
    
    tester = RimeLoadTester(
        host=args.host,
        port=args.port,
        concurrent_users=args.users,
        test_duration=args.duration
    )
    
    await tester.run_load_test()

if __name__ == "__main__":
    asyncio.run(main())
```

## 生产环境部署

### 云平台部署

#### AWS部署

**AWS ECS任务定义：**
```json
{
  "family": "unity-rime-server",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::account:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "rime-server",
      "image": "your-registry/unity-rime-python:latest",
      "portMappings": [
        {
          "containerPort": 9999,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "LOG_LEVEL",
          "value": "INFO"
        },
        {
          "name": "MAX_CONNECTIONS",
          "value": "100"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/unity-rime-server",
          "awslogs-region": "us-west-2",
          "awslogs-stream-prefix": "ecs"
        }
      },
      "healthCheck": {
        "command": [
          "CMD-SHELL",
          "python -c \"import socket; s=socket.socket(); s.connect(('localhost', 9999)); s.close()\" || exit 1"
        ],
        "interval": 30,
        "timeout": 5,
        "retries": 3,
        "startPeriod": 60
      }
    }
  ]
}
```

**AWS CloudFormation模板：**
```yaml
AWSTemplateFormatVersion: '2010-09-09'
Description: 'Unity Rime输入法服务部署'

Parameters:
  VpcId:
    Type: AWS::EC2::VPC::Id
    Description: VPC ID
  
  SubnetIds:
    Type: List<AWS::EC2::Subnet::Id>
    Description: Subnet IDs
  
  ImageUri:
    Type: String
    Description: Docker镜像URI

Resources:
  # ECS集群
  ECSCluster:
    Type: AWS::ECS::Cluster
    Properties:
      ClusterName: unity-rime-cluster
      CapacityProviders:
        - FARGATE
      DefaultCapacityProviderStrategy:
        - CapacityProvider: FARGATE
          Weight: 1

  # 安全组
  SecurityGroup:
    Type: AWS::EC2::SecurityGroup
    Properties:
      GroupDescription: Unity Rime服务安全组
      VpcId: !Ref VpcId
      SecurityGroupIngress:
        - IpProtocol: tcp
          FromPort: 9999
          ToPort: 9999
          CidrIp: 0.0.0.0/0

  # 负载均衡器
  LoadBalancer:
    Type: AWS::ElasticLoadBalancingV2::LoadBalancer
    Properties:
      Name: unity-rime-alb
      Scheme: internet-facing
      Type: application
      Subnets: !Ref SubnetIds
      SecurityGroups:
        - !Ref SecurityGroup

  # 目标组
  TargetGroup:
    Type: AWS::ElasticLoadBalancingV2::TargetGroup
    Properties:
      Name: unity-rime-targets
      Port: 9999
      Protocol: HTTP
      VpcId: !Ref VpcId
      TargetType: ip
      HealthCheckPath: /health
      HealthCheckProtocol: HTTP

  # 监听器
  Listener:
    Type: AWS::ElasticLoadBalancingV2::Listener
    Properties:
      DefaultActions:
        - Type: forward
          TargetGroupArn: !Ref TargetGroup
      LoadBalancerArn: !Ref LoadBalancer
      Port: 80
      Protocol: HTTP

  # ECS服务
  ECSService:
    Type: AWS::ECS::Service
    DependsOn: Listener
    Properties:
      ServiceName: unity-rime-service
      Cluster: !Ref ECSCluster
      TaskDefinition: !Ref TaskDefinition
      DesiredCount: 2
      LaunchType: FARGATE
      NetworkConfiguration:
        AwsvpcConfiguration:
          SecurityGroups:
            - !Ref SecurityGroup
          Subnets: !Ref SubnetIds
          AssignPublicIp: ENABLED
      LoadBalancers:
        - ContainerName: rime-server
          ContainerPort: 9999
          TargetGroupArn: !Ref TargetGroup

  # 任务定义
  TaskDefinition:
    Type: AWS::ECS::TaskDefinition
    Properties:
      Family: unity-rime-server
      NetworkMode: awsvpc
      RequiresCompatibilities:
        - FARGATE
      Cpu: 512
      Memory: 1024
      ExecutionRoleArn: !Ref ExecutionRole
      TaskRoleArn: !Ref TaskRole
      ContainerDefinitions:
        - Name: rime-server
          Image: !Ref ImageUri
          PortMappings:
            - ContainerPort: 9999
          LogConfiguration:
            LogDriver: awslogs
            Options:
              awslogs-group: !Ref LogGroup
              awslogs-region: !Ref AWS::Region
              awslogs-stream-prefix: ecs

  # IAM角色
  ExecutionRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Statement:
          - Effect: Allow
            Principal:
              Service: ecs-tasks.amazonaws.com
            Action: sts:AssumeRole
      ManagedPolicyArns:
        - arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy

  TaskRole:
    Type: AWS::IAM::Role
    Properties:
      AssumeRolePolicyDocument:
        Statement:
          - Effect: Allow
            Principal:
              Service: ecs-tasks.amazonaws.com
            Action: sts:AssumeRole

  # CloudWatch日志组
  LogGroup:
    Type: AWS::Logs::LogGroup
    Properties:
      LogGroupName: /ecs/unity-rime-server
      RetentionInDays: 30

Outputs:
  LoadBalancerDNS:
    Description: 负载均衡器DNS名称
    Value: !GetAtt LoadBalancer.DNSName
    Export:
      Name: !Sub ${AWS::StackName}-LoadBalancerDNS
```

#### Kubernetes部署

**Kubernetes部署清单：**
```yaml
# namespace.yaml
apiVersion: v1
kind: Namespace
metadata:
  name: unity-rime

---
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: rime-config
  namespace: unity-rime
data:
  LOG_LEVEL: "INFO"
  MAX_CONNECTIONS: "100"
  HEARTBEAT_INTERVAL: "30"

---
# deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: unity-rime-server
  namespace: unity-rime
  labels:
    app: unity-rime-server
spec:
  replicas: 3
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
        image: your-registry/unity-rime-python:latest
        ports:
        - containerPort: 9999
        envFrom:
        - configMapRef:
            name: rime-config
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          exec:
            command:
            - python
            - -c
            - "import socket; s=socket.socket(); s.connect(('localhost', 9999)); s.close()"
          initialDelaySeconds: 30
          periodSeconds: 30
        readinessProbe:
          exec:
            command:
            - python
            - -c
            - "import socket; s=socket.socket(); s.connect(('localhost', 9999)); s.close()"
          initialDelaySeconds: 5
          periodSeconds: 10

---
# service.yaml
apiVersion: v1
kind: Service
metadata:
  name: unity-rime-service
  namespace: unity-rime
spec:
  selector:
    app: unity-rime-server
  ports:
  - port: 9999
    targetPort: 9999
  type: ClusterIP

---
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: unity-rime-ingress
  namespace: unity-rime
  annotations:
    nginx.ingress.kubernetes.io/rewrite-target: /
spec:
  rules:
  - host: rime.yourdomain.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: unity-rime-service
            port:
              number: 9999

---
# hpa.yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: unity-rime-hpa
  namespace: unity-rime
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: unity-rime-server
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### 移动平台部署

#### iOS平台部署

**iOS构建配置：**
```bash
#!/bin/bash
# build-ios.sh

set -e

echo "开始iOS平台构建..."

# 环境变量
UNITY_PATH="/Applications/Unity/Hub/Editor/2021.3.0f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)/UnityProject"
BUILD_PATH="$(pwd)/Builds/iOS"

# 清理构建目录
rm -rf "$BUILD_PATH"
mkdir -p "$BUILD_PATH"

# 编译iOS静态库
echo "编译iOS静态库..."
cd dll_component
mkdir -p build-ios
cd build-ios

# 配置CMake for iOS
cmake .. \
  -G Xcode \
  -DCMAKE_TOOLCHAIN_FILE=../ios.toolchain.cmake \
  -DPLATFORM=OS64 \
  -DDEPLOYMENT_TARGET=12.0 \
  -DCMAKE_BUILD_TYPE=Release

# 构建静态库
xcodebuild -project UnityRimeDLL.xcodeproj -configuration Release -sdk iphoneos

# 复制静态库到Unity项目
cp Release-iphoneos/librime_dll.a "$PROJECT_PATH/Assets/Plugins/iOS/"

# Unity构建
echo "Unity iOS构建..."
"$UNITY_PATH" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -buildTarget iOS \
  -customBuildTarget iOS \
  -customBuildPath "$BUILD_PATH" \
  -executeMethod BuildScript.BuildiOS \
  -logFile build.log

echo "iOS构建完成！"
echo "构建输出: $BUILD_PATH"
```

**Unity iOS构建脚本 (BuildScript.cs)：**
```csharp
using UnityEngine;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using System.IO;

public class BuildScript
{
    [MenuItem("Build/Build iOS")]
    public static void BuildiOS()
    {
        string[] scenes = {
            "Assets/Scenes/MainScene.unity"
        };
        
        string buildPath = GetCommandLineArg("-customBuildPath");
        if (string.IsNullOrEmpty(buildPath))
        {
            buildPath = "Builds/iOS";
        }
        
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.iOS;
        buildPlayerOptions.options = BuildOptions.None;
        
        // 配置iOS设置
        PlayerSettings.iOS.targetOSVersionString = "12.0";
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        
        // 构建
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("iOS构建成功！");
            
            // 后处理Xcode项目
            PostProcessXcodeProject(buildPath);
        }
        else
        {
            Debug.LogError("iOS构建失败！");
            EditorApplication.Exit(1);
        }
    }
    
    private static void PostProcessXcodeProject(string buildPath)
    {
        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        PBXProject proj = new PBXProject();
        proj.ReadFromString(File.ReadAllText(projPath));
        
        string target = proj.GetUnityMainTargetGuid();
        
        // 添加静态库
        proj.AddFileToBuild(target, proj.AddFile("librime_dll.a", "librime_dll.a", PBXSourceTree.Source));
        
        // 添加系统框架
        proj.AddFrameworkToProject(target, "Foundation.framework", false);
        proj.AddFrameworkToProject(target, "UIKit.framework", false);
        
        // 设置编译标志
        proj.AddBuildProperty(target, "OTHER_LDFLAGS", "-ObjC");
        
        // 保存项目
        File.WriteAllText(projPath, proj.WriteToString());
        
        Debug.Log("Xcode项目后处理完成");
    }
    
    private static string GetCommandLineArg(string name)
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }
}
```

#### Android平台部署

**Android构建配置：**
```bash
#!/bin/bash
# build-android.sh

set -e

echo "开始Android平台构建..."

# 环境变量
UNITY_PATH="/opt/Unity/Editor/Unity"
PROJECT_PATH="$(pwd)/UnityProject"
BUILD_PATH="$(pwd)/Builds/Android"
ANDROID_NDK="/opt/android-ndk"

# 清理构建目录
rm -rf "$BUILD_PATH"
mkdir -p "$BUILD_PATH"

# 编译Android库
echo "编译Android库..."
cd dll_component

# 为不同架构编译
for ARCH in arm64-v8a armeabi-v7a x86_64; do
    echo "编译架构: $ARCH"
    
    mkdir -p build-android-$ARCH
    cd build-android-$ARCH
    
    cmake .. \
      -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake \
      -DANDROID_ABI=$ARCH \
      -DANDROID_PLATFORM=android-24 \
      -DCMAKE_BUILD_TYPE=Release
    
    make -j4
    
    # 复制库文件到Unity项目
    mkdir -p "$PROJECT_PATH/Assets/Plugins/Android/$ARCH"
    cp librime_dll.so "$PROJECT_PATH/Assets/Plugins/Android/$ARCH/"
    
    cd ..
done

# Unity构建
echo "Unity Android构建..."
"$UNITY_PATH" \
  -batchmode \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -buildTarget Android \
  -customBuildTarget Android \
  -customBuildPath "$BUILD_PATH" \
  -executeMethod BuildScript.BuildAndroid \
  -logFile build.log

echo "Android构建完成！"
echo "构建输出: $BUILD_PATH"
```

## 监控和维护

### 监控系统

#### Prometheus监控配置

**监控指标收集 (metrics.py)：**
```python
from prometheus_client import Counter, Histogram, Gauge, start_http_server
import time
import threading

class RimeMetrics:
    def __init__(self):
        # 计数器
        self.requests_total = Counter('rime_requests_total', 'Total requests', ['method', 'status'])
        self.connections_total = Counter('rime_connections_total', 'Total connections')
        
        # 直方图
        self.request_duration = Histogram('rime_request_duration_seconds', 'Request duration')
        self.processing_time = Histogram('rime_processing_time_seconds', 'Processing time')
        
        # 仪表
        self.active_connections = Gauge('rime_active_connections', 'Active connections')
        self.memory_usage = Gauge('rime_memory_usage_bytes', 'Memory usage')
        self.cpu_usage = Gauge('rime_cpu_usage_percent', 'CPU usage')
    
    def record_request(self, method, status, duration):
        self.requests_total.labels(method=method, status=status).inc()
        self.request_duration.observe(duration)
    
    def record_processing_time(self, duration):
        self.processing_time.observe(duration)
    
    def set_active_connections(self, count):
        self.active_connections.set(count)
    
    def update_system_metrics(self):
        import psutil
        
        # 更新内存使用
        memory = psutil.virtual_memory()
        self.memory_usage.set(memory.used)
        
        # 更新CPU使用
        cpu_percent = psutil.cpu_percent()
        self.cpu_usage.set(cpu_percent)

# 全局指标实例
metrics = RimeMetrics()

def start_metrics_server(port=8000):
    """启动指标服务器"""
    start_http_server(port)
    
    # 启动系统指标更新线程
    def update_system_metrics():
        while True:
            metrics.update_system_metrics()
            time.sleep(10)
    
    thread = threading.Thread(target=update_system_metrics, daemon=True)
    thread.start()
```

**Prometheus配置 (prometheus.yml)：**
```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "rime_rules.yml"

scrape_configs:
  - job_name: 'unity-rime-server'
    static_configs:
      - targets: ['localhost:8000']
    scrape_interval: 5s
    metrics_path: /metrics

alerting:
  alertmanagers:
    - static_configs:
        - targets:
          - alertmanager:9093
```

**告警规则 (rime_rules.yml)：**
```yaml
groups:
- name: unity-rime-alerts
  rules:
  - alert: HighErrorRate
    expr: rate(rime_requests_total{status="error"}[5m]) > 0.1
    for: 2m
    labels:
      severity: warning
    annotations:
      summary: "Unity Rime服务错误率过高"
      description: "错误率超过10%，当前值: {{ $value }}"

  - alert: HighResponseTime
    expr: histogram_quantile(0.95, rate(rime_request_duration_seconds_bucket[5m])) > 0.5
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: "Unity Rime服务响应时间过长"
      description: "95%分位响应时间超过500ms，当前值: {{ $value }}s"

  - alert: ServiceDown
    expr: up{job="unity-rime-server"} == 0
    for: 1m
    labels:
      severity: critical
    annotations:
      summary: "Unity Rime服务不可用"
      description: "服务已停止响应超过1分钟"

  - alert: HighMemoryUsage
    expr: rime_memory_usage_bytes / (1024*1024*1024) > 1
    for: 5m
    labels:
      severity: warning
    annotations:
      summary: "Unity Rime服务内存使用过高"
      description: "内存使用超过1GB，当前值: {{ $value }}GB"
```

#### Grafana仪表板

**仪表板配置 (grafana-dashboard.json)：**
```json
{
  "dashboard": {
    "id": null,
    "title": "Unity Rime输入法监控",
    "tags": ["unity", "rime", "input-method"],
    "timezone": "browser",
    "panels": [
      {
        "id": 1,
        "title": "请求速率",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(rime_requests_total[5m])",
            "legendFormat": "{{method}} - {{status}}"
          }
        ],
        "yAxes": [
          {
            "label": "请求/秒"
          }
        ]
      },
      {
        "id": 2,
        "title": "响应时间",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.50, rate(rime_request_duration_seconds_bucket[5m]))",
            "legendFormat": "50%分位"
          },
          {
            "expr": "histogram_quantile(0.95, rate(rime_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95%分位"
          },
          {
            "expr": "histogram_quantile(0.99, rate(rime_request_duration_seconds_bucket[5m]))",
            "legendFormat": "99%分位"
          }
        ],
        "yAxes": [
          {
            "label": "秒"
          }
        ]
      },
      {
        "id": 3,
        "title": "活跃连接数",
        "type": "singlestat",
        "targets": [
          {
            "expr": "rime_active_connections"
          }
        ]
      },
      {
        "id": 4,
        "title": "系统资源使用",
        "type": "graph",
        "targets": [
          {
            "expr": "rime_memory_usage_bytes / (1024*1024)",
            "legendFormat": "内存使用 (MB)"
          },
          {
            "expr": "rime_cpu_usage_percent",
            "legendFormat": "CPU使用率 (%)"
          }
        ]
      }
    ],
    "time": {
      "from": "now-1h",
      "to": "now"
    },
    "refresh": "5s"
  }
}
```

### 日志管理

#### 结构化日志配置

**日志配置 (logging_config.py)：**
```python
import logging
import json
import sys
from datetime import datetime

class StructuredFormatter(logging.Formatter):
    """结构化日志格式化器"""
    
    def format(self, record):
        log_entry = {
            'timestamp': datetime.utcnow().isoformat(),
            'level': record.levelname,
            'logger': record.name,
            'message': record.getMessage(),
            'module': record.module,
            'function': record.funcName,
            'line': record.lineno
        }
        
        # 添加异常信息
        if record.exc_info:
            log_entry['exception'] = self.formatException(record.exc_info)
        
        # 添加自定义字段
        if hasattr(record, 'user_id'):
            log_entry['user_id'] = record.user_id
        if hasattr(record, 'session_id'):
            log_entry['session_id'] = record.session_id
        if hasattr(record, 'request_id'):
            log_entry['request_id'] = record.request_id
        
        return json.dumps(log_entry, ensure_ascii=False)

def setup_logging(log_level='INFO', log_file=None):
    """设置日志配置"""
    
    # 创建根日志器
    root_logger = logging.getLogger()
    root_logger.setLevel(getattr(logging, log_level.upper()))
    
    # 清除现有处理器
    for handler in root_logger.handlers[:]:
        root_logger.removeHandler(handler)
    
    # 创建格式化器
    formatter = StructuredFormatter()
    
    # 控制台处理器
    console_handler = logging.StreamHandler(sys.stdout)
    console_handler.setFormatter(formatter)
    root_logger.addHandler(console_handler)
    
    # 文件处理器（如果指定）
    if log_file:
        file_handler = logging.FileHandler(log_file)
        file_handler.setFormatter(formatter)
        root_logger.addHandler(file_handler)
    
    # 设置第三方库日志级别
    logging.getLogger('urllib3').setLevel(logging.WARNING)
    logging.getLogger('requests').setLevel(logging.WARNING)

# 日志装饰器
def log_function_call(logger):
    """函数调用日志装饰器"""
    def decorator(func):
        def wrapper(*args, **kwargs):
            start_time = time.time()
            
            logger.info(
                f"调用函数: {func.__name__}",
                extra={
                    'function': func.__name__,
                    'args_count': len(args),
                    'kwargs_count': len(kwargs)
                }
            )
            
            try:
                result = func(*args, **kwargs)
                duration = time.time() - start_time
                
                logger.info(
                    f"函数执行完成: {func.__name__}",
                    extra={
                        'function': func.__name__,
                        'duration': duration,
                        'status': 'success'
                    }
                )
                
                return result
                
            except Exception as e:
                duration = time.time() - start_time
                
                logger.error(
                    f"函数执行失败: {func.__name__}",
                    extra={
                        'function': func.__name__,
                        'duration': duration,
                        'status': 'error',
                        'error': str(e)
                    },
                    exc_info=True
                )
                
                raise
        
        return wrapper
    return decorator
```

### 备份和恢复

#### 数据备份策略

**备份脚本 (backup.sh)：**
```bash
#!/bin/bash

set -e

# 配置
BACKUP_DIR="/opt/backups/unity-rime"
RETENTION_DAYS=30
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_NAME="unity-rime-backup-${TIMESTAMP}"

# 创建备份目录
mkdir -p "$BACKUP_DIR"

echo "开始备份 Unity Rime 数据..."

# 备份配置文件
echo "备份配置文件..."
tar -czf "$BACKUP_DIR/${BACKUP_NAME}-config.tar.gz" \
    /opt/unity-rime/config/ \
    /opt/unity-rime/dictionaries/ \
    /opt/unity-rime/schemas/

# 备份用户数据
echo "备份用户数据..."
if [ -d "/opt/unity-rime/userdata" ]; then
    tar -czf "$BACKUP_DIR/${BACKUP_NAME}-userdata.tar.gz" \
        /opt/unity-rime/userdata/
fi

# 备份日志文件
echo "备份日志文件..."
if [ -d "/var/log/unity-rime" ]; then
    tar -czf "$BACKUP_DIR/${BACKUP_NAME}-logs.tar.gz" \
        /var/log/unity-rime/
fi

# 备份数据库（如果有）
echo "备份数据库..."
if command -v pg_dump &> /dev/null; then
    pg_dump unity_rime > "$BACKUP_DIR/${BACKUP_NAME}-database.sql"
fi

# 创建备份清单
echo "创建备份清单..."
cat > "$BACKUP_DIR/${BACKUP_NAME}-manifest.txt" << EOF
备份时间: $(date)
备份版本: ${TIMESTAMP}
包含文件:
- ${BACKUP_NAME}-config.tar.gz (配置文件)
- ${BACKUP_NAME}-userdata.tar.gz (用户数据)
- ${BACKUP_NAME}-logs.tar.gz (日志文件)
- ${BACKUP_NAME}-database.sql (数据库)
- ${BACKUP_NAME}-manifest.txt (本清单)
EOF

# 清理旧备份
echo "清理旧备份..."
find "$BACKUP_DIR" -name "unity-rime-backup-*" -mtime +$RETENTION_DAYS -delete

echo "备份完成！"
echo "备份位置: $BACKUP_DIR"
echo "备份文件: ${BACKUP_NAME}-*"
```

**恢复脚本 (restore.sh)：**
```bash
#!/bin/bash

set -e

# 参数检查
if [ $# -ne 1 ]; then
    echo "用法: $0 <备份时间戳>"
    echo "示例: $0 20250718_143000"
    exit 1
fi

TIMESTAMP=$1
BACKUP_DIR="/opt/backups/unity-rime"
BACKUP_NAME="unity-rime-backup-${TIMESTAMP}"

# 检查备份文件是否存在
if [ ! -f "$BACKUP_DIR/${BACKUP_NAME}-manifest.txt" ]; then
    echo "错误: 备份文件不存在: $BACKUP_DIR/${BACKUP_NAME}-*"
    exit 1
fi

echo "开始恢复 Unity Rime 数据..."
echo "备份时间戳: $TIMESTAMP"

# 显示备份信息
echo "备份信息:"
cat "$BACKUP_DIR/${BACKUP_NAME}-manifest.txt"
echo

# 确认恢复
read -p "确认要恢复此备份吗？这将覆盖现有数据。(y/N): " confirm
if [ "$confirm" != "y" ] && [ "$confirm" != "Y" ]; then
    echo "恢复已取消"
    exit 0
fi

# 停止服务
echo "停止 Unity Rime 服务..."
systemctl stop unity-rime-server || true
docker-compose -f /opt/unity-rime/docker-compose.yml down || true

# 备份当前数据
echo "备份当前数据..."
CURRENT_BACKUP="/tmp/unity-rime-current-$(date +%Y%m%d_%H%M%S)"
mkdir -p "$CURRENT_BACKUP"
cp -r /opt/unity-rime/config "$CURRENT_BACKUP/" 2>/dev/null || true
cp -r /opt/unity-rime/userdata "$CURRENT_BACKUP/" 2>/dev/null || true

# 恢复配置文件
echo "恢复配置文件..."
if [ -f "$BACKUP_DIR/${BACKUP_NAME}-config.tar.gz" ]; then
    tar -xzf "$BACKUP_DIR/${BACKUP_NAME}-config.tar.gz" -C /
fi

# 恢复用户数据
echo "恢复用户数据..."
if [ -f "$BACKUP_DIR/${BACKUP_NAME}-userdata.tar.gz" ]; then
    tar -xzf "$BACKUP_DIR/${BACKUP_NAME}-userdata.tar.gz" -C /
fi

# 恢复数据库
echo "恢复数据库..."
if [ -f "$BACKUP_DIR/${BACKUP_NAME}-database.sql" ]; then
    if command -v psql &> /dev/null; then
        psql unity_rime < "$BACKUP_DIR/${BACKUP_NAME}-database.sql"
    fi
fi

# 设置权限
echo "设置文件权限..."
chown -R unity-rime:unity-rime /opt/unity-rime/
chmod -R 755 /opt/unity-rime/config/
chmod -R 700 /opt/unity-rime/userdata/

# 启动服务
echo "启动 Unity Rime 服务..."
systemctl start unity-rime-server || \
docker-compose -f /opt/unity-rime/docker-compose.yml up -d

# 验证恢复
echo "验证服务状态..."
sleep 10
if systemctl is-active --quiet unity-rime-server || \
   docker-compose -f /opt/unity-rime/docker-compose.yml ps | grep -q "Up"; then
    echo "✅ 恢复成功！服务正在运行"
else
    echo "❌ 恢复可能有问题，请检查服务状态"
    echo "当前数据已备份到: $CURRENT_BACKUP"
fi

echo "恢复完成！"
```

## 安全配置

### 网络安全

**防火墙配置：**
```bash
#!/bin/bash
# firewall-setup.sh

# UFW配置（Ubuntu）
if command -v ufw &> /dev/null; then
    echo "配置UFW防火墙..."
    
    # 重置规则
    ufw --force reset
    
    # 默认策略
    ufw default deny incoming
    ufw default allow outgoing
    
    # 允许SSH
    ufw allow ssh
    
    # 允许Unity Rime服务端口（仅限内网）
    ufw allow from 10.0.0.0/8 to any port 9999
    ufw allow from 172.16.0.0/12 to any port 9999
    ufw allow from 192.168.0.0/16 to any port 9999
    
    # 允许监控端口（仅限内网）
    ufw allow from 10.0.0.0/8 to any port 8000
    ufw allow from 172.16.0.0/12 to any port 8000
    ufw allow from 192.168.0.0/16 to any port 8000
    
    # 启用防火墙
    ufw --force enable
    
    echo "UFW防火墙配置完成"
fi

# iptables配置
if command -v iptables &> /dev/null; then
    echo "配置iptables防火墙..."
    
    # 清空现有规则
    iptables -F
    iptables -X
    iptables -t nat -F
    iptables -t nat -X
    
    # 默认策略
    iptables -P INPUT DROP
    iptables -P FORWARD DROP
    iptables -P OUTPUT ACCEPT
    
    # 允许回环
    iptables -A INPUT -i lo -j ACCEPT
    
    # 允许已建立的连接
    iptables -A INPUT -m state --state ESTABLISHED,RELATED -j ACCEPT
    
    # 允许SSH
    iptables -A INPUT -p tcp --dport 22 -j ACCEPT
    
    # 允许Unity Rime服务端口（仅限内网）
    iptables -A INPUT -p tcp --dport 9999 -s 10.0.0.0/8 -j ACCEPT
    iptables -A INPUT -p tcp --dport 9999 -s 172.16.0.0/12 -j ACCEPT
    iptables -A INPUT -p tcp --dport 9999 -s 192.168.0.0/16 -j ACCEPT
    
    # 保存规则
    if command -v iptables-save &> /dev/null; then
        iptables-save > /etc/iptables/rules.v4
    fi
    
    echo "iptables防火墙配置完成"
fi
```

### SSL/TLS配置

**SSL证书配置：**
```bash
#!/bin/bash
# ssl-setup.sh

set -e

DOMAIN="rime.yourdomain.com"
EMAIL="admin@yourdomain.com"
CERT_DIR="/etc/ssl/unity-rime"

echo "配置SSL证书..."

# 创建证书目录
mkdir -p "$CERT_DIR"

# 使用Let's Encrypt获取证书
if command -v certbot &> /dev/null; then
    echo "使用Let's Encrypt获取SSL证书..."
    
    certbot certonly \
        --standalone \
        --email "$EMAIL" \
        --agree-tos \
        --no-eff-email \
        -d "$DOMAIN"
    
    # 复制证书到应用目录
    cp "/etc/letsencrypt/live/$DOMAIN/fullchain.pem" "$CERT_DIR/cert.pem"
    cp "/etc/letsencrypt/live/$DOMAIN/privkey.pem" "$CERT_DIR/key.pem"
    
    # 设置权限
    chmod 644 "$CERT_DIR/cert.pem"
    chmod 600 "$CERT_DIR/key.pem"
    chown unity-rime:unity-rime "$CERT_DIR"/*
    
    echo "SSL证书配置完成"
    
    # 设置自动续期
    echo "设置证书自动续期..."
    (crontab -l 2>/dev/null; echo "0 12 * * * /usr/bin/certbot renew --quiet --post-hook 'systemctl reload nginx'") | crontab -
    
else
    echo "生成自签名证书..."
    
    # 生成私钥
    openssl genrsa -out "$CERT_DIR/key.pem" 2048
    
    # 生成证书签名请求
    openssl req -new -key "$CERT_DIR/key.pem" -out "$CERT_DIR/cert.csr" -subj "/CN=$DOMAIN"
    
    # 生成自签名证书
    openssl x509 -req -in "$CERT_DIR/cert.csr" -signkey "$CERT_DIR/key.pem" -out "$CERT_DIR/cert.pem" -days 365
    
    # 清理临时文件
    rm "$CERT_DIR/cert.csr"
    
    # 设置权限
    chmod 644 "$CERT_DIR/cert.pem"
    chmod 600 "$CERT_DIR/key.pem"
    
    echo "自签名证书生成完成"
fi
```

## 故障排除

### 常见部署问题

#### 1. 端口冲突

**问题症状：**
```
Error: bind: address already in use
```

**诊断步骤：**
```bash
# 检查端口占用
netstat -tulpn | grep :9999
lsof -i :9999

# 查找占用进程
ps aux | grep 9999
```

**解决方案：**
```bash
# 停止占用进程
sudo kill -9 <PID>

# 或更改服务端口
export RIME_SERVER_PORT=9998
```

#### 2. 权限问题

**问题症状：**
```
Permission denied: '/opt/unity-rime/config'
```

**解决方案：**
```bash
# 设置正确的文件权限
sudo chown -R unity-rime:unity-rime /opt/unity-rime/
sudo chmod -R 755 /opt/unity-rime/config/
sudo chmod -R 700 /opt/unity-rime/userdata/

# 检查SELinux状态（如果适用）
sestatus
sudo setsebool -P httpd_can_network_connect 1
```

#### 3. 依赖库缺失

**问题症状：**
```
ImportError: No module named 'rime'
```

**解决方案：**
```bash
# 安装系统依赖
sudo apt-get install librime1 librime-dev  # Ubuntu
sudo yum install librime librime-devel     # CentOS

# 安装Python依赖
pip install -r requirements.txt

# 检查库路径
ldconfig -p | grep rime
```

### 性能调优

#### 系统级优化

**内核参数调优：**
```bash
# /etc/sysctl.conf
net.core.somaxconn = 65535
net.core.netdev_max_backlog = 5000
net.ipv4.tcp_max_syn_backlog = 65535
net.ipv4.tcp_fin_timeout = 30
net.ipv4.tcp_keepalive_time = 1200
net.ipv4.tcp_max_tw_buckets = 5000

# 应用配置
sysctl -p
```

**文件描述符限制：**
```bash
# /etc/security/limits.conf
unity-rime soft nofile 65535
unity-rime hard nofile 65535

# /etc/systemd/system/unity-rime.service
[Service]
LimitNOFILE=65535
```

#### 应用级优化

**Python服务器优化：**
```python
# 优化配置
import asyncio
import uvloop  # 高性能事件循环

# 使用uvloop
asyncio.set_event_loop_policy(uvloop.EventLoopPolicy())

# 连接池配置
MAX_CONNECTIONS = 1000
CONNECTION_TIMEOUT = 30
KEEPALIVE_TIMEOUT = 60

# 内存优化
import gc
gc.set_threshold(700, 10, 10)  # 调整垃圾回收阈值
```

---

*本部署指南最后更新于2025年7月18日*

