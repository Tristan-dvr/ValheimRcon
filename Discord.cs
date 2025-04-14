using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ValheimRcon
{
    static class Discord
    {
        internal static string Send(string mssgBody, string userName, string webhook)
        {
            var postParameters = new Dictionary<string, object>
            {
                { "username", userName },
                { "content", mssgBody }
            };

            var webResponse = FormUpload.MultipartFormDataPost(webhook, postParameters);

            var responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            responseReader.Close();
            webResponse.Close();

            responseReader.Dispose();
            webResponse.Dispose();

            return fullResponse;
        }

        internal static string SendFile(
            string mssgBody,
            string filename,
            string fileformat,
            string filepath,
            string userName,
            string webhook)
        {
            // Read file data
            FileStream fs = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            byte[] data = new byte[fs.Length];
            fs.Read(data, 0, data.Length);
            fs.Close();

            // Generate post objects
            var postParameters = new Dictionary<string, object>
            {
                { "filename", filename },
                { "fileformat", fileformat },
                { "file", new FormUpload.FileParameter(data, filename, "application/msexcel") },

                { "username", userName },
                { "content", mssgBody }
            };

            // Create request and receive response
            HttpWebResponse webResponse = FormUpload.MultipartFormDataPost(webhook, postParameters);

            // Process response
            StreamReader responseReader = new StreamReader(webResponse.GetResponseStream());
            string fullResponse = responseReader.ReadToEnd();
            webResponse.Close();

            fs.Dispose();
            webResponse.Dispose();

            //return string with response
            return fullResponse;
        }

        static class FormUpload //formats data as a multi part form to allow for file sharing
        {
            private static readonly Encoding Encoding = Encoding.UTF8;

            public static HttpWebResponse MultipartFormDataPost(string postUrl, Dictionary<string, object> postParameters)
            {
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());

                string contentType = "multipart/form-data; boundary=" + formDataBoundary;

                byte[] formData = GetMultipartFormData(postParameters, formDataBoundary);

                return PostForm(postUrl, contentType, formData);
            }

            private static HttpWebResponse PostForm(string postUrl, string contentType, byte[] formData)
            {
                HttpWebRequest request = WebRequest.Create(postUrl) as HttpWebRequest;

                if (request == null) throw new ArgumentException("request is not a http request");

                // Set up the request properties.
                request.Method = "POST";
                request.ContentType = contentType;
                request.CookieContainer = new CookieContainer();
                request.ContentLength = formData.Length;

                // Send the form data to the request.
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(formData, 0, formData.Length);
                    requestStream.Close();
                }

                return request.GetResponse() as HttpWebResponse;
            }

            private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
            {
                var formDataStream = new MemoryStream();
                bool needsCLRF = false;

                foreach (var param in postParameters)
                {
                    if (needsCLRF)
                        formDataStream.Write(Encoding.GetBytes("\r\n"), 0, Encoding.GetByteCount("\r\n"));

                    needsCLRF = true;

                    if (param.Value is FileParameter)
                    {
                        FileParameter fileToUpload = (FileParameter)param.Value;

                        string header = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n",
                            boundary,
                            param.Key,
                            fileToUpload.FileName ?? param.Key,
                            fileToUpload.ContentType ?? "application/octet-stream");

                        formDataStream.Write(Encoding.GetBytes(header), 0, Encoding.GetByteCount(header));

                        formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                    }
                    else
                    {
                        string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                            boundary,
                            param.Key,
                            param.Value);
                        formDataStream.Write(Encoding.GetBytes(postData), 0, Encoding.GetByteCount(postData));
                    }
                }

                // Add the end of the request.  Start with a newline
                string footer = "\r\n--" + boundary + "--\r\n";
                formDataStream.Write(Encoding.GetBytes(footer), 0, Encoding.GetByteCount(footer));

                // Dump the Stream into a byte[]
                formDataStream.Position = 0;
                byte[] formData = new byte[formDataStream.Length];
                formDataStream.Read(formData, 0, formData.Length);
                formDataStream.Close();

                return formData;
            }

            internal class FileParameter
            {
                public byte[] File;
                public string FileName;
                public string ContentType;

                public FileParameter(byte[] file, string filename, string contenttype)
                {
                    File = file;
                    FileName = filename;
                    ContentType = contenttype;
                }
            }
        }
    }
}
