using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.WiFi;
using Windows.Devices.WiFiDirect;
using Windows.Networking.Connectivity;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WIFIManager
{
    class Server
    {
        private string APIUrl = "http://wifiapi02.51y5.net/wifiapi/fa.cmd";
        private string SALT = "LQ9$ne@gH*Jq%KOL";
        internal const string AES_Key = "jh16@`~78vLsvpos";
        internal const string AES_IV = "j#bd0@vp0sj!3jnv";
        private Random random = new Random(DateTime.Now.Millisecond);
        private HttpClient client = new HttpClient();
        private string _mac;

        private string MAC
        {
            get
            {
                if (string.IsNullOrEmpty(_mac))
                {
                    var mac = $"d{random.Next(0,9)}:86:e6:{random.Next(0, 9)}f:a{random.Next(0, 9)}:7c";
                    _mac = mac;
                }
                return _mac;
            }
        }

        public async void WifiPwd()
        {
            //var infos = new List<WiFiInfo>();
            //infos.Add(new WiFiInfo {BSSID = "48:d2:24:5d:e2:e4", SSID = "test" });
            //infos.Add(new WiFiInfo {BSSID = "14:e6:e4:88:44:7c", SSID = "518" });
            //infos.Add(new WiFiInfo {BSSID = "00:87:36:00:ed:80", SSID = "一叶知秋" });
            //infos.Add(new WiFiInfo {BSSID = "38:59:f9:e3:aa:f7", SSID = "居然有WiFi" });
            //infos.Add(new WiFiInfo {BSSID = "0a:a3:c4:c0:cc:1d", SSID = "ATY-PC" });
            //infos.Add(new WiFiInfo {BSSID = "16:e5:43:ba:52:67", SSID = "uuuuuu" });
            //infos.Add(new WiFiInfo {BSSID = "e6:d3:32:06:e1:31", SSID = "TP-LINK_517" });
            //infos.Add(new WiFiInfo { BSSID = "88:53:2e:d0:d1:bd", SSID = "360WiFi-0009" });
            //infos.Add(new WiFiInfo { BSSID = "d8:24:bd:76:60:aa", SSID = "cisco-60A8" });
            //var str = await GetWifiPwd(infos);
            
        }

        public async Task<WifiPwdInfo> GetWifiPwds(List<WiFiInfo> infos) 
        {
            try
            {
                var param = new Dictionary<string, string>()
                {
                    {"och", "wandoujia"},
                    {"ii", ""},
                    {"appid", "0001"},
                    {"pid", "qryapwd:commonswitch"},
                    {"lang", "cn"},
                    {"v", "633"},
                    {"uhid", "a0000000000000000000000000000001"},
                    {"method", "getDeepSecChkSwitch"},
                    {"st", "m"},
                    {"chanid", "guanwang"},
                    {"sign", ""},
                    {"bssid", ""},
                    {"ssid", ""},
                    {"dhid", "4028b2964e01aa00014e1a8641aa4675"},
                    {"mac", MAC},
                };
                foreach (var w in infos)
                {
                    param["bssid"] += w.BSSID + ",";
                    param["ssid"] += w.SSID + ",";
                }
                param["dhid"] = await InitDHID();
                var pd = param.OrderBy(x => x.Key).ToDictionary(o => o.Key, p => p.Value);
                var value = pd.Aggregate("", (current, p) => current + p.Value);
                value += SALT;
                param["sign"] = GetMd5RuntimeString(value);
                var resp = await client.PostAsync(APIUrl, new FormUrlEncodedContent(param));
                var str = await resp.Content.ReadAsStringAsync();
                Debug.WriteLine(str);
                var json = JsonConvert.DeserializeObject<WifiPwdInfo>(str);
                return json;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }
        public async Task<string> InitDHID()
        {
            var param = new Dictionary<string, string>()
            {
                {"capbssid", "d8:86:e6:6f:a8:7c"},
                {"model", "Nexus+4"},
                {"och", "wandoujia"},
                {"appid", "0001"},
                {"mac", "d8:86:e6:6f:a8:7c"},
                {"wkver", "2.9.38"},
                {"lang", "cn"},
                //{"capbssid", "test"},
                {"uhid", ""},
                {"st", "m"},
                {"chanid", "guanwang"},
                {"dhid", ""},
                {"os", "android"},
                {"scrs", "768"},
                {"imei", "355136052333516"},
                {"manuf", "LGE"},
                {"osvercd", "19"},
                {"ii", "355136052391516"},
                {"osver", "5.0.2"},
                {"pid", "initdev:commonswitch"},
                {"misc", "google/occam/mako:4.4.4/KTU84P/1227136:user/release-keys"},
                {"sign", ""},
                {"v", "633"},
                {"sim", ""},
                {"method", "getTouristSwitch"},
                {"scrl", "1184"}
            };
            var pd = param.OrderBy(x => x.Key).ToDictionary(o => o.Key, p => p.Value);
            var value = pd.Aggregate("", (current, p) => current + p.Value);
            value += SALT;
            param["sign"] = GetMd5RuntimeString(value);
            var resp = await client.PostAsync(APIUrl, new FormUrlEncodedContent(param));
            var str =await resp.Content.ReadAsStringAsync(); 
            Debug.WriteLine(str);
            var json = (JObject)JsonConvert.DeserializeObject(str);
            return json["initdev"]["dhid"].ToString();
        }
        public static string GetMd5RuntimeString(string source)
        {
            var hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var buffer = CryptographicBuffer.ConvertStringToBinary(source, BinaryStringEncoding.Utf8);
            var digest = hashAlgorithm.HashData(buffer);
            var str = CryptographicBuffer.EncodeToHexString(digest).ToUpper();
            return str;
        }
    }

    public class WiFiInfo
    {
        public WiFiInfo(string bssid, string ssid)
        {
            BSSID = bssid;
            SSID = ssid;
        }

        public WiFiInfo()
        {
            
        }
        public string SSID { get; set; }
        public string BSSID { get; set; }

        public string Pwd
        {
            get { return _pwd; }
            set
            {
                var buffkey = CryptographicBuffer.ConvertStringToBinary(Server.AES_Key, BinaryStringEncoding.Utf8);
                IBuffer toDecryptBuffer = CryptographicBuffer.DecodeFromHexString(value);
                var buffiv = CryptographicBuffer.ConvertStringToBinary(Server.AES_IV, BinaryStringEncoding.Utf8);
                SymmetricKeyAlgorithmProvider aes = SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithmNames.AesCbc);

                // Create a symmetric key.
                var symetricKey = aes.CreateSymmetricKey(buffkey);
                var buffDecrypted = CryptographicEngine.Decrypt(symetricKey, toDecryptBuffer, buffiv);

                string strDecrypted = CryptographicBuffer.ConvertBinaryToString(BinaryStringEncoding.Utf8, buffDecrypted);
              
                var lenght = int.Parse(strDecrypted.Substring(0,3));
                _pwd = strDecrypted.Substring(3,lenght);
                Debug.WriteLine(_pwd);
            }
        }
        private string _pwd;
        public string Hid { get; set; } 
    }

    public class QryaPwd
    {
        public string RetCd { get; set; }
        public string Qid { get; set; }
        public string SysTime { get; set; }
        public Dictionary<string,WiFiInfo> Psws { get; set; } 
    }

    public class WifiPwdInfo
    {
        public string RetSn { get; set; }
        public string RetCd { get; set; }
        public QryaPwd QryaPwd { get; set; }
        public CommonSwitch CommonSwitch { get; set; }
    }
    
    public class CommonSwitch
    {
        public string RetCd { get; set; }
        public string SwitchFlag { get; set; }
    }
}
