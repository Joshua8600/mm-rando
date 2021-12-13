;==================================================================================================
; Freestanding Models (Heart Piece)
;==================================================================================================

.headersize G_CODE_DELTA

; Heart Piece draw function call.
; Replaces:
;   jal     0x800A75B8
.org 0x800A7188
    jal     Models_DrawHeartPiece

; Bio Baba Heart Piece rotation fix.
; Replaces:
;   bnez    at, 0x800A68F0
;   addiu   at, r0, 0x0017
;   beq     v0, at, 0x800A68F0
.org 0x800A68B4
    jal     Models_BioBabaHeartPieceRotationFix_Hook
    nop
    bnez    at, 0x800A68F0

;==================================================================================================
; Freestanding Models (Item00 Not Heart Piece)
;==================================================================================================

.headersize G_CODE_DELTA

; Remove original "disappear flicker" handling. Now handled in Models_DrawItem00.
; Replaces:
;   lh      t6, 0x014E (a2)
;   lh      t7, 0x0150 (a2)
;   and     t8, t6, t7
;   bnezl   t8, 0x800A72A0
;   lw      ra, 0x0014 (sp)
.org 0x800A7138
    nop
    nop
    nop
    nop
    nop

; Item draw function call.
; Replaces:
;   lui     at, 0x801E
;   addu    at, at, t9
;   lw      t9, 0xBFF4 (at)
.org 0x800A715C
    jal     Models_DrawItem00_Hook
    nop
    nop

; Item set scale in constructor
; Replaces:
;   lui     at, 0x801E
;   addu    at, at, t7
;   lw      t7, 0xBDF4 (at)
.org 0x800A5E6C
    jal     Models_Item00_SetActorSize_Hook
    or      a1, s0, r0
    nop

;==================================================================================================
; Freestanding Models (Skulltula Token)
;==================================================================================================

.headersize G_EN_SI_DELTA

; Skulltula Token draw function.
; Replaces:
;   addiu   sp, sp, -0x18
;   sw      ra, 0x0014 (sp)
.org 0x8098CD0C
    j       Models_DrawSkulltulaToken
    nop

;==================================================================================================
; Freestanding Models (Stray Fairy)
;==================================================================================================

.headersize G_EN_ELFORG_DELTA

; Stray Fairy main function.
; Replaces:
;   sw      a0, 0x0018 (sp)
;   lw      t9, 0x022C (a0)
.org 0x80ACD7A0
    jal     Models_BeforeStrayFairyMain_Hook
    sw      a0, 0x0018 (sp)

; Stray Fairy draw function.
; Replaces:
;   addiu   sp, sp, -0x40
;   sw      s0, 0x0028 (sp)
;   or      s0, a1, r0
;   sw      ra, 0x002C (sp)
.org 0x80ACD8C0
    addiu   sp, sp, -0x40
    sw      ra, 0x002C (sp)
    jal     Models_DrawStrayFairy_Hook
    sw      s0, 0x0028 (sp)

;==================================================================================================
; Freestanding Models (Heart Container)
;==================================================================================================

.headersize G_ITEM_B_HEART_DELTA

; Heart Container draw function.
; Replaces:
;   or      a3, r0, r0
;   lw      a2, 0x0000 (a1)
.org 0x808BCFCC
    jal     Models_DrawHeartContainer_Hook
    nop

;==================================================================================================
; Freestanding Models (Boss Remains)
;==================================================================================================

.headersize G_DM_HINA_DELTA

; Overwrite draw function call for Odolwa's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD54
    jal     Models_DrawBossRemains_Hook

; Overwrite draw function call for Goht's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD64
    jal     Models_DrawBossRemains_Hook

; Overwrite draw function call for Gyorg's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD74
    jal     Models_DrawBossRemains_Hook

; Overwrite draw function call for Twinmold's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD84
    jal     Models_DrawBossRemains_Hook

.headersize G_CODE_DELTA

; Replace behaviour of Boss Remains' Get-Item function always writing DList instruction to set
; object segment address.
; Replaces:
;   sw      t6, 0x02B0 (s0)
;   ori     t7, t7, 0x0018
;   addu    t1, s1, t0
;   lui     t2, 0x0001
;   addu    t2, t2, t1
;   sw      t7, 0x0000 (v1)
;   lw      t2, 0x7D98 (t2)
;   sw      t2, 0x0004 (v1)
.org 0x800EFD94
.area 0x20
    or      a0, s1, r0
    jal     Models_WriteBossRemainsObjectSegment
    lw      a1, 0x003C (sp)
    nop
    nop
    nop
    nop
    nop
.endarea

