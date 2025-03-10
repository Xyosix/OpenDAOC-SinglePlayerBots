﻿using DOL.AI.Brain;
using System;
using System.Collections.Generic;
using System.Linq;
using DOL.Database;
using DOL.GS.Scripts;

namespace DOL.GS {

    /// <summary>
    /// ROGMobGenerator
    /// At the moment this generator only adds ROGs to the loot
    /// </summary>
    public class ROGMobGenerator : LootGeneratorBase {

        //base chance in %
        public static ushort BASE_ROG_CHANCE = 15;


        /// <summary>
        /// Generate loot for given mob
        /// </summary>
        /// <param name="mob"></param>
        /// <param name="killer"></param>
        /// <returns></returns>
        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList loot = base.GenerateLoot(mob, killer);

            try
            {
                Console.WriteLine("A mob has died!");
                GamePlayer player = killer as GamePlayer;

                if (killer is GameNPC && ((GameNPC)killer).Brain is IControlledBrain)
                {
                    //This can possibly cause a fail from a Mimic's pet.
                    Console.WriteLine("Killer is a controlled brain");
                    player = ((ControlledMobBrain)((GameNPC)killer).Brain).GetPlayerOwner();                 
                }
              
                if (killer is MimicNPC)
                {
                    var group = ((MimicNPC)killer).Group.GetMembersInTheGroup();
                    Console.WriteLine("Killer was a Mimic; Group Member Count: " + group.Count);

                    foreach (var member in group)
                    {
                        if(member is GamePlayer)
                        {
                            Console.WriteLine("Player found in mimic's group! Group Mate: " + member.Name);
                            player = member as GamePlayer;
                        }
                    }
                }

                if (player == null)
                {
                    //Console.WriteLine("Player is still null; exiting");
                    return loot;
                }

                int killedcon = player.GetConLevel(mob); 

                //grey con dont drop loot
                if (killedcon <= -3)
                {
                    //Console.WriteLine("Mob too low; exiting");
                    return loot;
                }

                eCharacterClass classForLoot = (eCharacterClass)player.CharacterClass.ID;
                // allow the leader to decide the loot realm
                if (player.Group != null)
                {
                    //Console.WriteLine("In a group");
                    player = player.Group.Leader;
                }

                // chance to get a RoG Item
                int chance = BASE_ROG_CHANCE + ((killedcon < 0 ? killedcon + 1 : killedcon) * 3);

                //chance = 100;
                
                BattleGroup bg = player.TempProperties.GetProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY);

                if (bg != null)
                {
                    //Console.WriteLine("Battlegroup baby!");
                    var maxDropCap = bg.PlayerCount / 50;
                    if (maxDropCap < 1) maxDropCap = 1;
                    if (mob is GameEpicNPC)
                        maxDropCap *= 2;
                    chance = 2;

                    int numDrops = 0;
                    foreach (GamePlayer bgMember in bg.Members.Keys)
                    {
                        if(bgMember.GetDistance(player) > WorldMgr.VISIBILITY_DISTANCE)
                            continue;
                        
                        if (Util.Chance(chance) && numDrops < maxDropCap)
                        {
                            classForLoot = GetRandomClassFromBattlegroup(bg);
                            var item = GenerateItemTemplate(player, classForLoot, (byte)(mob.Level + 1), killedcon);
                            loot.AddFixed(item, 1);
                            numDrops++;
                        }
                    }
                }

