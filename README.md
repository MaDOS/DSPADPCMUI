# DSPADPCMUI
A DSPADPCM UI wrapper for the Gamecube SDK which also converts the input wav into a dsp convertable 16bit wav automatically. 

Download is available directly from the [bin/Release](https://github.com/MaDOS/DSPADPCMUI/blob/master/bin/Release/) folder. I reccomend downloading the [.zip](https://github.com/MaDOS/DSPADPCMUI/blob/master/bin/Release/Release.zip) file.

This will convert nearly any wav file to a dsp convertable 16 bit 11025 bitrate wav file and then use dspadpcm to convert it into a dsp.

This programm has to be in one directory with dspadpcm.exe, dsptool.dll and soundfile.dll found in the Gamecube SDK. It also needs NAudio.dll which it should ship with.

Browse for an input *.wav file and specify and out *.dsp file then click convert. Done.

Should you encounter any bugs please contact me on github. (https://github.com/MaDOS/DSPADPCMUI)
