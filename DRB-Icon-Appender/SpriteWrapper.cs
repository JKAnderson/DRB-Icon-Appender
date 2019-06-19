using SoulsFormats;
using System;
using System.Collections.Generic;

namespace DRB_Icon_Appender
{
    internal class SpriteWrapper : IComparable<SpriteWrapper>
    {
        private DRB.Dlgo Dlgo;
        private DRB.Shape.Sprite Sprite => Dlgo.Shape as DRB.Shape.Sprite;
        private List<string> Textures;

        public int ID
        {
            get => int.Parse(Dlgo.Name.Substring("EquIcon_".Length));
            set => Dlgo.Name = $"EquIcon_{value:D4}";
        }

        public string Texture
        {
            get => Textures[Sprite.TextureIndex];
            set => Sprite.TextureIndex = (short)Textures.IndexOf(value);
        }

        public short TopEdge
        {
            get => Sprite.TexTopEdge;
            set => Sprite.TexTopEdge = value;
        }

        public short LeftEdge
        {
            get => Sprite.TexLeftEdge;
            set => Sprite.TexLeftEdge = value;
        }

        public int Width
        {
            get => Sprite.TexRightEdge - Sprite.TexLeftEdge;
            set => Sprite.TexRightEdge = (short)(value + Sprite.TexLeftEdge);
        }

        public int Height
        {
            get => Sprite.TexBottomEdge - Sprite.TexTopEdge;
            set => Sprite.TexBottomEdge = (short)(value + Sprite.TexTopEdge);
        }

        public SpriteWrapper(DRB.Dlgo dlgo, List<string> textures)
        {
            Dlgo = dlgo;
            Textures = textures;
        }

        public int CompareTo(SpriteWrapper other)
        {
            return ID.CompareTo(other.ID);
        }
    }
}
