import os
import shutil
def classify_by_station(file):
    if not file.endswith(".jpg"):
        return
    station = file.split("_")[1]
    sourcePath = os.path.join(path, file)
    targetPath = os.path.join(path, station)
    if not os.path.exists(targetPath):
        os.makedirs(name=targetPath, exist_ok=True)
    a = os.path.join(targetPath, "误检")
    b = os.path.join(targetPath, "异常")
    if not os.path.exists(a):
        os.makedirs(a, exist_ok=True)
    if not os.path.exists(b):
        os.makedirs(b, exist_ok=True)
    shutil.move(sourcePath, os.path.join(a, file))
    print(f"{sourcePath} -> {targetPath}")
def clean(item_path):
    if os.path.isdir(item_path):
        item_files = os.listdir(item_path)
        for item_file in item_files:
            temp_path = os.path.join(item_path, item_file)
            if item_file.endswith(".jpg"):
                os.remove(temp_path)
                print(f"delete {temp_path}")
            if item_file == "result":
                shutil.rmtree(temp_path)
                print(f"delete {temp_path}/*")
            if item_file == "异常" or item_file == "误检":

                new_files = os.listdir(temp_path)
                for new_file in new_files:
                    new_file_path = os.path.join(temp_path, new_file)
                    if new_file.endswith(".jpg"):
                        os.remove(new_file_path)

                        print(f"delete {new_file_path}")
path = r".\训练集"
files = os.listdir(path)
for file in files:
    item_path = os.path.join(path, file)
    try:
        # classify_by_station(file)
        if file == "异常" or file == "误检":
            for label in os.listdir(item_path):
                label_path = os.path.join(item_path, label)
                cnt = 1
                for jpg in os.listdir(label_path):
                    try:
                        jpg_path = os.path.join(label_path, jpg)
                        if os.path.isdir(jpg_path):
                            shutil.rmtree(jpg_path)
                        from datetime import datetime
    
                        # 获取当前时间，包含微秒
                        now = datetime.now()
    
                        # 格式化为 yyyy-MM-dd-HH-mm-ss
                        # 注意：毫秒部分需要单独处理
                        formatted_time = now.strftime('%Y-%m-%d-%H-%M-%S')
    
                        # 添加毫秒部分（ttt）
                        milliseconds = now.microsecond // 1000  # 微秒转毫秒
                        result = f"{formatted_time}-{milliseconds:03d}"
    
                        postfix = jpg.split(label)[-1]
                        newName = f"{result}-{label}{postfix}"
                        jpg_new_path = os.path.join(label_path, newName)
                        os.rename(jpg_path, jpg_new_path)
                        print(f"{jpg_path} -> {jpg_new_path}")
                        cnt += 1
                    except Exception as  e:
                        print(e)
                        input("")
        
    except Exception as  e:
        print(e)
        input("")
input("")
