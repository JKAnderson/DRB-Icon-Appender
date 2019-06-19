using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace DRB_Icon_Appender
{
    public class DRBRaw
    {
        private static readonly Encoding UTF16 = Encoding.Unicode;

        public STR str;
        public TEXI texi;
        public Section shpr;
        private Section ctpr;
        private Section anip;
        private Section intp;
        private Section scdp;
        public SHAP shap;
        private Section ctrl;
        private Section anik;
        private Section anio;
        private Section anim;
        private Section scdk;
        private Section scdo;
        private Section scdl;
        public DLG dlg;

        public static DRBRaw Read(byte[] bytes)
        {
            return new DRBRaw(bytes);
        }

        private DRBRaw(byte[] bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            br.AssertASCII("DRB\0");
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);

            str = new STR(br);
            texi = new TEXI(br);
            shpr = new Section(br, "SHPR");
            ctpr = new Section(br, "CTPR");
            anip = new Section(br, "ANIP");
            intp = new Section(br, "INTP");
            scdp = new Section(br, "SCDP");
            shap = new SHAP(br);
            ctrl = new Section(br, "CTRL");
            anik = new Section(br, "ANIK");
            anio = new Section(br, "ANIO");
            anim = new Section(br, "ANIM");
            scdk = new Section(br, "SCDK");
            scdo = new Section(br, "SCDO");
            scdl = new Section(br, "SCDL");
            Dictionary<int, DLGOEntry> dlgoEntries = readDLGO(br, str);
            dlg = new DLG(br, str, dlgoEntries);

            br.AssertASCII("END\0");
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
        }

        public byte[] Write()
        {
            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteASCII("DRB\0");
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);

            str.Write(bw);
            texi.Write(bw);
            shpr.Write(bw);
            ctpr.Write(bw);
            anip.Write(bw);
            intp.Write(bw);
            scdp.Write(bw);
            shap.Write(bw);
            ctrl.Write(bw);
            anik.Write(bw);
            anio.Write(bw);
            anim.Write(bw);
            scdk.Write(bw);
            scdo.Write(bw);
            scdl.Write(bw);
            dlg.Write(bw);

            bw.WriteASCII("END\0");
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            return bw.FinishBytes();
        }

        private static void readSectionHeader(BinaryReaderEx br, string name, out int entrySize, out int entryCount)
        {
            br.AssertASCII(name);
            entrySize = br.ReadInt32();
            entryCount = br.ReadInt32();
            br.AssertInt32(0);
        }

        public class Section
        {
            public readonly string Name;
            public int Count;
            public byte[] Bytes;

            public Section(BinaryReaderEx br, string name)
            {
                Name = name;
                readSectionHeader(br, name, out int entrySize, out Count);
                Bytes = br.ReadBytes(entrySize);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteASCII(Name);
                bw.WriteInt32(Bytes.Length);
                bw.WriteInt32(Count);
                bw.WriteInt32(0);
                bw.WriteBytes(Bytes);
            }
        }

        public class STR
        {
            private Dictionary<int, string> strings;

            public STR(BinaryReaderEx br)
            {
                readSectionHeader(br, "STR\0", out int entrySize, out int entryCount);

                strings = new Dictionary<int, string>();
                int start = (int)br.Position;
                for (int i = 0; i < entryCount; i++)
                {
                    int offset = (int)br.Position - start;
                    string s = br.ReadUTF16();
                    strings[offset] = s;
                }

                br.Pad(0x10);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteASCII("STR\0");
                bw.ReserveInt32("STRDataSize");
                bw.WriteInt32(strings.Values.Count);
                bw.WriteInt32(0);

                int start = (int)bw.Position;
                List<int> offsets = new List<int>(strings.Keys);
                offsets.Sort();
                foreach (int offset in offsets)
                    bw.WriteUTF16(strings[offset], true);

                bw.Pad(0x10);
                bw.FillInt32("STRDataSize", (int)bw.Position - start);
            }

            public string GetString(int offset)
            {
                return strings[offset];
            }

            public int GetOffset(string s)
            {
                foreach (int offset in strings.Keys)
                    if (strings[offset] == s)
                        return offset;
                return -1;
            }

            public int AddString(string s)
            {
                List<int> offsets = new List<int>(strings.Keys);
                offsets.Sort();
                int offset = offsets[offsets.Count - 1];
                offset = offset + UTF16.GetByteCount(strings[offset]) + 2;
                strings[offset] = s;
                return offset;
            }
        }

        public class TEXI
        {
            public List<(int, int)> Entries;

            public TEXI(BinaryReaderEx br)
            {
                readSectionHeader(br, "TEXI", out int entrySize, out int entryCount);

                Entries = new List<(int, int)>();
                for (int i = 0; i < entryCount; i++)
                {
                    Entries.Add((br.ReadInt32(), br.ReadInt32()));
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }
                br.Pad(0x10);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteASCII("TEXI");
                bw.ReserveInt32("TEXIDataSize");
                bw.WriteInt32(Entries.Count);
                bw.WriteInt32(0);

                int start = (int)bw.Position;
                foreach ((int, int) entry in Entries)
                {
                    bw.WriteInt32(entry.Item1);
                    bw.WriteInt32(entry.Item2);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }
                bw.Pad(0x10);
                bw.FillInt32("TEXIDataSize", (int)bw.Position - start);
            }
        }

        public class SHAP
        {
            public List<(int, int)> Entries;

            public SHAP(BinaryReaderEx br)
            {
                readSectionHeader(br, "SHAP", out int entrySize, out int entryCount);

                Entries = new List<(int, int)>();
                for (int i = 0; i < entryCount; i++)
                {
                    Entries.Add((br.ReadInt32(), br.ReadInt32()));
                }
                br.Pad(0x10);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteASCII("SHAP");
                bw.ReserveInt32("SHAPDataSize");
                bw.WriteInt32(Entries.Count);
                bw.WriteInt32(0);

                int start = (int)bw.Position;
                foreach ((int, int) entry in Entries)
                {
                    bw.WriteInt32(entry.Item1);
                    bw.WriteInt32(entry.Item2);
                }
                bw.Pad(0x10);
                bw.FillInt32("SHAPDataSize", (int)bw.Position - start);
            }
        }
        
        public Dictionary<int, DLGOEntry> readDLGO(BinaryReaderEx br, STR str)
        {
            readSectionHeader(br, "DLGO", out int entrySize, out int entryCount);

            int start = (int)br.Position;
            Dictionary<int, DLGOEntry> dlgoEntries = new Dictionary<int, DLGOEntry>();
            for (int i = 0; i < entryCount; i++)
            {
                int offset = (int)br.Position - start;
                dlgoEntries[offset] = new DLGOEntry(br, str);
            }
            br.Pad(0x10);

            return dlgoEntries;
        }

        public class DLGOEntry
        {
            private int nameOffset;
            public string Name;
            public int ShapOffset;
            public int CtrlOffset;

            public DLGOEntry(BinaryReaderEx br, STR str)
            {
                nameOffset = br.ReadInt32();
                ShapOffset = br.ReadInt32();
                CtrlOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Name = str.GetString(nameOffset);
            }

            public DLGOEntry(int nameOffset, int shapOffset, int ctrlOffset)
            {
                this.nameOffset = nameOffset;
                ShapOffset = shapOffset;
                CtrlOffset = ctrlOffset;
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(nameOffset);
                bw.WriteInt32(ShapOffset);
                bw.WriteInt32(CtrlOffset);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }
        }

        public class DLG
        {
            public List<DLGEntry> Entries;

            public DLG(BinaryReaderEx br, STR str, Dictionary<int, DLGOEntry> dlgoEntries)
            {
                readSectionHeader(br, "DLG\0", out int entrySize, out int entryCount);

                Entries = new List<DLGEntry>();
                for (int i = 0; i < entryCount; i++)
                {
                    Entries.Add(new DLGEntry(br, str, dlgoEntries));
                }

                if (dlgoEntries.Count != 0)
                    throw null;
            }

            public void Write(BinaryWriterEx bw)
            {
                List<DLGOEntry> dlgoEntries = new List<DLGOEntry>();
                foreach (DLGEntry dlgEntry in Entries)
                    dlgEntry.PrepareDLGOs(dlgoEntries);

                bw.WriteASCII("DLGO");
                bw.ReserveInt32("DLGODataSize");
                bw.WriteInt32(dlgoEntries.Count);
                bw.WriteInt32(0);

                int start = (int)bw.Position;
                foreach (DLGOEntry dlgoEntry in dlgoEntries)
                    dlgoEntry.Write(bw);
                bw.Pad(0x10);
                bw.FillInt32("DLGODataSize", (int)bw.Position - start);

                bw.WriteASCII("DLG\0");
                bw.ReserveInt32("DLGDataSize");
                bw.WriteInt32(Entries.Count);
                bw.WriteInt32(0);

                start = (int)bw.Position;
                foreach (DLGEntry dlgEntry in Entries)
                    dlgEntry.Write(bw);
                bw.Pad(0x10);
                bw.FillInt32("DLGDataSize", (int)bw.Position - start);
            }
        }

        public class DLGEntry
        {
            private int nameOffset;
            public string Name;
            public int ShapOffset;
            public int CtrlOffset;
            public short LeftEdge, TopEdge, RightEdge, BottomEdge;
            public short Unk14;

            public List<DLGOEntry> DLGOEntries;
            private int dlgoOffset;

            public DLGEntry(BinaryReaderEx br, STR str, Dictionary<int, DLGOEntry> dlgoEntries)
            {
                nameOffset = br.ReadInt32();
                ShapOffset = br.ReadInt32();
                CtrlOffset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                int dlgoCount = br.ReadInt32();
                int dlgoOffset = br.ReadInt32();
                LeftEdge = br.ReadInt16();
                TopEdge = br.ReadInt16();
                RightEdge = br.ReadInt16();
                BottomEdge = br.ReadInt16();
                Unk14 = br.ReadInt16();
                br.AssertInt16(-1);
                br.AssertInt32(-1);
                br.AssertInt16(-1);
                br.AssertInt16(0);
                br.AssertInt32(0);

                Name = str.GetString(nameOffset);
                DLGOEntries = new List<DLGOEntry>();
                for (int i = 0; i < dlgoCount; i++)
                {
                    int offset = dlgoOffset + i * 0x20;
                    DLGOEntries.Add(dlgoEntries[offset]);
                    dlgoEntries.Remove(offset);
                }
            }

            public void PrepareDLGOs(List<DLGOEntry> dlgoEntries)
            {
                dlgoOffset = dlgoEntries.Count * 0x20;
                foreach (DLGOEntry dlgoEntry in DLGOEntries)
                    dlgoEntries.Add(dlgoEntry);
            }

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(nameOffset);
                bw.WriteInt32(ShapOffset);
                bw.WriteInt32(CtrlOffset);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);

                bw.WriteInt32(DLGOEntries.Count);
                bw.WriteInt32(dlgoOffset);
                bw.WriteInt16(LeftEdge);
                bw.WriteInt16(TopEdge);
                bw.WriteInt16(RightEdge);
                bw.WriteInt16(BottomEdge);
                bw.WriteInt16(Unk14);
                bw.WriteInt16(-1);
                bw.WriteInt32(-1);
                bw.WriteInt16(-1);
                bw.WriteInt16(0);
                bw.WriteInt32(0);
            }
        }
    }
}