;==================================================================================================
; Freestanding Models (Moon's Tear)
;==================================================================================================

.headersize G_OBJ_MOON_STONE_DELTA

; Before Moon's Tear main function.
; Replaces:
;   lw      v0, 0x1CCC (a1)
;   lui     at, 0x1000
;   ori     at, at, 0x0282
.org 0x80C068D8
    jal     Models_BeforeMoonsTearMain_Hook
    nop
    lw      v0, 0x1CCC (a1)

; Moon's Tear draw function.
; Replaces:
;   sw      s1, 0x0018 (sp)
;   or      s1, a1, r0
;   sw      ra, 0x001C (sp)
.org 0x80C06914
    sw      ra, 0x001C (sp)
    jal     Models_DrawMoonsTear_Hook
    sw      s1, 0x0018 (sp)

;==================================================================================================
; Freestanding Models (Lab Fish Heart Piece)
;==================================================================================================

.headersize G_EN_COL_MAN_DELTA

; Lab Fish Heart Piece draw function.
; Replaces:
;   sw      s0, 0x0018 (sp)
;   sw      a0, 0x0030 (sp)
;   sw      a1, 0x0034 (sp)
.org 0x80AFE41C
    sw      s0, 0x0018 (sp)
    jal     Models_DrawLabFishHeartPiece_Hook
    sw      a0, 0x0030 (sp)

;==================================================================================================
; Freestanding Models (Seahorse)
;==================================================================================================

.headersize G_EN_OT_DELTA

; Before Seahorse main function.
; Replaces:
;   sw      s0, 0x0018 (sp)
;   or      s0, a0, r0
;   sw      ra, 0x001C (sp)
.org 0x80B5DAF0
    sw      ra, 0x001C (sp)
    jal     Models_BeforeSeahorseMain_Hook
    sw      s0, 0x0018 (sp)

; Seahorse draw function.
; Replaces:
;   sw      s0, 0x0028 (sp)
;   or      s0, a0, r0
;   sw      ra, 0x002C (sp)
;   sw      a1, 0x0054 (sp)
.org 0x80B5DD24
    sw      ra, 0x002C (sp)
    sw      s0, 0x0028 (sp)
    jal     Models_DrawSeahorse_Hook
    or      s0, a0, r0

;==================================================================================================
; Freestanding Models (Shops)
;==================================================================================================

.headersize G_EN_GIRLA_DELTA

; Overwrite draw function call for shop inventory.
; Replaces:
;   jal     0x800EE320
.org 0x80864A14
    jal     Models_DrawShopInventory_Hook

;==================================================================================================
; Model Rotation (En_Item00)
;==================================================================================================

.headersize G_CODE_DELTA

; Allow rotating backwards for En_Item00 (Heart Piece).
; Replaces:
;   lh      t7, 0x00BE (s0)
;   lh      v1, 0x001C (s0)
;   addiu   t8, t7, 0x03C0
;   b       0x800A6550
;   sh      t8, 0x00BE (s0)
.org 0x800A6454
    jal     Models_RotateEnItem00
    nop
    lh      v1, 0x001C (s0)
    b       0x800A6550
    nop

; Allow rotating backwards for bouncing En_Item00.
; Replaces:
;   lh      t7, 0x00BE (s0)
;   addiu   t8, t7, 0x03C0
;   sh      t8, 0x00BE (s0)
.org 0x800A6674
    jal     Models_RotateEnItem00
    nop
    nop

; Allow items that normally don't rotate to rotate.
; Replaces:
;   LH      V1, 0x001C (S0)
;   SLTI    AT, V1, 0x0003
;   BNEZ    AT, 0x800A6454
;   ADDIU   AT, R0, 0x0003
;   BNEL    V1, AT, 0x800A6444
;   ADDIU   AT, R0, 0x0006
;   LH      T6, 0x0152 (S0)
;   BLTZ    T6, 0x800A6454
;   ADDIU   AT, R0, 0x0006
;   BEQ     V1, AT, 0x800A6454
;   ADDIU   AT, R0, 0x0007
;   BNEL    V1, AT, 0x800A646C
;   SLTI    AT, V1, 0x0016
.org 0x800A6420
.area 0x34
    jal     Models_ShouldEnItem00Rotate
    nop
    beqz    v0, 0x800A6468
    lh      v1, 0x001C (s0)
    nop
    nop
    nop
    nop
    nop
    nop
    nop
    nop
    nop
.endarea

;==================================================================================================
; Model Rotation (Skulltula Token)
;==================================================================================================

.headersize G_EN_SI_DELTA

; Allows rotating backwards for Skulltula Tokens.
; Replaces:
;   addiu   t2, t1, 0x038E
;   sh      t2, 0x00BE (a0)
.org 0x8098CBC4
    jal     Models_RotateSkulltulaToken
    nop

;==================================================================================================
; Model Rotation (Heart Container)
;==================================================================================================

.headersize G_ITEM_B_HEART_DELTA

; Allows rotating backwards for Heart Containers.
; Replaces:
;   lh      t6, 0x00BE (s0)
;   lui     a1, 0x3ECC
;   lui     a2, 0x3DCC
;   lui     a3, 0x3C23
;   addiu   t7, t6, 0x0400
;   sh      t7, 0x00BE (s0)
.org 0x808BCF68
.area 0x1C
    jal     Models_RotateHeartContainer
    nop
    nop
    lui     a1, 0x3ECC
    lui     a2, 0x3DCC
    lui     a3, 0x3C23
.endarea

;==================================================================================================
; Model Rotation (Lab Fish Heart Piece)
;==================================================================================================

.headersize G_EN_COL_MAN_DELTA

; Allows rotating backwards for Lab Fish Heart Piece.
; Replaces:
;   bnezl   t1, 0x80AFDE78
;   sw      a0, 0x0020 (sp)
;   lh      t2, 0x00BE (a0)
;   addiu   t3, t2, 0x03E8
;   sh      t3, 0x00BE (a0)
;   sw      a0, 0x0020 (sp)
.org 0x80AFDE60
.area 0x1C
    bnez    t1, 0x80AFDE78
    sw      a0, 0x0020 (sp)
    jal     Models_RotateLabFishHeartPiece
    sw      a1, 0x0024 (sp)
    lw      a0, 0x0020 (sp)
    lw      a1, 0x0024 (sp)
.endarea

;==================================================================================================
; Freestanding Models (Scopecoin)
;==================================================================================================

.headersize G_EN_SCOPECOIN_DELTA

; Scopecoin draw function.
; Replaces:
;   sw      s0, 0x0018 (sp)
;   sw      a0, 0x0038 (sp)
;   sw      a1, 0x003C (sp)
;   lw      t6, 0x003C (sp)
.org 0x80BFD184
    jal     Models_DrawScopecoin_Hook
    nop
    bnez    v0, 0x80BFD240
    nop

;==================================================================================================
; Model Rotation (Scopecoin)
;==================================================================================================

.headersize G_EN_SCOPECOIN_DELTA

; Replaces:
;   SW      A1, 0x0004 (SP)
;   LH      T6, 0x00BE (A0)
.org 0x80BFCFA0
    j       Models_RotateScopecoin
    nop

;==================================================================================================
; Freestanding Models (ScRuppe)
;==================================================================================================

.headersize G_EN_SC_RUPPE_DELTA

; ScRuppe draw function.
; Replaces:
;   sw      s0, 0x0018 (sp)
;   sw      a0, 0x0038 (sp)
;   sw      a1, 0x003C (sp)
;   lw      t6, 0x003C (sp)
.org 0x80BD6D20
    jal     Models_DrawScRuppe_Hook
    nop
    bnez    v0, 0x80BD6DDC
    nop

;==================================================================================================
; Model Rotation (ScRuppe)
;==================================================================================================

.headersize G_EN_SC_RUPPE_DELTA

; Replaces:
;   OR      A0, A2, R0
;   ADDIU   T2, T1, 0x01F4
;   JAL     0x800B6A88
;   SH      T2, 0x00BE (A2)
.org 0x80BD6AF8
    jal     0x800B6A88
    or      a0, a2, r0
    jal     Models_RotateScRuppe_Hook
    nop

;==================================================================================================
; Freestanding Models (Deku Playground Rupee)
;==================================================================================================

.headersize G_EN_GAMELUPY_DELTA

; Replaces:
;   sw      s0, 0x0018 (sp)
;   sw      a0, 0x0038 (sp)
;   sw      a1, 0x003C (sp)
;   lw      t6, 0x003C (sp)
.org 0x80AF6C00
    jal     Models_DrawDekuScrubPlaygroundRupee_Hook
    nop
    bnez    v0, 0x80AF6CBC
    nop

;==================================================================================================
; Model Rotation (Deku Playground Rupee)
;==================================================================================================

.headersize G_EN_GAMELUPY_DELTA

; Replaces:
;   ADDIU   T2, T1, 0x01F4
;   SH      T2, 0x00BE (A1)
.org 0x80AF6A24
    jal     Models_RotateDekuScrubPlaygroundRupee
    lw      a1, 0x001C (sp)

;==================================================================================================
; Freestanding Models (Masks)
;==================================================================================================

.headersize G_DM_CHAR05_DELTA

; Replaces:
;   jal     0x80133B3C
.org 0x80AADA5C
    jal     Models_DrawGoronMask

; Replaces:
;   jal     0x80133B3C
.org 0x80AADB18
    jal     Models_DrawZoraMask

; Replaces:
;   jal     0x80133F28
.org 0x80AADBCC
    jal     Models_DrawGibdoMask


;==================================================================================================
; Freestanding Models (Hero's Shield)
;==================================================================================================

.headersize G_CODE_DELTA

; Replaces:
;   jal     0x800EE320
.org 0x800A726C
    jal     Models_DrawItem00Shield

.org 0x800A6088
    .dw 0x00000000

;==================================================================================================
; Freestanding Models (Ocarina)
;==================================================================================================

.headersize G_DM_CHAR02_DELTA

; Replaces:
;   jal     0x801343C0
.org 0x80AAB370
    jal     Models_DrawOcarina

.headersize G_DM_STK_DELTA
; Replaces:
;   lw      v0, 0x02B0 (a1)
;   lui     t9, 0x0601
;   addiu   t9, t9, 0xCAD0
;   addiu   t4, v0, 0x0008
;   sw      t4, 0x02B0 (a1)
;   sw      t9, 0x0004 (v0)
;   sw      t2, 0x0000 (v0)
.org 0x80AA32B4
    lw      a0, 0x00A0 (sp)
    jal     Models_DrawOcarinaLimb
    or      a1, s0, r0
    lw      a1, 0x0030 (sp)
    nop
    nop
    nop
