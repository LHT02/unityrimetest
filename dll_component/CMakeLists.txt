# Unity Rime输入法集成 - DLL组件CMake配置
# 
# 作者: Manus AI
# 版本: 1.0.0

cmake_minimum_required(VERSION 3.10)
project(UnityRimeDLL VERSION 1.0.0)

# 设置C++标准
set(CMAKE_CXX_STANDARD 11)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 设置编译选项
if(WIN32)
    # Windows特定设置
    add_definitions(-DRIME_DLL_EXPORTS)
    set(CMAKE_WINDOWS_EXPORT_ALL_SYMBOLS ON)
elseif(APPLE)
    # macOS特定设置
    set(CMAKE_MACOSX_RPATH ON)
    set(CMAKE_INSTALL_RPATH "@loader_path")
else()
    # Linux特定设置
    set(CMAKE_INSTALL_RPATH "$ORIGIN")
endif()

# 源文件
set(SOURCES
    rime_dll.cpp
)

# 头文件
set(HEADERS
    rime_dll.h
)

# 创建动态库
add_library(UnityRimeDLL SHARED ${SOURCES} ${HEADERS})

# 设置输出名称
set_target_properties(UnityRimeDLL PROPERTIES
    OUTPUT_NAME "rime_dll"
    VERSION ${PROJECT_VERSION}
    SOVERSION 1
)

# 在实际项目中，应该链接真正的librime
# find_package(PkgConfig REQUIRED)
# pkg_check_modules(RIME REQUIRED rime-1)
# target_link_libraries(UnityRimeDLL ${RIME_LIBRARIES})
# target_include_directories(UnityRimeDLL PRIVATE ${RIME_INCLUDE_DIRS})
# target_compile_options(UnityRimeDLL PRIVATE ${RIME_CFLAGS_OTHER})

# 设置安装目录
install(TARGETS UnityRimeDLL
    LIBRARY DESTINATION lib
    ARCHIVE DESTINATION lib
    RUNTIME DESTINATION bin
)

install(FILES ${HEADERS}
    DESTINATION include
)

# 创建测试程序
add_executable(test_dll test_dll.cpp)
target_link_libraries(test_dll UnityRimeDLL)

# 打印构建信息
message(STATUS "Building Unity Rime DLL")
message(STATUS "  Version: ${PROJECT_VERSION}")
message(STATUS "  System: ${CMAKE_SYSTEM_NAME}")
message(STATUS "  Compiler: ${CMAKE_CXX_COMPILER_ID}")
message(STATUS "  Build type: ${CMAKE_BUILD_TYPE}")

# 设置默认构建类型
if(NOT CMAKE_BUILD_TYPE)
    set(CMAKE_BUILD_TYPE Release)
endif()

# 编译选项
if(CMAKE_CXX_COMPILER_ID MATCHES "GNU|Clang")
    target_compile_options(UnityRimeDLL PRIVATE
        -Wall -Wextra -Wpedantic
        $<$<CONFIG:Debug>:-g -O0>
        $<$<CONFIG:Release>:-O3 -DNDEBUG>
    )
elseif(CMAKE_CXX_COMPILER_ID MATCHES "MSVC")
    target_compile_options(UnityRimeDLL PRIVATE
        /W4
        $<$<CONFIG:Debug>:/Od /Zi>
        $<$<CONFIG:Release>:/O2 /DNDEBUG>
    )
endif()

