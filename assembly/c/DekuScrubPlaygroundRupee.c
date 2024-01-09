#include <z64.h>
#include "BaseRupee.h"
#include "MMR.h"
#include "Player.h"

void DekuScrubPlaygroundRupee_GiveItem(GlobalContext* ctxt, ActorEnGamelupy* actor) {
    u16 giIndex = Rupee_GetGiIndex(&actor->base);
    if (giIndex > 0) {
        Rupee_SetGiIndex(&actor->base, 0);
        if (!MMR_GiveItem(ctxt, &actor->base, giIndex)) {
            actor->base.currPosRot.pos = GET_PLAYER(ctxt)->base.currPosRot.pos;
            actor->base.currPosRot.pos.y += 40.0f;
            Player_Pause(ctxt);
        }
    } else {
        z2_AddRupees(actor->isBlueRupee ? 5 : 1);
        z2_PlaySfxAtActor(&actor->base, 0x4803);
    }
}

u16 DekuScrubPlaygroundRupee_BeforeDisappearing(ActorEnGamelupy* actor, GlobalContext* ctxt) {
    u16 giIndex = Rupee_GetDrawGiIndex(&actor->base);
    if (giIndex > 0) {
        GetItemEntry* entry = MMR_GetGiEntry(giIndex);
        if (entry->item == 0xB0) {
            // Ice Trap
            z2_PlayLoopingSfxAtActor(&actor->base, 0x31A4);
            return actor->disappearCountdown < 0x40;
        }
    }
    return actor->disappearCountdown < 0x1F;
}
