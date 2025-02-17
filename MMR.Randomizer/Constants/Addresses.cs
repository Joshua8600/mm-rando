﻿namespace MMR.Randomizer.Constants
{
    public static class Addresses
    {
        // these are addresses for tables that point to the specific objects for audio


        // not sure where DB got the instrument set map pointer, its not in seq64
        //  looks like its part of the Sequence Banks Map file, as that starts C77960 and has len 210
        public const int InstSetMap         = 0xC77A60; // pointer table: sequence -> instrumentset
        public const int AudioSequence      = 0x046AF0; // audioseq
        public const int SeqTable           = 0xC77B80; // audioseq table (70 + 0x10)
        public const int AudiobankTable     = 0xC776D0; // audiobank index (c0 + 0x10) (RAM: 801E1190)
        public const int Audiobank          = 0x020700;

        // every overylay type has a different table for dynamic file loading
        public const int EffectOverlayTable     = 0xC449E0;
        public const int ActorOverlayTable      = 0xC45510; // 0x10 is player, empty, metadata before this
        public const int ObjectList             = 0xC58C88;
        public const int GameStateOverlayTable  = 0xC53E50;
        public const int TransitionOverlayTable = 0xC670F0;

        // TODO add audiobank and soundbank pointers

        public const int CodeFile           = 0xB3C000; // "Code" is the name of the file
    }
}