                //players below level 50 will always get loot for their class, 
                //or a valid class for one of their groupmates
                else if (player.Group != null)
                {
                    //Group Loot Roll

                    //Console.WriteLine("In a group2, I think.");
                    var MaxDropCap = Math.Round((decimal) (player.Group.MemberCount)/3);
                    if (MaxDropCap < 1) MaxDropCap = 1;
                    if (MaxDropCap > 3) MaxDropCap = 3;
                    if (mob.Level > 65) MaxDropCap++; //increase drop cap beyond lvl 60
                    int guaranteedDrop = mob.Level > 67 ? 1 : 0; //guarantee a drop for very high level mobs
                    
                    if (mob.Level > 27)
                        chance -= 3;

                    if (mob.Level > 40)
                        chance -= 3;

                    int numDrops = 0;
                    //roll for an item for each player in the group
                    foreach (var groupPlayer in player.Group.GetNearbyPlayersInTheGroup(player))
                    {
                        //Console.WriteLine("Group player count: " + player.Group.MemberCount + " Name: " + groupPlayer.Name);
                        if (groupPlayer.GetDistance(player) > WorldMgr.VISIBILITY_DISTANCE)
                            continue;
                        
                        if (Util.Chance(chance) && numDrops < MaxDropCap)
                        {
                                Console.WriteLine("Make loot roll succeded!");
                                classForLoot = GetRandomClassFromGroup(player.Group);
                                var item = GenerateItemTemplate(player, classForLoot, (byte)(mob.Level + 1), killedcon);
                                loot.AddFixed(item, 1);
                                numDrops++;
                        }
                    }

                    //if we're under the cap, add in the guaranteed drop
                    if (numDrops < MaxDropCap && guaranteedDrop > 0)
                    {
                        classForLoot = GetRandomClassFromGroup(player.Group);
                        var item = GenerateItemTemplate(player, classForLoot, (byte)(mob.Level + 1), killedcon);
                        loot.AddFixed(item, 1);
                    }

                    //Add Beads
                    if(player.Level < 50 || mob.Level < 50)
                    {
                        var item = AtlasROGManager.GenerateBeadOfRegeneration();
                        loot.AddRandom(2, item, 1);
                    }

                    //Add Merchant Barrell
                    if(player.Level <= mob.Level)
                    {
                        var item = AtlasROGManager.GenerateSummonMerchant();
                        loot.AddRandom(2, item, 1);
                    }

                    if(true)
                    {
                        Console.WriteLine("Dropping Random Mimic");
                        var item = AtlasROGManager.GenerateMimic(player.Realm);
                        loot.AddRandom(5, item, 1);
                    }

                }
                else
                {
                    //Solo Loot Roll
                    //Console.WriteLine("Made it this far!");
                    int tmpChance = player.OutOfClassROGPercent > 0 ? player.OutOfClassROGPercent : 0;
                    if (player.Level == 50 && Util.Chance(tmpChance))
                    {
                        classForLoot = GetRandomClassFromRealm(player.Realm);
                    }

                    chance += 10; //solo drop bonus
                    
                    DbItemTemplate item = null;
                    if (Util.Chance(chance))
                    {
                        GeneratedUniqueItem tmp = AtlasROGManager.GenerateMonsterLootROG(player.Realm, classForLoot, (byte)(mob.Level + 1), player.CurrentZone?.IsOF ?? false);
                        item = tmp;
                        item.MaxCount = 1;
                        loot.AddFixed(item, 1);
                    }
                    //tmp.GenerateItemQuality(killedcon);
                    //tmp.CapUtility(mob.Level + 1);
                    
                    /*
                    if (mob.Level < 5)
                    {
                        chance += 50;
                        loot.AddRandom(chance, item, 1);
                    }
                    else if (mob.Level < 10)
                        loot.AddRandom(chance + (100 - mob.Level * 10), item, 1);
                    //25% bonus drop rate at lvl 5, down to normal chance at level 10
                    else
                        loot.AddRandom(chance, item, 1);
                        */

                    if(player.Level < 50 || mob.Level < 50)
                    {
                        item = AtlasROGManager.GenerateBeadOfRegeneration();
                        loot.AddRandom(2, item, 1);
                    }

                    if (true)
                    {
                        Console.WriteLine("Dropping Random Mimic");
                        item = AtlasROGManager.GenerateMimic(player.Realm);
                        loot.AddRandom(5, item, 1);
                    }
                }

                //chance = 100;

               

            }
            catch
            {
                Console.WriteLine("Everything failed; catch");
                return loot;
            }

