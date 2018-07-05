
# DRB Icon Appender
[NexusMods Page](https://www.nexusmods.com/darksouls/mods/1457)  
An item icon editing and creation tool for Dark Souls 1 and Remastered.  
Requires [.NET 4.7.2](https://www.microsoft.com/net/download/thank-you/net472) - Windows 10 users should already have this.  

# Instructions
[Video instructions](https://youtu.be/-K-tO9adAbY)

The Steam DSR installation will be opened by default. If your game is located somewhere else, use the Browse button to select your game .exe. If the path is correct but you want to edit DS1 instead, click Close to close the file without making changes, then browse to it.  
Once you select the .exe the existing icons will be loaded automatically. Existing icons can be edited, and new IDs can be registered via the Add Icon button in the bottom right corner. After making any changes, click Save to write them to the file. The unmodified file will be backed up automatically and you can use the Restore button to restore it and reload the original icons.

# Adding Textures
There are quite a few empty spaces in the existing textures you can register first, but if needed you can add more. Adding new icon textures requires a .tpf unpacker & repacker; I will be using [Wulf's BND Rebuilder](https://mega.nz/#!f9ZHxATK!cld3fJtM08NYgeylufXvYYTNchfMRAfMzK_jBBSsnaI).  
First extract menu\menu.tpf (PtDE) or menu.tpf.dcx (Remastered). Inside you'll find a bunch of blue square textures; copy any of them and rename it to whatever you want your texture to be called. Open filelist.txt and add your new file to the **bottom** of the list with values 5 and 0, then rebuild the .tpf.  
Next extract any of the menu_#.tpfs, add the actual icon texture with the same name as before, add it to filelist.txt, and rebuild.  
If done correctly, the texture will now be listed as an option in Icon Appender and loaded in-game.  

# Building
Standard users: ignore this section.  
If you want to build the app yourself, you must download [DSFormats](https://github.com/JKAnderson/DSFormats), then "Add existing project" and add a reference to the project.

# Credits
[Costura.Fody](https://github.com/Fody/Costura) by Simon Cropp, Cameron MacFarland

[Octokit](https://github.com/octokit/octokit.net) by GitHub

[Semver](https://github.com/maxhauser/semver) by Max Hauser
