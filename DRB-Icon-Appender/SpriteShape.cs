using SoulsFormats;
using System;
using System.Collections.Generic;

namespace DRB_Icon_Appender
{
    class SpriteShape
    {
        public int ID { get; private set; }
        public string Texture { get; set; }
        public short LeftEdge { get; set; }
        public short TopEdge { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        private bool dsr;
        public int ShprOffset;

        private short unk1, unk2, unk3, unk4;
        private short dsr1, dsr2, dsr3, dsr4;
        private byte orientationFlags;
        private byte unk11;
        private int unk12;
        private byte unk13, unk14, unk15, unk16;

        public SpriteShape(DRBRaw.DLGOEntry dlgo, DRBRaw drb, List<string> textures, bool dsr)
        {
            if (!dlgo.Name.Contains("EquIcon_"))
                throw null;

            this.dsr = dsr;
            ID = Int32.Parse(dlgo.Name.Substring("EquIcon_".Length));

            BinaryReaderEx br = new BinaryReaderEx(false, drb.shpr.Bytes);
            ShprOffset = drb.shap.Entries[dlgo.ShapOffset / 8].Item2;
            br.Position = ShprOffset;

            unk1 = br.ReadInt16();
            unk2 = br.ReadInt16();
            unk3 = br.ReadInt16();
            unk4 = br.ReadInt16();
            if (dsr)
            {
                dsr1 = br.ReadInt16();
                dsr2 = br.ReadInt16();
                dsr3 = br.ReadInt16();
                dsr4 = br.ReadInt16();
            }
            LeftEdge = br.ReadInt16();
            TopEdge = br.ReadInt16();
            Width = br.ReadInt16() - LeftEdge;
            Height = br.ReadInt16() - TopEdge;
            Texture = textures[br.ReadInt16()];
            orientationFlags = br.ReadByte();
            unk11 = br.ReadByte();
            unk12 = br.ReadInt32();
            unk13 = br.ReadByte();
            unk14 = br.ReadByte();
            unk15 = br.ReadByte();
            unk16 = br.ReadByte();
        }

        public SpriteShape(int id, DRBRaw drb, List<string> textures, bool dsr)
        {
            this.dsr = dsr;
            ID = id;

            unk1 = -1356;
            unk2 = -244;
            unk3 = -1292;
            unk4 = -148;
            if (dsr)
            {
                dsr1 = -1;
                dsr2 = -1;
                dsr3 = 0;
                dsr4 = 0;
            }
            LeftEdge = 1;
            TopEdge = 1;
            Width = 80;
            Height = 90;
            Texture = "Icon00";
            orientationFlags = 0;
            unk11 = 1;
            unk12 = 0;
            unk13 = 255;
            unk14 = 255;
            unk15 = 255;
            unk16 = 255;

            BinaryWriterEx bw = new BinaryWriterEx(false);
            bw.WriteBytes(drb.shpr.Bytes);
            ShprOffset = (int)bw.Position;
            WriteSHPR(bw, textures);
            drb.shpr.Bytes = bw.FinishBytes();

            int spriteOffset = drb.str.GetOffset("Sprite");
            drb.shap.Entries.Add((spriteOffset, ShprOffset));
            int shapOffset = (drb.shap.Entries.Count - 1) * 8;

            int nameOffset = drb.str.AddString("EquIcon_" + id);
            int ctrlOffset = dsr ? 0x3968 : 0x38D0;
            foreach (DRBRaw.DLGEntry dlg in drb.dlg.Entries)
            {
                if (dlg.Name == "Icon")
                    dlg.DLGOEntries.Add(new DRBRaw.DLGOEntry(nameOffset, shapOffset, ctrlOffset));
            }
        }

        public void WriteSHPR(BinaryWriterEx bw, List<string> textures)
        {
            bw.WriteInt16(unk1);
            bw.WriteInt16(unk2);
            bw.WriteInt16(unk3);
            bw.WriteInt16(unk4);
            if (dsr)
            {
                bw.WriteInt16(dsr1);
                bw.WriteInt16(dsr2);
                bw.WriteInt16(dsr3);
                bw.WriteInt16(dsr4);
            }
            bw.WriteInt16(LeftEdge);
            bw.WriteInt16(TopEdge);
            bw.WriteInt16((short)(LeftEdge + Width));
            bw.WriteInt16((short)(TopEdge + Height));
            bw.WriteInt16((short)textures.IndexOf(Texture));
            bw.WriteByte(orientationFlags);
            bw.WriteByte(unk11);
            bw.WriteInt32(unk12);
            bw.WriteByte(unk13);
            bw.WriteByte(unk14);
            bw.WriteByte(unk15);
            bw.WriteByte(unk16);
        }
    }
}
