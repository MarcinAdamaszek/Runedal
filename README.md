# Runedal - RPG game

## Table of contents
* [Description](#description)
* [Technologies](#technologies)
* [Features](#features)
* [Flaws/Bugs](#flawsissues)
* [Setup](#setup)

## Description
Runedal is a practice and hobby project I made to hone my C# skills. 
It's a single player, fantasy RPG game, where most of the action
is displayed in a form of plain, often colored text. You control your
character by typing predefined commands, allowing you to 
explore the world, slay enemies, gather items, talk
and trade with NPCs and develop your character in an unique way.

## Technologies
- Windows Presentation Foundation (WPF)
- .NET 6.0
- C# 10

## Features
- Real-time gameplay system
- Triggerable hints, allowing player to learn the game through playing
- Minimalistic GUI, including health/mana bars, action bar and minimap
- Balanced stats and attributes system allowing for an unique character development
- Save/load system
- Command history (up to 10 last typed commands)
- Entire game content in a form of easily editable JSON files

## Flaws/Issues
- Only one language option (polish)
- Limited to Windows platform
- Poor game content - no lore, no quests, just exploring the world and slaying enemies.
- The game won't launch if installed into 'Program Files' or 'Program Files (x86)' directory
	
## Setup
Download the installer [here](https://github.com/MarcinAdamaszek/Runedal/releases/latest/download/Runedal.1.0.0.Setup.zip)

After download, extract the package and run [ Runedal.1.0.0.Setup.msi ] to install 
the game on your machine. The installer will create shortcuts on your desktop
and in programs menu.

Note: DO NOT install into 'Program Files' or 'Program Files (x86)' directory
or the game will fail to launch.