A xiami downloader using .NET framework 4.5, with metro style interface..
#功能列表
##通用UI
边界吸附和自动隐藏功能（类似qq）

可设定主题颜色，保存即改变

标题栏空白部分拖动可更改窗口位置

窗口边缘拖动可改变窗口大小

关闭时记录窗口位置和大小，下次启动在同一位置打开
##所有音乐列表
按ctrl或shift多选项目

可显示视图中的条目个数

增加和删除项目时有动画效果

根据选择项目的类型和页面环境，在页面底部列出相应的操作命令

按名称排序

按名称过滤，列表随输入变化立即生效

##搜索音乐列表
从xiami、豆瓣、百度搜索和显示歌曲

支持关键词搜索和页面链接搜索

可在xiami搜索的类型为歌曲、艺术家、专辑和精选集

可搜索音乐的关联项目，包括：
- 歌曲所在专辑的其他歌曲
- 歌曲的艺术家的最受欢迎歌曲（前20）
- 艺术家的类似艺术家
-艺术家的专辑按年份排列
-专辑中的歌曲
-精选集中的歌曲

可取消搜索

记录搜索的历史，包括关键字、搜索次数
##下载中歌曲列表
###内容
可下载歌曲、专辑封面和歌词（如果有）

下载的歌曲内嵌了封面、专辑和艺术家等id3v2信息

动画显示下载进度和信息
###控制
多线程下载
	
可控制同时下载的项目个数（限速）
	
支持断点续传
	
支持对选中的下载项目进行单独控制（开始、暂停、取消等）
###定制
可指定下载位置
	
可指定同时下载数量（限速）
	
可指定下载歌曲的命名规则
	
可指定歌曲是否按目录归档，以及目录的命名规则
##本地歌曲列表
###列表操作
按下载到本地的时间排序

当找不到符合过滤关键词的项目时，将出现按钮：“在网上搜索该关键词”
###播放控制
双击选中项目播放歌曲，再次双击暂停播放
	
播放歌曲时会显示进度条，可拖动进度条来快进快退
	
当选中的项目不是正在播放的项目时，提供选中正在播放的项目的功能	
	
可播放下一首歌曲，支持三种模式：
-单曲循环
-按列表顺序播放
-随机播放	
###选中项目后的操作
打开文件所在位置

拖动到其他窗口（如资源管理器）可复制歌曲文件到相应位置

将文件复制到剪贴板

选中多个项目时，可导出播放列表文件