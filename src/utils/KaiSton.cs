using EasyHttp.Http;
using Newtonsoft.Json.Linq;
using System;
using HawkNet;

namespace Nine_colored_deer_Sharp.utils
{
    public class KaiSton
    {
        public static string settingsStr = "{\"dev\":{\"model\":\"GoFlip2\",\"imei\":\"123456789012345\",\"type\":999999,\"brand\":\"AlcatelOneTouch\",\"os\":\"KaiOS\",\"version\":\"3.2\",\"ua\":\"Mozilla/5.0 (Mobile; GoFlip2; rv:104.0) Gecko/104.0 Firefox/104.0 KAIOS/3.2\",\"cu\":\"4044O-2BAQUS1-R\",\"mcc\":\"0\",\"mnc\":\"0\"},\"api\":{\"app\":{\"id\":\"CAlTn_6yQsgyJKrr-nCh\",\"name\":\"KaiOS Plus\",\"ver\":\"2.5.4\"},\"server\":{\"url\":\"https://api.kaiostech.com\"},\"ver\":\"3.0\"}}";

        static string authmethod = "api-key";
        static string authkey = "baJ_nea27HqSskijhZlT";
        static JObject jsonSetting = null;

        private static string token { get; set; }

        public static string getKey()
        {
            if (jsonSetting == null)
            {
                jsonSetting = JObject.Parse(settingsStr);
            }
            var ret = "";

            HttpClient httpClient = new HttpClient();

            var datajson = new JObject();
            datajson["brand"] = jsonSetting["dev"]["brand"];
            datajson["device_id"] = jsonSetting["dev"]["imei"];
            datajson["device_type"] = jsonSetting["dev"]["type"];
            datajson["model"] = jsonSetting["dev"]["model"];
            datajson["os"] = jsonSetting["dev"]["os"];
            datajson["os_version"] = jsonSetting["dev"]["version"];
            datajson["reference"] = jsonSetting["dev"]["cu"];

            var path = "/v3.0/applications/" + jsonSetting["api"]["app"]["id"].ToString() + "/tokens";

            httpClient.Request.AddExtraHeader("Authorization", "Key " + authkey);

            string url = jsonSetting["api"]["server"]["url"].ToString() + path;

            httpClient.Request.AddExtraHeader("Kai-API-Version", jsonSetting["api"]["ver"].ToString());


            var reqinfo = "ct=\"wifi\", rt=\"auto\", utc=\"" + GetTimeStamp() + "\", utc_off=\"1\", " + "mcc=\"" + jsonSetting["dev"]["mcc"].ToString() + "\", " + "mnc=\"" + jsonSetting["dev"]["mnc"].ToString() + "\", " + "net_mcc=\"null\", " + "net_mnc=\"null\"";

            httpClient.Request.AddExtraHeader("Kai-Request-Info", reqinfo);

            httpClient.Request.AddExtraHeader("Kai-Device-Info", "imei=\"" + jsonSetting["dev"]["imei"].ToString() + "\", curef = \"" + jsonSetting["dev"]["cu"].ToString() + "\"");
            httpClient.Request.UserAgent = jsonSetting["dev"]["ua"].ToString();
            httpClient.Request.ContentType = "application/json";


            ret = httpClient.Post(url, datajson.ToString(), "application/json").RawText;
            token = ret;
            return ret;

        }

