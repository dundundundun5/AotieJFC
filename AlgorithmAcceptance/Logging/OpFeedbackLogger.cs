using System;
using System.Drawing;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace AlgorithmAcceptanceTool.Logging
{
    class OpFeedbackLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;
        private readonly ListView _listView;

        public OpFeedbackLogger(ListView listView, LogLevel minimumLevel)
        {
            //_listView = listView ?? throw new Exception("必须提供操作日志显示控件");
            _minimumLevel = minimumLevel;
        }
        /// <summary>
        /// 只有level相等时才记录日志到控件，
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel level)
        {
            return IsOpLogLevel(level) && _listView != null && _listView.IsHandleCreated && !_listView.IsDisposed;
        }

        public void Log(LogLevel level, string message)
        {
            if (!IsEnabled(level))
                return;
            AppendLog(message, level);
        }

        public void Log(LogLevel level, string message, Exception exception)
        {
            if (!IsEnabled(level))
                return;
            AppendLog(message, level);
        }

        private bool IsOpLogLevel(LogLevel level)
        {
            return level >= _minimumLevel && (level == LogLevel.OpTrace || level == LogLevel.OpDebug || level == LogLevel.OpInfo || level == LogLevel.OpWarn || level == LogLevel.OpError || level == LogLevel.OpFatal);
        }

        private void AppendLog(string log, LogLevel level)
        {
            if (!_listView.InvokeRequired)
            {
                AppendLogItem(log, level);
                return;
            }
            _listView.BeginInvoke((MethodInvoker)delegate
            {
                if (!_listView.IsHandleCreated)
                    return;
                if (_listView.IsDisposed)
                    return;
                AppendLogItem(log, level);
            });
        }

        private void AppendLogItem(string log, LogLevel level)
        {
            ListViewItem item = new ListViewItem($"【{GetLogLevelTag(level)}】")
            {
                ForeColor = GetLogLevelColor(level),
                Font=new Font("微软雅黑",9, FontStyle.Bold),
                UseItemStyleForSubItems =false
            };
            item.SubItems.AddRange(new ListViewSubItem[]
            {
                new ListViewSubItem()
                {
                    Text=DateTime.Now.ToString("HH:mm:ss"),
                    ForeColor=Color.Empty
                },
                new ListViewSubItem()
                {
                    Text=log,
                    ForeColor = Color.Empty
                },
            });

            _listView.BeginUpdate();
            _listView.Items.Add(item);
            _listView.Items[_listView.Items.Count - 1].EnsureVisible();//滚动到最后
            _listView.EndUpdate();
            _listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private string GetLogLevelTag(LogLevel level)
        {
            if(level>= LogLevel.Error)
                return "错误";
            if (level >= LogLevel.Warn)
                return "警告";
            return "信息";
        }

        private Color GetLogLevelColor(LogLevel level)
        {
            if(level>=LogLevel.Error)
                return Color.Red;
            if (level >= LogLevel.Warn)
                return Color.RosyBrown;
            return Color.Empty;
        }
    }
}
