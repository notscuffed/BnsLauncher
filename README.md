# BNS Launcher

Custom Blade & Soul game launcher with ability to pick xml profile and couple other options.


## Features:

   - Multiclient - run multiple games at once
   - Multiple profiles thanks to runtime xml modifying using dll by zeffy
   - Few hidden client settings
  
### Screenshoot

![Alt text](/Screenshots/BnsLauncher.png?raw=true "Bns Launcher")
  
### Profile sample
Cut down sample profile.
```xml
<?xml version="1.0" encoding="utf-8"?>
<patches name="Local" background="green" foreground="white">
  <patch file="release.config2.xml">
    <select-node query="/config">
      <!-- Lobby gate address -->
      <select-node query="./option[@name='lobby-gate-address']">
        <set-value value="192.168.200.130" />
      </select-node>
      
      <!-- Lobby gate port -->
      <select-node query="./option[@name='lobby-gate-port']">
        <set-value value="10900" />
      </select-node>

      <!-- Don't minimize other windows -->
      <select-node query="./option[@name='minimize-window']/@value">
        <set-value value="false" />
      </select-node>
      
      <!-- NP Address -->
      <select-node query="./option[@name='np-address']">
        <set-value value="192.168.200.130" />
      </select-node>
      
      <!-- NP Port -->
      <select-node query="./option[@name='np-port']">
        <set-value value="6600" />
      </select-node>
    </select-node>
  </patch>
</patches>
```

  
## Acknowledgements:
- [bnsmodpolice/**bnspatch**][0.0] (MIT License)
- [bnsmodpolice/**pluginloader**][0.1] (MIT License)
- [unitycontainer/**unity**][0.2] (Apache License 2.0)
- [Fody/**PropertyChanged**][0.3] (MIT License)
- [Newtonsoft/**Json**][0.4] (MIT License)
- [MaterialDesignInXAML/**MaterialDesignInXamlToolkit**][0.5] (MIT License)
- [Caliburn/**Micro**][0.6] (MIT License)
- [sourcechord/**FluentWPF**][0.7] (MIT License)

[0.0]: https://github.com/bnsmodpolice/bnspatch
[0.1]: https://github.com/bnsmodpolice/pluginloader
[0.2]: https://github.com/unitycontainer/unity
[0.3]: https://github.com/Fody/PropertyChanged 
[0.4]: https://github.com/JamesNK/Newtonsoft.Json
[0.5]: https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit
[0.6]: https://github.com/Caliburn-Micro/Caliburn.Micro
[0.7]: https://github.com/sourcechord/FluentWPF