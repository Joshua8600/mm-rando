﻿using System;
using System.Linq;

namespace MMR.Randomizer.Attributes
{
    public class ChestAttribute : Attribute
    {
        public int[] Addresses { get; private set; }
        public AppearanceType Type { get; private set; }

        public ChestAttribute(int address, AppearanceType type = AppearanceType.Normal, params int[] additionalAddresses)
        {
            Addresses = additionalAddresses.Concat(new[] { address }).ToArray();
            Type = type;
        }

        public enum AppearanceType
        {
            Normal = 0,
            Invisible = 1,
            AppearsClear = 2,
            AppearsSwitch = 3,
        }

        public static byte GetType(ChestTypeAttribute.ChestType chestType, AppearanceType appearanceType)
        {
            var type = (byte)chestType;
            type <<= 2;
            type += (byte)appearanceType;
            return type;
        }
    }
}
