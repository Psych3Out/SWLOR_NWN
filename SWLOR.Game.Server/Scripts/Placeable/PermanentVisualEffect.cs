﻿using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.GameObject;
using SWLOR.Game.Server.NWN;
using SWLOR.Game.Server.NWN.Enum;

namespace SWLOR.Game.Server.Scripts.Placeable
{
    public class PermanentVisualEffect: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable self = _.OBJECT_SELF;

            int vfxID = self.GetLocalInt("PERMANENT_VFX_ID");
            
            if (vfxID > 0)
            {
                _.ApplyEffectToObject(DurationType.Permanent, _.EffectVisualEffect(vfxID), self);
            }

            _.SetEventScript(self, EventScript.Placeable_OnHeartbeat, string.Empty);
        }
    }
}
