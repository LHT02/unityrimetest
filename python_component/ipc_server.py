#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
Unity Rime输入法集成 - IPC服务器
通过TCP Socket与Unity进行通信

作者: Manus AI
版本: 1.0.0
"""

import socket
import json
import threading
import logging
import signal
import sys
import time
from typing import Dict, Any, Optional
from rime_wrapper import RimeWrapper

# 配置日志
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('ipc_server.log'),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger(__name__)

class IPCServer:
    """IPC服务器，处理与Unity的通信"""
    
    def __init__(self, host: str = "127.0.0.1", port: int = 9999):
        """
        初始化IPC服务器
        
        Args:
            host: 服务器地址
            port: 服务器端口
        """
        self.host = host
        self.port = port
        self.server_socket = None
        self.client_socket = None
        self.is_running = False
        self.rime_wrapper = None
        
        # 设置信号处理
        signal.signal(signal.SIGINT, self._signal_handler)
        signal.signal(signal.SIGTERM, self._signal_handler)
    
    def _signal_handler(self, signum, frame):
        """信号处理函数"""
        logger.info(f"接收到信号 {signum}，正在关闭服务器...")
        self.stop()
        sys.exit(0)
    
    def start(self):
        """启动服务器"""
        try:
            # 初始化Rime包装器
            logger.info("初始化Rime包装器...")
            self.rime_wrapper = RimeWrapper()
            
            if not self.rime_wrapper.is_initialized:
                logger.error("Rime包装器初始化失败")
                return False
            
            # 创建服务器套接字
            self.server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.server_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
            self.server_socket.bind((self.host, self.port))
            self.server_socket.listen(1)
            
            self.is_running = True
            logger.info(f"IPC服务器启动成功，监听 {self.host}:{self.port}")
            
            # 等待客户端连接
            self._accept_connections()
            
            return True
            
        except Exception as e:
            logger.error(f"启动服务器失败: {e}")
            return False
    
    def _accept_connections(self):
        """接受客户端连接"""
        while self.is_running:
            try:
                logger.info("等待Unity客户端连接...")
                client_socket, client_address = self.server_socket.accept()
                logger.info(f"Unity客户端已连接: {client_address}")
                
                # 处理客户端连接
                self._handle_client(client_socket)
                
            except socket.error as e:
                if self.is_running:
                    logger.error(f"接受连接失败: {e}")
                break
    
    def _handle_client(self, client_socket):
        """处理客户端请求"""
        self.client_socket = client_socket
        
        try:
            while self.is_running:
                # 接收数据
                data = self._receive_message(client_socket)
                if not data:
                    break
                
                # 处理请求
                response = self._process_request(data)
                
                # 发送响应
                self._send_message(client_socket, response)
                
        except Exception as e:
            logger.error(f"处理客户端请求失败: {e}")
        finally:
            client_socket.close()
            self.client_socket = None
            logger.info("客户端连接已关闭")
    
    def _receive_message(self, client_socket) -> Optional[Dict[str, Any]]:
        """接收消息"""
        try:
            # 先接收消息长度（4字节）
            length_data = client_socket.recv(4)
            if len(length_data) != 4:
                return None
            
            message_length = int.from_bytes(length_data, byteorder='little')
            
            # 接收完整消息
            message_data = b''
            while len(message_data) < message_length:
                chunk = client_socket.recv(message_length - len(message_data))
                if not chunk:
                    return None
                message_data += chunk
            
            # 解析JSON
            message_str = message_data.decode('utf-8')
            return json.loads(message_str)
            
        except Exception as e:
            logger.error(f"接收消息失败: {e}")
            return None
    
    def _send_message(self, client_socket, message: Dict[str, Any]):
        """发送消息"""
        try:
            # 序列化为JSON
            message_str = json.dumps(message, ensure_ascii=False)
            message_data = message_str.encode('utf-8')
            
            # 发送消息长度（4字节）
            length_data = len(message_data).to_bytes(4, byteorder='little')
            client_socket.send(length_data)
            
            # 发送消息内容
            client_socket.send(message_data)
            
        except Exception as e:
            logger.error(f"发送消息失败: {e}")
    
    def _process_request(self, request: Dict[str, Any]) -> Dict[str, Any]:
        """处理请求"""
        try:
            command = request.get('command', '')
            params = request.get('params', {})
            
            logger.info(f"处理命令: {command}")
            
            if command == 'process_key':
                # 处理按键
                key_code = params.get('key_code', 0)
                return self.rime_wrapper.process_key(key_code)
                
            elif command == 'select_candidate':
                # 选择候选词
                index = params.get('index', 0)
                return self.rime_wrapper.select_candidate(index)
                
            elif command == 'clear_composition':
                # 清空输入
                return self.rime_wrapper.clear_composition()
                
            elif command == 'get_state':
                # 获取当前状态
                return self.rime_wrapper.get_current_state()
                
            elif command == 'ping':
                # 心跳检测
                return {"success": True, "message": "pong"}
                
            else:
                return {"error": f"未知命令: {command}"}
                
        except Exception as e:
            logger.error(f"处理请求失败: {e}")
            return {"error": str(e)}
    
    def stop(self):
        """停止服务器"""
        self.is_running = False
        
        if self.client_socket:
            try:
                self.client_socket.close()
            except:
                pass
        
        if self.server_socket:
            try:
                self.server_socket.close()
            except:
                pass
        
        logger.info("IPC服务器已停止")

class IPCClient:
    """IPC客户端，用于测试与服务器的通信"""
    
    def __init__(self, host: str = "127.0.0.1", port: int = 9999):
        """
        初始化IPC客户端
        
        Args:
            host: 服务器地址
            port: 服务器端口
        """
        self.host = host
        self.port = port
        self.socket = None
        self.is_connected = False
    
    def connect(self) -> bool:
        """连接到服务器"""
        try:
            self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
            self.socket.connect((self.host, self.port))
            self.is_connected = True
            logger.info(f"已连接到服务器 {self.host}:{self.port}")
            return True
        except Exception as e:
            logger.error(f"连接服务器失败: {e}")
            return False
    
    def send_request(self, command: str, params: Dict[str, Any] = None) -> Optional[Dict[str, Any]]:
        """发送请求"""
        if not self.is_connected:
            return None
        
        try:
            # 构建请求
            request = {
                "command": command,
                "params": params or {}
            }
            
            # 发送请求
            self._send_message(request)
            
            # 接收响应
            return self._receive_message()
            
        except Exception as e:
            logger.error(f"发送请求失败: {e}")
            return None
    
    def _send_message(self, message: Dict[str, Any]):
        """发送消息"""
        message_str = json.dumps(message, ensure_ascii=False)
        message_data = message_str.encode('utf-8')
        
        # 发送消息长度
        length_data = len(message_data).to_bytes(4, byteorder='little')
        self.socket.send(length_data)
        
        # 发送消息内容
        self.socket.send(message_data)
    
    def _receive_message(self) -> Optional[Dict[str, Any]]:
        """接收消息"""
        # 接收消息长度
        length_data = self.socket.recv(4)
        if len(length_data) != 4:
            return None
        
        message_length = int.from_bytes(length_data, byteorder='little')
        
        # 接收完整消息
        message_data = b''
        while len(message_data) < message_length:
            chunk = self.socket.recv(message_length - len(message_data))
            if not chunk:
                return None
            message_data += chunk
        
        # 解析JSON
        message_str = message_data.decode('utf-8')
        return json.loads(message_str)
    
    def disconnect(self):
        """断开连接"""
        if self.socket:
            self.socket.close()
            self.is_connected = False
            logger.info("已断开与服务器的连接")

def test_client():
    """测试客户端功能"""
    logger.info("启动IPC客户端测试")
    
    client = IPCClient()
    
    # 连接到服务器
    if not client.connect():
        logger.error("无法连接到服务器")
        return
    
    try:
        # 测试心跳
        response = client.send_request("ping")
        logger.info(f"心跳测试: {response}")
        
        # 测试输入
        test_keys = [110, 105, 104, 97, 111]  # "nihao"
        for key in test_keys:
            response = client.send_request("process_key", {"key_code": key})
            logger.info(f"输入键码 {key}: {json.dumps(response, ensure_ascii=False, indent=2)}")
            time.sleep(0.1)
        
        # 测试选择候选词
        response = client.send_request("select_candidate", {"index": 0})
        logger.info(f"选择候选词: {json.dumps(response, ensure_ascii=False, indent=2)}")
        
    finally:
        client.disconnect()

def main():
    """主函数"""
    if len(sys.argv) > 1 and sys.argv[1] == "test":
        # 测试模式
        test_client()
    else:
        # 服务器模式
        server = IPCServer()
        server.start()

if __name__ == "__main__":
    main()

