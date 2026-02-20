# Unity-Injector
Light DI Container for Unity

## Installation

There is several options to install this package:
- UPM
- directly in manifest

### Unity Package Manager

Open Unity Package Manager and go to **Add package from git URL...** and paste [https://github.com/ostrzolekpawel/UnityInjector.git?path=Assets/Injector](https://github.com/ostrzolekpawel/UnityInjector.git?path=Assets/Injector)

### Manifest
Add link to package from repository directly to manifest.json

**Example**
```json
{
    "dependencies": {
        // other packages
        // ...
        "com.osirisgames.eventbroker": "https://github.com/ostrzolekpawel/UnityInjector.git?path=Assets/Injector"
    }
}
```