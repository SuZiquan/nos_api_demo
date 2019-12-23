using System;
using System.Net;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Web;

namespace nos_csharp_demo
{
    class Program
    {

        static void Main(string[] args)
        {
            string endpoint = "nos endpoint";
            string accessKey = "your accessKey";
            string secretKey = "your secretKey";

            string bucketName = "your bucketname";
            string objectName = "your objectname";
            string filePath = "file path";

            //listBucket(endpoint, accessKey, secretKey);

            //listObject(endpoint, accessKey, secretKey, bucketName);

            putObject(endpoint, accessKey, secretKey, bucketName, objectName, filePath);

            //getObject(endpoint, accessKey, secretKey, bucketName, objectName);
        }

        static void listBucket(string endpoint, string accessKey, string secretKey)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + endpoint + "/");
            request.Method = "GET";
            request.Date = DateTime.Now;
            request.Headers.Add("Authorization", getAuthHeader(accessKey, secretKey, request.Method, "", "", request.Date, new Dictionary<string, string>(), "/"));
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void listObject(string endpoint, string accessKey, string secretKey, string bucketName)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + bucketName + "." + endpoint + "/");
            request.Method = "GET";
            request.Date = DateTime.Now;
            request.Headers.Add("Authorization", getAuthHeader(accessKey, secretKey, request.Method, "", "", request.Date, new Dictionary<string, string>(), "/" + bucketName + "/"));
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }


        static void getObject(string endpoint, string accessKey, string secretKey, string bucketName, string objectName)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + bucketName + "." + endpoint + "/" + objectName);
            request.Method = "GET";
            request.Date = DateTime.Now;

            request.Headers.Add("Authorization", getAuthHeader(accessKey, secretKey, request.Method, "", "", request.Date, new Dictionary<string, string>(), "/" + bucketName + "/" + upperCaseUrlEncode(objectName)));
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        static void putObject(string endpoint, string accessKey, string secretKey, string bucketName, string objectName, string filePath)
        {
            var request = (HttpWebRequest)WebRequest.Create("http://" + bucketName + "." + endpoint + "/" + objectName);
            request.Method = "PUT";
            request.ServicePoint.Expect100Continue = false;
            request.Date = DateTime.Now;

            var contentType = "image/png";
            var contentMD5 = "";


            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    contentMD5 = BitConverter.ToString((md5.ComputeHash(stream))).Replace("-", String.Empty).ToLower();
                }
            }
            request.Headers.Add("Content-MD5", contentMD5);

            request.Headers.Add("Authorization", getAuthHeader(accessKey, secretKey, request.Method, contentMD5, contentType, request.Date, new Dictionary<string, string>(), "/" + bucketName + "/" + upperCaseUrlEncode(objectName)));

            request.ContentType = contentType;

            var data = File.ReadAllBytes(filePath);

            request.ContentLength = data.Length;
            var newStream = request.GetRequestStream();
            newStream.Write(data, 0, data.Length);
            newStream.Close();

            try
            {
                foreach (var key in request.Headers.AllKeys)
                {
                    Console.WriteLine(key + "=" + request.Headers[key] + Environment.NewLine);
                }

                var response = (HttpWebResponse)request.GetResponse();

                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());
                }
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    Console.WriteLine("statusCode: " + response.StatusCode);
                    Console.WriteLine("responseBody: " + reader.ReadToEnd());

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        // 计算签名头部，文档： https://www.163yun.com/help/documents/55485278220111872
        static string getAuthHeader(string accessKey, string secretKey, string httpMethod,
            string contentMD5, string contentType, DateTime date,
            Dictionary<string, string> canonicalizedHeaders, string canonicalizedResource)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(httpMethod).Append("\n")
                    .Append(contentMD5).Append("\n")
                    .Append(contentType).Append("\n")
                    .Append(date.ToUniversalTime().ToString("r")).Append("\n");

            Dictionary<string, string> keyLowCasedAndSortedCanonicalizedHeaders = canonicalizedHeaders.ToDictionary(o => o.Key.ToLower(), p => p.Value)
                .OrderBy(o => o.Key)
                .ToDictionary(o => o.Key, p => p.Value);

            foreach (var entry in keyLowCasedAndSortedCanonicalizedHeaders)
            {
                string canonicalizedValue = entry.Value;
                canonicalizedValue = Regex.Replace(canonicalizedValue, @"\s", "");
                stringBuilder.Append(entry.Key.Trim()).Append(":").Append(canonicalizedValue).Append("\n");
            }

            stringBuilder.Append(canonicalizedResource);
            string stringToSign = stringBuilder.ToString();
            string signature = Convert.ToBase64String(new HMACSHA256(Encoding.ASCII.GetBytes(secretKey)).ComputeHash(Encoding.ASCII.GetBytes(stringToSign)));
            return "NOS " + accessKey + ":" + signature;
        }

        static string upperCaseUrlEncode(string str)
        {
            return new Regex(@"%[a-f0-9]{2}").Replace(WebUtility.UrlEncode(str), m => m.Value.ToUpperInvariant());
        }

    }
}
