import os
import pandas as pd
import numpy as np
import time



def do_task():
    global dt
    path = r"Z:\个人文件夹\张灵顿"
    res_data = np.empty((0,5))
    for csv in os.listdir(path):
        try:
            if not csv.endswith(".csv"):
                continue
            station_name = csv.split(".")[0]
            csv_path = os.path.join(path, csv)
            data:list = pd.read_csv(csv_path, header=None).to_numpy().tolist()[0]
            dt = data[0].replace("-", "_") # 日期
            data.insert(0, station_name)
            res_data = np.concatenate([res_data, np.array(data).reshape(1, -1)], axis=0)
        except Exception as e:
            continue
    temp = pd.DataFrame(res_data)
    print(res_data)
    temp.to_csv(os.path.join(path, f"{dt}.csv"), index=False, header=False)

if __name__ == "__main__":
    while True:
        try:
            print(f"开始执行任务，时间: {time.strftime('%Y-%m-%d %H:%M:%S')}")
            do_task()
            print(f"任务执行完成，时间: {time.strftime('%Y-%m-%d %H:%M:%S')}")
            print("等待24小时后再次执行...")
            time.sleep(86400)  # 24小时 = 86400秒
        except KeyboardInterrupt:
            print("\n程序被用户中断")
            break
        except Exception as e:
            print(f"执行任务时发生错误: {e}")
            print("等待1小时后重试...")
            time.sleep(3600)  # 出错后等待1小时再重试
