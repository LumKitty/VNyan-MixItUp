# MixItUp plugin for VNyan

Allows you to send chat messages, run custom MixItUpCommands, and retrieve user information from MixItUp via the MixItUp developer API

![image](https://github.com/user-attachments/assets/baf87f57-0813-4c22-a74b-a5258bcd66aa)

## Installation
1. Download from https://github.com/LumKitty/VNyan-MixItUp/releases
2. Copy the contents of the zip file into VNyan\Items\Assemblies (no subfolders)
3. Enable plugins in VNyan: Settings -> Misc -> Additional Settings -> Allow 3rd party mods/plugins  
4. Enable the MixItUp developer API: Services -> Developer API
5. Restart VNyan
6. (Optional) Import the example node graph

## Configuration
The default configuration assumes MixItUp is running on the same PC as VNyan and you are using Twitch.  
Use ```_lum_miu_config``` if you need to change this. (See Available Triggers below)

## Known Issues
Only tested on Twitch. Other platforms should work but I can't test it. If you stream on YouTube or Trovo and would be willing to test, please get in touch with me!  

## Available triggers:
Unless otherwise specified, All functions take a callback trigger name on Text 3, and a Session ID on Value 3. Many also take a Platform ID on Value 2. See the sections below for a description of these.  

```_lum_miu_chat``` - Send a chat message  

Text 1 - Chat message to send  
Text 3 (Optional) - Callback  
Number 1 (Optional) - Send as Streamer (set to 1 to send as streamer, set to 0 or leave unset to send as bot. If you do not have a bot account, this will always send as streamer)  
Number 2 (Optional) - PlatformID  

Callback:  
Value 1 - HTTP result  

```_lum_miu_command``` - Run a MixItUp command  
Text 1 - Name of command in MixItUp (not chat trigger, actual name)  
Text 2 (Optional) - Arguments to the command This will appear in $AllArgs in MixItUp  
Text 3 (Optional) - Callback  
Number 2 (Optional) - PlatformID  

```_lum_miu_getcommands``` - Get the full list of commands MixItUp has available, including your custom ones.  
Text 1 (Optional) - Delimeter to use (defaults to comma if not specified) use a different delimeter such as || if you have command names with commas in them. You are not limited to one character.  
Text 3 - Callback  

Callback:  
Text 1 - Comma (or specified delimiter) separated list of commands available to you  

This trigger also forces the plugin to refresh & cache the full list of commands from MIU. The main miu_command trigger will also do this if you request a command that isn't in the list  
Note: The list is blank on startup. If you have a lot of commands you may wish to call this trigger on startup.  

```_lum_miu_getuser``` - Get information about a given user  
Text 1 - Username  
Text 3 - Callback trigger name (returns userdata)  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - User's custom title  
Text 2 - User in JSON format. See section below on this  
Text 3 - Username  
Number 1 - Time watched in minutes  
Number 2 - User is specifically excluded in MIU  

```_lum_miu_getcurrency``` - Get a the value for a specific currency and specific user  
Text 1 - Username  
Text 2 - Currency name  
Text 3 - Callback  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Currency name  
Number 1 - Currency amount  
Number 2 (Optional) - PlatformID  

```_lum_miu_setcurrency``` - Set a the value for a specific currency and specific user  
Text 1 - Username  
Text 2 - Currency name  
Text 3 (Optional) - Callback  
Number 1 - Currency amount
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Currency name  
Number 1 - Currency amount  
Number 2 (Optional) - PlatformID  

```_lum_miu_addcurrency``` - Add a value to a specific currency for a specific user  
Text 1 - Username  
Text 2 - Currency name  
Text 3 (Optional) - Callback  
Number 1 - Currency amount to amend by (e.g. -10 to spend 10 coins)  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Currency name  
Number 1 - Currency amount  
Number 2 (Optional) - PlatformID  

```_lum_miu_usecurrency``` - Subtract a value from a specific currency for a specific user - if the user has enough currency  
Text 1 - Username  
Text 2 - Currency name  
Text 3 (Optional) - Callback  
Number 1 - Currency amount to amend by (e.g. -10 to spend 10 coins)  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Currency name - if negative then the user had insufficient currency and the subtraction did not take place  
Number 1 - Currency amount  
Number 2 (Optional) - PlatformID  

```_lum_miu_getinventory``` - Get all items in a particular inventory, for a specific user  
Text 1 - Username  
Text 2 - Inventory name  
Text 3 - Callback  
Number 2 (Optional - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Inventory name  
Text 3 - Inventory JSON - convert to dictionary to get <item> <value> pairs  

```_lum_miu_getitem_<INVENTORYNAME>``` - Get the amount of a specific item within the inventory INVENTORYNAME  
Text 1 - Username  
Text 2 - Item name  
Text 3 - Callback  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Item name  
Number 1 - Item amount  

```_lum_miu_setitem_<INVENTORYNAME>``` - Set the amount of a specific item within the inventory INVENTORYNAME  
Text 1 - Username  
Text 2 - Item name  
Text 3 (Optional) - Callback  
Number 1 - New item amount  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Item name  
Text 3 (Optional) - Callback  
Number 1 - New item amount  
Number 2 (Optional) - PlatformID  

```_lum_miu_additem_<INVENTORYNAME>``` - Add to the amount of a specific item within the inventory INVENTORYNAME  
Text 1 - Username  
Text 2 - Item name  
Text 3 (Optional) - Callback  
Number 1 - Amount to adjust item amount by (e.g. -1 to consume an inventory item)  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Item name  
Text 3 (Optional) - Callback  
Number 1 - New item amount  
Number 2 (Optional) - PlatformID  

```_lum_miu_useitem_<INVENTORYNAME>``` - Subtract to the amount of a specific item within the inventory INVENTORYNAME, if the user has sufficient items  
Text 1 - Username  
Text 2 - Item name  
Text 3 (Optional) - Callback  
Number 1 - Amount to adjust item amount by (e.g. -1 to consume an inventory item)  
Number 2 (Optional) - PlatformID  

Callback:  
Text 1 - Username  
Text 2 - Item name  
Text 3 (Optional) - Callback  
Number 1 - New item amount - If negative then it means the user had insufficient items  
Number 2 (Optional) - PlatformID  

```_lum_miu_getstatus``` - Get information about the plugin and MIU  
Callback:  
Text 1 - Plugin version  
Text 2 - MixItUp version  
Text 3 (Optional) - Callback  
Number 1 - HTTP result from MixItUp  
Number 3 - SessionID  

```_lum_miu_config``` - Get or set plugin configuration  
Text 1 - Set Default platform
Text 2 - Set MixItUp API URL (must have trailing /)  
Text 3 (Optional if setting, required if getting) - Callback  

Callback:  
Text 1 - Default platform  
Text 2 - MixItUp API URL  
Text 2 - Error filename (if the plugin crashes, a log goes here)  

```_lum_miu_seterrorfile``` - Change the error file location  
Text 1 - Full path to error file  
Text 2 - Full path to log file  
Text 3 - Callback  

Callback is same as above

Debug function:  
```_lum_miu_error```
This cannot be called from VNyan but if an error occurs it will attempt to call a VNyan trigger
named _lum_miu_error to let you know something is wrong.  
Text 1 - Exception info  
Exception info will also be dumped to the specified error file, which defaults to Lum_MIU_Error.txt in your user temp directory

## Callback triggers
Specify the name of a trigger in Text 3 and once the call to MixItUp is complete, it will call a VNyan trigger with this name with the results of your command.  
SessionID is a number you can pass in on Value 3. It will be included on any callback function, as Value 3, so you can match it to its original call. All triggers that accept a callback on Text 3 also accept a SessionID on Number 3. For most use cases you will probably never need to set or read session IDs

## Platform ID
While the plugin allows you to configure a default platform. This can be overridden by passing in a platform ID number on Value 2. Possible values are:  
0 (or unset) - Use the configured default platform
1 - Twitch
2 - YouTube
3 - Trovo

## PlatformData
Contains JSON compatible with the JSON to dictionary node, with the following keys:  
```username``` - typically in Lower case  
```watchtime``` - in minutes  
```customtitle``` - can be set in MixItUp  
```excluded``` - the "Is specially exluded" option in MixItUp. 0 = not excluded, 1 = excluded  
```notes``` - User notes you may have set in MixItUp  
```platform``` - Twitch, YouTube or Trovo  
```displayname``` - May differ from username, will use whatever case the user prefers  
```avatarlink``` - URL to the user's profile pic  
```roles``` - comma separated list of roles the user has in MIU e.g. moderator, vip etc. Can be split to a TArray and then searched for a specific value  
```subscribertier``` - 0, 1, 2 or 3, for Twitch. No idea if this does anything on YouTube or Trovo  

As always, if you find this useful, consider sending a follow or a raid my way, and if you make millions with it, consider sending me some :D

### https://twitch.tv/LumKitty 
