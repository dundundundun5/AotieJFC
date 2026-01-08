# feature_jfc-tool-package分支
## 算法协议

- segment```json
{"code":200,"data":{"imageWidth":1024,"imageHeight":2048,"defectList":[{"defectType":"T","defectScore":0.962,"topLeft":{"x":1.7185547351837158,"y":1281.633544921875},"bottomRight":{"x":1023.7366333007812,"y":1708.046630859375}}],"centerX":-1}}```

- risk```json
{"code":200,"data":{"imageWidth":9982,"imageHeight":2048,"defectList":[{"defectType":"BT","defectScore":0.2523,"topLeft":{"x":9103.660522460938,"y":662.0606689453125},"bottomRight":{"x":9115.3076171875,"y":709.891357421875},"defectArea":850,"defectValue":113}]}}```

- ocr```json
    {"code":200,"data":{"imageWidth":15187,"imageHeight":2048,"defectList":[{"defectType":"CZ","defectContent":"KZ2","defectKind":"FT","defectScore":0.6742,"topLeft":{"x":1576,"y":1319},"bottomRight":{"x":1699,"y":1382}},{"defectType":"CH","defectContent":"12008","defectKind":"","defectScore":0.6712,"topLeft":{"x":1710,"y":1318},"bottomRight":{"x":1877,"y":1383}}]}}```



解决方案 $\rightarrow$ AotieJFC



## AotieJFC.AlgorithmAcceptanceTool
Windows WinForm应用：算法验收工具，三合一

如果碰到编译无法完成，就删除AssemblyInfo.cs内的所有内容
### `Main`
- `主窗口可以无限多开子窗口实例`
- `Bug1：子窗口实例创建的文件夹在子窗口关闭后仍然会被主窗口占用`
### `共有优化`
- `标记错误优化：还将带标注框的错误短图放入errorWithDrawing文件夹`
- `快捷键匹配：右方向键、下方向键->下一张；上方向键、左方向键->上一张；Enter->标记错误图片`
### `Segment`
- `实时日志优化：目前仅在算法请求完成后才打印日志`
- `循环优化：目前每张图片的请求均设置try-catch，请求失败则打印日志，随后跳转到下张`

### `RiskDetect`
- `实时日志优化：直接显示标签类型，defectValue以及defectScore，无标签则返回[]`
- `实时日志优化：计数检出的样本的个数；计算检出样本的defectScore的数字特征`

### `OCR`
- `实时日志优化：直接显示OCR识别结果，如 XXX_XXX.jpg -> 识别车种_识别车号.jpg`
- `OCR结果显示优化：将框出来的结果裁剪出来放大，并覆盖在原图的左上角和左下角，将OCR识别的结果放在裁剪图片的下方，方便测试者查看`

## AotieJFC.DunAutoUpdateTool
控制台应用：FTP拉取脚本，用于在18.254/软件工具中拉取其他三个工具的zip到本地，解压后删除

## AotieJFC.PickImagesTool
Windows WPF应用：找原图，目前主要用于异常检测告警的原图收集和整理
## AotieJFC.SegmentationTestTool
Windows WPF应用：切割测试客户端，主要用于收集算法切错的短图
