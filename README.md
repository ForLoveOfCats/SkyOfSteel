# SkyOfSteel

### Welcome to the GitHub repository of the game SkyOfSteel!

SkyOfSteel is a work in progress game built on the Godot game engine with C#, currently builds with Godot Mono 3.1.1 Stable
The current stable release can be downloaded at [Itch.io](https://forloveofcats.itch.io/skyofsteel "Itch.io link")

Contributions are welcome!


# Table of Contents
- [SkyOfSteel](#skyofsteel)
        - [Welcome to the GitHub repository of the game SkyOfSteel!](#welcome-to-the-github-repository-of-the-game-skyofsteel)
- [Table of Contents](#table-of-contents)
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



## Download
[SkyOfSteel can be downloaded from Itch.io](https://forloveofcats.itch.io/skyofsteel "Itch.io link")



## Newsletter
[Read the weekly Sunday of Steel news letter here](https://skyofsteel.org/Posts "Blog link")



## Videos
[0.1.2 Release Video](https://www.youtube.com/watch?v=D9XTBXHrNhc "0.1.2 release video link")



## Building

### Prerequisites

* A functioning installation of Godot Mono 3.1.1 Stable
  * Decently recent versions of both MSBuild and Nuget must be installed (Windows users make sure
  to install [this](https://www.mono-project.com/download/stable/#download-win) and
  [this](https://www.microsoft.com/en-us/download/details.aspx?id=56119), Ubuntu users make
  sure to add [this PPA](https://www.mono-project.com/download/stable/#download-lin), update,
  then install the mono packages as for Arch/Manjaro users make sure to install `msbuild-stable` from the AUR)


### Performing the Build

* Clone the git repo (or download as zip and extract)
* Cd into the project root and run `nuget restore SkyOfSteel.sln` (This will pull in Newtonsoft.Json and Roslyn)
* Open the Godot editor to the project and let it reimport all the assets then hit "Play" near the upper right corner (play icon)
  * The editor will proceed to build and launch the project



## Gameplay

SkyOfSteel is a sandbox building game set in the sky. Envisioned to be
a game where one can build intricate factories and sprawling supply
chains while engaging in a PVP turf war with your fellow server
inhabitants. Enjoy highly fluid movement while destroying others with
highly powered weaponry. Fight for technological superiority as you
race through a deep tech tree constantly edging out your
opponents. When you tire from the vanilla game, the custom gamemode
and modding support adds even more depth and content.

Currently we are remarkably far along in achieving this goal. However
we are admittedly not there yet. At the moment we have a fully
functional building system, chunk based world save and loading, highly
stable multiplayer, Quake/Source engine style air strafing, automatic
bunny hopping, wall kicking, and a rudimentary yet extremely flexible
system for adding user created gamemodes.



## Modding

SkyOfSteel has a gamemode modding system utilizing [Roslyn](https://github.com/dotnet/roslyn "Roslyn Github Page") to provide runtime
C# compilation. The intention is to allow for fully custom gamemodes to be developed with full access to internal game APIs both
server side *and* client side. In addition there is a full ingame REPL console.

The gamemode API has a concept of game events. Whenever something occurs ingame it is
processed as an "event" which can be filtered and discarded by gamemode scripts programmatically.
This allows for a great deal of flexibility before even touching the normal API.

Read more here: https://github.com/ForLoveOfCats/ModsOfSteel



## Player Input Handling

All player input is routed through the binding system which depending on the input will call
a console command passing in a `float` ranging between 0 and 1 (up to infinity in some cases
such as mouse movement). Key input pass in a 1 for keydown and a 0 for keyup while analog
inputs such as mouse motion pass in a 0 for no movement on the specified axis and a number
greater than 0 for motion on that axis (the number is the amount of motion).

**Note:** This is not restricted to built in movement and player control console commands.
Any function declared in the console REPL can be bound to any key or input.



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

This is an ambitious project with much work left to do and you can help!

Feel free to contribute in the following ways:

* Test and report bugs
  * Much work has already been done and there are doubtless many bugs which remain undiscovered
* Fix reported bugs by getting down and dirty with the source code :)
  * Much work is left to be done and if you feel up to it you can help!
* Contribute art and sound skills
  * The term [Programmer Art](https://en.wikipedia.org/wiki/Programmer_art "Wikipedia page on Programmer Art")
describes the temporary art and sound assets created hastily by those untalented in those fields in
order to continue the development of a game. If you are talented in with modeling, texturing, or sound
design please do contribute!
* Suggest new features
  * Are you an ideas man? You can still help!
* Spread information about SkyOfSteel
  * All projects need good PR :)



## Contact
For questions, comments, or to just discuss please join our
[Discord](https://www.discord.gg/Ag5Yckw "Discord Server Invite Link"),
check out our [Youtube](https://www.youtube.com/channel/UCK3ptxlx1ahtbI8PZa8_Tig "SkyOfSteel Youtube Channel")
and support my work on [Patreon](https://www.patreon.com/ForLoveOfCats "ForLoveOfCats Patreon Page").
Also feel free to follow me on [Twitter](https://twitter.com/ForLoveOfCats "ForLoveOfCats Twitter Page")



## License

SkyOfSteel is licensed under the MIT license and utilizes
[Roslyn](https://github.com/dotnet/roslyn "Roslyn Github Page")
and [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
which are under the Apache-2.0 and the MIT licenses respectively.
