BankAmount_BeforeHandleInput_Hook:
    addiu   sp, sp, -0x18
    sw      ra, 0x0014 (sp)

    jal     BankAmount_BeforeHandleInput
    sw      a0, 0x0018 (sp)
    lw      a0, 0x0018 (sp)

    ; Displaced code
    lui     t0, 0x0001
    addiu   a3, a0, 0x4908
    addu    s0, a3, t0

    lw      ra, 0x0014 (sp)
    jr      ra
    addiu   sp, sp, 0x18
