;==================================================================================================
; Freestanding Models (Heart Piece)
;==================================================================================================

.headersize(G_CODE_RAM - G_CODE_FILE)

; Heart Piece draw function call.
; Replaces:
;   jal     0x800A75B8
.org 0x800A7188
    jal     models_draw_heart_piece

;==================================================================================================
; Freestanding Models (Skulltula Token)
;==================================================================================================

.headersize(G_EN_SI_VRAM - G_EN_SI_FILE)

; Skulltula Token draw function.
; Replaces:
;   addiu   sp, sp, -0x18
;   sw      ra, 0x0014 (sp)
.org 0x8098CD0C
    j       models_draw_skulltula_token
    nop

;==================================================================================================
; Freestanding Models (Stray Fairy)
;==================================================================================================

.headersize(G_EN_ELFORG_VRAM - G_EN_ELFORG_FILE)

; Stray Fairy main function.
; Replaces:
;   sw      a0, 0x0018 (sp)
;   lw      t9, 0x022C (a0)
.org 0x80ACD7A0
    jal     models_before_stray_fairy_main_hook
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
    jal     models_draw_stray_fairy_hook
    sw      s0, 0x0028 (sp)

;==================================================================================================
; Freestanding Models (Heart Container)
;==================================================================================================

.headersize(G_ITEM_B_HEART_VRAM - G_ITEM_B_HEART_FILE)

; Heart Container draw function.
; Replaces:
;   or      a3, r0, r0
;   lw      a2, 0x0000 (a1)
.org 0x808BCFCC
    jal     models_draw_heart_container_hook
    nop

;==================================================================================================
; Freestanding Models (Boss Remains)
;==================================================================================================

.headersize(G_DM_HINA_VRAM - G_DM_HINA_FILE)

; Overwrite draw function call for Odolwa's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD54
    jal     models_draw_boss_remains_hook

; Overwrite draw function call for Goht's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD64
    jal     models_draw_boss_remains_hook

; Overwrite draw function call for Gyorg's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD74
    jal     models_draw_boss_remains_hook

; Overwrite draw function call for Twinmold's Remains.
; Replaces:
;   jal     0x800EE320
.org 0x80A1FD84
    jal     models_draw_boss_remains_hook

.headersize(G_CODE_RAM - G_CODE_FILE)

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
    jal     models_write_boss_remains_object_segment
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

.headersize(G_OBJ_MOON_STONE_VRAM - G_OBJ_MOON_STONE_FILE)

; Moon's Tear draw function.
; Replaces:
;   sw      s1, 0x0018 (sp)
;   or      s1, a1, r0
;   sw      ra, 0x001C (sp)
.org 0x80C06914
    sw      ra, 0x001C (sp)
    jal     models_draw_moons_tear_hook
    sw      s1, 0x0018 (sp)
