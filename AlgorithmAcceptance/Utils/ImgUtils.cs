using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptanceTool.Utils
{
    public static class ImgUtils
    {
        public static List<string> GetImgCollection(string path)
        {
            string[] imgarray = Directory.GetFiles(path);
            var result = from imgstring in imgarray
                where imgstring.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                      imgstring.EndsWith("png", StringComparison.OrdinalIgnoreCase) ||
                      imgstring.EndsWith("bmp", StringComparison.OrdinalIgnoreCase)
                select imgstring;
            return result.ToList();
        }
    }
}
