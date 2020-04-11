using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Configuration;
using AlgorithmAcceptance.Logging;
using AlgorithmAcceptance.Models;
using AlgorithmAcceptance.Utils;

namespace AlgorithmAcceptance.Managers
{
    public class RemoteManager
    {
        private static string CarriageNumberServiceEndPoint = ConfigurationManager.AppSettings["CarriageNumberServiceEndPoint"];
        private static string token=string.Empty;
        private ILogger logger;

        private static object tokenObj = new object();
        private RemoteManager()
        {
        }

        private static RemoteManager _instance;
        public static RemoteManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RemoteManager();
                }
                return _instance;
            }
        }

        public void Init()
        {
            logger = LogManager.CreateLogger("RemoteManager");
        }

        #region ocr v2

        /// <summary>
        /// 获取算法分析结果
        /// </summary>
        /// <returns></returns>
        public Tuple<string, byte[]> AnalysisImg(byte[] imgStream)
        {
            try
            {
                var result = HttpClient.Instance.Post<SingletonResponse<AnalysisResultModel>>(
                    $"{CarriageNumberServiceEndPoint}/ocr",
                    new {imgStream});

                return result.Code == 200
                    ? new Tuple<string, byte[]>(string.Empty, result.Data.AnalysisResult)
                    : new Tuple<string, byte[]>(result.ResultMessage, null);
            }
            catch (System.Exception ex)
            {
                return new Tuple<string, byte[]>(ex.Message, null);
            }
        }

        #endregion
    }
}
