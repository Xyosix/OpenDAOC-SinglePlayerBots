using System;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Base Handler for the focus shell
    /// </summary>
    [SpellHandler("PrayerHeal")]
    public class PrayerSpellHandler : SpellHandler
    {
        //public override ECSGameSpellEffect CreateECSEffect(ECSGameEffectInitParams initParams)
        //{
        //    return new FocusECSEffect(initParams);
        //}

        public PrayerSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            //Caster.Say("Begins Prayers");
            m_caster.Mana -= PowerCost(target);
            target.ChangeEndurance(target, eEnduranceChangeType.Spell, (int)Spell.Value);
            target.ChangeHealth(target, eHealthChangeType.Spell, (int)Spell.Value);
            base.FinishSpellCast(target);
        }

    }
}
