using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;

namespace DotNetCoreWebApi
{
    public class HttpHelper
    {
        public class JsonContent : StringContent
        {
            public JsonContent(object obj) :
               base(JsonConvert.SerializeObject(obj), Encoding.UTF8, "application/json")
            { }
        }
        public static string Post(string Url, string datajson)
        {
            HttpClient httpClient = new HttpClient();//http对象
            //表头参数
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            //token
            //var authenticationHeaderValue = new AuthenticationHeaderValue("bearer", " ");
            //httpClient.DefaultRequestHeaders.Authorization = authenticationHeaderValue;
            //转为链接需要的格式
            HttpContent httpContent = new JsonContent(datajson);
            //请求
            HttpResponseMessage response = httpClient.PostAsync(Url, httpContent).Result;
            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                if (t != null)
                {
                    return t.Result;
                }
            }
            return "";
        }
    }
}