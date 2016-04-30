//
// Created by Shouta on 2016/04/29.
//
#include <string.h>
#include <assert.h>
#include <jni.h>

extern "C" {
jstring Java_jp_sofuken_shouta_ndktest_MainActivity_stringFromJNI(JNIEnv **env, jobject thiz) {
        jstring jstr =  (*env)->NewStringUTF("Hello JNI!");
        assert(jstr);
        return jstr;
    }
}

