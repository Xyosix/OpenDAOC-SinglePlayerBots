using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;
using System;
using DOL.GS.Scripts;
using DOL.Database;
using DOL.GS.API;

namespace DOL.GS.Spells
{
    [SpellHandler(eSpellType.SummonMimic)]
    public class SummonMimicHandler : SpellHandler
    {

        public SummonMimicHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void ApplyEffectOnTarget(GameLiving target)
        {
            if (Caster is GamePlayer p)
            {
                MimicNPC mimic = null;
                Point3D playerLocation = new Point3D(p.X, p.Y, p.Z);
                eGender gender = Util.RandomBool() ? eGender.Male : eGender.Female;
                string name = MimicNames.GetName(gender, p.Realm);
                eSpecType spec = eSpecType.None;

                double ClassID = m_spell.Value;

                mimic = MimicManager.GetMimic((eMimicClass)ClassID, p.Level, name, gender, eSpecType.None, false);
                //    case 1: //Paladin
                //    case 2: //Armsman
                //    case 3: //Scout
                //    case 4: //Minstrel
                //    case 5: //Theurgist
                //    case 6: //Cleric               
                //    case 7: //Wizard
                //    case 8: //Sorcerer
                //    case 9: //Infiltrator
                //    case 10: //Friar
                //    case 11: //Mercenary
                //    case 13: //Cabalist
                //    case 19: //Reaver

                //    case 21: //Thane
                //    case 22: //Warrior
                //    case 23: //Shadowblade
                //    case 24: //Skald
                //    case 25: //Hunter
                //    case 26: //Healer
                //    case 27: //Spiritmaster
                //    case 28: //Shaman
                //    case 29: //Runemaster
                //    case 30: //Bonedancer
                //    case 31: //Berserker
                //    case 32: //Savage

                //    case 40: //Eldritch
                //    case 41: //Enchanter
                //    case 42: //Mentalist
                //    case 43: //Blademaster
                //    case 44: //Hero
                //    case 45: //Champion
                //    case 46: //Warden
                //    case 47: //Druid
                //    case 48: //Bard
                //    case 49: //Nightshade
                //    case 50: //Ranger
                //    case 56: //Valewalker

                if (mimic != null)
                {
                    if (p.Group == null)
                    {
                        p.Group = new Group(p);
                        p.Group.AddMember(p);
                    }

                    MimicManager.AddMimicToWorld(mimic, playerLocation, p.CurrentRegionID);
                    //p.Say("Homina Homina, who flung dung? KHAZAAAM!");
                    p.Group.AddMember(mimic);
                }
                else
                {
                    p.Say("Something Wrong!");
                }
            }
        }
    }
}
