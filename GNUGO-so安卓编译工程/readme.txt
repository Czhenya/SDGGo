此工程是使用cmake管理用于编译.so库的工程，以便加入到unity中作为插件发布到安卓平台；

使用ndk-r10d以下的版本直接进行编译即可，此处提供了ndk-r10d的安装包；

自行封装的接口头文件在 ..\SDGGo\GNUGO-SO安卓封装工程\project\gnugo-3.8\utils目录下的Go.c文件中；

编译方法：
1. 配置好ndk环境；
2. 修改当前目录中的build脚本文件，将路径指向当前目录的Application.mk
   例如：ndk-build NDK_PROJECT_PATH=. NDK_APPLICATION_MK=C:\GNUGO\Application.mk
   然后拖入终端运行即可，编译好的库在输出目录\libs中。