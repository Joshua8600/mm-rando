﻿using System;

namespace MMR.Randomizer.Attributes.Actor
{
    /// <summary>
    ///  Placement of Switch flags (0x80) in an Actor's X Rotation
    /// </summary>
    class SwitchFlagsPlacementXRotAttribute : Attribute { }

    /// <summary>
    ///  Placement of Switch flags (0x80) in an Actor's Z Rotation
    /// </summary>
    class SwitchFlagsPlacementZRotAttribute : Attribute { }

    /// <summary>
    ///  Placement of Switch flags (0x80) in an Actor's params/variant
    /// </summary>
    class SwitchFlagsPlacementAttribute : Attribute
    {
        public int Mask;
        public int Shift;

        public SwitchFlagsPlacementAttribute(int mask, int shift) {
            Mask = mask;
            Shift = shift;
        }
    }
}
