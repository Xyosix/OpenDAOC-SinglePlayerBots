Note: Now requires the latest OpenDAoC Database https://github.com/OpenDAoC/OpenDAoC-Database or running the commands found in https://github.com/OpenDAoC/OpenDAoC-Database/commit/c6153398bf65faa61b665b6b4cae68b5fa8c0862 for AF buffs to work correctly.

This fork focuses on having bots that are treated as players as far as having player classes, give and take damage as players do, have player abilities, player specs, and can be grouped with for PvE or in RvR. RvR currently only includes Thidranki as far as automated spawning and grouping. As this is still being tested, some commands are available to players that normally shouldn't be.

Everything is currently very command based. Bracketed commands are required, parenthesis commands are optional.

/mlfg (Index) - Shows a list of available bots. It is currently based on the level of the player calling it. Multiple players being able to call is yet to be implemented.
   - Index - Attempts to add the bot of the index given to your group. They may decline if they are higher level than the player.

/mcamp [Set/Remove/Aggrorange/Filter]
   - Set - Sets the spot the group will wait at and return to after battle based on your ground target. They aggro anything around this point.
   - Remove - Returns the group to follow the leader. They will attack anything the leader is attacking or casting towards.
   - Aggrorange [1-6000] - Sets the range the group will attack anything around their camp point. Default is 250 in dungeons, 550 outside of them. They will aggro through walls in dungeons. Visibility range is 6000.
   - Filter [green/blue/yellow/orange/red/purple] - Sets the minimum con level the puller will pull.

/mrole [leader/puller/tank/cc/assist]
   - Leader - Not implemented. The group member everyone follows.
   - Assist - Not implemented. Will provide a target everyone should focus on.
   - Puller - Only avaible for classes that can use a bow/crossbow currentely. The puller will run pull mobs to the point set with /mcamp set.
   - Tank - Will only use taunt styles and defensive styles. Ensures every mob is attacking themselves.
   - CC - Will attempt to CC any adds from the puller.

/mguard [target name] - If the mimic is in your group and has the Guard ability they will guard the target given. Mimics can also be told to guard the player in the right-click interact menu.

/msummon - Summons your group to you. Used to summon your group if you zone into a dungeon or teleport somewhere.

/mpvp [true/false] - Sets PvPMode for mimics. PvPMode bots ignore mobs unless you are attacked. When false, bots will immediately attack mobs when you cast or go into attack mode with one targeted.
	- With a target: Sets PvPMode for target mimic.
	- With no target: Sets PvPMode for any grouped mimics.	

For PvE: 

   Use the command "/mlfg" to see a list of potential groupmembers. It's currently very single-player focused, so the list and level of bots will be based on the first person to call it until some time passes. Integration for more players will be implemented eventually. They might not join if they are higher level than you.

   By default, anyone in your group will follow you and attack or begin casting if you enter attack mode or begin casting on an valid target. This means you can go into attack mode with a target in order to make your group attack your target, useful if you're a healer or caster without debuffs to pull with.
   
   The bots can have roles set that are only considered in PvE. Currently they are MainTank, MainCC, MainPuller. The roles are set with the /mrole command or via right-click interaction and only apply when a camp point is set with the /mcamp set command. Without a /mcamp set or removed via /mccamp remove, bots will only attack targets you go into attack mode against or cast a spell at. Basically:

   "/mcamp set" to set a point with a ground target and the grouped bots will stay at this point and attack anything in a small range, adjustable with the /mcamp aggrorange command. Any bot set to the "puller" role will seek a target to pull into the camp. Currently limited to classes that can use a bow/crossbow. They will wait for everyone to finish "sitting", which is shown by the "Drink" emote. When they are ready, they will perform the "LetsGo" emote. Anyone set to the CC role with the appropriate spells will root or mez adds. The bot set with the tank role will focus on taunts to make them targeted provided the target isn't CC'd. This all works outside dungeons provided the puller isn't set. A puller in dungeons isn't tested. They will likely pull and run through walls.

