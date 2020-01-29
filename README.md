# SkyOfSteel

### Welcome to the GitHub repository of the game SkyOfSteel!

SkyOfSteel is a work in progress game built on the Godot game engine with C#, currently builds with Godot Mono 3.2 Stable
The current stable release can be downloaded at [Itch.io](https://forloveofcats.itch.io/skyofsteel "Itch.io link")

Contributions are welcome!


# Table of Contents
- [SkyOfSteel](#skyofsteel)
        - [Welcome to the GitHub repository of the game SkyOfSteel!](#welcome-to-the-github-repository-of-the-game-skyofsteel)
- [Table of Contents](#table-of-contents)
    - [Why Open Source?](#why-open-source)
    - [Download](#download)
    - [Newsletter](#newsletter)
    - [Videos](#videos)
    - [Building](#building)
        - [Prerequisites](#prerequisites)
        - [Performing the Build](#performing-the-build)
    - [Gameplay](#gameplay)
    - [Modding](#modding)
    - [Player Input Handling](#player-input-handling)
    - [Internal Structure](#internal-structure)
    - [Contributing](#contributing)
    - [Contact](#contact)
    - [License](#license)


## Why Open Source?
[This](./FOSS.md) seperate document explains why SkyOfSteel is open
source and what that means for its development.



## Download
[SkyOfSteel can be downloaded from Itch.io](https://forloveofcats.itch.io/skyofsteel "Itch.io link")



## Newsletter
[Read the weekly Sunday of Steel news letter here](https://skyofsteel.org/Posts "Blog link")



## Videos
[0.1.2 Release Video](https://www.youtube.com/watch?v=D9XTBXHrNhc "0.1.2 release video link")



## Building

### Prerequisites

* A functioning installation of Godot Mono 3.2 Stable
  * Decently recent versions of both MSBuild and Nuget must be installed (Windows users make sure
  to install [this](https://www.mono-project.com/download/stable/#download-win) and
  [this](https://www.microsoft.com/en-us/download/details.aspx?id=56119), Ubuntu users make
  sure to add [this PPA](https://www.mono-project.com/download/stable/#download-lin), update,
  then install the mono packages as for Arch/Manjaro users make sure to install `msbuild-stable` from the AUR)


### Performing the Build

* Clone the git repo (or download as zip and extract)
* Cd into the project root and run `nuget restore SkyOfSteel.sln` (This will pull in `Newtonsoft.Json` and `Optional`)
* Open the Godot editor to the project and let it reimport all the assets then hit "Play" near the upper right corner (play icon)
  * The editor will proceed to build and launch the project



## Gameplay

SkyOfSteel is a sandbox building game set in the sky. Envisioned to be
a game where one can build intricate factories and sprawling supply
chains while engaging in a PVP turf war with your fellow server
inhabitants. Enjoy highly fluid movement while destroying others with
highly powered weaponry. Fight for technological superiority as you
race through a deep tech tree constantly edging out your
opponents.

While SkyOfSteel is remarkably far along in achieving this goal it is
not yet there. At the moment there is a fully functional building
system, chunk based world save and loading, highly stable multiplayer,
Quake/Source engine style air strafing, and automatic bunny
hopping. The current state of SkyOfSteel could be said to be more of a
proof of concept of the further gameplay possibilities in the works.



## Player Input Handling

All player input is routed through the binding system which depending
on the input will call the specified function while passing in a
`float` ranging between 0 and 1 (up to infinity in some cases such as
mouse movement). Key input pass in a 1 for keydown and a 0 for keyup
while analog inputs such as mouse motion pass in a 0 for no movement
on the specified axis and a number greater than 0 for motion on that
axis (the number is the amount of motion).



## Internal Structure

SkyOfSteel is built up of a number of singleton "libraries". Each one of these libraries deals
with a portion of the game's actions and has a number of public static methods in order to be
called from other libraries and classes.

Beyond these "libraries" the project directory is split up into pretty self explanatory folders
which are almost like "modules" in that they (under most conditions) contain everything related
to the folder name (ex: the `UI` folder has alls GUI scenes, script, and textures). Note that
these folders are not self contained but instead all source files are pretty much entwined though
the folder names are a good indication of where to look when trying to find the implementation
of a specific feature.



## Contributing

While SkyOfSteel is not open source because it needs contributions,
there are still ways for you to help!

* Test and report bugs
  * Play the game! If you see a bug, report it!
* Suggest new features
  * Got a great idea? Awesome! Open an issue detailing what you think
    should be added/changed!
* Spread the word about SkyOfSteel
  * All projects need good PR ;)
* Finally if you want you are welcome contribute to the codebase
  * Open an issue detailing what you want to do and lets have a
    discussion. This will prevent work being done on something and
    then not being able to be merged.


## Contact
For questions, comments, or to just discuss please join our
[Discord](https://www.discord.gg/Ag5Yckw "Discord Server Invite Link"),
check out our [Youtube](https://www.youtube.com/channel/UCK3ptxlx1ahtbI8PZa8_Tig "SkyOfSteel Youtube Channel"),
and also feel free to follow me on [Twitter](https://twitter.com/ForLoveOfCats "ForLoveOfCats Twitter Page").



## License

SkyOfSteel is licensed under the MIT license and utilizes
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) and
[Optional](https://github.com/nlkl/Optional) which are both under the
MIT license as well.
