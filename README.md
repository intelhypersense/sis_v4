# Sound in Silence V4 Build Instructions

## PC software
* Install Visual Studio IDE
* Choose menu of File->Open->Project/Solution.
* Choose project file “MidiAndMic1.sln”
* Click “Start” to run the project.

## Intel Edison software

### Install Eclipse IDE
* Download Eclipse for intel: http://iotdk.intel.com/sdk/1.1/iotdk-ide-win.7z
* Extract file from iotdk-ide-win.7z.
* Run devkit-launcher.bat to open Eclipse.

### Import project
* Choose File->Import->Existing Projects into Workspace.
* Choose the directory of source code to import the project.
* Build the project to output bin file.

### Setup Edison
* Download putty: http://the.earth.li/~sgtatham/putty/latest/x86/putty.exe.
* Plug in the serial port of Edison and open the serial port in putty.
* Login Edison with user name: root
* Input command ”configure_edison --enableOneTimeSetup” to setup SSID and password.
* Use WinSCP to send the bin file in Edison “/mnt/yourbinfile”.
* Create script to run your bin file:
```
#!/bin/sh
/mnt/yourbinfile
```
* Set the script to run automatically on startup - http://tektyte.com/docs/docpages/edison-reference/runonstartup.html#direct-method
* Restart Edison to run the bin you output.

### Firmware for actuator / led PCB
* Install IDE “IAR for stm8”
* Open Project file ” SIS.eww”
* Connect the debug port of deivce via ST-LinkV2.
* Click “Download and Debug” to Run the project in device.
