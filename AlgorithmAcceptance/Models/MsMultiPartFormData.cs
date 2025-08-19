using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmAcceptanceTool.Models
{
	public class MsMultiPartFormData
	{
		private List<byte> formData;
		public string Boundary = "---------------------------7db1851cd1158";
		private string fieldName = "Content-Disposition: form-data; name=\"{0}\"";
		private string fileContentType = "Content-Type: {0}";
		private string fileField = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"";
		private Encoding encode = Encoding.GetEncoding("UTF-8");
		public MsMultiPartFormData()
		{
			formData = new List<byte>();
		}
		public void AddFormField(string FieldName, string FieldValue)
		{
            string newFieldName = fieldName;
			newFieldName = string.Format(newFieldName, FieldName);
			formData.AddRange(encode.GetBytes("--" + Boundary + "\r\n"));
			formData.AddRange(encode.GetBytes(newFieldName + "\r\n\r\n"));
			formData.AddRange(encode.GetBytes(FieldValue + "\r\n"));
		}
		public void AddFile(string FieldName, string FileName, byte[] FileContent, string ContentType)
		{
            string newFileField = fileField;
            string newFileContentType = fileContentType;
			newFileField = string.Format(newFileField, FieldName, FileName);
			newFileContentType = string.Format(newFileContentType, ContentType);
			formData.AddRange(encode.GetBytes("--" + Boundary + "\r\n"));
			formData.AddRange(encode.GetBytes(newFileField + "\r\n"));
			formData.AddRange(encode.GetBytes(newFileContentType + "\r\n\r\n"));
			formData.AddRange(FileContent);
			formData.AddRange(encode.GetBytes("\r\n"));
		}
		public void AddStreamFile(string FieldName, string FileName, byte[] FileContent)
		{
			AddFile(FieldName, FileName, FileContent, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
		}
		public void PrepareFormData()
		{
			formData.AddRange(encode.GetBytes("--" + Boundary + "--"));
		}
		public List<byte> GetFormData()
		{
			return formData;
		}
	}
}
