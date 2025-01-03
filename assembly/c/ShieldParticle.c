#include <z64.h>
#include "Misc.h"

#include "ShieldParticle.h"

// extern PlayState* Effect_GetPlayState(void);
extern GlobalContext* Effect_GetPlayState(void);

// the original doesnt check if lightNode == null, if actorizer spawns too many lights a sheild particle can form that doesnt have a valid light
// when destroyed on scene transfer, this destrucor fires late and the light still doenst exist
void ShieldParticle_Destroy2(void* thisx){
    EffectShieldParticle* this = (EffectShieldParticle*) thisx;

    if ((this != NULL) && (this->lightDecay == true) && this->lightNode != NULL) {
        //if (this->lightNode == Effect_GetPlayState()->lightCtx.listHead) {
        if (this->lightNode == Effect_GetPlayState()->lightCtx.lightsHead) {
            //Effect_GetPlayState()->lightCtx.listHead = this->lightNode->next;
            Effect_GetPlayState()->lightCtx.lightsHead = this->lightNode->next;
        }
        LightContext_RemoveLight(Effect_GetPlayState(), &Effect_GetPlayState()->lightCtx, this->lightNode);
    }
}
