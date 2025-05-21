# vrblocks
UNT Senior Capstone Project. VR Block Programming video game.

**The Garbanzo Boys**

- Tyler Braun
- Malcolm Case
- Dawson Finklea
- Johnathon Lawton
- Kevin Pham


# Project Outline

This project is intended as an educational tool to teach fundamental programming skills through the medium of Virtual Reality.

The project takes inspiration from existing educational tools like Blockly and Scratch, while extending the metaphor of block-based
programming to the third dimension.

## Principals

The project should be progressive, introducing simple constructs and functionality, and using them to build grandually more complex algorithms to achieve specific goals.

The project should be relevant to computer science. It may introduce concepts using toy examples, but ultimately it should
teach practical and realistic examples that can be applied to real programs.

The project should be fun. Programming is a complex, and often extremely dry subject. Introducing the concept to children and
adult beginners in a way that is engaging, is perhaps a never-ending endeavor for computer science educators. This project
should employ the techniques of game design to create real motivation to solve the given problems.
Progression systems, achievements, unlockables, and easter eggs are all viable mechanisms to engage players.

## Documentation

Documentation for the scripts and assets in this project are available in the [Wiki](https://github.com/reckoncrafter/vrblocks/wiki).

## Installation

This project is currently built for Oculus/Meta Quest headsets, and tested on Oculus/Meta Quest 2 headsets. The APK be downloaded from the [Releases](https://github.com/reckoncrafter/vrblocks/releases) page.

The project is not available on the Meta Quest store, and must be sideloaded.

To allow sideloading on Meta headsets, Developer Mode must be enabled. Instructions for enabling developer mode can be found [here](https://developers.meta.com/horizon/documentation/native/android/mobile-device-setup/)

### Android Debug Bridge

If you don't already have Android SDK Platform Tools installed, they can be downloaded [here](https://developer.android.com/tools/releases/platform-tools)

First connect your device to `adb`. Make sure the ADB daemon is started by running.
```
$ adb devices
```

When plugging in your headset over USB, your device should prompt you to allow USB debugging.
Once USB debugging is allowed, running `$ adb devices` should now show your device's serial number and name.

To install the project, run:
```
$ adb install vrblocks_android_oculus.apk
```
### SideQuest

SideQuest is a tool for installing applications for the Oculus/Meta Quest that aren't available in the Meta Quest Store.

It can be installed [here](https://sidequestvr.com/setup-howto).

After downloading the APK for the project, open SideQuest, and plug in your headset. You may be prompted to allow USB debugging.

Click on the icon labeled "Install APK from file on computer" and select the APK.

