﻿using SWLOR.Game.Server.Core;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Entity;
using SWLOR.Game.Server.Feature.StatusEffectDefinition.StatusEffectData;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.StatusEffectService;

namespace SWLOR.Game.Server.Feature
{
    public static class NaturalRegeneration
    {
        /// <summary>
        /// On module heartbeat, process a player's HP/FP/STM regeneration.
        /// </summary>
        [NWNEventHandler("interval_pc_6s")]
        public static void ProcessRegeneration()
        {
            var player = OBJECT_SELF;
            if (!GetIsPC(player) || GetIsDM(player)) return;

            var tick = GetLocalInt(player, "NATURAL_REGENERATION_TICK") + 1;
            if (tick >= 5) // 6 seconds * 5 = 30 seconds
            {
                var vitalityBonus = GetAbilityModifier(AbilityType.Vitality, player);
                if (vitalityBonus < 0)
                    vitalityBonus = 0;

                var playerId = GetObjectUUID(player);
                var dbPlayer = DB.Get<Player>(playerId);
                var hpRegen = dbPlayer.HPRegen;
                var fpRegen = dbPlayer.FPRegen;
                var stmRegen = dbPlayer.STMRegen;
                var foodEffect = StatusEffect.GetEffectData<FoodEffectData>(player, StatusEffectType.Food);

                if (foodEffect != null)
                {
                    hpRegen += foodEffect.HPRegen + vitalityBonus * 4;
                    fpRegen += foodEffect.FPRegen + vitalityBonus * 2;
                    stmRegen += foodEffect.STMRegen + vitalityBonus * 2;
                }

                if (hpRegen > 0)
                {
                    ApplyEffectToObject(DurationType.Instant, EffectHeal(hpRegen), player);
                }

                if (fpRegen > 0)
                {
                    Stat.RestoreFP(player, fpRegen, dbPlayer);
                }

                if (stmRegen > 0)
                {
                    Stat.RestoreStamina(player, stmRegen, dbPlayer);
                }

                tick = 0;
            }

            SetLocalInt(player, "NATURAL_REGENERATION_TICK", tick);
        }
    }
}
