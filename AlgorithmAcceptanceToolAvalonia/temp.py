import os
import shutil
path = r"Z:\个人文件夹\张灵顿\manual_error\archive"
target_path = r"Z:\个人文件夹\张灵顿\defections"

for jpg in os.listdir(path):
    jpg_path = os.path.join(path, jpg)
    if not jpg.endswith(".jpg"):
        continue
    label = jpg.split("_")[2].upper()

    target_jpg = os.path.join(target_path, label)
    if not os.path.exists(target_jpg):
        os.makedirs(target_jpg, exist_ok=True)
    target_jpg_path = os.path.join(target_jpg, jpg)
    shutil.copy2(jpg_path, target_jpg_path)
    print(f"{jpg} -> {target_jpg_path} ")
