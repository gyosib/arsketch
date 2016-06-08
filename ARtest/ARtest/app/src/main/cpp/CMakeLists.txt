cmake_minimum_required(VERSION 3.4.1)

add_library( test-jni SHARED
            main.cpp
            )

# Include libraries needed for hello-jni lib
target_link_libraries( test-jni log android)