For RvR:

   All roles are not considered. If you summon or group with any bots, they will follow and aggro enemies if you are attacked, and group members will attack if you do or cast against enemies. 
   
   Battlegrounds:
   
   Only Thidranki is implemented.
   
   /mbattle [Region] [Start/Stop/Clear]
   
   - Region - "thid" is the only working region command.
   - Start - Begins spawning all realms.
   - Stop - Stops all spawning of realms.
   - Clear - Stops all spawning of realms and kills and removes all npcs.
   

Commands for testing:

/m [Classname] (Level) - Summons a single bot. Defaults to callers level if level is omitted.
   - Classname - summons a mimic with a level equal to the caller.
   - Level - The level of the bot.

/mgroup (Realm) (Amount) (Level) - Summons a group of bots. With no arguments, all arguments will be the caller's realm, 8 bots, and the level of the caller.
- Realm - The realm of the bots.
- Amount - The amount in the group.
- Level - The level of all bots in the group.

/mpc [true/false] - Sets PreventCombat setting. Will make a bot passive and not aggro or attack.
	- With a target: Sets PreventCombat for Mimic. If the Mimic is in a group, the whole group will be set.
	- With no target: Sets PreventCombat for any grouped Mimics.

There are also commands from interacting with MimicNPCs. They are mostly for testing with the exception of
[Group] - [Leader] - [MainPuller] - [MainCC] - [MainTank] - [MainAssist]. These set their respective roles or attempt to group with the bot.
 
Do not click [Weapon] [Helm] [Torso] [Legs] [Arms] [Hands] [Boots] [Jewelry]
if you don't need to, these simply place the items equipped into your inventory. You can't put them back at the moment.

# OpenDAoC
[![Build and Release](https://github.com/OpenDAoC/OpenDAoC-Core/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/OpenDAoC/OpenDAoC-Core/actions/workflows/build-and-release.yml)

## About

OpenDAoC is an emulator for Dark Age of Camelot (DAoC) servers, originally a fork of the [DOLSharp](https://github.com/Dawn-of-Light/DOLSharp) project.

Now completely rewritten with ECS architecture, OpenDAoC ensures performance and scalability for many players, providing a robust platform for creating and managing DAoC servers.

While the project focuses on recreating the DAoC 1.65 experience, it can be adapted for any patch level.

## Documentation

The easiest way to get started with OpenDAoC is to use Docker. Check out the `docker-compose.yml` file in the repository root for an example setup.

For detailed instructions and additional setup options, refer to the full [OpenDAoC Documentation](https://www.opendaoc.com/docs/).

## Releases

Releases for OpenDAoC are available at [OpenDAoC Releases](https://github.com/OpenDAoC/OpenDAoC-Core/releases).

OpenDAoC is also available as a Docker image, which can be pulled from the following registries:

- [GitHub Container Registry](https://ghcr.io/opendaoc/opendaoc-core) (recommended): `ghcr.io/opendaoc/opendaoc-core/opendaoc:latest`
- [Docker Hub](https://hub.docker.com/repository/docker/claitz/opendaoc/): `claitz/opendaoc:latest`

For detailed instructions and additional setup options, refer to the documentation.

## Companion Repositories

Several companion repositories are part of the [OpenDAoC project](https://github.com/OpenDAoC).

Some of the main repositories include:

- [OpenDAoC Database v1.65](https://github.com/OpenDAoC/OpenDAoC-Database)
- [Account Manager](https://github.com/OpenDAoC/opendaoc-accountmanager)
- [Client Launcher](https://github.com/OpenDAoC/OpenDAoC-Launcher)

## License

OpenDAoC is licensed under the [GNU General Public License (GPL)](https://choosealicense.com/licenses/gpl-3.0/) v3 to serve the DAoC community and promote open-source development.  
See the [LICENSE](LICENSE) file for more details.
