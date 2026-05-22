import os
import shutil
source = r"Z:\个人文件夹\张灵顿\异常"
target = r"Z:\算法\接发车\测试异常\异常检测\正确告警"
paths = os.listdir(source)
paths.reverse()
for jpg in paths:
    try:
        if not jpg.endswith(".jpg"):
            continue
        jpg_source_path = os.path.join(source, jpg)

        label = jpg.split("_")[2]
        jpg_target_path = target
        for folder in os.listdir(target):
            folder_label = folder.split("_")[0]
            if label.upper() == folder_label.upper():
                jpg_target_path = os.path.join(jpg_target_path, folder)
                break
        filepath = os.path.join(jpg_target_path, jpg)
        if not os.path.exists(filepath):
            shutil.copy2(jpg_source_path, jpg_target_path)
            print(f"{jpg_source_path} -> {jpg_target_path}")
        else:
            print(f"{jpg} 已归类，无需复制")
    except Exception as e:
        print(jpg)