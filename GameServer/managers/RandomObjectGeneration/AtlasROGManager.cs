using System;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
    public static class AtlasROGManager
    {
        private static DbItemTemplate beadTemplate = null;
        private static DbItemTemplate summonMerchantTemplate = null;
        private static DbItemTemplate rustyTinderboxTemplate = null;
        private static DbItemTemplate eversparkTinderboxTemplate = null;
        private static DbItemTemplate enchantedTinderboxTemplate = null;
        private static DbItemTemplate summonMimicCleric = null;
        private static DbItemTemplate summonMimic = null;


        private static string _currencyID = ServerProperties.Properties.ALT_CURRENCY_ID;

        public static void GenerateROG(GameLiving living)
        {
            GenerateROG(living, false, (byte)(living.Level + 3));
        }

        public static void GenerateROG(GameLiving living, byte itemLevel)
        {
            GenerateROG(living, false, itemLevel);
        }

        public static void GenerateROG(GameLiving living, bool useEventColor)
        {
            GenerateROG(living, useEventColor, (byte)(living.Level + 3));
        }

        public static void GenerateROG(GameLiving living, bool UseEventColors, byte itemLevel)
        {
            if (living != null && living is GamePlayer)
            {
                GamePlayer player = living as GamePlayer;
                eRealm realm = player.Realm;
                eCharacterClass charclass = (eCharacterClass)player.CharacterClass.ID;

                GeneratedUniqueItem item = null;
                item = new GeneratedUniqueItem(realm, charclass, itemLevel);
                item.AllowAdd = true;
                item.IsTradable = true;

                if (UseEventColors)
                {
                    eColor color = eColor.White;

                    switch (realm)
                    {
                        case eRealm.Hibernia:
                            color = eColor.Green_4;
                            break;
                        case eRealm.Albion:
                            color = eColor.Red_4;
                            break;
                        case eRealm.Midgard:
                            color = eColor.Blue_4;
                            break;
                    }

                    item.Color = (int)color;
                }
                
                DbInventoryItem invitem = GameInventoryItem.Create<DbItemUnique>(item);
                invitem.IsROG = true;
                player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.PickupObject.YouGet", invitem.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
            }
        }

        public static void GenerateMinimumUtilityROG(GameLiving living, byte minimumUtility)
        {
            if (living != null && living is GamePlayer)
            {
                GamePlayer player = living as GamePlayer;
                eRealm realm = player.Realm;
                eCharacterClass charclass = (eCharacterClass)player.CharacterClass.ID;

                GeneratedUniqueItem item = null;
                item = new GeneratedUniqueItem(realm, charclass, (byte)(living.Level+1), minimumUtility);
                item.AllowAdd = true;
                item.IsTradable = true;

                DbInventoryItem invitem = GameInventoryItem.Create<DbItemUnique>(item);
                invitem.IsROG = true;
                player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.PickupObject.YouGet", invitem.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
            }
        }


        public static void GenerateJewel(GameLiving living, byte itemLevel, int minimumUtility = 0)
        {
            if (living != null && living is GamePlayer)
            {
                GamePlayer player = living as GamePlayer;
                eRealm realm = player.Realm;
                eCharacterClass charclass = (eCharacterClass) player.CharacterClass.ID;

                GeneratedUniqueItem item = null;
                
                if(minimumUtility > 0)
                    item = new GeneratedUniqueItem(realm, charclass, itemLevel, eObjectType.Magical, minimumUtility);
                else
                    item = new GeneratedUniqueItem(realm, charclass, itemLevel, eObjectType.Magical);
                
                item.AllowAdd = true;
                item.IsTradable = true;

                DbInventoryItem invitem = GameInventoryItem.Create<DbItemUnique>(item);
                invitem.IsROG = true;
                player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
                player.Out.SendMessage(
                    LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.PickupObject.YouGet",
                        invitem.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
            }
        }

        public static void GenerateReward(GameLiving living, int amount)
        {
            if (GameServer.Instance.Configuration.ServerType == EGameServerType.GST_PvP)
            {
                GenerateBPs(living, amount);
            }
            else
            {
                GenerateOrbAmount(living, amount);
            }
        }

        private static void GenerateBPs(GameLiving living, int amount)
        {
            if (amount == 0) return; 

            if (living != null && living is GamePlayer)
            {
                var player = living as GamePlayer;
                

                double numCurrentLoyalDays = LoyaltyManager.GetPlayerRealmLoyalty(player) != null ? LoyaltyManager.GetPlayerRealmLoyalty(player).Days : 0;

                if(numCurrentLoyalDays >= 30)
                {
                    numCurrentLoyalDays = 30;
                }

                var loyaltyBonus = ((amount * .2) * (numCurrentLoyalDays / 30));
                
                double relicBonus = (amount * (0.025 * RelicMgr.GetRelicCount(player.Realm)));

                var totBPs = amount + Convert.ToInt32(loyaltyBonus) + Convert.ToInt32(relicBonus);
                
                player.GainBountyPoints(totBPs, false);
                
                if (loyaltyBonus > 0)
                    player.Out.SendMessage($"You gained an additional {Convert.ToInt32(loyaltyBonus)} BPs due to your realm loyalty!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                if (relicBonus > 0)
                    player.Out.SendMessage($"You gained an additional {Convert.ToInt32(relicBonus)} BPs due to your realm's relic ownership!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            }
            
        }
        public static void GenerateOrbAmount(GameLiving living, int amount)
        {
            if (amount == 0) return; 

            if (living != null && living is GamePlayer)
            {
                var player = living as GamePlayer;
                
                var orbs = GameServer.Database.FindObjectByKey<DbItemTemplate>(_currencyID);
                
                if (orbs == null)
                {
                    player.Out.SendMessage("Error: Currency ID not found!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                DbInventoryItem item = GameInventoryItem.Create(orbs);

                double numCurrentLoyalDays = LoyaltyManager.GetPlayerRealmLoyalty(player) != null ? LoyaltyManager.GetPlayerRealmLoyalty(player).Days : 0;

                if(numCurrentLoyalDays >= 30)
                {
                    numCurrentLoyalDays = 30;
                }

                var loyaltyBonus = ((amount * .2) * (numCurrentLoyalDays / 30));
                
                double relicOrbBonus = (amount * (0.025 * RelicMgr.GetRelicCount(player.Realm)));

                var totOrbs = amount + Convert.ToInt32(loyaltyBonus) + Convert.ToInt32(relicOrbBonus);

                item.OwnerID = player.InternalID;

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.PickupObject.YouGetAmount", amount ,item.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                
                if (loyaltyBonus > 0)
                    player.Out.SendMessage($"You gained an additional {Convert.ToInt32(loyaltyBonus)} orb(s) due to your realm loyalty!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                if (relicOrbBonus > 0)
                    player.Out.SendMessage($"You gained an additional {Convert.ToInt32(relicOrbBonus)} orb(s) due to your realm's relic ownership!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);

                if (!player.Inventory.AddCountToStack(item,totOrbs))
                {
                    if(!player.Inventory.AddTemplate(item, totOrbs, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                    {
                        item.Count = totOrbs;
                        player.CreateItemOnTheGround(item);
                        player.Out.SendMessage($"Your inventory is full, your {item.Name}s have been placed on the ground.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }

                }
                
                player.Achieve(AchievementUtils.AchievementNames.Orbs_Earned, totOrbs);
            }
        }
        
        public static void GenerateBattlegroundToken(GameLiving living, int amount)
        {
            if (living != null && living is GamePlayer)
            {
                var player = living as GamePlayer;

                DbItemTemplate token = null;
                if(living.Level > 19 && living.Level < 25) //level bracket 20-24
                    token = GameServer.Database.FindObjectByKey<DbItemTemplate>("L20RewardToken");
                if(living.Level > 33 && living.Level < 40) //level bracket 34-39
                    token = GameServer.Database.FindObjectByKey<DbItemTemplate>("L35RewardToken");

                if (token == null) return;

                DbInventoryItem item = GameInventoryItem.Create(token);
                item.OwnerID = player.InternalID;

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.PickupObject.YouGetAmount", amount ,item.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);

                if (!player.Inventory.AddCountToStack(item,amount))
                {
                    if(!player.Inventory.AddTemplate(item, amount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                    {
                        item.Count = amount;
                        player.CreateItemOnTheGround(item);
                        player.Out.SendMessage($"Your inventory is full, your {item.Name}s have been placed on the ground.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }

                }
            }
        }
        
        public static void GenerateBeetleCarapace(GameLiving living, int amount = 1)
        {
            if (living != null && living is GamePlayer)
            {
                var player = living as GamePlayer;

                var itemTP = GameServer.Database.FindObjectByKey<DbItemTemplate>("beetle_carapace");

                DbInventoryItem item = GameInventoryItem.Create(itemTP);
                
                item.OwnerID = player.InternalID;

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.PickupObject.YouGetAmount", amount ,item.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);

                if (!player.Inventory.AddCountToStack(item,amount))
                {
                    if(!player.Inventory.AddTemplate(item, amount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                    {
                        item.Count = amount;
                        player.CreateItemOnTheGround(item);
                        player.Out.SendMessage($"Your inventory is full, your {item.Name}s have been placed on the ground.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    }

                }
                player.Achieve(AchievementUtils.AchievementNames.Carapace_Farmed, amount);
            }
        }

        public static GeneratedUniqueItem GenerateMonsterLootROG(eRealm realm, eCharacterClass charClass, byte level, bool isFrontierKill)
        {
            GeneratedUniqueItem item = null;
            
            if(isFrontierKill)
                item = new GeneratedUniqueItem(realm, charClass, level, level - Util.Random(-5,10));
            else
                item = new GeneratedUniqueItem(realm, charClass, level, level - Util.Random(15,20));
            
            item.AllowAdd = true;
            item.IsTradable = true;
            //item.CapUtility(level);
            return item;
            
        }

        public static DbItemUnique GenerateBeadOfRegeneration()
        {
            if(beadTemplate == null)
                beadTemplate = GameServer.Database.FindObjectByKey<DbItemTemplate>("Bead_Of_Regeneration");
            
            DbItemUnique item = new DbItemUnique(beadTemplate);
            
            return item;
        }
        
        public static DbItemUnique GenerateSummonCleric()
        {
            if (summonMimicCleric == null)
                summonMimicCleric = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Cleric");

            DbItemUnique item = new DbItemUnique(summonMimicCleric);

            return item;
        }

        public static DbItemUnique GenerateMimic(eRealm realm)
        {

            if (realm == eRealm.Albion)
            {
                var rand = Util.Random(1, 13);
                switch (rand)
                {
                    case 1:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Paladin");
                        break;
                    case 2:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Armsman");
                        break;
                    case 3:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Scout");
                        break;
                    case 4:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Minstrel");
                        break;
                    case 5:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Theurgist");
                        break;
                    case 6:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Cleric");
                        break;
                    case 7:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Wizard");
                        break;
                    case 8:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Sorcerer");
                        break;
                    case 9:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Infiltrator");
                        break;
                    case 10:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Friar");
                        break;
                    case 11:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Mercenary");
                        break;
                    case 12:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Cabalist");
                        break;
                    case 13:
                        summonMimic = GameServer.Database.FindObjectByKey<DbItemTemplate>("SummonMimic_Reaver");
                        break;
                }
            }             
            DbItemUnique item = new DbItemUnique(summonMimic);
            return item;
        }

        public static DbItemUnique GenerateRustyTinderbox()
        {
            if (rustyTinderboxTemplate == null)
                rustyTinderboxTemplate = GameServer.Database.FindObjectByKey<DbItemTemplate>("Rusty_Tinderbox");
            DbItemUnique item = new DbItemUnique(rustyTinderboxTemplate);      

            return item;
        }

        public static DbItemUnique GenerateEversparkTinderbox()
        {
            if (eversparkTinderboxTemplate == null)
                eversparkTinderboxTemplate = GameServer.Database.FindObjectByKey<DbItemTemplate>("Everspark_Tinderbox");
            
            DbItemUnique item = new DbItemUnique(eversparkTinderboxTemplate);

            return item;
        }

        public static DbItemUnique GenerateEnchatedTinderbox()
        {
            if (enchantedTinderboxTemplate == null)
                enchantedTinderboxTemplate = GameServer.Database.FindObjectByKey<DbItemTemplate>("Enchanted_Tinderbox");

            DbItemUnique item = new DbItemUnique(enchantedTinderboxTemplate);

            return item;
        }

        public static DbItemUnique GenerateSummonMerchant()
        {
            if (summonMerchantTemplate == null)
                summonMerchantTemplate = GameServer.Database.FindObjectByKey<DbItemTemplate>("summon_merchant");

            DbItemUnique item = new DbItemUnique(summonMerchantTemplate);

            return item;
        }
    }
}