        public static string Request(string method, string path, string data)
        {
            if (jsonSetting == null)
            {
                jsonSetting = JObject.Parse(settingsStr);
            }
            var ret = "";

            HttpClient httpClient = new HttpClient();

            var datajson = new JObject();
            datajson["brand"] = jsonSetting["dev"]["brand"];
            datajson["device_id"] = jsonSetting["dev"]["imei"];
            datajson["device_type"] = jsonSetting["dev"]["type"];
            datajson["model"] = jsonSetting["dev"]["model"];
            datajson["os"] = jsonSetting["dev"]["os"];
            datajson["os_version"] = jsonSetting["dev"]["version"];
            datajson["reference"] = jsonSetting["dev"]["cu"];

            //path = "/v3.0/applications/" + jsonSetting["api"]["app"]["id"].ToString() + "/tokens";

            //httpClient.Request.AddExtraHeader("Authorization", "Key " + authkey);

            string url = jsonSetting["api"]["server"]["url"].ToString() + path;

            httpClient.Request.AddExtraHeader("Kai-API-Version", jsonSetting["api"]["ver"].ToString());


            var reqinfo = "ct=\"wifi\", rt=\"auto\", utc=\"" + GetTimeStamp() + "\", utc_off=\"1\", " + "mcc=\"" + jsonSetting["dev"]["mcc"].ToString() + "\", " + "mnc=\"" + jsonSetting["dev"]["mnc"].ToString() + "\", " + "net_mcc=\"null\", " + "net_mnc=\"null\"";

            httpClient.Request.AddExtraHeader("Kai-Request-Info", reqinfo);

            httpClient.Request.AddExtraHeader("Kai-Device-Info", "imei=\"" + jsonSetting["dev"]["imei"].ToString() + "\", curef=\"" + jsonSetting["dev"]["cu"].ToString() + "\"");
            httpClient.Request.UserAgent = jsonSetting["dev"]["ua"].ToString();
            httpClient.Request.ContentType = "application/json";

            if (!string.IsNullOrWhiteSpace(token))
            {
                var jsontoken = JObject.Parse(token);
                //var hawkinfo = new JObject();
                //hawkinfo["credentials"] = new JObject();
                //hawkinfo["id"] = jsontoken["kid"];
                //hawkinfo["algorithm"] = "sha256";

                //hawkinfo["key"] = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsontoken["mac_key"].ToString()));
                ////hawkinfo["payload"] = data;
                //Types.Credentials credentials = new Types.Credentials(jsontoken["kid"].ToString(), jsontoken["mac_key"].ToString(), Types.Algo.SHA256);

                //Types.HttpMethod httpMethod = Types.HttpMethod.POST;
                //if (method == "PUT")
                //{
                //    httpMethod = Types.HttpMethod.PUT;
                //}
                //else if (method == "GET")
                //{
                //    httpMethod = Types.HttpMethod.GET;
                //}
                //ClientOptions options = new ClientOptions(credentials, new Instant().PlusTicks(DateTime.Now.Ticks), null, new Microsoft.FSharp.Core.FSharpOption<string>("application/json"), null, new Microsoft.FSharp.Core.FSharpOption<byte[]>(System.Text.Encoding.UTF8.GetBytes(data)), null, null, null, null);

                //var auth = Logibit.Hawk.Client.header(new Uri(url), httpMethod, options);
                HawkCredential hawkCredential = new HawkCredential();
                hawkCredential.Key = jsontoken["mac_key"].ToString();
                hawkCredential.Algorithm = "sha256";
                hawkCredential.Id = jsontoken["kid"].ToString();


                var auth = HawkNet.Hawk.GetAuthorizationHeader(new Uri(url).Host, method, new Uri(url), hawkCredential);

                httpClient.Request.AddExtraHeader("Authorization", "Hawk " + auth);

            }
            if (method == "POST")
            {
                ret = httpClient.Post(url, datajson.ToString(), "application/json").RawText;


            }
            else if (method == "GET")
            {

                ret = httpClient.Get(url, "application/json").RawText;

            }
            return ret;
        }

        /// <summary>
        /// 获得13位的时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            System.DateTime time = System.DateTime.Now;
            long ts = ConvertDateTimeToInt(time);
            return ts.ToString();
        } /// <summary>  
          /// 将c# DateTime时间格式转换为Unix时间戳格式  
          /// </summary>  
          /// <param name="time">时间</param>  
          /// <returns>long</returns>  
        private static long ConvertDateTimeToInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0, 0));
            long t = (time.Ticks - startTime.Ticks) / 10000;   //除10000调整为13位      
            return t;
        }
    }
}
