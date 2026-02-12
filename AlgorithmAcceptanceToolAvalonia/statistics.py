import os
import pandas as pd
import random
import locale
def do_task(date, version):
    image_path = r"Z:\个人文件夹\张灵顿\manual_error\archive"
    error_path = r"Z:\个人文件夹\张灵顿\异常"
    txt_path = r"Z:\个人文件夹\张灵顿"
    result_path = r"C:\Users\Administrator\Downloads"
    rows = pd.read_csv(os.path.join(txt_path, f"{date}.txt"), sep=",", header=None).to_numpy().tolist()
    results = []
    cols = [
        version ,
        "篷布飘起",
        "液体撒漏",
        "货物自燃",
        "人员扒乘",
        "车门开启",
        "车窗开启",
        "杂物",
        "标志灯",
        "列尾主机",
        "紧固器未拆除",
        "火星",
        "软管未连接",
        "闸链拉直",
        "折角塞门未关闭",
        "告警总数",
        "正确告警数",
        "车厢数",
        "车次数",
        "告警准确率",
        "误报率"
    ]
    for row in rows:
        tp = random.randint(1, 5)
        res = {
            version : "",
            "PB": 0,
            "SL": 0,
            "YW": 0,
            "P": 0,
            "M": 0,
            "C": 0,
            "ZW": 0,
            "BT": 0,
            "Z": 0,
            "JGQ": 0,
            "HX": 0,
            "RG": 0,
            "ZL": 0,
            "ZJ": 0,
            "告警总数": 0,
            "正确告警数": 0,
            "车厢数": 0,
            "车次数": 0,
            "告警准确率": "0.00%",
            "误报率": "0.00%"
        }
        header = list(res.keys())
        res[version] = row[0]
        res["告警总数"] = row[-1]
        res["车次数"] = row[-3]
        res["车厢数"] = row[-2]
       
        
        
        for image in os.listdir(image_path):
            try:
                station = image.split("_")[1]
                temp = image.replace("-", "_")
                if station == row[0] and date in temp: 
                    label = image.split("_")[2].upper()
                    if res.__contains__(label):
                        res[label] += 1
            except Exception as e:
                # print(e)
                continue
        error = 0
        for image in os.listdir(error_path):
            try:
                station = image.split("_")[1]
                temp = image.replace("-", "_")
                if station == row[0] and date in temp:
                    error += 1
            except Exception as e:
                # print(e)
                continue
        if error > int(res["告警总数"]):
            error = int(res["告警总数"])
        res["正确告警数"] = error
        false_alarm = int(res["告警总数"]) - error
        try:
            
            res["告警准确率"] = str(round(error / int(res["告警总数"]) * 100.0, 2)) + "%"
        except ZeroDivisionError:
            res["告警准确率"] = "无"
        res["误报率"] = str(round(false_alarm / int(row[-2]) * 100.0, 2)) + "%"
        results.append(list(res.values()))
    results = pd.DataFrame(results, columns=header)

    # 设置中文区域设置以支持中文排序
    try:
        locale.setlocale(locale.LC_COLLATE, 'zh_CN.UTF-8')
    except locale.Error:
        # 如果设置失败，尝试其他中文区域设置
        try:
            locale.setlocale(locale.LC_COLLATE, 'zh_CN')
        except locale.Error:
            # 如果还是失败，使用系统默认
            pass

    # 使用 locale.strxfrm 进行中文排序
    results = results.sort_values(
        by=f'{version}',
        axis=0,
        key=lambda col: col.map(lambda x: locale.strxfrm(str(x)) if pd.notnull(x) else '')
    )

    results.to_csv(os.path.join(result_path, f"{date}-{version}.csv"), index=False, header=cols)
    
    
if __name__ == "__main__":
    # dates = ["2026_02_09"]
    dates = ["2026_02_09","2026_02_10","2026_02_11"]
    version = "dev.012-Beta"
    for date in dates:
        do_task(date=date, version=version)