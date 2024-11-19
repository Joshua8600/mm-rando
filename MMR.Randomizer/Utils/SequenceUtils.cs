﻿using MMR.Randomizer.Constants;
using MMR.Randomizer.Models.Rom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Cryptography;
using MMR.Randomizer.Models.Settings;
using MMR.Randomizer.Models;
using MMR.Common.Utils;
using MMR.Randomizer.Asm;
using System.Text.RegularExpressions;
using MMR.Randomizer.Extensions;

namespace MMR.Randomizer.Utils
{
    /// <summary>
    /// Used for reading from JSON file
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("[{Name}]")]
    public class ReplacementSongSlot
    {
        public string          Name { get; set; }
        public string          Notes { get; set; }
        public List<string>    SceneFIDToModify { get; set; } // String because we sometimes need to provide a scene layer offset to find the right header to change
        public List<string>    NewSongSlotCategories { get; set; } // string because they are base 16 which json cannot handle
    }

    public class SequenceUtils
    {
        // these are places the player may never visit, if they do they are visited very briefly, and very little music is heard
        // 0F:sharp kills you, 05:clock tower, 7C:giantsleave, 04:skullkid theme
        // 42:gormon brothers, 27:musicbox house, 31:mayor's office, 45:kaepora's theme
        // 72:wagonride, 0E:boatcruise, 29:zelda, 2D:giants, 
        // 2E:guruguru, 7B:maskreveal(gaints summon cutscene), 73:keaton, 70:calling giants
        // 7D is reunion, 0x50 is sword school
        //public static List<int> lowUseMusicSlots = new List<int> { 0x0F, 0x05, 0x7C, 0x04,
        //                                                           0x42, 0x27, 0x31, 0x45,
        //                                                           0x72, 0x0E, 0x29, 0x2D,
        //                                                           0x2E, 0x7B, 0x73, 0x70, 0x7D, 0x50 };

        public static int MAX_BGM_BUDGET            = 0x6000; // vanilla: 0x3800
        public static int MAX_COMBAT_BUDGET         = 0x6000; // unk
        public static int MAX_TYPE2_MUSIC_BUDGET    = 0x6000; // vanilla: 0x4100

        public static int New_AudioBankTable = 0; // for mmfilelist
        public static int NewInstrumentSetAddress; // for bgm shuffle functions to work on
        public static int CurrentFreeBank = 0x29;
        public const int MARK_REQUIRES_NEW_BANK = 0x28; // 28 used to be the only free bank, this is legacy supported

        public static MD5 md5lib; // used for zip


        public static void ResetBudget()
        {
            MAX_BGM_BUDGET          = 0x6000;
            MAX_COMBAT_BUDGET       = 0x6000;
            MAX_TYPE2_MUSIC_BUDGET  = 0x6000;
        }

        public static void ReadSequenceInfo()
        {
            /// Reads the Seqs.txt file for sequence information about the game and user configuration

            md5lib = MD5.Create();

            RomData.SequenceList = new List<SequenceInfo>();
            RomData.TargetSequences = new List<SequenceInfo>();

            // if file exists, we read the file instead of the resource
            string[] lines;
            if (File.Exists(Path.Combine(Values.MusicDirectory, "SEQS.txt")))
            {
                Debug.WriteLine("We found a user SEQS.txt file that we can use");
                var list = new List<string>();
                string line;
                using (StreamReader sr = new StreamReader(Path.Combine(Values.MusicDirectory, "SEQS.txt")))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        list.Add(line);
                    }
                }
                lines = list.ToArray();
            }
            else
            {
                // load SEQS.txt from source memory
                lines = Properties.Resources.SEQS.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }

            // multiple directory search
            var directories = new List<string>();
            if (!Directory.Exists(Values.MusicDirectory))
            {
                Directory.CreateDirectory(Values.MusicDirectory); // we still need for MM-only
            }
            directories.AddRange(Directory.GetDirectories(Values.MusicDirectory).ToList());
            foreach (string d in directories.ToList()) // another layer deep to be safe
            {
                List<String> deeper_directories = Directory.GetDirectories(d).ToList();
                directories.AddRange(deeper_directories);
            }
            directories.Add(Values.MusicDirectory);

            foreach (String directory in directories)
            {
                int i = 0;
                while (i < lines.Length)
                {
                    try
                    {
                        string sourceName = lines[i];
                        List<int> sourceType = new List<int>();
                        foreach (String part in lines[i + 1].Split(','))
                        {
                            sourceType.Add(Convert.ToInt32(part, 16));
                        }

                        int sourceInstrument = Convert.ToInt32(lines[i + 2], 16);

                        var targetName = lines[i];
                        var targetType = sourceType;
                        var targetInstrument = Convert.ToInt32(lines[i + 2], 16);

                        SequenceInfo sourceSequence = new SequenceInfo
                        {
                            Name = sourceName,
                            Categories = sourceType,
                            Instrument = sourceInstrument
                        };

                        SequenceInfo targetSlot = new SequenceInfo
                        {
                            Name = targetName,
                            Categories = targetType,
                            Instrument = targetInstrument
                        };

                        // this is legacy: adding sequences to the seqs.txt file that arent vanilla songs is no longer useful
                        if (sourceSequence.Name.StartsWith("mm-"))
                        {
                            targetSlot.Replaces = Convert.ToInt32(lines[i + 3], 16);
                            sourceSequence.MM_seq = Convert.ToInt32(lines[i + 3], 16);
                            if (i + 4 < lines.Length && lines[i + 4] == "no-recycle")
                            {
                                //Debug.WriteLine("Player does not want to reuse song: " + sourceSequence.Name);
                                sourceSequence.Name = "drop";
                                i += 1;
                            }
                            i += 4;
                            if (RomData.TargetSequences.Find(u => u.Name == sourceName) != null)
                            {
                                continue; //old already have it
                            }
                            sourceSequence.Directory = "";

                            RomData.TargetSequences.Add(targetSlot);
                        }
                        else
                        {
                            i += 3;
                            if (File.Exists(Path.Combine(directory, sourceName)) == false)
                            {
                                // if sequence file doesn't exist, was removed by user, ignore it
                                continue;
                            }
                            sourceSequence.Directory = directory;
                        };

                        if (sourceSequence.MM_seq != 0x18 && sourceSequence.Name != "drop")
                        {
                            RomData.SequenceList.Add(sourceSequence);
                        };
                    }
                    catch (Exception e)
                    {
                        string aboveLines = "";
                        string nl = "\n";
                        if (i > 3)
                        {
                            aboveLines += lines[i - 3] + nl + lines[i - 2] + nl + lines[i - 1] + nl;
                        }
                        throw new Exception( "Error while reading SEQS.txt:\n"
                                           + e.Message + "\n\n"
                                           + "Caused by the line with the arrow:\n\n"
                                           + aboveLines
                                           + lines[i] + "  <--\n"
                                           + lines[i+1] + nl + lines[i+2] + nl + lines[i+3]);
                    }
                } // end while (i < lines.Length)

                // MMR changes the music that plays when the player uses SOT
                // however the original SOT doesn't have a unique sequence/slot, so we have to use an unused one
                RomData.SequenceList.Add(new SequenceInfo
                {
                    Name = nameof(Properties.Resources.mmr_f_sot),
                    Directory = "",
                    Categories = new List<int> { 8 },
                    Instrument = 3,
                    Replaces = 0x75,
                });

                ScanZSEQUENCE(directory); // scan for base zseq in music folder
                ScanForMMRS(directory); // scan for base mmrs in music folder
            }
        }

        private static int RoundTo16(int value)
        {
            return (value + 0xF) & ~0xF;
        }

        public static int GetSequenceSize(SequenceInfo seq)
        {
            // if it was loading at the MMRS read time, the zseq is already loaded in memory
            if (seq.SequenceBinaryList != null && seq.SequenceBinaryList.Count > 0)
            {
                return RoundTo16(seq.SequenceBinaryList[0].SequenceBinary.Length);
            }
            else if (seq.Name.StartsWith("mm-")) // vanilla mm, look up from audioseq index table
            {
                // we know code file is already decompressed at this point
                int codeFID = RomUtils.GetFileIndexForWriting(Addresses.SeqTable);
                var codeFile = RomData.MMFileList[codeFID];
                int audioseqIndexTableOffset = Addresses.SeqTable - codeFile.Addr;

                int entryaddr = audioseqIndexTableOffset + (seq.MM_seq * 16); // table entries are 16 bytes wide
                var size = (int) ReadWriteUtils.Arr_ReadU32(codeFile.Data, entryaddr + 4);
                return RoundTo16(size);
            }
            else // not already loaded, we have to search for the file and look it up
            {
                byte[] data;
                if (File.Exists(seq.Filename))
                {
                    using (var reader = new BinaryReader(File.OpenRead(seq.Filename)))
                    {
                        data = new byte[(int)reader.BaseStream.Length];
                        return RoundTo16(data.Length);
                    }
                }
            }

            throw new Exception($"GetSequenceSize: Sequence File is missing: [{seq.Filename}]");
        }

        public static void ScanZSEQUENCE(string directory)
        {
            /// check if files were added by user to music directory
            // format: FILENAME_InstrumentSet_Categories-separated-by-commas.zseq
            //  where the filename, instrumentset, and categories are separated by single underscore

            // This method of adding music is deprecated, MMRS has rendered this filetype unnecessary,
            // however I cant convince people to stop making zseq files, and legacy support is appreciated

            foreach (String filePath in Directory.GetFiles(directory, "*.zseq"))
            {
                String filename = Path.GetFileName(filePath);
                try
                {
                    // test if file has enough delimiters to separate data into name_bank_formats
                    String[] tokens = filename.Split('_');
                    if (tokens.Length != 3)
                    {
                        continue;
                    }

                    var sourceName = filename;
                    // for zseq, categories/instrument are part of the filename, we need to extract
                    string sourceCategoriesString = tokens[2].Substring(0, tokens[2].Length - 5);
                    int sourceInstrument = Convert.ToInt32(tokens[1], 16);
                    List<int> sourceCategories = new List<int>();
                    foreach (String part in sourceCategoriesString.Split('-'))
                    {
                        sourceCategories.Add(Convert.ToInt32(part, 16));
                    }

                    SequenceInfo sourceSequence = new SequenceInfo
                    {
                        Name = filename,
                        Directory = directory,
                        Categories = sourceCategories,
                        Instrument = sourceInstrument
                    };

                    RomData.SequenceList.Add(sourceSequence);
                }
                catch (FormatException)
                {
                    throw new Exception("Music: Filename is unparsable: " + filename);
                }
                catch (Exception e)
                {
                    throw new Exception("Music: could not read file: " + filename);
                }
            }
        }

        public static void ScanMMRSCategories(SequenceInfo song, ZipArchiveEntry categoriesFileEntry)
        {
            var categoryData = new StreamReader(categoriesFileEntry.Open(), Encoding.Default).ReadToEnd();
            var categoriesList = new List<int>();
            var delimitingChar = ',';
            if (categoryData.Contains(",") == false) // default delimiter missing, use alternatives
            {
                if (categoryData.Contains("-")) // someone will mess this up, its an easy thing to check for here tho
                {
                    delimitingChar = '-';
                }
                else if (categoryData.Contains("\n"))
                {
                    delimitingChar = '\n';
                }
            }

            foreach (var line in categoryData.Split(delimitingChar))
            {
                if (line == null || line.Length == 0) continue; // They probably left an extra comma at the end, not an error just ignore

                try
                {
                    categoriesList.Add(Convert.ToInt32(line.Trim(), 16));
                }
                catch // empty line wont convert or bad category value, ignore
                {
                    //#if RELEASE
                    //continue; // Release ignores music bugs and keeps going
                    //#endif
                    throw new Exception($"Error: Categories cannot be read: {song.Name}");
                }
            }

            song.Categories = categoriesList;
        }

        private static List<(ZipArchiveEntry, string)> ScanForMMRSSequenceFiles(ZipArchive zip)
        {
            /// the raw sequences can come in multiple file format names, all of these files are the same format but the name was not standardized
            List<(ZipArchiveEntry, string)> fileToupleList = new List<(ZipArchiveEntry, string)>();

            List<string> typeStrings = new List<string>
            {
                ".zseq", // for "zelda sequence" name was used heavily in Hylian Modding zelda romhacking community because of their focus on OOT
                ".seq",  // for "sequence" name was used by the Ocarina of Time Randomizer community
                ".aseq"  // for "audio sequence" name was used by the decompilation project after they realized the audio engine is shared with multiple Nintendo games
            };

            for(int i = 0; i < typeStrings.Count; i++)
            {
                string fileType = typeStrings[i];
                List<ZipArchiveEntry> entries = zip.Entries.Where(e => e.Name.Contains(fileType)).ToList();
                for(int f = 0; f < entries.Count; f++)
                {
                    var file = entries[f];
                    fileToupleList.Add((file, fileType));
                }
            }

            return fileToupleList;
        }

        private static int ScanForMMRSSequenceInstrumentSet(SequenceInfo song, string sequenceName, SequenceBinaryData combo, ZipArchive zip)
        {
            /// the instrument set named "zbank" is a binary, comes with a metadata file
            /// returns true/false if this sequence uses a bank... except because its c# and bool is not an int, we us int rather than use a convert class

            // this used to be filename
            var bankFileEntry = zip.GetEntry(sequenceName + ".zbank");
            if (bankFileEntry != null) // custom bank detected
            {
                // read bank file
                byte[] zBankData = new byte[bankFileEntry.Length];
                bankFileEntry.Open().Read(zBankData, 0, zBankData.Length);

                // read bankmeta file
                var bankmetaFileEntry = zip.GetEntry(sequenceName + ".bankmeta");
                var bankmetaData = new byte[bankmetaFileEntry.Length];
                bankmetaFileEntry.Open().Read(bankmetaData, 0, bankmetaData.Length);

                combo.InstrumentSet = new InstrumentSetInfo()
                {
                    BankBinary = zBankData,
                    BankSlot = song.Instrument,
                    BankMetaData = bankmetaData,
                    Modified = 1,
                    Hash = BitConverter.ToInt64(md5lib.ComputeHash(zBankData), 0),
                };
                return 1; // bank was used
            }

            return 0; // no bank
        }

        private static void ScanForMMRSFormMask(SequenceInfo song, string sequenceName, SequenceBinaryData combo, ZipArchive zip)
        {
            /// read form mask file, that controls which channels of the sequence turn on and off

            var formMaskFileEntry = zip.GetEntry(sequenceName + ".formmask");
            if (formMaskFileEntry != null)
            {
                using var reader = new StreamReader(formMaskFileEntry.Open(), Encoding.Default);
                var formMaskJson = reader.ReadToEnd();
                try
                {
                    // playState is configured in the file as "play in these states", but in the code it's "mute in these states"
                    // so we need to reverse it
                    var playState = JsonSerializer.Deserialize<SequencePlayState[]>(formMaskJson);

                    // ensure backwards compatibility with 1.15 sequences
                    if (!playState.Any(s => s.HasFlag(SequencePlayState.FierceDeity) && !s.HasFlag(SequencePlayState.Human)))
                    {
                        for (var i = 0; i < playState.Length; i++)
                        {
                            if (playState[i].HasFlag(SequencePlayState.Human))
                            {
                                playState[i] |= SequencePlayState.FierceDeity;
                            }
                        }
                    }

                    // ensure unused cumulative states don't cause music to get muted in those states
                    foreach (var cumulativeState in Enum.GetValues<SequencePlayState>().Where(s => s > SequencePlayState.All))
                    {
                        if (!playState.Any(s => s.HasFlag(cumulativeState)))
                        {
                            playState[0x10] |= cumulativeState;
                        }
                    }

                    combo.FormMask = ConvertUtils.U16ArrayToBytes(playState.Cast<ushort>().ToArray());
                }
                catch (Exception e)
                {
                    throw new Exception($"Sequence formmask file is invalid - {e.Message}", e);
                }
            }
        }

        private static void ScanMMRSSequences(SequenceInfo song, ZipArchive zip)
        {
            /// sequences mmr uses are all binary raw files that are precompiled 

            var claimedBankCount = 0;

            // each mmrs can have _multiple_ sequences, for redundancy
            var sequences = ScanForMMRSSequenceFiles(zip);
            foreach ((ZipArchiveEntry sequenceFile, string type) in sequences)
            {
                // read sequence binary file
                var rawSeqData = new byte[sequenceFile.Length];
                sequenceFile.Open().Read(rawSeqData, 0, rawSeqData.Length);
                var sequence = new SequenceBinaryData() { SequenceBinary = rawSeqData };

                // zseq filename is the instrument set
                var sequenceFilename = sequenceFile.Name.Substring(0, sequenceFile.Name.LastIndexOf(type));

                var commentSplit = sequenceFilename.Split('_'); // everything before _ is a comment, readability, discard here
                var fileNameInstrumentSet = commentSplit.Length > 1 ? commentSplit[commentSplit.Length - 1] : sequenceFilename;
                try
                {
                    song.Instrument = Convert.ToInt32(fileNameInstrumentSet, 16);
                }
                catch (FormatException e)
                {
                    song.Instrument = MARK_REQUIRES_NEW_BANK; // filename was not an intrument set at all, assume we need a new bank
                }

                var customBankIncluded = ScanForMMRSSequenceInstrumentSet(song, sequenceFilename, sequence, zip);

                // now that we have bank expansion working without known issues, bank overwriting causes more glitches for us than it helps
                if (song.Instrument > 0x28 || customBankIncluded == 1)
                {
                    song.Instrument = MARK_REQUIRES_NEW_BANK;
                    foreach (var seq in song.SequenceBinaryList.FindAll(set => set.InstrumentSet != null))
                    {
                        seq.InstrumentSet.BankSlot = song.Instrument;
                    }
                }
                if (song.Instrument == MARK_REQUIRES_NEW_BANK && customBankIncluded == 0)
                {
                    #if DEBUG
                    throw new Exception($"File with no bank has a bad sequence filename: {sequenceFilename}");
                    #else
                    continue; // currently, we do NOT throw errors for regular users
                    #endif
                }

                claimedBankCount += customBankIncluded;

                ScanForMMRSFormMask(song, sequenceFilename, sequence, zip); // TODO this probably doesn't have to run per-sequence               

                song.SequenceBinaryList.Add(sequence);
            }

            // sometime music makers forget that the bank and zseq files have to match,
            // if this happens, zseq files will be used without their banks being detected, and songs will play with the wrong instruments
            // normally, only the vanilla instruments from the bank will play, sometimes nothing plays
            if (claimedBankCount < zip.Entries.Where(e => e.Name.Contains("zbank")).Count())
            {
                throw new Exception($"ERROR: more banks than zseq found for {song.Name}\n (Probably a misnamed zseq)");
            }

        }


        public static void ScanForMMRS(string directory)
        {
            // check if user has added mmrs packed sequence files to the music folder
            //  mmrs is just a zip that has all the small files:
            //  the sequence itself, the categories, and the instrument set value (as part of the zseq name)
            //    if the song requires a custom audiobank, the bank and bank meta data are also here (where the name is the bank being replaced)
            //    if the song requires custom instrument samples, those are also here
            //  the user should be able to pack the archive with multiple sequences and multiple banks to match,
            //   where the redundancy increases likley hood of a song being able to be placed in a free audiobank slot

            foreach (string filePath in Directory.GetFiles(directory, "*.mmrs"))
            {
                try
                {
                    using (ZipArchive zip = ZipFile.OpenRead(filePath))
                    {
                        var currentSong = new SequenceInfo();
                        var splitFilePath = filePath.Split('\\');
                        currentSong.Name = splitFilePath[splitFilePath.Length - 1];
                        currentSong.Directory = directory;

                        // read categories file
                        ZipArchiveEntry categoriesFileEntry = zip.GetEntry("categories.txt");
                        if (categoriesFileEntry == null) {
                            // we should have folder detecting code here
                            var entry = zip.Entries.First(e => e.FullName.Contains("/"));
                            if (entry != null)
                            {
                                throw new Exception($"ERROR: cannot find categories.txt, mmrs contains folder?\n mmrs elements need to be in base of zip, not inside of folder");
                            }
                            throw new Exception($"ERROR: cannot find a categories.txt for {currentSong.Name}");
                        }

                        ScanMMRSCategories(currentSong, categoriesFileEntry);


                        // read list of sound samples
                        var samplesList = new List<SequenceSoundSampleBinaryData>();
                        foreach (ZipArchiveEntry zSoundFile in zip.Entries.Where(e => e.Name.Contains(".zsound")))
                        {
                            var sampleData = new byte[zSoundFile.Length];
                            zSoundFile.Open().Read(sampleData, 0, sampleData.Length);
                            var sampleNameSplit = zSoundFile.Name.Split('_'); // everything before _ is a comment, readability, discard here
                            var sampleName = sampleNameSplit.Length > 1 ? sampleNameSplit[sampleNameSplit.Length - 1] : zSoundFile.Name;
                            sampleName = sampleName.Split('.')[0];        // we don't need the filetype after here either at this point
                            uint sampleNameMarker = Convert.ToUInt32(sampleName, 16);
                            samplesList.Add(
                                new SequenceSoundSampleBinaryData()
                                {
                                    BinaryData = sampleData,
                                    Addr = sampleNameMarker,
                                    Marker = sampleNameMarker,
                                    Hash = BitConverter.ToInt64(md5lib.ComputeHash(sampleData), 0)
                                }
                            );
                        }
                        currentSong.InstrumentSamples = samplesList;
                        currentSong.SequenceBinaryList = new List<SequenceBinaryData>();

                        ScanMMRSSequences(currentSong, zip);

                        if (currentSong != null && currentSong.SequenceBinaryList != null)
                        {
                            RomData.SequenceList.Add(currentSong);
                        }

                    }
                }
                catch (Exception e)
                {
                    throw new Exception($"Error attempting to read archive: {filePath} -- \n" + e);
                }
            }
        }

        public static void PointerizeSequenceSlots(OutputSettings _settings, RandomizedResult _results, CosmeticSettings cosmeticSettings)
        {
            // if music availablilty is low, pointerize some slots
            // why? because in Z64 fairy fountain and fileselect are the same song,
            //  with one being a pointer at the other, so we have 78 slots and 77 songs, not enough
            //  also some categories can get exhausted leaving slots unfillable with remaining music,
            // several slots that players will never/rarely hear are nullified (pointed at another song)
            // this "fills" those slots, now we have fewer slots to fill with remaining music (77 fills 73)
            //  so pointerized slots play the same music, and don't waste a song
            //  but if the player does find this music in-game, it still plays sufficiently random music
            //ConvertSequenceSlotToPointer(0x29, 0x0B); // point zelda(SOTime get cs) at healed

            // with shortened cutscenes, we pointerize more slots that the player would not hear
            // if using a patch, _randomized is not set, lookup a shortened cutscene byte instead
            // =========================================================
            // File: 0x02CBF000, Address: 0x02CBFD48, Offset: 0x00000D48
            // Name: Z2_KONPEKI_ENT::Great Bay(Cutscene) -Scene File
            // =========================================================
            // Replaces:
            //   .dw 0x00010294  94
            // .orga 0x02CBFD48     ->
            //   .dw 0x00010000        00
            // checking if not 94 instead if 00 because 94 is vanilla and 00 is replacement
            //  thinking ahead, it's possible the adjusted value will change one day, but vanilla is static
            // if the file's data is null, nothing in that file was changed and therefore it is vanilla.
            bool shortenedCutscenes = RomData.MMFileList[1472].Data[0xD48 + 3] != 0x94;

            if (shortenedCutscenes)
            {
                // these cutscene songs are never heard if shorten cutscenes is enabled, just pointerize it
                ConvertSequenceSlotToPointer(0x72, 0x45); // point wagonride at kaeopora 
                //ConvertSequenceSlotToPointer(0x2D, 0x3A); // point giants world (oath get cutscene) at observatory
                ConvertSequenceSlotToPointer(0x70, 0x0B); // point call the giants( cutscene confronting skullkid) at healed
                ConvertSequenceSlotToPointer(0x7B, 0x0D); // point maskreveal, the song that plays when the mask shows its alive during moon cutscene, at aliens
                //ConvertSequenceSlotToPointer(0x7D, 0x05); // point reunion at clocktower
            }

            // if ocarina is NOT randomized, pointerize skullkid's seq since it gets used nowhere
            // ocarina rando applies patch:fix_ocarina_checks, use a change applied by this fix to detect
            // ============================================================================
            // File: 0x00B3C000, Address: 0x00BC66A0, Offset: 0x0008A6A0, Patch: 0x000000A8
            // Name: code
            // ============================================================================
            // Replaces:
            //   .dh    0x14F9  F9
            // .org 0x80130160    ->
            //   .dh    0x1000       00
            bool ocarinaNotRandomized = RomData.MMFileList[31].Data[0x8A6A0 + 1] == 0xF9;

            if (ocarinaNotRandomized)
            {
                ConvertSequenceSlotToPointer(0x04, 0x1A); // point skullkid's theme, during skullkid's backstory cutscene, at combat
            }

            // if our replacement pool is small (MM only and low variety) pointerize a few more
            if (RomData.TargetSequences.Count + 30 > RomData.SequenceList.Count)
            {
                ConvertSequenceSlotToPointer(0x76, 0x15); // point titlescreen at clocktownday1
                ConvertSequenceSlotToPointer(0x08, 0x09); // point chasefail(skullkid chase) at fail
                ConvertSequenceSlotToPointer(0x19, 0x78); // point clearshort(epona get cs) at dungeonclearshort
            }

            // create some pointerized slots that are otherwise ignored, beacuse this pool gets re-used later for new song slots
            ConvertSequenceSlotToPointer( 0x1E, 0x76, "mm-introcutscene1");
            ConvertSequenceSlotToPointer(0x58, 0xB, "mm-mikaustory1"); // healed

            // got a report this isn't working, although I tested it myself, what is happening...
            //ConvertSequenceSlotToPointer(0x60, 0xB, "mm-old-final-hours-pointer");

            // think this one breaks healing goron child, as this pointer is used for post-playing fanfares I think
            //ConvertSequenceSlotToPointer("mm-goron-lulliby-intro-pointer", 0x61, 0xB);

            // to pointerize milk bar we have to change the obj_sound actor in themilkbar
            ConvertSequenceSlotToPointer(seqSlotIndex: 0x56, substituteSlotIndex:0x1F, "mm-milk-bar-pointer"); // house
            if (_results.Settings.RandomizeEnemies)
            {
                var milkbarScene = RomData.SceneList.Find(scene => scene.File == GameObjects.Scene.MilkBar.FileID());
                milkbarScene.Maps[0].Actors[17].Variants[0] = 0x13C; // from 0x156, the pointer, to 3C the actual milkbar song
            }
            else
            {
                // except we can't do this in enemizer we can't know for certain anyone is playing with both
                RomUtils.CheckCompressed(GameObjects.Scene.MilkBar.FileID() + 1);
                var milkbarData = RomData.MMFileList[GameObjects.Scene.MilkBar.FileID() + 1].Data;
                milkbarData[0x193] = 0x3C; // obj_sound (actor 17) parameter from 0x156 to 0x13C (where 56 is milkbar ptr, 3C is actual milkbar slot)
            }

            // if combat music is disabled, that slot should be usable
            if (cosmeticSettings.DisableCombatMusic == true) // I think this is what zoey has us using currently??
            {
                ConvertSequenceSlotToPointer(0x1A, 0x38); // combat pointed at miniboss, but its going to be replaced more than likely
            }

            // if fanfares are disabled, can we pointerize some of those slots?
            if (cosmeticSettings.DisableFanfares == true) // I think this is what zoey has us using currently??
            {
                ConvertSequenceSlotToPointer(0x22, 0x08); // get item -> event failure
                ConvertSequenceSlotToPointer(0x24, 0x08); // get heart container -> event failure
                ConvertSequenceSlotToPointer(0x37, 0x08); // get mask-> event failure
                ConvertSequenceSlotToPointer(0x39, 0x08); // get small item  -> event failure
            }

        }

        public static void ConvertSequenceSlotToPointer(int seqSlotIndex, int substituteSlotIndex, string name = "")
        {
            // turns the sequence slot into a pointer, which points at another song, at SubstituteSlotIndex
            // the slot at SeqSlotIndex is marked such that, instead of a new sequence being put there
            //  a pointer to another song, at SubstituteSlotIndex, is used instead.
            // this frees up a song slot but its not completely empty if someone finds it
            //  this is the same concept DB used to nulify the intro song
            var targetSeq = RomData.TargetSequences.Find(u => u.Replaces == seqSlotIndex);
            var substituteSeq = RomData.TargetSequences.Find(u => u.Replaces == substituteSlotIndex);
            if (targetSeq != null && substituteSeq != null)
            {
                targetSeq.PreviousSlot = targetSeq.Replaces; // we'll need at audioseq build
                targetSeq.Replaces = substituteSeq.Replaces; // point the target at the substitute
                RomData.PointerizedSequences.Add(targetSeq); // save the sequence for audioseq
                RomData.TargetSequences.Remove(targetSeq);   // close the slot
            }
            else
            {
                //throw new IndexOutOfRangeException("Could not convert slot to pointer:" + SeqSlotIndex.ToString("X2"));
                Debug.WriteLine("Adding new pointer slot for un-shuffled music: " + seqSlotIndex.ToString("X") + " and " + substituteSlotIndex.ToString("X"));
                RomData.PointerizedSequences.Add(new SequenceInfo() { Name = name, MM_seq = seqSlotIndex, PreviousSlot = seqSlotIndex, Replaces = substituteSlotIndex });
            }
        }


        // gets passed RomData.SequenceList in Builder.cs::WriteAudioSeq
        public static void RebuildAudioSeq(List<SequenceInfo> sequenceList, int? sequenceMaskFileIndex, int? sequenceNamesFileIndex)
        {
            // spoiler log output DEBUG
            StringBuilder log = new StringBuilder();
            void WriteOutput(string str)
            {
                Debug.WriteLine(str); // we still want debug output though
                log.AppendLine(str);
            }

            List<MMSequence> oldSeq = new List<MMSequence>();
            int f = RomUtils.GetFileIndexForWriting(Addresses.SeqTable);
            int basea = RomData.MMFileList[f].Addr;

            for (int i = 0; i < 128; i++)
            {
                MMSequence entry = new MMSequence();

                int entryaddr = Addresses.SeqTable + (i * 16);
                entry.Addr = (int)ReadWriteUtils.Arr_ReadU32(RomData.MMFileList[f].Data, entryaddr - basea);
                var size = (int)ReadWriteUtils.Arr_ReadU32(RomData.MMFileList[f].Data, (entryaddr - basea) + 4);
                if (size > 0)
                {
                    entry.Data = new byte[size];
                    Array.Copy(RomData.MMFileList[4].Data, entry.Addr, entry.Data, 0, entry.Size);
                }
                else
                {
                    int j = sequenceList.FindIndex(u => u.Replaces == i);
                    if (j != -1)
                    {
                        if ((entry.Addr > 0) && (entry.Addr < 128))
                        {
                            if (sequenceList[j].Replaces != 0x28) // 28 (fairy fountain)
                            {
                                sequenceList[j].Replaces = entry.Addr;
                            }
                            else
                            {
                                entry.Data = oldSeq[0x18].Data;
                            }
                        }
                    }
                }
                oldSeq.Add(entry);
            }

            List<MMSequence> newSeq = new List<MMSequence>();
            int addr = 0;
            byte[] newAudioSeq = new byte[0];
            for (int i = 0; i < 128; i++)
            {
                MMSequence newentry = new MMSequence();
                if (oldSeq[i].Size == 0)
                {
                    newentry.Addr = oldSeq[i].Addr;
                }
                else
                {
                    newentry.Addr = addr;
                }

                if (sequenceList.FindAll(u => u.Replaces == i).Count > 1)
                {
                    WriteOutput("Error: Slot " + i.ToString("X") + " has multiple songs pointing at it!");
                }

                int p = RomData.PointerizedSequences.FindIndex(u => u.PreviousSlot == i);
                int j = sequenceList.FindIndex(u => u.Replaces == i);
                if (p != -1)
                {
                    // found song we want to pointerize
                    newentry.Addr = RomData.PointerizedSequences[p].Replaces;
                }
                else if (j != -1)
                {
                    // new song to replace old slot found
                    if (sequenceList[j].MM_seq != -1)
                    {
                        newentry.Data = oldSeq[sequenceList[j].MM_seq].Data;
                        WriteOutput("Slot " + i.ToString("X2") + " -> " + sequenceList[j].Name);

                    }
                    else if (sequenceList[j].SequenceBinaryList != null && sequenceList[j].SequenceBinaryList.Count > 0)
                    {
                        if (sequenceList[j].SequenceBinaryList.Count > 1)
                        {
                            WriteOutput("Warning: writing song with multiple sequence/bank combos, selecting first available");
                        }
                        newentry.Data = sequenceList[j].SequenceBinaryList[0].SequenceBinary;
                        WriteOutput("Slot " + i.ToString("X2") + " := " + sequenceList[j].Name + " *");

                    }
                    else // non mm, load file and add
                    {
                        byte[] data;
                        if (File.Exists(sequenceList[j].Filename))
                        {
                            using (var reader = new BinaryReader(File.OpenRead(sequenceList[j].Filename)))
                            {
                                data = new byte[(int)reader.BaseStream.Length];
                                reader.Read(data, 0, data.Length);
                            }
                        }
                        else if (sequenceList[j].Name == nameof(Properties.Resources.mmr_f_sot))
                        {
                            data = Properties.Resources.mmr_f_sot;
                        }
                        else
                        {
                            throw new Exception("Music not found as file or built-in resource." + sequenceList[j].Filename);
                        }

                        // I think this checks if the sequence type is correct for MM
                        //  because DB ripped sequences from SF64/SM64/MK64 without modifying them
                        if (data[1] != 0x20)
                        {
                            data[1] = 0x20;
                        }

                        newentry.Data = data;
                        WriteOutput("Slot " + i.ToString("X2") + " := " + sequenceList[j].Name);

                    }
                }
                else // not found, song wasn't touched by rando, just transfer over
                {
                    newentry.Data = oldSeq[i].Data;
                }

                // if the sequence is not padded to 16 bytes, the DMA fails
                //  music can stop from playing and on hardware it will just straight crash
                var padding = 0x10 - newentry.Size % 0x10;
                if (padding != 0x10)
                {
                    newentry.Data = newentry.Data.Concat(new byte[padding]).ToArray();
                }

                newSeq.Add(newentry);
                // TODO is there not a better way to write this?
                if (newentry.Data != null)
                {
                    newAudioSeq = newAudioSeq.Concat(newentry.Data).ToArray();
                }

                addr += newentry.Size;
            }

            // discovered when MM-only music was fixed, if the audioseq is left in it's old spot
            // audio quality is garbage, sounds like static
            //if (addr > (RomData.MMFileList[4].End - RomData.MMFileList[4].Addr))
            //else
            //RomData.MMFileList[4].Data = NewAudioSeq;

            int index = RomUtils.AppendFile(newAudioSeq);
            ResourceUtils.ApplyHack(Resources.mods.reloc_audio);
            RelocateSeq(index);
            RomData.MMFileList[4].Data = new byte[0];
            RomData.MMFileList[4].Addr = RomData.MMFileList[4].End;
            RomData.MMFileList[4].Cmp_Addr = -1;
            RomData.MMFileList[4].Cmp_End = -1;

            //update sequence index pointer table
            f = RomUtils.GetFileIndexForWriting(Addresses.SeqTable);
            for (int i = 0; i < 128; i++)
            {
                ReadWriteUtils.Arr_WriteU32(RomData.MMFileList[f].Data, (Addresses.SeqTable + (i * 16)) - basea, (uint)newSeq[i].Addr);
                ReadWriteUtils.Arr_WriteU32(RomData.MMFileList[f].Data, 4 + (Addresses.SeqTable + (i * 16)) - basea, (uint)newSeq[i].Size);
            }

            //update inst sets used by each new seq
            // this is NOT the audiobank, its the complementary instrument set value for each sequence
            //   IE, sequence 7 uses instrument set "10", we replaced it with sequnece ae which needs bank "23"
            f = RomUtils.GetFileIndexForWriting(Addresses.InstSetMap);
            basea = RomData.MMFileList[f].Addr;
            for (int i = 0; i < 128; i++)
            {
                // huh? paddr? pointer? padding?
                int paddr = (Addresses.InstSetMap - basea) + (i * 2) + 2;

                int j = -1;
                if (newSeq[i].Size == 0) // pointer, we need to copy the instrumnet set from the destination
                {
                    j = sequenceList.FindIndex(u => u.Replaces == newSeq[i].Addr);
                }
                else
                {
                    j = sequenceList.FindIndex(u => u.Replaces == i);
                }

                byte[] formMask = null;
                string name = null;

                if (j != -1)
                {
                    RomData.MMFileList[f].Data[paddr] = (byte)sequenceList[j].Instrument;

                    if (sequenceMaskFileIndex.HasValue)
                    {
                        formMask = sequenceList[j].SequenceBinaryList?.FirstOrDefault()?.FormMask;
                    }

                    if (sequenceNamesFileIndex.HasValue)
                    {
                        name = Path.GetFileNameWithoutExtension(sequenceList[j].Name);
                        if (Path.GetExtension(sequenceList[j].Name) == ".zseq")
                        {
                            name = name.Split('_')[0];
                        }
                        if (name.Contains("songforce"))
                        {
                            name = Regex.Replace(name, "(\\W|^)songforce\\W", string.Empty);
                            name = name.Replace("songforce", string.Empty);
                        }
                    }
                }

                if (sequenceMaskFileIndex.HasValue)
                {
                    if (formMask == null)
                    {
                        formMask = Enumerable.Repeat<byte>(0xFF, 0x20).ToArray();
                    }
                    Array.Resize(ref formMask, MusicConfig.SEQUENCE_DATA_SIZE);
                    ReadWriteUtils.Arr_Insert(formMask, 0, MusicConfig.SEQUENCE_DATA_SIZE, RomData.MMFileList[sequenceMaskFileIndex.Value].Data, i * MusicConfig.SEQUENCE_DATA_SIZE);
                }

                if (sequenceNamesFileIndex.HasValue)
                {
                    name ??= "";
                    if (name.Length > MusicConfig.SEQUENCE_NAME_MAX_SIZE - 1)
                    {
                        name = name.Substring(0, MusicConfig.SEQUENCE_NAME_MAX_SIZE - 4) + "...";
                    }
                    name += "\0";
                    var nameBytes = Encoding.ASCII.GetBytes(name);
                    Array.Resize(ref nameBytes, MusicConfig.SEQUENCE_NAME_MAX_SIZE);
                    ReadWriteUtils.Arr_Insert(nameBytes, 0, MusicConfig.SEQUENCE_NAME_MAX_SIZE, RomData.MMFileList[sequenceNamesFileIndex.Value].Data, i * MusicConfig.SEQUENCE_NAME_MAX_SIZE);
                }
            }

            //// DEBUG spoiler log output
            //String dir = Path.GetDirectoryName(_settings.OutputROMFilename);
            //String path = $"{Path.GetFileNameWithoutExtension(_settings.OutputROMFilename)}";
            //// spoiler log should already be written by the time we reach this far
            //if (File.Exists(Path.Combine(dir, path + "_SpoilerLog.txt")))
            //    path += "_SpoilerLog.txt";
            //else // TODO add HTML log compatibility
            //    path += "_SongLog.txt";

            //using (StreamWriter sw = new StreamWriter(Path.Combine(dir, path), append: true))
            //{
            //    sw.WriteLine(""); // spacer
            //    sw.Write(log);
            //}
        }

        /// <summary>
        /// Patch instructions to use new sequence data file.
        /// </summary>
        /// <param name="f">File index</param>
        /// <remarks>
        /// In memory: 0x80190E5C
        /// Replaces:
        ///   lui     a1, 0x0004
        ///   addiu   a1, a1, 0x6AF0
        /// With:
        ///   lui     t0, 0x800A
        ///   lw      a1, offset (t0)
        /// Note: File table in memory starts at 0x8009F8B0.
        /// </remarks>
        private static void RelocateSeq(int f)
        {
            var fileTable = 0xF8B0;
            var offset = (fileTable + (f * 0x10) + 8) & 0xFFFF;
            ReadWriteUtils.WriteToROM(0x00C2739C, new byte[] { 0x3C, 0x08, 0x80, 0x0A, 0x8D, 0x05, (byte)(offset >> 8), (byte)(offset & 0xFF) });
        }

        public static void MoveAudioBankTable()
        {
            // grab original audiobanktable out of code, plus extra for modifying
            var table = ReadWriteUtils.ReadBytes(0xB3C000 + 0x13B6C0, 0x820);
            // Move to unused fbdemo.c (0x80163DC0)
            ReadWriteUtils.WriteToROM(0xB3C000 + 0xBE300, table);

            ReadWriteUtils.WriteU16ToROM(0xB3C000 + 0xBE300, 0x0080); // Increase AudioBankTable amount
            ReadWriteUtils.WriteCodeUInt32(0x80190E18, 0x3C0A8016);
            ReadWriteUtils.WriteCodeUInt32(0x80190E28, 0x254A3DC0);
            NewInstrumentSetAddress = 0xB3C000 + 0xBE300 + 0x10;

            // clear old audiobank
            var zero = new byte[0x2A0];
            ReadWriteUtils.WriteToROM(0xB3C000 + 0x13B6C0, zero);

            // instrumentset_patch: modifies audiobank metadata read and writes, instrument/drum/sfx pointer read and writes,
            // nops a metadata copy function, and sets a fixed size for the audiobank pointer index
            ResourceUtils.ApplyHack(Resources.mods.instrumentset_patch);

            // moveaudiostatebytes: sets where read and writes for sequence and instrumentset states go
            // in this hack, they're moved from 0x80205008 to end of old instrumentset table in code and given more space
            // if these don't get moved, new banks at 0x30 and up will overflow into sequence states and can knock out sound
            ResourceUtils.ApplyHack(Resources.mods.moveaudiostatebytes);


            // insert dummy metadata (kamaro's dance bank duplicates)
            int dummybankindexOffset = NewInstrumentSetAddress + 0x280;
            int totaldummybanks = 0x58;
            ulong dummybankmetadata0 = 0x00021880000000D0;
            ulong dummybankmetadata1 = 0x020101FF01000000;

            for (int dummybankIndex = 0; dummybankIndex <= totaldummybanks; ++dummybankIndex)
            {
                ReadWriteUtils.WriteU64ToROM(dummybankindexOffset, dummybankmetadata0);
                ReadWriteUtils.WriteU64ToROM(dummybankindexOffset + 0x08, dummybankmetadata1);
                dummybankindexOffset += 0x10;
            }
            
        }

        public static void ResetFreeBankIndex()
        {
            CurrentFreeBank = 0x29;
        }


        public static bool TestIfAvailableBanks(SequenceInfo testSeq, SequenceInfo targetSlot, StringBuilder log, Random rng, List<SequenceInfo> unassignedSequences)
        {
            /// test if the testSeq can be used with available instrument set slots

            // check if custom instrument sets exist for this sequence
            if (testSeq.SequenceBinaryList != null && testSeq.SequenceBinaryList.Count > 0 && testSeq.SequenceBinaryList.Any(u => u.InstrumentSet != null))
            {
                // randomize instrument sets last second, so the early banks don't get ravaged based on order
                if (testSeq.SequenceBinaryList.Count > 1)
                {
                    testSeq.SequenceBinaryList = testSeq.SequenceBinaryList.OrderBy(x => rng.Next()).ToList();
                }

                //testSeq.ClearUnavailableBanks(); // clear the sequence list of {bank/sequence} we cannot use
                //if (testSeq.SequenceBinaryList.Count == 0) // all removed, song is dead.
                //{
                //    log.AppendLine($"{ testSeq.Name,-50}  cannot be used because it requires custom audiobank(s) already claimed ");
                //    unassignedSequences.Remove(testSeq);
                //    return false;
                //}

                var testBanks = testSeq.CheckAvailableBanks();
                if (testBanks == true) 
                {
                    testSeq.ClearUnavailableBanks(); // remove any already claimed bank sequences
                }
                else // all custom banks have been claimed
                {
                    if (CurrentFreeBank > 0x0080)
                    {
                        return false; // can't overwrite any more entries
                    }

                    testSeq.SequenceBinaryList[0].InstrumentSet.BankSlot = CurrentFreeBank;
                }

                // some slots are rarely heard in-game, dont waste a custom instrument set on them, check if this slot is one of them
                //if (IsBlockedByLowUse(testSeq, targetSlot, log))
                //{
                //    return false;
                //}
            }
            return true; // sequences with banks, or without needing banks, available
        }

        // depreciated with expanded banks
        /*public static bool IsBlockedByLowUse(SequenceInfo testSeq, SequenceInfo targetSlot, StringBuilder log)
        {
            /// if the slot we are checking is a rarely used slot, and this song requires a custom instrument
            ///  skip so we don't waste precious instrument set slots on rarely heard music
            /// exception: if the song uses the unique song category, not general purpose; author wanted it to be in this slot, bypass
            var uniqueCategory = targetSlot.Replaces + 0x100;
            if (lowUseMusicSlots.Contains(targetSlot.Replaces) && !testSeq.Categories.Contains(uniqueCategory)
                && !testSeq.SequenceBinaryList.Any(u => u.InstrumentSet == null))
            {
                var previouslyUsedBanks = testSeq.SequenceBinaryList.FindAll(u => (u.InstrumentSet.Hash != 0 && u.InstrumentSet.Hash == RomData.InstrumentSetList[u.InstrumentSet.BankSlot].Hash));
                if (previouslyUsedBanks.Count >= 1)
                {
                    // exception: if another song already chosen has claimed a bank this song can use
                    log.AppendLine($"{ testSeq.Name,-50}  was accepted despite low use because a usable bank was already selected from another song");
                    return false;
                }

                if (targetSlot.Categories[0] < 8) // to reduce spam, limit logging this only to the regular categories
                {
                    log.AppendLine($"{testSeq.Name,-50}  skipped for slot {targetSlot.Replaces.ToString("X2")} because it's a low use slot and requires a custom bank");
                }

                return true;
            }

            return false;
        }// */

        public static void TryBackupSongPlacement(SequenceInfo targetSlot, StringBuilder log, List<SequenceInfo> unassignedSequences, OutputSettings settings)
        {
            /// sometimes, the remaining song slots can't find a compatible song, here we loosen restrictions until we can build a seed

            // first attempt: just merge BGM and fanfare into super categories and attempt to find replacement
            // the first category of the type is the MAIN type, the rest are secondary
            SequenceInfo replacementSong = null;
            if (targetSlot.Categories[0] <= 7 || targetSlot.Categories[0] == 0x16)  // bgm or cutscene
            {
                replacementSong = unassignedSequences.Find(u => u.Categories[0] <= 7 || u.Categories[0] == 0x16);
            }
            else //if (targetSlot.Type[0] <= 8) // fanfares
            {
                replacementSong = unassignedSequences.Find(u => u.Categories[0] >= 8 && u.Categories[0] < 0x10);
            }

            if (replacementSong != null)
            {
                log.AppendLine(" * generalized replacement with " + replacementSong.Name + " song, with categories: " + String.Join(", ", replacementSong.Categories.Select(x => "0x" + x.ToString("X2"))));
                AssignSequenceSlot(targetSlot, replacementSong, unassignedSequences, "APROX", log);
                return;
            }

            // second attempt, copy a song already used
            replacementSong = RomData.SequenceList.Find(u => u.Categories.Intersect(targetSlot.Categories).Any());
            if (replacementSong != null)
            {
                RomData.SequenceList.Add
                (
                    new SequenceInfo
                    {
                        Name = replacementSong.Name,
                        Directory = replacementSong.Directory,
                        MM_seq = replacementSong.MM_seq,
                        Categories = replacementSong.Categories,
                        Instrument = replacementSong.Instrument,
                        SequenceBinaryList = replacementSong.SequenceBinaryList,
                        PreviousSlot = replacementSong.PreviousSlot,
                        Replaces = targetSlot.Replaces
                    }
                );

                log.AppendLine(" * double dipping with song " + replacementSong.Name + ", with categories: " + String.Join(", ", replacementSong.Categories.Select(x => "0x" + x.ToString("X2"))));
                log.AppendLine($"{targetSlot.Name, -40} {"COPY", +10} -> " + replacementSong.Name);
                return;
            }

            // should not make it this far, throw error
            log.AppendLine(" out of remaining songs:");
            foreach (SequenceInfo RemainingSong in unassignedSequences)
            {
                log.AppendLine(" * [" + RemainingSong.Name + "] with categories [" + String.Join(",", RemainingSong.Categories) + "]");
            }
            WriteSongLog(log, settings);
            throw new Exception($"Cannot randomize music on this seed with available music: \nSlot Name:[{targetSlot.Name}] PreviousSlot: [{targetSlot.Replaces.ToString("X")}]");
        }

        public static void WriteSongLog(StringBuilder log, OutputSettings settings)
        {
            String dir = Path.GetDirectoryName(settings.OutputROMFilename);
            String path = $"{Path.GetFileNameWithoutExtension(settings.OutputROMFilename)}";

            // spoiler log should already be written by the time we reach this far
            // if no text log, create separate song log (mostly for mmr users)
            if (File.Exists(Path.Combine(dir, path + "_SpoilerLog.txt")))
            {
                path += "_SpoilerLog.txt";
            }
            else
            {
                path += "_SongLog.txt";
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(dir, path), append: true))
            {
                sw.WriteLine(""); // spacer between spoiler log and song log
                sw.Write(log);
            }
        }

        private static (int sequenceBankIndex, int bankListIndex) FindMatchingInstrumentSetDuplicate(SequenceInfo replacementSequence)
        {
            for(int b = 0; b < replacementSequence.SequenceBinaryList.Count(); b++)
            {
                var bank = replacementSequence.SequenceBinaryList[b].SequenceBinary;
                if (bank != null)
                {
                    var searchResult = RomData.InstrumentSetList.FindIndex(match => match.BankBinary == bank);
                    if (searchResult != -1)
                    {
                        return (b, searchResult);
                    }
                }
            }

            return (-1,-1);
        }

        public static void AssignSequenceSlot(SequenceInfo slotSequence, SequenceInfo replacementSequence, List<SequenceInfo> remainingSequences, string debugString, StringBuilder log)
        {
            // if the song has a custom instrument set: lock the sequence, update inst set value, debug output
            if (replacementSequence.SequenceBinaryList != null && replacementSequence.SequenceBinaryList[0] != null && replacementSequence.SequenceBinaryList[0].InstrumentSet != null)
            {
                (int sequenceBankIndex, int bankListIndex) duplicateBankSearch = FindMatchingInstrumentSetDuplicate(replacementSequence);
                if (duplicateBankSearch.sequenceBankIndex != -1)
                {
                    RomData.InstrumentSetList[duplicateBankSearch.bankListIndex].Modified += 1;
                    replacementSequence.Instrument = duplicateBankSearch.bankListIndex;
                    log.AppendLine(" -- v -- Instrument set number " + replacementSequence.Instrument.ToString("X2") + " is being reused -- v --");
                    replacementSequence.SequenceBinaryList = new List<SequenceBinaryData> {
                        replacementSequence.SequenceBinaryList[duplicateBankSearch.sequenceBankIndex]
                    };
                }
                else // no duplicate bank found, add new one
                {
                    replacementSequence.Instrument = CurrentFreeBank++; // update to the one we want to use
                    replacementSequence.SequenceBinaryList[0].InstrumentSet.BankSlot = replacementSequence.Instrument;
                    RomData.InstrumentSetList[replacementSequence.Instrument] = replacementSequence.SequenceBinaryList[0].InstrumentSet;
                    RomData.InstrumentSetList[replacementSequence.Instrument].InstrumentSamples = replacementSequence.InstrumentSamples;
                    log.AppendLine(" -- v -- Instrument set number " + replacementSequence.Instrument.ToString("X2") + " has been claimed -- v --");
                    replacementSequence.SequenceBinaryList = new List<SequenceBinaryData> { replacementSequence.SequenceBinaryList[0] }; // reduce to one for later
                }
            }

            replacementSequence.Replaces = slotSequence.Replaces; // tells the rando later what song to put into slot_seq
            //the -40 and +10 pad out the text to allign on the same middle section for visual clarity
            log.AppendLine($"{slotSequence.Name, -40} {debugString, +10} -> " + replacementSequence.Name);
            remainingSequences.Remove(replacementSequence);
        }

        public static void CheckSongTest(List<SequenceInfo> sequences, StringBuilder log)
        {
            /// For song makers: songtest is a debug token found in the song filename
            ///  specifies to flood the seed with the music for testing

            SequenceInfo songtestSequence = RomData.SequenceList.Find(u => u.Name.Contains("songtest") == true);
            if (songtestSequence == null)
            {
                return;
            }

            // we always put songtest music on fileselect, titlescreen, ctd1, and combat music
            SequenceInfo fileselectSlot = RomData.TargetSequences.Find(u => u.Name.Contains("mm-fileselect"));
            AssignSequenceSlot(fileselectSlot, songtestSequence, sequences, "SONGTEST", log); // file select

            // since we know songtest is the focus, we can adjust the budget first at the start
            var songtestSize = GetSequenceSize(songtestSequence);
            if (songtestSequence.Categories.Contains(5) || songtestSequence.Categories.Contains(0x1A))
            {
                MAX_COMBAT_BUDGET = songtestSize;
                MAX_BGM_BUDGET = MAX_TYPE2_MUSIC_BUDGET - MAX_COMBAT_BUDGET;
            }
            // else if not fanfare
            else if (!(songtestSequence.Categories.Contains(8) || songtestSequence.Categories.Contains(9)
                        || songtestSequence.Categories.Contains(0x10) || songtestSequence.Categories.Contains(0x16)))
            {
                MAX_BGM_BUDGET = songtestSize;
                MAX_COMBAT_BUDGET = MAX_TYPE2_MUSIC_BUDGET - MAX_BGM_BUDGET;
            }

            ConvertSequenceSlotToPointer(0x76, 0x18);  // titlescreen
            ConvertSequenceSlotToPointer(0x15, 0x18);  // clocktown 1

            // in addition, we take all song slots that share a category with the song and add those too
            var allMatchingSlots = RomData.TargetSequences.FindAll(u => u.Categories.Intersect(songtestSequence.Categories).Any());
            allMatchingSlots.Remove(fileselectSlot); // dont re-pointerize it
            foreach (SequenceInfo songslot in allMatchingSlots)
            {
                ConvertSequenceSlotToPointer(songslot.Replaces, 0x18);
            }
            RomData.TargetSequences.Remove(fileselectSlot);

            // in addition, because songs that use custom banks replace the original bank
            //  by design, the replacement bank should be a super set of the original, and old songs should still work
            //  however, sometimes, the old instruments in the new bank are broken, we need to test this
            // we will test this by turning the lottery into a new song slot with a vanilla song using the songtest bank

            if (songtestSequence.SequenceBinaryList == null)
            {
                return; // not a song with a custom bank, we can leave early
            }

            void ConvertRoomForSongTest(int sceneFID, int roomFID, int actorIDOffset, int musicOffset, List<SequenceInfo> replacementSequences)
            {
                if (replacementSequences.Count > 0)
                {
                    // pull a sequence from randomized list
                    var validSequence = replacementSequences[0];
                    // we have a list of slots we pointerize, we're recycling those
                    var newSongSlot = RomData.PointerizedSequences[0].PreviousSlot;
                    RomData.PointerizedSequences.RemoveAt(0);
                    // update sequence to use the slot
                    validSequence.Replaces = newSongSlot;
                    replacementSequences.Remove(validSequence);
                    sequences.Remove(validSequence);
                    log.AppendLine($" -- ^ -- Instrument set number {validSequence.Instrument.ToString("X2")} also used by {validSequence.Name}");

                    // set the scene to use this new song slot for background music
                    RomUtils.CheckCompressed(sceneFID);
                    var scene = RomData.MMFileList[sceneFID].Data;
                    scene[musicOffset] = (byte) newSongSlot;
                }
                // mute the previous music by killing the sfx actor which plays the filtered shop music
                RomUtils.CheckCompressed(roomFID);
                var room = RomData.MMFileList[roomFID].Data;
                room[actorIDOffset]   = 0xFF; // kill the sfx actor by setting its room slot ID to -1
                room[actorIDOffset+1] = 0xFF;
            }

            // generate a list of sequences that use the vanilla version of the bank our songtest replaces
            var sharedBankSequences = RomData.SequenceList.FindAll(u => u.Instrument == songtestSequence.Instrument);
            sharedBankSequences.Remove(songtestSequence);
            sharedBankSequences.Remove(fileselectSlot);   // file select was already set, so the values are broken
            sharedBankSequences.RemoveAll(u => u.SequenceBinaryList != null);
            Random newRandom = new Random();
            sharedBankSequences = sharedBankSequences.OrderBy(x => newRandom.Next()).ToList(); // random shuffle

            ConvertRoomForSongTest(sceneFID: 1334, 1335, actorIDOffset: 0x98, 0x7, sharedBankSequences); // lottery
            ConvertRoomForSongTest(sceneFID: 1158, 1159, actorIDOffset: 0x88, 0x7, sharedBankSequences); // honey and darling
            ConvertRoomForSongTest(sceneFID: 1188, 1189, actorIDOffset: 0x88, 0x7, sharedBankSequences); // treasure shop
            ConvertRoomForSongTest(sceneFID: 1502, 1503, actorIDOffset: 0xC4, 0x7, sharedBankSequences); // bomb shop
        }

        public static void CheckSongForce(List<SequenceInfo> sequences, StringBuilder log, Random rng)
        {
            /// songs with 'songforce' are higher priorty and get shuffled to the top of the previously randomized list of sequences
            List<SequenceInfo> forcedSequences = RomData.SequenceList.FindAll(u => u.Name.Contains("songforce")
                                                                                || u.Directory.Contains("songforce"));

            forcedSequences = forcedSequences.OrderBy(x => rng.Next()).ToList(); // re-randomize order per seed
            if (forcedSequences != null && forcedSequences.Count > 0)
            {
                foreach (SequenceInfo seq in forcedSequences)
                {
                    log.AppendLine("Forcing song (" + seq.Name + ") to top of the song pool");
                    sequences.Remove(seq);
                    sequences.Insert(0, seq);
                }
            }
        }

        public static bool SearchForValidSongReplacement(CosmeticSettings cosmeticSettings, List<SequenceInfo> unassignedSequences, SequenceInfo targetSlot, Random rng, StringBuilder log)
        {
            // we could replace this with a findall(compatible types) but then we lose the small chance of random category music
            foreach (var testSeq in unassignedSequences.ToList())
            {
                // increases chance of getting non-mm music, but only if we have lots of music remaining
                // disable until we can modify this in UI, as there is now enough music it feels unnecessary
                //if (unassigned.Count > 77 && testSeq.Name.StartsWith("mm") && testSeq.Type[0] < 0x100 && (random.Next(100) < 40))
                //    continue;

                // check if this song still has available banks or sequences, if not remove song and continue
                var songNotStarved = TestIfAvailableBanks(testSeq, targetSlot, log, rng, unassignedSequences);
                if (songNotStarved == false)
                {
                    continue; // song unacceptable, continue
                }

                var maxSize = targetSlot.Replaces == 0x1A ? MAX_COMBAT_BUDGET : MAX_BGM_BUDGET;
                var seqSize = GetSequenceSize(testSeq);
                if (seqSize > maxSize)
                {
                    continue; // too big
                }

                // do the target slot and the possible match seq share a category?
                if (testSeq.Categories.Intersect(targetSlot.Categories).Any())
                {
                    AssignSequenceSlot(targetSlot, testSeq, unassignedSequences, "", log);
                    return true;
                }

                // Deathbasket wanted there to be a small chance of getting out of category music
                //  but not put fanfares into bgm, or visa versa
                // also restrict this nature to when there is plenty of music to work with
                // (testSeq.Categories.Count > targetSlot.Categories.Count) DBs code, maybe thought to be safer?
                else if (unassignedSequences.Count > 30
                    && testSeq.Categories.Count > targetSlot.Categories.Count
                    && cosmeticSettings.MusicLuckRollChance > 0 && (decimal)(rng.NextDouble() * 100.0) < cosmeticSettings.MusicLuckRollChance
                    && targetSlot.Categories[0] <= 0x16
                    && testSeq.Categories[0] <= 0x16
                    && (testSeq.Categories[0] & 8) == (targetSlot.Categories[0] & 8)
                    && testSeq.Categories.Contains(0x10) == targetSlot.Categories.Contains(0x10)
                    && !testSeq.Categories.Contains(0x16))
                {
                    AssignSequenceSlot(targetSlot, testSeq, unassignedSequences, "LUCK", log);
                    return true;
                }
            }
            return false; // ran out of songs to try
        }

        public static void CheckBGMCombatMusicBudget(CosmeticSettings cosmeticSettings, List<SequenceInfo> unassignedSequences, Random rng, StringBuilder log)
        {
            /// in any scene, BGM and Combat music share the same buffer, loading to the other side,
            /// if their sum is greater than the size of the buffer they clip into each other when one loads, this kills one, usually bgm

            var combatSequences     = RomData.SequenceList.FindAll(u => u.Categories.Contains(5));
            var BGMSlots            = RomData.TargetSequences.FindAll(u => u.Categories.Contains(0) || u.Categories.Contains(2) || u.MM_seq == 0x12); // 0x12 is deku palace, which has an enemy
            var usedBGMSequences    = new List<SequenceInfo>();
            foreach (var slot in BGMSlots)
            {
                var searchResult = RomData.SequenceList.Find(u => u.Replaces == slot.Replaces);
                if (searchResult != null)
                {
                    usedBGMSequences.Add(searchResult);
                }
            }

            // we pick combat or BGM as the limiting factor, the other have to be smaller than what we chose
            bool combatVsBGMCoinToss = rng.Next(2) == 1;

            var usedCombatSequence = RomData.SequenceList.Find(u => u.Replaces == 0x1A && u.Name != "mm-combat");
            if (usedCombatSequence == null) // songtest removes the sequence, points it at fileselect for testing
            {
                combatVsBGMCoinToss = true; // "COMBAT" manually selected because of combat songtest
                usedCombatSequence = RomData.SequenceList.Find(u => u.Replaces == 0x18 && u.Name != "mm-fileselect");
            }
            else if (RomData.SequenceList.Find(u => u.Name.Contains("songtest")) != null)
            {
                combatVsBGMCoinToss = false; // "BGM" manually selected because of non-combat songtest
            }

            if (cosmeticSettings.DisableCombatMusic)
            {
                combatVsBGMCoinToss = false; // "BGM" manually selected because combat music is disabled.
            }

            var combatSize = GetSequenceSize(usedCombatSequence);

            string coinResult = (combatVsBGMCoinToss ? ("COMBAT") : ("BGM"));
            log.AppendLine($" SECOND PASS: Scanning for oversized BGM or combat cointoss: ({coinResult})");

            if (combatVsBGMCoinToss) // Combat chosen
            {
                if (usedBGMSequences.Count <= 0)
                {
                    return;
                }
                // get new BGM budget from combat file
                var newBGMBudget = MAX_BGM_BUDGET = MAX_TYPE2_MUSIC_BUDGET - combatSize;
                log.AppendLine($" new BGM budget: {MAX_BGM_BUDGET.ToString("X")}, from combat size: {combatSize.ToString("X")}");

                // per BGM sequence
                foreach (var seq in usedBGMSequences)
                {
                    var bgmSeqSize = GetSequenceSize(seq);
                    if (bgmSeqSize > newBGMBudget)
                    {
                        var seqName = seq.Name;
                        log.AppendLine($"BGM sequence {seqName} was too big to match your combat music, replacing ... ");
                        var bgmSlot = RomData.TargetSequences.Find(u => u.Replaces == seq.Replaces);
                        seq.Replaces = -1; // cancel using this song
                        bool status = SearchForValidSongReplacement(cosmeticSettings, unassignedSequences, bgmSlot, rng, log);
                        if (status == false)
                        {
                            throw new Exception("Music Budget Error: this seed cannot find acceptable music for this combat slot\n" +
                                "Try another!");
                        }
                    }
                }

            }
            else // BGM chosen
            {
                // get new combat budget by comparing to largest BGM
                var largestBGMSize = usedBGMSequences.Max(s => (int?)GetSequenceSize(s)) ?? 0;
                log.AppendLine($" new Combat budget: {MAX_COMBAT_BUDGET.ToString("X")} from bgm size: {largestBGMSize.ToString("X")}");

                // per BGM sequence
                var newCombatBudget = MAX_COMBAT_BUDGET = MAX_TYPE2_MUSIC_BUDGET - largestBGMSize;
                if (combatSize > newCombatBudget)
                { 
                    var seqName = usedCombatSequence.Name;
                    log.AppendLine($"Combat sequence {seqName} was too big to match your BGM music, replacing ... ");
                    var combatSlot = RomData.TargetSequences.Find(u => u.Name == "mm-combat");
                    if (combatSlot == null) // it was already removed, no reason to deal with it
                        return;
                    usedCombatSequence.Replaces = -1; // cancel using this song
                    bool status = SearchForValidSongReplacement(cosmeticSettings, unassignedSequences, combatSlot, rng, log);
                    if (status == false)
                    {
                        throw new Exception("Music Budget Error: this seed cannot find acceptable music for this combat slot\n" +
                            "Try another!");
                    }
                }

            }
        }

        public static bool SearchAndReplaceSceneBGM(string sceneFIDStr, byte slotReplacement)
        {
            /// scan the files for the header that contains scene music (0x15 first byte)

            // however, the header data we want COULD be in a different scene layer and we want to be able to search from the offset into the file
            int sceneFID = -1;
            var strSplit = sceneFIDStr.Split(":");
            if (int.TryParse(strSplit[0], out var sceneIntattempt))
            {
                sceneFID = sceneIntattempt;
            }
            if (sceneFID == -1) return false;

            // if we are searching to replace a scene layer, we need to know the offset, it wont be at the start of the scene file
            uint searchOffset = 0;
            if (strSplit.Length > 1)
            {
                searchOffset = Convert.ToUInt32(strSplit[1], 16);

            }

            RomUtils.CheckCompressed(sceneFID);
            var sceneData = RomData.MMFileList[sceneFID].Data;
            for (int b = (int)searchOffset; b < searchOffset + (0x10 * 70); b += 8)
            {
                // sound settings header command: 15xx0000 0000yyzz where zz is the sequence pointer byte
                if (sceneData[b] == 0x15) 
                {
                    Debug.WriteLine($"[{sceneFID}]Replacing previous music index byte: " + sceneData[b + 0x7].ToString("X2") + " at offset:" + b);
                    sceneData[b + 0x7] = slotReplacement;

                    return true;
                }
                if (sceneData[b] == 0x14)
                {
                    throw new Exception("Error: adding new song slots ran out of space and coult not modify anything");
                    break; // this is the last header command, telling us the header is finished, don't waste time going deeper
                }
            }

            return false;
        }



        public static void ReassignSongSlots(StringBuilder log, Random rng)
        {
            /// new extra song slots
            // read all assginement slots from json files
            var replacementSongSlots = new List<ReplacementSongSlot>();
            // resources folder got nuked, for now lets assume this will be in the main folder
            foreach (var filePath in Directory.GetFiles(".", "*AdditionalSongSlots.json"))
            {
                try
                {
                    var filetext = File.ReadAllText(filePath);
                    // the json string enum converter lets us read strings as songnames instead of int(slots) for readability
                    var buildingList = JsonSerializer.Deserialize<List<ReplacementSongSlot>>(filetext);
                    //var buildingList = System.Text.Json.JsonSerializer.Deserialize<List<ReplacementSongSlot>>(filetext, JsonSerializer._AdditionalSequenceSerializerOptions);

                    Debug.Assert(buildingList[0].Name != null);
                    Debug.Assert(buildingList[0].NewSongSlotCategories != null);
                    replacementSongSlots = replacementSongSlots.Concat(buildingList).ToList();
                }
                catch (IOException ex)
                {
                    Debug.WriteLine("Error reading file: " + filePath + ", error: " + ex.ToString());
                }
                catch (System.Text.Json.JsonException e)
                {
                    Debug.WriteLine("Error reading json: " + filePath + ", error: " + e.ToString());
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error unknown error: " + filePath + ", error: " + e.ToString());
                }

            }

            // randomize the order of the list, in case we run out of slots which ones get used will be random per seed
            replacementSongSlots = replacementSongSlots.OrderBy(x => rng.Next()).ToList();

            // these are song slots that were previously pointed at a different slot to save space
            //  song slots that had songs we wanted to add to the pool, but because they are (nearly) unreachable in rando, putting music
            //  in those slots dooms the music to never be heard in the seed, it was increadibly common for me to want to listen to a song and it gets up here
            // so they got "pointed" at a different slot, if they were discovered in game because of glitches, they would still play the song they point at
            var availableSlots = RomData.PointerizedSequences.ToList();
            // note: there are multiple slots in the game that we cannot hear that are fanfares that could get added to this list if we need more
            // like the opening cutscene, and there are several pointed slots for clocktown we might be able to add

            for (int i = 0; availableSlots.Count > 0 && i < replacementSongSlots.Count && RomData.PointerizedSequences.Count > 0; i++)
            {
                ReplacementSongSlot newSongSlot = replacementSongSlots[i];
                SequenceInfo availableSlot = availableSlots.ElementAt(0);
                log.AppendLine($"++ Adding a new song slot: [{newSongSlot.Name}] at previously unused slot [{availableSlot.PreviousSlot.ToString("X2")}][{availableSlot.Name}]");
                availableSlots.RemoveAt(0);

                // replace the song byte for every scene in this replacement event
                foreach (string sceneFID in newSongSlot.SceneFIDToModify)
                {
                    bool result = SearchAndReplaceSceneBGM(sceneFID, (byte)availableSlot.PreviousSlot);
                    if (!result)
                    {
                        log.AppendLine("Error: could not replace the bgm byte in scene fid: " + sceneFID);
                    }
                }

                try
                {
                    SequenceInfo newMusicSlot = new SequenceInfo
                    {
                        // we add mmr- because if the user adds mm- it will confuse our weak parser later
                        Name = "mmr-" + newSongSlot.Name,
                        MM_seq = availableSlot.MM_seq, // previoyusly PreviousSlot
                        Replaces = availableSlot.PreviousSlot,
                        Categories = newSongSlot.NewSongSlotCategories.Select(int.Parse).ToList(),
                        Instrument = 0
                    };
                    RomData.TargetSequences.Add(newMusicSlot);
                }
                catch (Exception)
                {
                    log.AppendLine("Error while converting an additional song slot to a sequence for music rando, slot: " + newSongSlot.Name);
                }

                // astral observatory is a special case where going backwards, from observatory into the sewer is hard coded, we have to update that
                if (newSongSlot.SceneFIDToModify.Contains(GameObjects.Scene.AstralObservatory.FileID().ToString()))
                {
                    // in Audio_PlayObjSoundBgm it checks if you are going from astral observatory and back, and sets that ID in hardcode instead
                    var codeFile = RomData.MMFileList[31].Data;
                    // 0FBD04 801A17C4 0C06A26A */  jal  AudioSeq_QueueSeqCmd
                    // 0FBD08 801A17C8 3484003B */  ori   $a0, $a0, 003B <- replace 3B with our byte
                    codeFile[0xFBD08 + 3] = (byte) availableSlot.PreviousSlot;
                }

                RomData.PointerizedSequences.Remove(availableSlot);
            }
        } // */

        public static void ReadInstrumentSetList()
        {
            /// traverse the whole audiobank index and grab details about every bank
            ///  use those details to generate a list from the vanilla game that we can modify as needed
            RomData.InstrumentSetList = new List<InstrumentSetInfo>();
            // audiobankindex can go up to 0x80 with current extended bank table file
            for (int audiobankIndex = 0; audiobankIndex <= 0x80; ++audiobankIndex)
            {
                // each bank has one 16 byte sentence of data, first word is address, second is length, last 2 words metadata
                int audiobankIndexAddr = NewInstrumentSetAddress + (audiobankIndex * 0x10);
                int audiobankBankOffset = (ReadWriteUtils.ReadU16(audiobankIndexAddr) << 16) + ReadWriteUtils.ReadU16(audiobankIndexAddr + 2);
                int bankLength = (ReadWriteUtils.ReadU16(audiobankIndexAddr + 4) << 16) + ReadWriteUtils.ReadU16(audiobankIndexAddr + 6);

                byte[] bankMetadata = new byte[8];
                for (int b = 0; b < 8; ++b)
                {
                    bankMetadata[b] = ReadWriteUtils.Read(audiobankIndexAddr + 8 + b);
                }

                byte[] bankData = new byte[bankLength];
                for (int b = 0; b < bankLength; ++b)
                {
                    bankData[b] = ReadWriteUtils.Read(Addresses.Audiobank + audiobankBankOffset + b);
                }

                var newInstrumentSet = new InstrumentSetInfo
                {
                    BankSlot = audiobankIndex,
                    BankMetaData = bankMetadata,
                    BankBinary = bankData
                };

                RomData.InstrumentSetList.Add(newInstrumentSet);
            }
        }

        public static void UpdateBankInstrumentPointers(byte[] ROM)
        {
            /// the audiobank and new samples are already written to rom, now we need to go back and update all the pointers
            ///   since we cannot know where those samples are on the rom until A) the soundbank is written, and B) the sample file is written
            ///   because the pointer is an offset of the soundbank rom location, and both can shift in BuildRom()

            if (RomData.InstrumentSetList == null)
            {
                return;
            }

            int soundbankAddr = RomData.MMFileList[5].Cmp_Addr; // in vanilla it's 0x97f70 but MMR can shift it up because AudioSeq gets re-located
            int audiobankInstSetAddr = RomData.MMFileList[3].Cmp_Addr; // pointer to specific instrument set, starting with 0, updating per loop
            foreach (var instrumentset in RomData.InstrumentSetList)
            {
                if (instrumentset.InstrumentSamples != null && instrumentset.InstrumentSamples.Count > 0)
                {
                    foreach (var sample in instrumentset.InstrumentSamples)
                    {
                        // for each sample if the bank uses sample, lookup new rom for the sample
                        //   get soundbank offset for said address: address of collection + offset of specific sample in collection, minus soundbank
                        int soundbankSampleOffset = RomData.MMFileList[RomData.SamplesFileID].Cmp_Addr
                                                      + (int)RomData.ListOfSamples.Find(u => u.Hash == sample.Hash).Addr
                                                      - soundbankAddr;
                        byte[] sampleOffsetBytes = BitConverter.GetBytes(soundbankSampleOffset);
                        byte[] markerBytes = BitConverter.GetBytes(sample.Marker);

                        // find the location in the bank where the Sample Marker is
                        //   per byte, replace with the bytes from SampleOffsetBytes
                        for (int i = 0; i < (instrumentset.BankBinary.Length - 4); i += 1)
                        {
                            if (ROM[audiobankInstSetAddr + i + 0] == markerBytes[3]
                                && ROM[audiobankInstSetAddr + i + 1] == markerBytes[2]
                                && ROM[audiobankInstSetAddr + i + 2] == markerBytes[1]
                                && ROM[audiobankInstSetAddr + i + 3] == markerBytes[0])
                            {
                                ROM[audiobankInstSetAddr + i + 0] = sampleOffsetBytes[3];
                                ROM[audiobankInstSetAddr + i + 1] = sampleOffsetBytes[2];
                                ROM[audiobankInstSetAddr + i + 2] = sampleOffsetBytes[1];
                                ROM[audiobankInstSetAddr + i + 3] = sampleOffsetBytes[0];
                            }
                        }
                    }
                }
                audiobankInstSetAddr += instrumentset.BankBinary.Length;
            }
        }

        public static void WriteNewSoundSamples(List<InstrumentSetInfo> InstrumentSetList, OutputSettings settings)
        {
            /// Writing all of our new samples in a single file at the end
            //  in the event we run out of MMFile DMA indexes: These files dont need to be a hard file in the filesystem
            //  they can be placed anywhere after the soundbank starting address on rom, instrument sample lookup doesnt use file system
            //  adding to the file system is just useful for shifting in BuildROM()

            StringBuilder log = new StringBuilder();
            void WriteOutput(string str)
            {
                Debug.WriteLine(str);
                log.AppendLine(str);
            }
            string Hex(int input)
            {
                return "0x" + input.ToString("X");
            }

            // issue: we don't know right now where the samples will be written to, because BuildRom will shift the files
            //  for now we write samples, after audiobank/soundbank/samples are written, we'll update audiobank pointers
            // save extra soundsamples fid, and the samples with their data, for later (UpdateBankInstrumentPointers())
            int fid = RomData.SamplesFileID = RomUtils.AppendFile(new byte[0x0]);
            RomData.ListOfSamples = new List<SequenceSoundSampleBinaryData>();

            WriteOutput($" Sample Injections:\n===================");

            // for each custom instrument set that needs a custom sample
            foreach (InstrumentSetInfo instrumentSet in InstrumentSetList)
            {
                if (instrumentSet.InstrumentSamples != null && instrumentSet.InstrumentSamples.Count > 0)
                {
                    foreach (SequenceSoundSampleBinaryData sample in instrumentSet.InstrumentSamples)
                    {
                        // test if sample was already added by another song (OOT instruments for instance)
                        var previouslyWrittenSample = RomData.ListOfSamples.Find(u => sample.Hash == u.Hash);
                        if (previouslyWrittenSample == null) // if sample not already written once before
                        {
                            // get the rom addr of our new file, our file will start at the end of the last file
                            sample.Addr = (uint)RomData.MMFileList[fid].Data.Length;
                            // concat our sample to sample collection file
                            RomData.MMFileList[fid].Data = RomData.MMFileList[fid].Data.Concat(sample.BinaryData).ToArray();
                            // I don't know if samples need to be padded to 0x10 like sequences but might as well
                            int paddingRemainder = RomData.MMFileList[fid].Data.Length % 0x10;
                            if (paddingRemainder > 0)
                            {
                                RomData.MMFileList[fid].Data = RomData.MMFileList[fid].Data.Concat(new byte[paddingRemainder]).ToArray();
                            }
                            
                            WriteOutput($"New sample added for bank [{Hex(instrumentSet.BankSlot)}]\n - sample location: [{Hex((int)sample.Addr)}] size: [{Hex(sample.BinaryData.Length)}]");
                            RomData.ListOfSamples.Add(sample);
                        }
                        else // get address of previously used sample
                        {
                            WriteOutput($" ♻ Bank [{Hex(instrumentSet.BankSlot)}] is reusing existing sample: offset: [{Hex((int)previouslyWrittenSample.Addr)}]");
                            sample.Addr = previouslyWrittenSample.Addr;
                        }
                    }
                }
            }

            WriteSongLog(log, settings);
        }

        public static void RebuildAudioBank(List<InstrumentSetInfo> InstrumentSetList)
        {
            // get index for the old audiobank, we're putting it back in the same spot but letting it expand into audioseq's spot, which was moved to the end
            int fid = RomUtils.GetFileIndexForWriting(NewInstrumentSetAddress);
            // the DMA table doesn't point directly to the indextable on the rom, its part of a larger yaz0 file, we have to use an offset to get the address in the file
            int audiobankIndexOffset = NewInstrumentSetAddress - RomData.MMFileList[RomUtils.GetFileIndexForWriting(NewInstrumentSetAddress)].Addr;

            int audiobankBankOffset = 0;
            var audiobankData = new byte[0];

            // for each bank, concat onto the new bank byte object, update the table to match the new instrument sets
            // CurrentFreeBank is used so not all unused dummy audiobanks get written to rom
            for (int audiobankIndex = 0; audiobankIndex <= CurrentFreeBank; ++audiobankIndex)
            {
                var currentBank = InstrumentSetList[audiobankIndex];
                audiobankData = audiobankData.Concat(currentBank.BankBinary).ToArray();

                // update address of the bank in the index table
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 0] = (byte)((audiobankBankOffset & 0xFF000000) >> 24);
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 1] = (byte)((audiobankBankOffset & 0xFF0000) >> 16);
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 2] = (byte)((audiobankBankOffset & 0xFF00) >> 8);
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 3] = (byte)(audiobankBankOffset & 0xFF);

                // update length of the bank in the table
                int currentBankLength = currentBank.BankBinary.Length;
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 4] = (byte)((currentBankLength & 0xFF000000) >> 24);
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 5] = (byte)((currentBankLength & 0xFF0000) >> 16);
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 6] = (byte)((currentBankLength & 0xFF00) >> 8);
                RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 7] = (byte)(currentBankLength & 0xFF);

                // update metadata of the bank in the table
                for (int metadataIter = 0; metadataIter < 8; ++metadataIter)
                {
                    RomData.MMFileList[fid].Data[audiobankIndexOffset + (audiobankIndex * 16) + 8 + metadataIter] = currentBank.BankMetaData[metadataIter];
                }

                // adjust the address for the next bank to use
                audiobankBankOffset += currentBankLength;
                int paddingRemainder = currentBankLength % 0x10;
                if (paddingRemainder > 0) // in the event the user made an audiobank instrument set that isn't padded
                {
                    audiobankData = audiobankData.Concat(new byte[paddingRemainder + 0x10]).ToArray(); // padding with a spare 16 byte line sounds cheap enough to try
                    audiobankBankOffset += paddingRemainder;
                }
            }

            // write new audiobank back to file
            var audiobankFile = RomData.MMFileList[RomUtils.GetFileIndexForWriting(Addresses.Audiobank)];
            audiobankFile.Data = audiobankData;
            audiobankFile.End = audiobankFile.Addr + audiobankFile.Data.Length;
        }
    }
}
