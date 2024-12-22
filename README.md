# MixItUp plugin for VNyan

This is early days. It works but use at your own risk. Three triggers are provided

```_lum_miu_chat``` - Send a chat message  
Text 1 - Chat message to send  
Text 3 - Callback trigger name (returns HTTP result on Value1, e.g. 200 OK)  
Value 1 - Send as Streamer (set to 1 to send as streamer, set to 0 or leave unset to send as bot. If you do not have a bot account, this will always send as streamer)  
Value 2 - PlatformID  
Value 3 - SessionID  

```_lum_miu_command``` - Run a MixItUp command  
Text 1 - Name of command in MixItUp (not chat trigger, actual name)  
Text 2 - Arguments to the command  
Text 3 - Callback trigger name (returns HTTP result on Value1, e.g. 200 OK)  
Value 2 - PlatformID  
Value 3 - SessionID  

```_lum_miu_getcommands``` - Get the full list of commands MixItUp has available, including your custom ones  
Text 1 - Delimeter to use (defaults to comma if not specified) use a different delimeter such as || if you have command names with commas in them. You are not limited to one character.  
Text 3 - Callback trigger name (returns full list of commands on text1, e.g. Shoutout,Add Quote,Custom Command)  
  
This trigger also forces the plugin to refresh & cache the full list of commands from MIU. The main miu_command trigger will also do this if you request a command that isn't in the list (Note: The list is blank on startup). If you have a lot of commands you may wish to call this trigger on startup.  
Future versions of the plugin may cache the command list between sessions

```_lum_miu_getuser``` - Get information about a given user  
Text 1 - Username  
Text 2 - Callback trigger name (returns userdata)  
Value 2 - PlatformID  
Value 3 - SessionID  
Callback:  
Value 1 - Time watched in minutes  
Value 2 - User is specifically excluded in MIU  
Value 3 - SessionID  
Text 1 - User's custom title  
Text 2 - Platform-specific data in JSON format  
Text 3 - Username  
```_lum_miu_getstatus``` - Get information about the plugin and MIU  
Value 3 - SessionID  
Text 2 - Caallback trigger name  
Callback:
Value 1 - HTTP result from MixItUp  
Value 3 - SessionID  
Text 1 - Plugin version  
Text 2 - MixItUp version  
```_lum_miu_config``` - Get or set plugin configuration  
Text 1 - Set Default platform
Text 2 - Set MixItUp API URL (must have trailing /)  
Text 3 - Callback trigger name  
Callback:
Value 2 - SessionID  
Text 1 - Default platform  
Text 2 - MixItUp API URL  
Text 2 - Error filename (if the plugin crashes, full logs go here)  
```_miu_seterrorfile``` - Change the error file location  
Value 3 - SessionID  
Text 1 - Full path to error file  
Text 2 - Callback trigger name  
Callback is same as above

Debug function:  
```_lum_miu_error```
This cannot be called from VNyan but if an error occurs it will attempt to call a VNyan trigger
named _lum_miu_error to let you know something is wrong.  
Text 1 - Exception info

## Callback triggers
Specify the name of a trigger in Text 3 and once the call to MixItUp is complete, it will call a VNyan trigger with this name with the results of your command.  
As this library grows this will be more useful, e.g. querying users and checking inventory levels etc. will use this mechanism.  
SessionID is a number you can pass in on Value 3. It will be included on any callback function so you can match it to its original call. For most use cases you will probably never need to set this.  
If this doesn't make sense, import the example node graph and it should be a bit more clear!

## Installation
Copy the contents of the zip file into VNyan\Items\Assemblies (no subfolders)  
Enable the MixItUp developer API: Services -> Developer API

## Configuration
The default configuration assumes MixItUp is running on the same PC as VNyan and you are using Twitch.  
After the first run, a config file will be saved in %USERPROFILE%\AppData\LocalLow\Suvidriel\VNyan\Lum-MixItUp.cfg you can change the API URL and platform here

As always, if you find this useful, consider sending a follow or a raid my way, and if you make millions with it, consider sending me some :D

### https://twitch.tv/LumKitty 
