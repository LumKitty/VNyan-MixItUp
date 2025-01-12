using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
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
        private string ErrorFile; // = System.IO.Path.GetTempPath() + "\\Lum_MIU_Error.txt";
        private string LogFile;   // = System.IO.Path.GetTempPath() + "\\Lum_MIU_Log.txt";
        private string[] Platforms   = { "Twitch", "Twitch", "YouTube", "Trovo" }; // [0] is the user-selectable default platform, 1-3 are fixed, Twitch is default, hence the double
        private const string Version = "0.6-beta";
        private string miuURL;    // = "http://localhost:8911/api/v2/";
        private static HttpClient client = new HttpClient();
        private Dictionary<String, String> miuCommands    = new Dictionary<string, string>();
        private Dictionary<String, String> miuUsers       = new Dictionary<string, string>();
        private Dictionary<String, String> miuCurrencies  = new Dictionary<string, string>();
        private Dictionary<String, String> miuInventories = new Dictionary<string, string>();
        private Dictionary<String, Dictionary<String, String> > miuItems       = new Dictionary<string, Dictionary<String, String>>();
        /*                 InventoryID
         *                                    Name    ItemID
         */
        

        private void Log(string message) {
            if (LogFile.ToString().Length > 0) {
                System.IO.File.AppendAllText(LogFile, message + "\r\n");
            }
        }
        public void Awake() {
            try {
                
                VNyanInterface.VNyanInterface.VNyanTrigger.registerTriggerListener(this);
                loadPluginSettings();
                System.IO.File.WriteAllText(LogFile, "Started VNyan-MixItUp v"+Version+"\r\n");
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        private void loadPluginSettings() {
            // Get settings in dictionary
            Dictionary<string, string> settings = VNyanInterface.VNyanInterface.VNyanSettings.loadSettings("Lum-MixItUp.cfg");
            if (settings != null) {
                // Read string value
                string temp_MiuURL;
                string temp_Platform;
                string temp_ErrorFile;
                string temp_LogFile;
                settings.TryGetValue("MixItUpURL", out temp_MiuURL);
                settings.TryGetValue("DefaultPlatform", out temp_Platform);
                settings.TryGetValue("ErrorFile", out temp_ErrorFile);
                settings.TryGetValue("LogFile", out temp_LogFile);
                if (temp_MiuURL != null) { 
                    miuURL = temp_MiuURL; 
                } else {
                    miuURL = "http://localhost:8911/api/v2/";
                }
                if (temp_Platform != null) { 
                    Platforms[0] = temp_Platform; 
                } else {
                    Platforms[0] = "Twitch";
                }
                if (temp_ErrorFile != null) {
                    ErrorFile = temp_ErrorFile;
                } else {
                    ErrorFile = System.IO.Path.GetTempPath() + "\\Lum_MIU_Error.txt";
                }
                if (temp_LogFile != null) { 
                    LogFile = temp_LogFile;
                }
                else {
                    LogFile = System.IO.Path.GetTempPath() + "\\Lum_MIU_Log.txt";
                }

        // Convert second value to decimal
        //if (settings.TryGetValue("SomeValue2", out string s))
        //{
        //    float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out someValue2);
        //}

            }
        }
        private void OnApplicationQuit() {
            // Save settings
            savePluginSettings();
        }
        private void savePluginSettings() {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings["MixItUpURL"] = miuURL;
            settings["DefaultPlatform"] = Platforms[0];
            settings["ErrorFile"] = ErrorFile;
            settings["LogFile"] = LogFile;
            // settings["SomeValue2"] = someValue2.ToString(CultureInfo.InvariantCulture); // Make sure to use InvariantCulture to avoid decimal delimeter errors

            VNyanInterface.VNyanInterface.VNyanSettings.saveSettings("Lum-MixItUp.cfg", settings);
        }
        static string EscapeJSON(string value) {
            const char BACK_SLASH = '\\';
            const char SLASH = '/';
            const char DBL_QUOTE = '"';

            var output = new StringBuilder(value.Length);
            foreach (char c in value) {
                switch (c) {
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
        } // TODO: Get rid of this (used in SendMiuChat)
        async Task updateMiuCommands() {
            try {
                miuCommands.Clear();
                int skip = 0;
                int count = 0;
                do {
                    var GetResult = await client.GetAsync(miuURL + "commands?pagesize=10&skip=" + skip.ToString());
                    string Response = GetResult.Content.ReadAsStringAsync().Result;

                    dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
                    count = Results.Commands.Count;
                    // Console.WriteLine("Count: " + count.ToString());
                    foreach (dynamic Result in Results.Commands) {
                        // Console.WriteLine(Result.ToString());
                        // Console.WriteLine(Result.Name + " : " + Result.ID);
                        miuCommands.Add(Result.Name.ToString().ToLower(), Result.ID.ToString());
                        // Console.WriteLine("Added");
                    }
                    GetResult.Dispose();
                    skip += 10;
                } while (count >= 10);
            }
            catch (Exception e) {
                ErrorHandler(e);
            }
        }
        async Task httpRequest(string Method, string URL, string Content, string Callback, int SessionID) {
            try {
                var jsonData = new StringContent(Content, Encoding.ASCII);
                jsonData.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                string Response="";
                int httpStatus = 0;
                
                switch (Method) {
                    case "POST":
                        Log("POST: " + URL + " : " + Content);
                        var PostResult = await client.PostAsync(URL, jsonData);
                        Response = PostResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PostResult.StatusCode);
                        Console.WriteLine(PostResult.ToString());
                        PostResult.Dispose();
                        break;
                    case "PUT":
                        Log("PUT: " + URL + " : " + Content);
                        var PutResult = await client.PutAsync(URL, jsonData);
                        Response = PutResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PutResult.StatusCode);
                        PutResult.Dispose();
                        break;
                    case "GET":
                        Log("GET: " + URL + " : " + Content);
                        var GetResult = await client.GetAsync(URL);
                        Response = GetResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)GetResult.StatusCode);
                        GetResult.Dispose();
                        break;
                    case "PATCH":
                        Log("PATCH: " + URL + " : " + Content);
                        var request = new HttpRequestMessage(new HttpMethod("PATCH"), URL);
                        request.Content = jsonData;
                        var PatchResult = await client.SendAsync(request);
                        Response = PatchResult.Content.ReadAsStringAsync().Result;
                        httpStatus = ((int)PatchResult.StatusCode);
                        PatchResult.Dispose();
                        break;
                }
                //Console.WriteLine(Response.ToString());
                if (Callback.Length > 0) {
                    CallVNyan(Callback, httpStatus, 0, SessionID, "", "", "");
                }
            }
            catch (Exception e) {
                ErrorHandler(e);
            }
        }
        async Task runMiuCommand(string command, string args, string Callback, string Platform, int SessionID) {
            try {
                if (!miuCommands.ContainsKey(command)) {
                    await updateMiuCommands();
                    if (!miuCommands.ContainsKey(command)) { Console.WriteLine("command not found"); return; }
                }

                string Content = "{ \"Platform\": \"" + Platform + "\", \"Arguments\": \"" + args + "\" }";
                httpRequest("POST", miuURL + "commands/" + miuCommands[command], Content, Callback, SessionID);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        void CallVNyan(string TriggerName, int int1, int int2, int int3, string Text1, string Text2, string Text3) {
            if (TriggerName.Length > 0) {
                Log("Calling " + TriggerName + " with " + int1.ToString() + ", " + int2.ToString() + ", " + int3.ToString() + ", " + Text1 + ", " + Text2 + ", " + Text3);
                VNyanInterface.VNyanInterface.VNyanTrigger.callTrigger(TriggerName, int1, int2, int3, Text1, Text2, Text3);
            }  else {
                Log("Invalid trigger name");
            }
        }
        void ErrorHandler(Exception e) {
            System.IO.File.WriteAllText(ErrorFile, e.ToString());
            CallVNyan("_lum_miu_error", 0, 0, 0, e.ToString(), "", "");
        }
        async Task getMiuCommands(string delimiter, string Callback, int SessionID) {
            try {
                await updateMiuCommands();
                string result = "";
                foreach (string value in miuCommands.Keys)
                {
                    result += delimiter + value;
                }
                result = result.Substring(delimiter.Length);
                CallVNyan(Callback, 0, SessionID, 0, result, "", "");
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        async Task SendMiuChat(string Message, bool SendAsStreamer, string Callback, string Platform, int SessionID) {
            try {
                string URL = miuURL + "chat/message";
                string Method = "POST";
                string Content = "{ \"Message\": \"" + EscapeJSON(Message) + "\", \"Platform\": \"" + Platform + "\", \"SendAsStreamer\": "; // TODO: Convert to newtonsoft JSON
                if (SendAsStreamer)
                {
                    Content += "true }";
                }
                else
                {
                    Content += "false }";
                }
                httpRequest(Method, URL, Content, Callback, SessionID);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        async Task ClearMiuChat(string Callback, int SessionID) {
            string URL = miuURL + "chat/clear";
            httpRequest("POST", URL, "", Callback, SessionID);
        }
        dynamic GetMiuUserDetails(string UserName, string Platform) {
            string URL = miuURL + "users/" + Platform + "/" + UserName;
            Task<HttpResponseMessage> APICall = client.GetAsync(URL);
            APICall.Wait();
            string Response = APICall.Result.Content.ReadAsStringAsync().Result;
            dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
            return Results;
        }
        string GetMiuUserID(string UserName, string Platform) {
            string fullUserName = Platform + UserName;
            if (miuUsers.ContainsKey(fullUserName)) {
                return miuUsers[fullUserName];
            } else {
                string ID = GetMiuUserDetails(UserName, Platform).User.ID;
                miuUsers.Add(fullUserName, ID);
                return ID;
            }
        }
        async Task GetMiuUser(string UserName, string Callback, string Platform, int SessionID) {
            try {
                dynamic Results;
                string fullUserName = Platform + UserName;
                Log("Searching Cache");
                if (miuUsers.ContainsKey(fullUserName)) {
                    Log("Using cache");
                    string URL = miuURL + "users/" + miuUsers[fullUserName];
                    var Result = await client.GetAsync(URL);
                    string Response = Result.Content.ReadAsStringAsync().Result;
                    Results = JsonConvert.DeserializeObject<dynamic>(Response);
                } else {
                    Log("No cache, calling GetMiuUserDetails");
                    Results = GetMiuUserDetails(UserName, Platform);
                    Log(Results.ToString());
                    miuUsers.Add(fullUserName, Results.User.ID.ToString());
                }
                int Excluded;
                string Roles = "";
                string CustomTitle = Results.User.CustomTitle;

                int MinutesWatched = Results.User.OnlineViewingMinutes;
                if ((bool)Results.User.IsSpecialtyExcluded) {
                    Excluded = 1;
                } else {
                    Excluded = 0;
                }
                foreach (string Role in Results.User.PlatformData[Platform].Roles) {
                    Roles += "," + Role;
                }
                Roles = Roles.Substring(1);

                JObject VNyanResult = new JObject(
                    new JProperty("username", UserName),
                    new JProperty("watchtime", Results.User.OnlineViewingMinutes.ToString()),
                    new JProperty("customtitle", Results.User.CustomTitle),
                    new JProperty("excluded", Excluded.ToString()),
                    new JProperty("notes", Results.User.Notes),
                    new JProperty("platform", Results.User.PlatformData[Platform].Platform),
                    new JProperty("displayname", Results.User.PlatformData[Platform].DisplayName),
                    new JProperty("avatarlink", Results.User.PlatformData[Platform].AvatarLink), 
                    new JProperty("roles", Roles),
                    new JProperty("subscribertier", Results.User.PlatformData[Platform].SubscriberTier.ToString() )
                );
                Log(VNyanResult.ToString());

                CallVNyan(Callback, MinutesWatched, Excluded, SessionID, CustomTitle, VNyanResult.ToString(), UserName);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        async Task GetStatus(string Callback, string Platform, int SessionID) {
            try {
                string URL = miuURL + "status/version";
                var Result = await client.GetAsync(URL);
                int httpResult = (int)Result.StatusCode;
                string MiuVersion = Result.Content.ReadAsStringAsync().Result.Replace("\"", "");

                CallVNyan(Callback, httpResult, 0, SessionID, Version, MiuVersion, "");
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }
        async Task Config(string Platform, string URL, string NewErrorFile, string NewLogFile, string Callback, int SessionID) {
            try {
                if (Platform.Length > 0)     { Platforms[0] = Platform; }
                if (URL.Length > 0)          { miuURL = URL; }
                if (NewErrorFile.Length > 0) { ErrorFile = NewErrorFile; }
                if (NewLogFile.Length > 0)   { LogFile = NewLogFile; }
                CallVNyan(Callback, 0, 0, SessionID, Platforms[0], miuURL, ErrorFile);
            } catch (Exception e) {
                ErrorHandler(e);
            }
        }

        string GetMiuInventoryItemID(string InventoryName, string ItemName)
        {
            string ItemID = "";
            InventoryName = InventoryName.ToLower();
            ItemName = ItemName.ToLower();
            if (miuInventories.ContainsKey(InventoryName))
            {
                Log("Found Inventory: " + InventoryName);
                Dictionary<string, string> tempItems = miuItems[miuInventories[InventoryName]];
                if (tempItems.ContainsKey(ItemName))
                {
                    Log("Found Item: " + ItemName);
                    ItemID = tempItems[ItemName];
                }
            }
            if (ItemID == "")
            {
                Log("No Inventory cache");
                miuInventories.Clear();
                miuItems.Clear();
                string URL = miuURL + "inventory";
                Task<HttpResponseMessage> APICall = client.GetAsync(URL);
                APICall.Wait();
                string Response = APICall.Result.Content.ReadAsStringAsync().Result;
                Log(Response);
                dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
                string tempInventoryID;
                string tempInventoryName;
                string tempItemName;
                string tempItemID;
                Dictionary<string, string> tempItemList = new Dictionary<string, string>();
                foreach (dynamic result in Results)
                {
                    tempInventoryName = result.Name.ToString().ToLower();
                    tempInventoryID = result.ID.ToString().ToLower();
                    miuInventories.Add(tempInventoryName, tempInventoryID);
                    tempItemList.Clear();
                    foreach (dynamic item in result.Items)
                    {
                        tempItemName = item.Name.ToString().ToLower();
                        tempItemID = item.ID.ToString().ToLower();
                        Log("Adding :" + tempItemName + " ID: " + tempItemID);
                        tempItemList.Add(tempItemName, tempInventoryID + "/" + tempItemID);
                        if (tempItemName == ItemName && tempInventoryName == InventoryName)
                        {
                            ItemID = tempInventoryID + "/" + tempItemID;
                        }
                    }
                    miuItems.Add(tempInventoryID, tempItemList);
                }
            }
            Log("Item ID: " + ItemID);
            return ItemID;
        }

        /* async Task GetMiuInventory(string UserName, string InventoryName, string Callback, string Platform, int SessionID) {
            InventoryName = InventoryName.ToLower();
            string UserID = GetMiuUserID(UserName, Platform);

            string URL = miuURL + "inventory";
            var Result = await client.GetAsync(URL);
            string Response = Result.Content.ReadAsStringAsync().Result;
            dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
            string InventoryID = "";
            foreach (dynamic result in Results) {
                if (result.Name.ToString().ToLower() == InventoryName) {
                    InventoryID = result.ID;
                }
            }
            if (InventoryID.Length > 0) {
                URL = miuURL + "inventory/" + InventoryID + "/" + UserID;
                Result = await client.GetAsync(URL);
                Response = Result.Content.ReadAsStringAsync().Result;
                Results = JsonConvert.DeserializeObject<dynamic>(Response);
                JObject VNyanResult = new JObject();
                foreach (dynamic result in Results) {
                    VNyanResult.Add(result.Name.ToString().ToLower(), result.Amount.ToString());
                }
                CallVNyan(Callback, 0, 0, SessionID, UserName, InventoryName, VNyanResult.ToString());
            } else {
                CallVNyan(Callback, 0, 1, SessionID, UserName, InventoryName, "{}");
            }
        } */

        async Task GetMiuInventoryItemAmount(string UserName, string InventoryName, string ItemName, string Callback, string Platform, int SessionID) {
            string UserID = GetMiuUserID(UserName, Platform);
            string ItemID = GetMiuInventoryItemID(InventoryName, ItemName);

            if (ItemID.Length > 0)
            {
                string URL = miuURL + "inventory/" + ItemID + "/" + UserID;
                Log(URL);
                var Result = await client.GetAsync(URL);
                string Response = Result.Content.ReadAsStringAsync().Result;
                Log(Response);
                CallVNyan(Callback, int.Parse(Response), 0, SessionID, UserName, ItemID, "");
            }
            else
            {
                CallVNyan(Callback, 0, 1, SessionID, UserName, InventoryName, ItemName);
            }
        }

        async Task SetMiuInventoryItemAmount(string UserName, string InventoryName, string ItemName, int Amount, string Callback, string Platform, int SessionID, string Method)
        {
            string UserID = GetMiuUserID(UserName, Platform);
            string ItemID = GetMiuInventoryItemID(InventoryName, ItemName);
            if (ItemID.Length > 0)
            {
                string URL = miuURL + "inventory/" + ItemID + "/" + UserID;
                Log(URL);

                JObject ItemChange = new JObject(
                    new JProperty("Amount", Amount.ToString())
                );

                httpRequest(Method, URL, ItemChange.ToString(), Callback, SessionID);
            }
            else
            {
                CallVNyan(Callback, 0, 1, SessionID, UserName, InventoryName, ItemName);
            }
        }

        string GetMiuCurrencyID(string CurrencyName) {
            CurrencyName = CurrencyName.ToLower();
            if (miuCurrencies.ContainsKey(CurrencyName)) {
                Log("Used currency cache");
                return miuCurrencies[CurrencyName];
            } else {
                Log("No currency cache");
                miuCurrencies.Clear();
                string URL = miuURL + "currency";
                Task<HttpResponseMessage> APICall = client.GetAsync(URL);
                APICall.Wait();
                string Response = APICall.Result.Content.ReadAsStringAsync().Result;
                Log(Response);
                dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
                string CurrencyID = "";
                string tempCurrencyName;
                foreach (dynamic result in Results) {
                    tempCurrencyName = result.Name.ToString().ToLower();
                    miuCurrencies.Add(tempCurrencyName, result.ID.ToString());
                    if (tempCurrencyName == CurrencyName) {
                        CurrencyID = result.ID.ToString();
                    }
                }
                return CurrencyID;
            }
        }
        async Task GetMiuCurrency(string UserName, string CurrencyName, string Callback, string Platform, int SessionID) {
            string UserID = GetMiuUserID(UserName, Platform);
            string CurrencyID = GetMiuCurrencyID(CurrencyName);
            
            if (CurrencyID.Length > 0) {
                string URL = miuURL + "currency/" + CurrencyID + "/" + UserID;
                Log(URL);
                var Result = await client.GetAsync(URL);
                string Response = Result.Content.ReadAsStringAsync().Result;
                Log(Response);
                CallVNyan(Callback, int.Parse(Response), 0, SessionID, UserName, CurrencyName, "");
            } else {
                CallVNyan(Callback, 0, 1, SessionID, UserName, CurrencyName, "");
            }
        }
        async Task SetMiuCurrency(string UserName, string CurrencyName, int Amount, string Callback, string Platform, int SessionID, string Method) {
            string UserID = GetMiuUserID(UserName, Platform);
            string CurrencyID = GetMiuCurrencyID(CurrencyName);
            if (CurrencyID.Length > 0) {
                string URL = miuURL + "currency/" + CurrencyID + "/" + UserID;
                Log(URL);

                JObject CurrencyChange = new JObject(
                    new JProperty("Amount", Amount.ToString())
                );

                httpRequest(Method, URL, CurrencyChange.ToString(), Callback, SessionID);
            } else {
                CallVNyan(Callback, 0, 1, SessionID, UserName, CurrencyName, "");
            }
        }
        async Task GetMiuUsers(string Callback, string Platform, int SessionID, bool All)
        {
            string URL = "";
            if (All) {
                URL = miuURL + "users";
            } else {
                URL = miuURL + "users/active";
            }
            string Result = "";
            Task<HttpResponseMessage> APICall = client.GetAsync(URL);
            APICall.Wait();
            string Response = APICall.Result.Content.ReadAsStringAsync().Result;
            //Log(Response);
            dynamic Results = JsonConvert.DeserializeObject<dynamic>(Response);
            Log("Found " + Results.Users.Count.ToString());
            foreach (dynamic User in Results.Users)
            {
                // Log(User.PlatformData[Platform].ToString());
                Log(User.PlatformData[Platform].DisplayName.ToString().Substring(0, 2));
                Result += "," + User.PlatformData[Platform].DisplayName.ToString();
            }
            Log(Result);
            if (Result.Length > 0)
            {
                CallVNyan(Callback, 0, 0, SessionID, Result.Substring(1), "", "");
            }
            else
            {
                CallVNyan(Callback, 0, 1, SessionID, "", "", "");
            }
        }

        public void triggerCalled(string name, int int1, int SessionID, int PlatformID, string text1, string text2, string Callback) {
            try {
                if (name.Substring(0,9) == "_lum_miu_") {
                    Log("Detected trigger: " + name + " with " + int1.ToString() + ", " + SessionID.ToString() + ", " + PlatformID.ToString() + ", " + text1 + ", " + text2 + ", " + Callback);
                    switch (name.Substring(8)) {
                        case "_chat":
                            SendMiuChat(text1, (int1 > 0), Callback, Platforms[PlatformID], SessionID);
                            break;
                        case "_clearchat":
                            ClearMiuChat(Callback, SessionID);
                            break;
                        case "_command":
                            runMiuCommand(text1.ToLower(), text2, Callback, Platforms[PlatformID], SessionID);
                            break;
                        case "_getcommands":
                            if (text1.Length == 0) { text1 = ","; }
                            getMiuCommands(text1, Callback, SessionID);
                            break;
                        case "_getuser":
                            GetMiuUser(text1, Callback, Platforms[PlatformID], SessionID);
                            break;
                        /*case "_getinventory":
                            GetMiuInventory(text1, text2, Callback, Platforms[PlatformID], SessionID);
                            break;*/
                        case "_getcurrency":
                            GetMiuCurrency(text1, text2, Callback, Platforms[PlatformID], SessionID);
                            break;
                        case "_setcurrency":
                            SetMiuCurrency(text2, text2, int1, Callback, Platforms[PlatformID], SessionID, "PUT");
                            break;
                        case "_addcurrency":
                            SetMiuCurrency(text2, text2, int1, Callback, Platforms[PlatformID], SessionID, "PATCH");
                            break;
                        case "_getstatus":
                            GetStatus(Callback, Platforms[PlatformID], SessionID);
                            break;
                        case "_config":
                            Config(text1, text2, "", "", Callback, SessionID);
                            break;
                        case "_seterrorfile":
                            Config("", "", text1, text2, Callback, SessionID);
                            break;
                        case "_getactiveusers":
                            GetMiuUsers(Callback, Platforms[PlatformID], SessionID, false);
                            break;
                        case "_getallusers":
                            GetMiuUsers(Callback, Platforms[PlatformID], SessionID, true);
                            break;
                        default:
                            if (name.Length > 17) {
                                string InventoryID = name.Substring(16);
                                Log("Inventory Name :" +InventoryID);
                                Log("Function Called :" + name.Substring(0,16));
                                switch (name.Substring(0,16)) {
                                    case "_lum_miu_getinv_":
                                        GetMiuInventoryItemAmount(text1, InventoryID, text2, Callback, Platforms[PlatformID], SessionID);
                                        break;
                                    case "_lum_miu_setinv_":
                                        SetMiuInventoryItemAmount(text1, InventoryID, text2, int1, Callback, Platforms[PlatformID], SessionID, "PUT");
                                        break;
                                    case "_lum_miu_addinv_":
                                        SetMiuInventoryItemAmount(text1, InventoryID, text2, int1, Callback, Platforms[PlatformID], SessionID, "PATCH");
                                        break;

                                }
                            }
                            break;
                        //_getallusers
                        //_getactiveusers
                    }
                }
            }
            catch (Exception e) {
                ErrorHandler(e);
            }
        }
        //public void Start() { }
    }
}
