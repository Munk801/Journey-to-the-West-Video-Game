using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace U5Designs
{
    public struct Texture
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Texture(int id, int width, int height)
            : this()
        {
            Id = id;
            Width = width;
            Height = height;
        }
    }
}
