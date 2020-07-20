# BNS Launcher

Custom Blade & Soul game launcher with profile support and more.
Works with live game & private server client.

Source for modified bnspatch/pluginloader for private server client is inside Dlls folder.

## Features:

   - Account manager
   - Auto login - automatically log in to account in game (thanks to Hora from mod police discord)
   - Auto pin - automatically put pin in game (thanks to Hora from mod police discord)
   - Replace datafile.bin/localfile.bin at runtime (thanks to Hora from mod police discord)
   - Multiclient - run multiple clients at once (thanks to [pilao aka. zeffy][0.0])
   - Profiles (thanks to [pilao aka. zeffy][0.0])
   - Completly stops game guard from initializing in private server client

[0.0]: https://github.com/zeffy

## Screenshoot

![Alt text](/Screenshots/BnsLauncher.png?raw=true "Bns Launcher")

## Installation

1. Extract BnsLauncher.zip and put the launcher wherever you want
2. Copy bin/bin64 containing required plugin loader & plugins to your client folder
3. Run the launcher
4. Click open profiles folder
5. Edit profile.xml inside game profile you want to use
6. Change clientpath to your client path
7. Check other xml configs for more settings
8. Add accounts if you want to use autologin feature (make sure it's enabled in loginhelper.xml in your profile folder)

## Additional info

- Profiles reload automatically on any file modification inside Profiles folder
- To create more profiles just duplicate one of them, change name inside profile.xml to change name
- Only profile.xml is required for profile to work you can delete other xmls you don't use
- Clicking play when there's no accounts that match profile to be launched will just start the game without auto login, otherwise you will be asked to select account

## Private server info

- If you get an error while starting the client make sure path to client doesn't contain non ASCII characters (chinese characters, other languages etc.)

## Acknowledgements:
- [bnsmodpolice/**bnspatch**][1.0] (MIT License)
- [bnsmodpolice/**pluginloader**][1.1] (MIT License)
- Hora from mod police discord for binloader.dll & loginhelper.dll
- [unitycontainer/**unity**][1.2] (Apache License 2.0)
- [Fody/**PropertyChanged**][1.3] (MIT License)
- [Newtonsoft/**Json**][1.4] (MIT License)
- [MaterialDesignInXAML/**MaterialDesignInXamlToolkit**][1.5] (MIT License)
- [Caliburn/**Micro**][1.6] (MIT License)
- [sourcechord/**FluentWPF**][1.7] (MIT License)

[1.0]: https://github.com/bnsmodpolice/bnspatch
[1.1]: https://github.com/bnsmodpolice/pluginloader
[1.2]: https://github.com/unitycontainer/unity
[1.3]: https://github.com/Fody/PropertyChanged 
[1.4]: https://github.com/JamesNK/Newtonsoft.Json
[1.5]: https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit
[1.6]: https://github.com/Caliburn-Micro/Caliburn.Micro
[1.7]: https://github.com/sourcechord/FluentWPF
