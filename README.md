# MixItUp plugin for VNyan

Provides access to the MixItUp developer API from VNyan via several custom triggers:

## Installation
Copy the contents of the zip file into VNyan\Items\Assemblies (no subfolders)  
Enable the MixItUp developer API: Services -> Developer API  
(Optional) Import the example node graph

## Configuration
The default configuration assumes MixItUp is running on the same PC as VNyan and you are using Twitch.  
Use ```_lum_miu_config``` if you need to change this. (See Available Triggers below)

## Known Issues
Only tested on Twitch. GetUser platform data for other platforms almost certainly will not work. If you stream on YouTube or Trovo and would be willing to help me grab some test platformdata info we can fix this, please get in touch with me!  
PlatformData is forced to lowercase. This only affects DisplayName, but currently can't be fixed

## Available triggers:
Unless otherwise specified, All functions take a callback trigger name on Text 3, and a Session ID on Value 3. Many also take a Platform ID on Value 2. See the sections below for a description of these.  

```_lum_miu_chat``` - Send a chat message  

Text 1 - Chat message to send  
Value 1 - Send as Streamer (set to 1 to send as streamer, set to 0 or leave unset to send as bot. If you do not have a bot account, this will always send as streamer)  
Value 2 - PlatformID  

Callback:  
Value 1 - HTTP result  

```_lum_miu_command``` - Run a MixItUp command  
Text 1 - Name of command in MixItUp (not chat trigger, actual name)  
Text 2 - Arguments to the command  
Text 3 - Callback trigger name (returns HTTP result on Value1, e.g. 200 OK)  
Value 2 - PlatformID  

```_lum_miu_getcommands``` - Get the full list of commands MixItUp has available, including your custom ones  
Text 1 - Delimeter to use (defaults to comma if not specified) use a different delimeter such as || if you have command names with commas in them. You are not limited to one character.  
Text 3 - Callback trigger name (returns full list of commands on text1, e.g. Shoutout,Add Quote,Custom Command)  

Callback:  
Text 1 - Comma (or specified delimiter) separated list of commands available to you  

This trigger also forces the plugin to refresh & cache the full list of commands from MIU. The main miu_command trigger will also do this if you request a command that isn't in the list  
Note: The list is blank on startup. If you have a lot of commands you may wish to call this trigger on startup.  

```_lum_miu_getuser``` - Get information about a given user  
Text 1 - Username  
Text 2 - Callback trigger name (returns userdata)  
Value 2 - PlatformID  

Callback:  
Value 1 - Time watched in minutes  
Value 2 - User is specifically excluded in MIU  
Text 1 - User's custom title  
Text 2 - Platform-specific data in JSON format. See section below on this
Text 3 - Username  

```_lum_miu_getstatus``` - Get information about the plugin and MIU  
Callback:  
Value 1 - HTTP result from MixItUp  
Value 3 - SessionID  
Text 1 - Plugin version  
Text 2 - MixItUp version  

```_lum_miu_config``` - Get or set plugin configuration  
Text 1 - Set Default platform
Text 2 - Set MixItUp API URL (must have trailing /)  

Callback:  
Text 1 - Default platform  
Text 2 - MixItUp API URL  
Text 2 - Error filename (if the plugin crashes, a log goes here)  

```_miu_seterrorfile``` - Change the error file location  
Text 1 - Full path to error file  
Text 2 - Callback trigger name  
Callback is same as above

Debug function:  
```_lum_miu_error```
This cannot be called from VNyan but if an error occurs it will attempt to call a VNyan trigger
named _lum_miu_error to let you know something is wrong.  
Text 1 - Exception info
Exception info will also be dumped to the specified error file, which defaults to Lum_MIU_Error.txt in your user temp directory

## Callback triggers
Specify the name of a trigger in Text 3 and once the call to MixItUp is complete, it will call a VNyan trigger with this name with the results of your command.  
SessionID is a number you can pass in on Value 3. It will be included on any callback function, as Value 3, so you can match it to its original call. For most use cases you will probably never need to set or read this!  

## Platform ID
While the plugin allows you to configure a default platform. This can be overridden by passing in a platform ID number on Value 2. Possible values are:  
0 (or unset) - Use the configured default platform
1 - Twitch
2 - YouTube
3 - Trovo

## PlatformData
The MixItUp get user API returns platform-specific data. For twitch it then makes a few modification to make it readable by VNyan's 'JSON to Dictionary' node. As I do not stream on YouTube or Trovo, I do not have access to this data structure and the resulting JSON is almost certainly incompatible. This will be fixed once I can get data from another VTuber

As always, if you find this useful, consider sending a follow or a raid my way, and if you make millions with it, consider sending me some :D

### https://twitch.tv/LumKitty 
