# MixItUp plugin for VNyan

This is early days. It works but use at your own risk. Three triggers are provided

```_lum_miu_chat``` - Send a chat message  
Value 1 - Send as Streamer (set to 1 to send as streamer, set to 0 or leave unset to send as bot. If you do not have a bot account, this will always send as streamer)  
Text 1 - Chat message to send  
Text 3 - Callback trigger name (returns HTTP result on Value1, e.g. 200 OK)  

```_lum_miu_command``` - Run a MixItUp command  
Text 1 - Name of command in MixItUp (not chat trigger, actual name)  
Text 2 - Arguments to the command  
Text 3 - Callback trigger name (returns HTTP result on Value1, e.g. 200 OK)  

```_lum_miu_getcommands``` - Get the full list of commands MixItUp has available, including your custom ones  
Text 1 - Delimeter to use (defaults to comma if not specified) use a different delimeter such as || if you have command names with commas in them. You are not limited to one character.  
Text 3 - Callback trigger name (returns full list of commands on text1, e.g. Shoutout,Add Quote,Custom Command)  
  
This trigger also forces the plugin to refresh & cache the full list of commands from MIU. The main miu_command trigger will also do this if you request a command that isn't in the list (Note: The list is blank on startup). If you have a lot of commands you may wish to call this trigger on startup.  
Future versions of the plugin may cache the command list between sessions

## Callback triggers
Specify the name of a trigger in Text 3 and once the call to MixItUp is complete, it will call a VNyan trigger with this name with the results of your command.  
As this library grows this will be more useful, e.g. querying users and checking inventory levels etc. will use this mechanism.  
If this doesn't make sense, import the example node graph and it should be a bit more clear!

## Installation
Copy the contents of the zip file into VNyan\Items\Assemblies (no subfolders)  
Enable the MixItUp developer API: Services -> Developer API

## Configuration
The default configuration assumes MixItUp is running on the same PC as VNyan and you are using Twitch.  
After the first run, a config file will be saved in %USERPROFILE%\AppData\LocalLow\Suvidriel\VNyan\Lum-MixItUp.cfg you can change the API URL and platform here

As always, if you find this useful, consider sending a follow or a raid my way, and if you make millions with it, consider sending me some :D

### https://twitch.tv/LumKitty 
