using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.VisualBasic.Logging;

namespace AlgorithmAcceptanceTool
{
    class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		
		[STAThread]
		public static void Main()
		{
            ProcessorMessage("FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F5 FA 07 34 23 17 10 00 00 00 11 15 19 15 13 10 18 11 13 3A 10 18 28 01 01 30 0C F6");

		}
		
		private static  string _lastRfidText = string.Empty;

        private static  void ProcessorMessage(string text)
        {
            Console.WriteLine($"raw:{text}");
          
            var content = _lastRfidText + " " + text;

            var messages = content.Split("FA").ToList();

            var index = 0;
            
            foreach (var message in messages)
            {
                if (string.IsNullOrWhiteSpace(message) && index == 0)
                {
                    continue;
                }

                index++;

                var rfidText = "FA " + message.Trim();
                if (rfidText.Split(" ").Length < 22)
                {
                    _lastRfidText = rfidText;
                    continue;

                }
                else
                {
                    _lastRfidText = string.Empty;
                }

                 ProcessorRfidContent(rfidText);


            }

            
        }

        private static  List<string> _currentTrainNumber = new List<string>();
        private static  List<string> _currentTrainNumberWithoutIndex = new List<string>();

        private static  readonly bool _trainNumberEnabled = false;
        private static  string _lastText = string.Empty;
        private static  DateTime _lastTime = DateTime.MinValue;
        private static  string _lastTrainRecordIndicator = string.Empty;
        private static  int _currentIndex = 1;
        public static byte[] StringToByteArray(string strHexValue)
        {
            string[] strAryHex = strHexValue.Split(' ');
            byte[] btAryHex = new byte[strAryHex.Length];

            try
            {
                int nIndex = 0;
                foreach (string strTemp in strAryHex)
                {
                    btAryHex[nIndex] = Convert.ToByte(strTemp, 16);
                    nIndex++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return btAryHex;
        }

        public static string ByteArrayToHexString(byte[] data)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            foreach (byte b in data)
            {
                sb.Append(Convert.ToString(b, 16).PadLeft(2, '0').PadRight(3, ' '));
            }

            return sb.ToString().ToUpper();
        }
        private static  bool ValidateTrainNumber(string text)
        {

            var length = text.Length;
            length = length > 6 ? 6 : length;
            text = text.Substring(0, length);

            text = text.Replace(" ", string.Empty);
            if (text.Length < 4)
            {
                return false;
            }

            var digits = GetDigitsFromString(text);

            if (string.IsNullOrEmpty(digits) || digits.Length != text.Length)
            {
                return false;
            }

            return true;
        }
        private static  string GetDigitsFromString(string input)
        {

            // 使用正则表达式匹配字符串开头的所有连续数字
            Match match = Regex.Match(input, @"^\d+");

            if (match.Success)
            {
                return match.Value;
              
            }

            return string.Empty;
        }
        private static  bool ValidateTrainMode(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (char c in text)
            {
                if (!char.IsLetterOrDigit(c) && _allowedTrainModeCharacters.All(a => a != c))
                    return false;
            }
            return true;
        }
        private static  readonly List<char> _allowedTrainModeCharacters = new List<char>() { ' ', '-', '*' };
        private static  void ProcessorRfidContent(string text)
        {
            try
            {

                Console.WriteLine(text);

               
                var items = text.Split(" ");

                if (items.Length < 22 || items[0] != "FA" || items[1] != "07")
                {
                    throw new Exception("Invalid rfid information");
                }

                var raw = text.Substring(0, 65);
                if (raw == _lastText)
                {
                    Console.WriteLine("重复消息");
                    return;
                }

                _lastText = raw;


                var flag = Convert.ToInt32(items[2], 16);
                if (flag < 33)
                {
                    Console.WriteLine("无效消息");
                    return;
                }

                if (items[2] == "26" || _lastTime.AddMinutes(1) < DateTime.Now)
                {
                    //duplicated 26 
                    if (items[2] == "26" && _lastTime.AddSeconds(20) > DateTime.Now)
                    {
                        return;
                    }
                   
                   

                    if (items[2] == "26")
                    {
                        _currentIndex = 0;
                    }
                    else
                    {

                        _currentIndex = 1;
                      
                    }

                    _currentTrainNumber = new List<string>();
                    _currentTrainNumberWithoutIndex = new List<string>();
                    _lastTime = DateTime.Now;
                }

              
                _lastTime = DateTime.Now;
                var dataItems = items.Skip(2).Take(20).ToArray();

                for (int i = 0; i < dataItems.Length; i++)
                {
                    var value = Convert.ToInt32(dataItems[i], 16);
                    value += 32;
                    dataItems[i] = value.ToString("X");
                }

                var buffer = StringToByteArray(string.Join(" ", dataItems));
                var result = Encoding.ASCII.GetString(buffer);
                Console.WriteLine($"index:{_currentIndex}, result:{result}");

                if (result.Length <= 7)
                {
                    Console.WriteLine($"index:{_currentIndex}, invalid rfid data");
                    return;
                }


              


                var trainType = result.Substring(0, 1);

                var trainMode = result.Substring(1, 6).Trim();
                trainMode = trainMode.Replace("*", string.Empty);

                if (!ValidateTrainMode(trainMode) && _currentIndex != 0)
                {
                    Console.WriteLine($"index:{_currentIndex}, invalid train mode");
                    return;
                }

                if (_currentIndex != 0 && !ValidateTrainNumber(result.Substring(7)))
                {
                    return;
                }

                var trainNumber = GetDigitsFromString(result.Substring(7).Trim());

              
                if (trainNumber.Length > 7)
                {
                    trainNumber = trainNumber.Substring(0, 7);
                }

               
               
                var trainNumberData = $"{trainType}-{trainMode}-{trainNumber}";
               
                if (!_currentTrainNumberWithoutIndex.Contains(trainNumberData))
                {
                    _currentTrainNumberWithoutIndex.Add(trainNumberData);
                    _currentIndex++;
                    _currentTrainNumber.Add($"{_currentIndex}: {trainNumberData}");
                    //SaveExternalCarriage(trainNumber, trainMode, _currentIndex, trainType);
                    Console.WriteLine($"车型标志:{trainType}, 车型:{trainMode}, 车号{trainNumber}");

                    
                }
              

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
	}
}
