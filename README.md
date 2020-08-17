# SkyOfSteel

### Welcome to the GitHub repository of the game SkyOfSteel!

SkyOfSteel is an unfinished game built on the Godot game engine with C# (Godot Mono 3.2.2).
Developement has halted. The last stable release can be downloaded at
[Itch.io](https://forloveofcats.itch.io/skyofsteel "Itch.io link")


## Development has halted

Over the last year or so I've had an increasing sense that the general world design
(the tile system) is unworkable for most mechanics I planned to implement.
I don't have a different approach in mind, the multiplayer features continued to sap
development time, and swapping out the world construction mechanics would necessitate
essentially an entire rewrite. As a result of this I've slowly decided to stop working
on this iteration of the game. This is actually the second iteration of the game with
the first having been in Unreal Engine 4. The idea of SkyOfSteel is not dead. The
concept of building an entire world of catwalks, factories, supply lines, defensive
forts, and transportation systems still greatly excites me. I plan on eventually
revisiting these ideas with a clean start, a more cohesive plan, and even more
technical experience. As a final act I've backported several fixes from master
back to the last stable release and rebuilt it with a newer Godot to fix a crash
and I'm releasing that as 0.1.7 which will be the final official release from this
codebase. This sofware is MIT licensed so if you reading this want to you can use
and portion of the code in this repo for any usage as long as you obey the attribution
restriction imposed by the license.


## Building

### Prerequisites

* A functioning installation of Godot Mono 3.2.2
  * Decently recent versions of both MSBuild and Nuget must be installed (Windows users make sure
  to install [this](https://www.mono-project.com/download/stable/#download-win) and
  [this](https://www.microsoft.com/en-us/download/details.aspx?id=56119), Ubuntu users make
  sure to add [this PPA](https://www.mono-project.com/download/stable/#download-lin), update,
  then install the mono packages as for Arch/Manjaro users make sure to install `msbuild-stable` from the AUR)


### Performing the Build

* Clone the git repo (or download as zip and extract)
* Cd into the project root and run `nuget restore SkyOfSteel.sln` (This will pull in Newtonsoft.Json)
* Open the Godot editor to the project and let it reimport all the assets then hit "Play" near the upper right corner (play icon)
  * The editor will proceed to build and launch the project


## License

SkyOfSteel is licensed under the MIT license and utilizes
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) which is
also under the MIT license.
