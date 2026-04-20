import os
import pandas as pd
import numpy as np
import time



def do_task(day):
    dt = f"2026_{day[:2]}_{day[2:]}"
    path = r"Z:\个人文件夹\张灵顿\station_archive"
    result_path= r"Z:\个人文件夹\张灵顿"
    res_data = np.empty((0,5))
    csvs = os.listdir(path)
    csvs.reverse()
    for csv in csvs:
        try:
            if not csv.__contains__(f"_2026{day}.csv"):
                continue
            station_name = csv.split("_")[0]
            csv_path = os.path.join(path, csv)
            data:list = pd.read_csv(csv_path, header=None).to_numpy().tolist()[0]
          
            data.insert(0, station_name)
            res_data = np.concatenate([res_data, np.array(data).reshape(1, -1)], axis=0)
        except Exception as e:
            continue
    temp = pd.DataFrame(res_data)
    print(res_data)
    temp.to_csv(os.path.join(result_path, f"{dt}.txt"), index=False, header=False)

if __name__ == "__main__":
    days = ["0416", "0415"]
    for day in days:
        do_task(day=day)
    