            return loot;
        }


        private DbItemTemplate GenerateItemTemplate(GamePlayer player, eCharacterClass classForLoot, byte lootLevel, int killedcon)
        {
            DbItemTemplate item = null;
                
                
            GeneratedUniqueItem tmp = AtlasROGManager.GenerateMonsterLootROG(player.Realm, classForLoot, lootLevel, player.CurrentZone?.IsOF ?? false);
            tmp.GenerateItemQuality(killedcon);
            //tmp.CapUtility(mob.Level + 1);
            item = tmp;
            item.MaxCount = 1;

            return item;
        }

        private eCharacterClass GetRandomClassFromGroup(Group group)
        {
            List<eCharacterClass> validClasses = new List<eCharacterClass>();

            //foreach (GamePlayer player in group.GetMembersInTheGroup())
            //{
            //    Console.WriteLine("Name: "+player.Name +" Class: "+ player.CharacterClass.Name);
            //    validClasses.Add((eCharacterClass)player.CharacterClass.ID);
            //}

            Console.WriteLine("Making loot for the following classes: ");
            foreach (var player in group.GetMembersInTheGroup())
            {               
                if (player is GamePlayer)
                {
                    GamePlayer p = (GamePlayer)player;
                    Console.WriteLine("Player Name: " + p.Name + " Class: " + p.CharacterClass.Name);
                    validClasses.Add((eCharacterClass)p.CharacterClass.ID);
                }
                else if (player is MimicNPC) 
                {
                    MimicNPC npc = (MimicNPC)player;
                    Console.WriteLine("NPC Name: " + npc.Name + " Class: " + npc.CharacterClass.Name);
                    validClasses.Add((eCharacterClass)npc.CharacterClass.ID);
                }
            }

            Console.WriteLine("Classes found: " + validClasses.Count);
            eCharacterClass ranClass = validClasses[Util.Random(validClasses.Count - 1)];
            return ranClass;
        }
        
        private eCharacterClass GetRandomClassFromBattlegroup(BattleGroup battlegroup)
        {
            List<eCharacterClass> validClasses = new List<eCharacterClass>();

            foreach (GamePlayer player in battlegroup.Members.Keys)
            {
                validClasses.Add((eCharacterClass)player.CharacterClass.ID);
            }
            eCharacterClass ranClass = validClasses[Util.Random(validClasses.Count - 1)];

            return ranClass;
        }

        private eCharacterClass GetRandomClassFromRealm(eRealm realm)
        {
            List<eCharacterClass> classesForRealm = new List<eCharacterClass>();
            switch (realm)
            {
                case eRealm.Albion:
                    classesForRealm.Add(eCharacterClass.Armsman);
                    classesForRealm.Add(eCharacterClass.Cabalist);
                    classesForRealm.Add(eCharacterClass.Cleric);
                    classesForRealm.Add(eCharacterClass.Friar);
                    classesForRealm.Add(eCharacterClass.Infiltrator);
                    classesForRealm.Add(eCharacterClass.Mercenary);
                    classesForRealm.Add(eCharacterClass.Necromancer);
                    classesForRealm.Add(eCharacterClass.Paladin);
                    classesForRealm.Add(eCharacterClass.Reaver);
                    classesForRealm.Add(eCharacterClass.Scout);
                    classesForRealm.Add(eCharacterClass.Sorcerer);
                    classesForRealm.Add(eCharacterClass.Theurgist);
                    classesForRealm.Add(eCharacterClass.Wizard);
                    break;
                case eRealm.Midgard:
                    classesForRealm.Add(eCharacterClass.Berserker);
                    classesForRealm.Add(eCharacterClass.Bonedancer);
                    classesForRealm.Add(eCharacterClass.Healer);
                    classesForRealm.Add(eCharacterClass.Hunter);
                    classesForRealm.Add(eCharacterClass.Runemaster);
                    classesForRealm.Add(eCharacterClass.Savage);
                    classesForRealm.Add(eCharacterClass.Shadowblade);
                    classesForRealm.Add(eCharacterClass.Skald);
                    classesForRealm.Add(eCharacterClass.Spiritmaster);
                    classesForRealm.Add(eCharacterClass.Thane);
                    classesForRealm.Add(eCharacterClass.Warrior);
                    break;
                case eRealm.Hibernia:
                    classesForRealm.Add(eCharacterClass.Animist);
                    classesForRealm.Add(eCharacterClass.Bard);
                    classesForRealm.Add(eCharacterClass.Blademaster);
                    classesForRealm.Add(eCharacterClass.Champion);
                    classesForRealm.Add(eCharacterClass.Druid);
                    classesForRealm.Add(eCharacterClass.Eldritch);
                    classesForRealm.Add(eCharacterClass.Enchanter);
                    classesForRealm.Add(eCharacterClass.Hero);
                    classesForRealm.Add(eCharacterClass.Mentalist);
                    classesForRealm.Add(eCharacterClass.Nightshade);
                    classesForRealm.Add(eCharacterClass.Ranger);
                    classesForRealm.Add(eCharacterClass.Valewalker);
                    classesForRealm.Add(eCharacterClass.Warden);
                    break;
            }

            return classesForRealm[Util.Random(classesForRealm.Count - 1)];
        }
    }
}