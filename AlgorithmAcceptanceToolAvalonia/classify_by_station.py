import os
import shutil
path = r"Z:\个人文件夹\张灵顿\manual_error"
files = os.listdir(path)
for file in files:
    try:
        if not file.endswith(".jpg"):
            continue
        station = file.split("_")[1]
        sourcePath = os.path.join(path, file)
        targetPath = os.path.join(path, station)
        if not os.path.exists(targetPath):
            os.makedirs(name=targetPath, exist_ok=True)

        shutil.move(sourcePath, os.path.join(targetPath, file))
        print(f"{sourcePath} -> {targetPath}")
    except Exception as  e:
        print(e)
