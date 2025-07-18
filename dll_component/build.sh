#!/bin/bash

# Unity Rime输入法集成 - DLL组件构建脚本
# 
# 作者: Manus AI
# 版本: 1.0.0

set -e

echo "Unity Rime DLL 构建脚本"
echo "======================="

# 检查依赖
echo "检查构建依赖..."

if ! command -v cmake &> /dev/null; then
    echo "错误: 未找到cmake，请先安装cmake"
    exit 1
fi

if ! command -v g++ &> /dev/null && ! command -v clang++ &> /dev/null; then
    echo "错误: 未找到C++编译器，请先安装g++或clang++"
    exit 1
fi

echo "依赖检查完成"

# 创建构建目录
BUILD_DIR="build"
if [ -d "$BUILD_DIR" ]; then
    echo "清理旧的构建目录..."
    rm -rf "$BUILD_DIR"
fi

echo "创建构建目录: $BUILD_DIR"
mkdir -p "$BUILD_DIR"
cd "$BUILD_DIR"

# 配置项目
echo "配置CMake项目..."
cmake .. -DCMAKE_BUILD_TYPE=Release

# 编译项目
echo "编译项目..."
make -j$(nproc)

# 运行测试
echo "运行测试程序..."
echo "==================="
./test_dll

echo ""
echo "构建完成！"
echo "生成的文件:"
echo "  - librime_dll.so (Linux动态库)"
echo "  - test_dll (测试程序)"
echo ""
echo "在Unity中使用时，请将librime_dll.so复制到Unity项目的Plugins目录下"

