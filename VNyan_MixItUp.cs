using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
// using UnityEngine.UI;

namespace VNyan_MixItUp
{
    public class MixItUp : MonoBehaviour, VNyanInterface.ITriggerHandler
    {
        private string ErrorFile = System.IO.Path.GetTempPath() + "\\Lum_MIU_Error.txt";
        private string[] Platforms = { "Twitch", "Twitch", "YouTube", "Trovo" };
        private const string Version = "0.5-alpha";
        private string miuURL = "http://localhost:8911/api/v2/";
        private static HttpClient client = new HttpClient();
        Dictionary<String, String> miuCommands = new Dictionary<string, string>();

        public void Awake()
        {
            try
            {
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                loadPluginSettings();
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        private void loadPluginSettings()
        {
            // Get settings in dictionary
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings("Lum-MixItUp.cfg");
            if (settings != null)
            {
                // Read string value
                settings.TryGetValue("MixItUpURL", out miuURL);
                settings.TryGetValue("DefaultPlatform", out Platforms[0]);
                settings.TryGetValue("ErrorFile", out ErrorFile);

                // Convert second value to decimal
                //if (settings.TryGetValue("SomeValue2", out string s))
                //{
                //    float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out someValue2);
                //}

            }
        }

        private void OnApplicationQuit()
        {
            // Save settings
            savePluginSettings();
        }

        private void savePluginSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["MixItUpURL"] = miuURL;
            settings["DefaultPlatform"] = Platforms[0];
            settings["ErrorFile"] = ErrorFile;
            // settings["SomeValue2"] = someValue2.ToString(CultureInfo.InvariantCulture); // Make sure to use InvariantCulture to avoid decimal delimeter errors

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings("Lum-MixItUp.cfg", settings);
        }
     static string EscapeJSON(string value)
        {
            const char BACK_SLASH = '\\';
            const char SLASH = '/';
            const char DBL_QUOTE = '"';

            var output = new StringBuilder(value.Length);
            foreach (char c in value)
            {
                switch (c)
                {
                    case SLASH:
                        output.AppendFormat("{0}{1}", BACK_SLASH, SLASH);
                        break;

                    case BACK_SLASH:
                        output.AppendFormat("{0}{0}", BACK_SLASH);
                        break;

                    case DBL_QUOTE:
                        output.AppendFormat("{0}{1}", BACK_SLASH, DBL_QUOTE);
                        break;

                    default:
                        output.Append(c);
                        break;
                }
            }

            return output.ToString();
        }
        async Task updateMiuCommands()
        {
            try
            {
                miuCommands.Clear();
                int skip = 0;
                int count = 0;
                do
                {
                    var GetResult = await client.GetAsync(miuURL + "commands?pagesize=10&skip=" + skip.ToString());
                    string Response = GetResult.Content.ReadAsStringAsync().Result;

                    dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
                    count = Results.Commands.Count;
                    // Console.WriteLine("Count: " + count.ToString());
                    foreach (dynamic Result in Results.Commands)
                    {
                        // Console.WriteLine(Result.ToString());
                        // Console.WriteLine(Result.Name + " : " + Result.ID);
                        miuCommands.Add(Result.Name.ToString().ToLower(), Result.ID.ToString());
                        // Console.WriteLine("Added");
                    }
                    GetResult.Dispose();
                    skip += 10;
                } while (count >= 10);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        async Task httpRequest(string Method, string URL, string Content, string Callback, int SessionID)
        {
            try
            {
                var jsonData = new System.Net.Http.StringContent(Content, Encoding.UTF8, "application/json");
                string Response = "";
                int httpStatus = 0;
                //Console.WriteLine(Method + ": " + URL + " : " + Content);
                switch (Method)
                {
                    case "POST":
                        var PostResult = await client.PostAsync(URL, jsonData);
                        Response = PostResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PostResult.StatusCode);
                        Console.WriteLine(PostResult.ToString());
                        PostResult.Dispose();
                        break;
                    case "PUT":
                        var PutResult = await client.PutAsync(URL, jsonData);
                        Response = PutResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PutResult.StatusCode);
                        PutResult.Dispose();
                        break;
                    case "GET":
                        var GetResult = await client.GetAsync(URL);
                        Response = GetResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)GetResult.StatusCode);
                        GetResult.Dispose();
                        break;
                    case "PATCH":
                        var request = new HttpRequestMessage(new HttpMethod("PATCH"), URL);
                        var PatchResult = await client.SendAsync(request);
                        Response = PatchResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PatchResult.StatusCode);
                        PatchResult.Dispose();
                        break;
                }
                //Console.WriteLine(Response.ToString());
                if (Callback.Length > 0)
                {
                    VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(Callback, httpStatus, 0, SessionID, "", "", "");
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        async Task runMiuCommand(string command, string args, string Callback, string Platform, int SessionID)
        {
            try
            {
                if (!miuCommands.ContainsKey(command))
                {
                    await updateMiuCommands();
                    if (!miuCommands.ContainsKey(command)) { Console.WriteLine("command not found"); return; }
                }

                string Content = "{ \"Platform\": \"" + Platform + "\", \"Arguments\": \"" + args + "\" }";
                httpRequest("POST", miuURL + "commands/" + miuCommands[command], Content, Callback, SessionID);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        void CallVNyan(string TriggerName, int int1, int int2, int int3, string Text1, string Text2, string Text3)
        {
            if (TriggerName.Length > 0)
            {
                VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(TriggerName, int1, int2, int3, Text1, Text2, Text3);
            }
        }

        void ErrorHandler(Exception e)
        {
            System.IO.File.WriteAllText(ErrorFile, e.ToString());
            CallVNyan("_lum_miu_error", 0, 0, 0, e.ToString(), "", "");
        }

        async Task getMiuCommands(string delimiter, string Callback, int SessionID)
        {
            try
            {
                await updateMiuCommands();
                string result = "";
                foreach (string value in miuCommands.Keys)
                {
                    result += delimiter + value;
                }
                result = result.Substring(delimiter.Length);
                CallVNyan(Callback, 0, SessionID, 0, result, "", "");
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        async Task SendMiuChat(string Message, bool SendAsStreamer, string Callback, string Platform, int SessionID)
        {
            try
            {
                string URL = miuURL + "chat/message";
                string Method = "POST";
                string Content = "{ \"Message\": \"" + EscapeJSON(Message) + "\", \"Platform\": \"" + Platform + "\", \"SendAsStreamer\": ";
                if (SendAsStreamer)
                {
                    Content += "true }";
                }
                else
                {
                    Content += "false }";
                }
                httpRequest(Method, URL, Content, Callback, SessionID);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        async Task GetMiuUser(string UserName, string Callback, string Platform, int SessionID)
        {
            try
            {
                string URL = miuURL + "users/" + Platform + "/" + UserName;
                var Result = await client.GetAsync(URL);
                int Excluded;
                string PlatformData;
                string Response = Result.Content.ReadAsStringAsync().Result;
                dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);

                int MinutesWatched = Results.User.OnlineViewingMinutes;
                if ((bool)Results.User.IsSpecialtyExcluded)
                {
                    Excluded = 1;
                }
                else
                {
                    Excluded = 0;
                }

                string CustomTitle = Results.User.CustomTitle;

                switch (Platform.ToLower())
                {
                    case "twitch":
                        dynamic PlatformDataJSON = Results.User.PlatformData["Twitch"];
                        string Roles = "";
                        foreach (string Role in PlatformDataJSON.Roles)
                        {
                            Roles += "," + Role;
                        }
                        Roles = Roles.Substring(1);
                        Console.WriteLine(Roles);
                        PlatformDataJSON.Roles = Roles;
                        // PlatformDataJSON.SubscribeDate = PlatformDataJSON.SubscribeDate.ToString();
                        // PlatformDataJSON.AccountDate = PlatformDataJSON.AccountDate.ToString();
                        // PlatformDataJSON.FollowDate = PlatformDataJSON.FollowDate.ToString();
                        // PlatformDataJSON.SubscriberTier = PlatformDataJSON.SubscriberTier.ToString();
                        PlatformData = PlatformDataJSON.ToString();
                        break;
                    case "youtube":
                        PlatformData = Results.User.PlatformData["Youtube"].ToString();
                        break;
                    case "trovo":
                        PlatformData = Results.User.PlatformData["Trovo"].ToString();
                        break;
                    default:
                        try
                        {
                            PlatformData = Results.User.PlatformData[Platform].ToString();
                        }
                        catch
                        {
                            PlatformData = "Could not find platform '" + Platform + "' Please check case-sensitivity. Raw data: " + Results.User.PlatformData.ToString();
                        }
                        break;
                }
                // Workaround for bug where VNyan doesn't like JSON keys to have uppercase, and carriage returns break monitoring
                PlatformData = PlatformData.Replace("\r\n", "").Replace("\n", "").ToLower();
                // End workaround
                CallVNyan(Callback, MinutesWatched, Excluded, SessionID, CustomTitle, PlatformData, UserName);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        async Task GetStatus(string Callback, string Platform, int SessionID)
        {
            try
            {
                string URL = miuURL + "status/version";
                var Result = await client.GetAsync(URL);
                int httpResult = (int)Result.StatusCode;
                string MiuVersion = Result.Content.ReadAsStringAsync().Result.Replace("\"", "");

                CallVNyan(Callback, httpResult, 0, SessionID, Version, MiuVersion, "");
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        async Task Config(string Platform, string URL, string NewErrorFile, string Callback, int SessionID)
        {
            try
            {
                if (Platform.Length > 0)
                {
                    Platforms[0] = Platform;
                }
                if (URL.Length > 0)
                {
                    miuURL = URL;
                }
                if (NewErrorFile.Length > 0)
                {
                    ErrorFile = NewErrorFile;
                }
                CallVNyan(Callback, 0, 0, SessionID, Platforms[0], miuURL, ErrorFile);
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }

        public void triggerCalled(string name, int int1, int SessionID, int PlatformID, string text1, string text2, string Callback)
        {

            /* string URL = "";
            string Method = "";
            string Content = ""; */
            try
            {
                switch (name)
                {
                    case "_lum_miu_chat":
                        SendMiuChat(text1, (int1 > 0), Callback, Platforms[PlatformID], SessionID);
                        break;
                    case "_lum_miu_command":
                        runMiuCommand(text1.ToLower(), text2, Callback, Platforms[PlatformID], SessionID);
                        break;
                    case "_lum_miu_getcommands":
                        if (text1.Length == 0) { text1 = ","; }
                        getMiuCommands(text1, Callback, SessionID);
                        break;
                    case "_lum_miu_getuser":
                        GetMiuUser(text1, Callback, Platforms[PlatformID], SessionID);
                        break;
                    case "_lum_miu_getstatus":
                        GetStatus(Callback, Platforms[PlatformID], SessionID);
                        break;
                    case "_lum_miu_config":
                        Config(text1, text2, "", Callback, SessionID);
                        break;
                    case "_lum_miu_seterrorfile":
                        Config("", "", text1, Callback, SessionID);
                        break;
                }
            }
            catch (Exception e)
            {
                ErrorHandler(e);
            }
        }
        public void Start() { }
    }
}
