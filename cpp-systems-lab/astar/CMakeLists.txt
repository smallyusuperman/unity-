cmake_minimum_required(VERSION 3.20)

project(astar_lab LANGUAGES CXX)

add_executable(astar_tests
    src/pathfinder.cpp
    tests/pathfinder_tests.cpp)

target_compile_features(astar_tests PRIVATE cxx_std_17)
target_include_directories(astar_tests PRIVATE include)

if(MSVC)
    target_compile_options(astar_tests PRIVATE /W4 /permissive-)
endif()

enable_testing()
add_test(NAME astar_tests COMMAND astar_tests)
