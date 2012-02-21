﻿using System;
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
        public double XLoc { get; set; }
        public double YLoc { get; set; }

        public Texture(int id, int width, int height)
            : this()
        {
            Id = id;
            Width = width;
            Height = height;
        }

        public void Draw2DTexture(double xPos = 0, double yPos = 0, float scaleX = 1.0f, float scaleY = 1.0f)
        {

            //GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            //GL.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            //GL.Clear(ClearBufferMask.ColorBufferBit);
            //GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, Id);

            //width = (int)(width * scaleX);
            //height = (int)(height * scaleY);
            //height = height / 3;
            //width = width / 3;
            //double halfHeight = height / 2;
            //double halfWidth = width / 2;


            float scaledWidth = (Width * scaleX);
            float scaledHeight = (Height * scaleY);
            double halfHeight = scaledHeight / 2;
            double halfWidth = scaledWidth / 2;
            // Quad positions
            double x = xPos;
            double y = yPos;
            double z = 0;


            XLoc = x - halfWidth;
            YLoc = y + halfHeight;

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
