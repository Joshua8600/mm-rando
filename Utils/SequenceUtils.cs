﻿using MMRando.Constants;
using MMRando.Models;
using MMRando.Models.Rom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace MMRando.Utils
{
    public class SequenceUtils
    {

        public static void ReadSequenceInfo()
        {
            RomData.SequenceList = new List<SequenceInfo>();
            RomData.TargetSequences = new List<SequenceInfo>();

            string[] lines = Properties.Resources.SEQS
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            int i = 0;
            while (i < lines.Length)
            {
                var sourceName = lines[i];
                var sourceType = Array.ConvertAll(lines[i + 1].Split(','), int.Parse).ToList();
                var sourceInstrument = Convert.ToInt32(lines[i + 2], 16);

                var targetName = lines[i];
                var targetType = Array.ConvertAll(lines[i + 1].Split(','), int.Parse).ToList();
                var targetInstrument = Convert.ToInt32(lines[i + 2], 16);

                SequenceInfo sourceSequence = new SequenceInfo
                {
                    Name = sourceName,
                    Type = sourceType,
                    Instrument = sourceInstrument
                };

                SequenceInfo targetSequence = new SequenceInfo
                {
                    Name = targetName,
                    Type = targetType,
                    Instrument = targetInstrument
                };

                if (sourceSequence.Name.StartsWith("mm-"))
                {
                    targetSequence.Replaces = Convert.ToInt32(lines[i + 3], 16);
                    sourceSequence.MM_seq = Convert.ToInt32(lines[i + 3], 16);
                    RomData.TargetSequences.Add(targetSequence);
                    i += 4;
                }
                else
                {
                    if (sourceSequence.Name == "mmr-f-sot")
                    {
                        sourceSequence.Replaces = 0x33;
                    }

                    i += 3;

                    // if file doesn't exist, was removed by user, ignore
                    if (File.Exists(Values.MusicDirectory + targetName) == false)
                    {
                        //TODO write debug to the debug log
                        continue;
                    }

                };

                if (sourceSequence.MM_seq != 0x18)
                {
                    RomData.SequenceList.Add(sourceSequence);
                };
            }; // end while (i < lines.Length)

            // check if files were added by user to music folder
            // we're not going to check for non-zseq here until I find an easy way to do that
            //  Just going to trust users aren't stupid enough to think renaming a mp3 to zseq will work
            foreach (String filePath in Directory.GetFiles(Values.MusicDirectory, "*.zseq"))
            {
                String filename = Path.GetFileName(filePath);

                // test if file has enough delimiters to separate data into name_bank_formats
                String[] pieces = filename.Split('_');
                if (pieces.Length != 3)
                {
                    continue;
                }

                var sourceName = filename;
                var sourceTypeString = pieces[2].Substring(0, pieces[2].Length - 5);
                var sourceInstrument = Convert.ToInt32(pieces[1], 16);
                var sourceType = Array.ConvertAll(sourceTypeString.Split('-'), int.Parse).ToList();

                SequenceInfo sourceSequence = new SequenceInfo
                {
                    Name = sourceName,
                    Type = sourceType,
                    Instrument = sourceInstrument
                };

                RomData.SequenceList.Add(sourceSequence);
            }

        }

        // gets passed RomData.SequenceList in Builder.cs::WriteAudioSeq
        public static void RebuildAudioSeq(List<SequenceInfo> SequenceList)
        {
            List<MMSequence> OldSeq = new List<MMSequence>();
            int f = RomUtils.GetFileIndexForWriting(Addresses.SeqTable);
            int basea = RomData.MMFileList[f].Addr;

            for (int i = 0; i < 128; i++)
            {
                MMSequence entry = new MMSequence();
                if (i == 0x1E) // intro music when link gets ambushed
                {
                    entry.Addr = 2;
                    entry.Size = 0;
                    OldSeq.Add(entry);
                    continue;
                }

                int entryaddr = Addresses.SeqTable + (i * 16);
                entry.Addr = (int)ReadWriteUtils.Arr_ReadU32(RomData.MMFileList[f].Data, entryaddr - basea);
                entry.Size = (int)ReadWriteUtils.Arr_ReadU32(RomData.MMFileList[f].Data, (entryaddr - basea) + 4);
                if (entry.Size > 0)
                {
                    entry.Data = new byte[entry.Size];
                    Array.Copy(RomData.MMFileList[4].Data, entry.Addr, entry.Data, 0, entry.Size);
                }
                else
                {
                    int j = SequenceList.FindIndex(u => u.Replaces == i);
                    if (j != -1)
                    {
                        if ((entry.Addr > 0) && (entry.Addr < 128))
                        {
                            if (SequenceList[j].Replaces != 0x28) // 28 (fairy fountain)
                            {
                                SequenceList[j].Replaces = entry.Addr;
                            }
                            else
                            {
                                entry.Data = OldSeq[0x18].Data;
                                entry.Size = OldSeq[0x18].Size;
                            }
                        }
                    }
                }
                OldSeq.Add(entry);
            }

            List<MMSequence> NewSeq = new List<MMSequence>();
            int addr = 0;
            byte[] NewAudioSeq = new byte[0];
            for (int i = 0; i < 128; i++)
            {
                MMSequence newentry = new MMSequence();
                if (OldSeq[i].Size == 0)
                {
                    newentry.Addr = OldSeq[i].Addr;
                }
                else
                {
                    newentry.Addr = addr;
                }

                int p = RomData.PointerizedSequences.FindIndex(u => u.PreviousSlot == i);
                int j = SequenceList.FindIndex(u => u.Replaces == i);
                if (p != -1) // found song we want to pointerize
                {
                    Debug.WriteLine("Sequence slot " + i.ToString("X") + " *->  " + RomData.PointerizedSequences[p].Replaces.ToString("X"));
                    newentry.Addr = RomData.PointerizedSequences[p].Replaces;
                    newentry.Size = 0;
                    // isn't there like 8 bytes of zeros here? where does that go?
                }
                else if (j != -1) // new song to replace old slot found
                {
                    if (SequenceList[j].MM_seq != -1) // old mm song, just copy over
                    {
                        newentry.Size = OldSeq[SequenceList[j].MM_seq].Size;
                        newentry.Data = OldSeq[SequenceList[j].MM_seq].Data;
                    }
                    else // non mm, load file and add
                    {
                        BinaryReader sequence = new BinaryReader(File.Open(SequenceList[j].Name, FileMode.Open));
                        int len = (int)sequence.BaseStream.Length;
                        byte[] data = new byte[len];
                        sequence.Read(data, 0, len);
                        sequence.Close();

                        // I think this checks if the sequence type is correct for MM
                        //  because DB ripped sequences from SF64/SM64/MK64 without modifying them
                        if (data[1] != 0x20)
                        {
                            data[1] = 0x20;
                        }

                        newentry.Size = len;
                        newentry.Data = data;
                    }
                }
                else // not found, song wasn't touched by rando, just transfer over
                {
                    newentry.Size = OldSeq[i].Size;
                    newentry.Data = OldSeq[i].Data;
                }

                NewSeq.Add(newentry);
                // TODO is there not a better way to write this?
                if (newentry.Data != null)
                {
                    NewAudioSeq = NewAudioSeq.Concat(newentry.Data).ToArray();
                }

                addr += newentry.Size;
            }

            // if (addr > (RomData.MMFileList[4].End - RomData.MMFileList[4].Addr))
            if ( true ) // TODO: figure out why leaving the audioseq in its orginal spot causes garbage audio
            {
                int index = RomUtils.AppendFile(NewAudioSeq);
                ResourceUtils.ApplyHack(Values.ModsDirectory + "reloc-audio");
                RelocateSeq(index);
                RomData.MMFileList[4].Data = new byte[0];
                RomData.MMFileList[4].Cmp_Addr = -1;
                RomData.MMFileList[4].Cmp_End = -1;
            }
            else
            {
                RomData.MMFileList[4].Data = NewAudioSeq;
            }

            //update pointer table
            f = RomUtils.GetFileIndexForWriting(Addresses.SeqTable);
            for (int i = 0; i < 128; i++)
            {
                ReadWriteUtils.Arr_WriteU32(RomData.MMFileList[f].Data, (Addresses.SeqTable + (i * 16)) - basea, (uint)NewSeq[i].Addr);
                ReadWriteUtils.Arr_WriteU32(RomData.MMFileList[f].Data, 4 + (Addresses.SeqTable + (i * 16)) - basea, (uint)NewSeq[i].Size);
            }

            //update inst sets
            f = RomUtils.GetFileIndexForWriting(Addresses.InstSetMap);
            basea = RomData.MMFileList[f].Addr;
            for (int i = 0; i < 128; i++)
            {
                int paddr = (Addresses.InstSetMap - basea) + (i * 2) + 2;
                int j = -1;

                if (NewSeq[i].Size == 0)
                {
                    j = SequenceList.FindIndex(u => u.Replaces == NewSeq[i].Addr);
                }
                else
                {
                    j = SequenceList.FindIndex(u => u.Replaces == i);
                }

                if (j != -1)
                {
                    RomData.MMFileList[f].Data[paddr] = (byte)SequenceList[j].Instrument;
                }

            }
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
            ReadWriteUtils.WriteToROM(0x00C2739C, new byte[] { 0x3C, 0x08, 0x80, 0x0A, 0x8D, 0x05, (byte) (offset >> 8), (byte)(offset & 0xFF) });
        }

    }
}
