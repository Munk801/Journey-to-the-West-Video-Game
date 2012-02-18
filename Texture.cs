using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;
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

        public void Draw2DTexture(int width, int height)
        {
            GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            height = height / 3;
            width = width / 3;
            double halfHeight = height / 2;
            double halfWidth = width / 2;
            // Quad positions
            double x = 0;
            double y = 0;
            double z = 0;

            // Quad color
            float red = 1;
            float green = 1;
            float blue = 1;
            float alpha = 1;

            float topUV = 0;
            float bottomUV = 1;
            float leftUV = 0;
            float rightUV = 1;

            GL.Begin(BeginMode.Triangles);
            {
                GL.Color4(red, green, blue, alpha);

                GL.TexCoord2(leftUV, topUV);
                GL.Vertex3(x - halfWidth, y + halfHeight, z); // top left
                GL.TexCoord2(rightUV, topUV);
                GL.Vertex3(x + halfWidth, y + halfHeight, z); // top right
                GL.TexCoord2(leftUV, bottomUV);
                GL.Vertex3(x - halfWidth, y - halfHeight, z); // bottom left


                GL.TexCoord2(rightUV, topUV);
                GL.Vertex3(x + halfWidth, y + halfHeight, z); // top right
                GL.TexCoord2(rightUV, bottomUV);
                GL.Vertex3(x + halfWidth, y - halfHeight, z); // bottom right
                GL.TexCoord2(leftUV, bottomUV);
                GL.Vertex3(x - halfWidth, y - halfHeight, z); // bottom left

            }
            GL.End();
        }
    }
}
