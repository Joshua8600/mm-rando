#ifndef ITEMS_H
#define ITEMS_H

enum CustomItem {
    // Non-progressive upgrades
    CUSTOM_ITEM_ROYAL_WALLET       = 0xA4,
    CUSTOM_ITEM_MAGIC_POWER        = 0xA5,
    CUSTOM_ITEM_SPIN_ATTACK        = 0xA6,
    CUSTOM_ITEM_DOUBLE_DEFENSE     = 0xA7,
    CUSTOM_ITEM_STRAY_FAIRY        = 0xA8,
    // Progressive Upgrades
    CUSTOM_ITEM_PROGRESSIVE_BOW    = 0xA9,
    CUSTOM_ITEM_PROGRESSIVE_BOMBS  = 0xAA,
    CUSTOM_ITEM_PROGRESSIVE_STICKS = 0xAB,
    CUSTOM_ITEM_PROGRESSIVE_NUTS   = 0xAC,
    CUSTOM_ITEM_PROGRESSIVE_SWORD  = 0xAD,
    CUSTOM_ITEM_PROGRESSIVE_MAGIC  = 0xAE,
    CUSTOM_ITEM_PROGRESSIVE_WALLET = 0xAF,
    // Traps
    CUSTOM_ITEM_ICE_TRAP           = 0xB0,
    // Other
    CUSTOM_ITEM_CRIMSON_RUPEE      = 0xB1,
    CUSTOM_ITEM_NOTEBOOK_ENTRY     = 0xB2,
    CUSTOM_ITEM_BOMBTRAP           = 0xB3,
    CUSTOM_ITEM_FROG               = 0xB4,
    CUSTOM_ITEM_RUPOOR             = 0xB5,
    CUSTOM_ITEM_NOTHING            = 0xB6,
};

#endif // ITEMS_H
