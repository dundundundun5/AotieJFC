import os
import pandas as pd
import random
import locale
def do_task(day, version):
    date = "2026_07_05"
    image_path = r"Z:\个人文件夹\张灵顿\manual_error"
    error_path = r"Z:\个人文件夹\张灵顿\异常"
    txt_path = r"Z:\个人文件夹\张灵顿"
    result_path = r"d:\Download"
    rows = pd.read_csv(os.path.join(txt_path, f"{date}.txt"), sep=",", header=None).to_numpy().tolist()
    results = []
    for prefix in ["客户端"]:
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
            "车厢数",
            "车次数",
            f"{prefix}误报率",
            f"{prefix}告警准确率",
            f"{prefix}正确告警数",
            f"{prefix}告警总数", 
        ]
        temp_list = []
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
                "车厢数": 0,
                "车次数": 0,
                "误报率": "无",
                "告警准确率": "无",
                "正确告警数": 0,
                "告警总数": 0,
            }
            header = list(res.keys())
            res[version] = row[0]
            
            # res["车次数"] = row[-3]
            # res["车厢数"] = row[-2]
       
        
            total = 0
          
            for image in os.listdir(image_path):
                try:
                    if not image.endswith(".jpg"):
                        continue
                    temp = image.replace("-", "_")
                    # print(image)
                    if date not in temp:
                        continue
                    station = image.split("_")[1]
                    label = image.split("_")[2].upper()
                    
                    if station == row[0] and date in temp: 
                        if res.__contains__(label) and temp.__contains__(prefix):
                            res[label] += 1
                            if label.upper() != "BT":
                                total += 1
                        
                except Exception as e:
                    print(f"{e}")
                    continue
            res["告警总数"] = total
            error = 0
        
            for image in os.listdir(error_path):
                try:
                    if not image.endswith(".jpg"):
                        continue
                    station = image.split("_")[1]
                    label = image.split("_")[2].upper()
                    temp = image.replace("-", "_")
                    
                    if station == row[0] and date in temp and prefix in temp:
                        if label.upper() != "BT":
                            error += 1
                except Exception as e:
                    print(e)
                    continue
            if error > int(res["告警总数"]):
                error = int(res["告警总数"])
        
            
            res["正确告警数"] = error
    
            res["BT"] = f"[{res["BT"]}]"

            temp_list.append(list(res.values()))
   
        try:
            locale.setlocale(locale.LC_COLLATE, 'zh_CN.UTF-8')
        except locale.Error:
        # 如果设置失败，尝试其他中文区域设置
            try:
                locale.setlocale(locale.LC_COLLATE, 'zh_CN')
            except locale.Error:
                # 如果还是失败，使用系统默认
                pass
        temp_list = pd.DataFrame(temp_list, columns=cols)
                # 使用 locale.strxfrm 进行中文排序
        temp_list = temp_list.sort_values(
            by=f'{version}',
            axis=0,
            key=lambda col: col.map(lambda x: locale.strxfrm(str(x)) if pd.notnull(x) else '')
        )
        results.append(cols)
        results.extend(temp_list.to_numpy().tolist())
            

    # 设置中文区域设置以支持中文排序

    results = pd.DataFrame(results, columns=None)
    results.to_csv(os.path.join(result_path, f"{date}-{version}.csv"), index=False, header=False)
    
    
if __name__ == "__main__":
    days = [
        "07_05",
        "07_04"
    ]
    version = "dev.019-Beta"
    for day in days:
        do_task(day=day, version=version)