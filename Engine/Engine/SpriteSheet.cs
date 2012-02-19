using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

using BitmapData = System.Drawing.Imaging.BitmapData;
using ImageLockMode = System.Drawing.Imaging.ImageLockMode;

/* Contains the sprite sheet for a particular in-game object which is to be rendered
 * in 2D.  Different in-game objects that share a sprite sheet may also share a single
 * SpriteSheet object, even if they are on different animation frames at a given time.
 * A SpriteSheet will contain all of the animation cycles for a given in-game object,
 * such as stand, walk, run, attack cycles.
 */

namespace Engine {
	public class SpriteSheet {

		private static readonly float[] vertices = {0.0f, 0.0f, 0.0f,  1.0f, 0.0f, 0.0f,  1.0f, 1.0f, 0.0f,  0.0f, 1.0f, 0.0f};
		private static readonly float[] texverts = {0.0f, 0.0f,        1.0f, 0.0f,        1.0f, 1.0f,        0.0f, 1.0f};
		private static readonly byte[] indices = {0, 1, 2, 3};
		public static ObjMesh quad;

		private byte[][][] tex; //tex[cycleNumber][frameNumber][y*w + x]
		private int texw, texh;
		private double framesPerSecond;
		private int texID;
		private int prevCycleNum, prevFrameNum;

		public SpriteSheet(Bitmap texbmp, int[] cycleStartNums, int[] cycleLengths, int _texw, int _texh, double _framesPerSecond = 1.0) {
			texw = _texw;
			texh = _texh;
			framesPerSecond = _framesPerSecond;

			BitmapData bmp_data = texbmp.LockBits(new Rectangle(0, 0, texbmp.Width, texbmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			tex = new byte[cycleStartNums.Length][][];
			for(int cycleNum = 0; cycleNum < cycleStartNums.Length; cycleNum++) {
				tex[cycleNum] = new byte[cycleLengths[cycleNum]][];
				for(int frameNum = 0; frameNum < cycleLengths[cycleNum]; frameNum++) {
					IntPtr tex_addr = IntPtr.Add(bmp_data.Scan0, (cycleStartNums[cycleNum] + frameNum) * texw * texh * 4);
					tex[cycleNum][frameNum] = new byte[texw * texh * 4];
					Marshal.Copy(tex_addr, tex[cycleNum][frameNum], 0, tex[cycleNum][frameNum].Length);
					//Rearrange BGRA -> RGBA and invert colors to proper encoding
					for(int i = 0; i < tex[cycleNum][frameNum].Length; i += 4) {
						byte temp = tex[cycleNum][frameNum][i];
						tex[cycleNum][frameNum][i] = (byte)~(tex[cycleNum][frameNum][i + 2]);
						tex[cycleNum][frameNum][i + 2] = (byte)~temp;
						tex[cycleNum][frameNum][i + 1] = (byte)~tex[cycleNum][frameNum][i + 1];
					}
				}
			}
			texbmp.UnlockBits(bmp_data);

			texID = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, texID);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, texw, texh, 0, PixelFormat.Rgba, PixelType.UnsignedByte, tex[0][0]);
			prevCycleNum = 0;
			prevFrameNum = 0;
			

// 			if(_texh == 1080) {
// 				CORasterSaveToBMP(tex[0][0], (uint)texw, (uint)texh, "C:\\Users\\Kendal\\Desktop\\test.bmp");
// 			} else if(_texh == 128) {
// 				CORasterSaveToBMP(tex[0][0], (uint)texw, (uint)texh, "C:\\Users\\Kendal\\Desktop\\test2.bmp");
// 			}
		}


        public void CORasterSaveToBMP(byte[] rasterPixels, uint rasterWidth, uint rasterHeight, String path)
        {
            //BMP (Windows V3)
            //Offset    Size    Description
            //0         2       the magic number u[]ed to identify the BMP file: 0x42 0x4D (Hex code points for B and M in big-endian order)
            //2         4       the size of the BMP file in bytes
            //6         2       reserved; actual value depends on the application that creates the image
            //8         2       reserved; actual value depends on the application that creates the image
            //10        4       the offset, i.e. starting address, of the byte where the bitmap data can be found.
            //14        4       the size of this header (40 bytes)
            //18        4       the bitmap width in pixels (signed integer).
            //22        4       the bitmap height in pixels (signed integer).
            //26        2       the number of color planes being used. Must be set to 1.
            //28        2       the number of bits per pixel, which is the color samplesPerPixel of the image. Typical values are 1, 4, 8, 16, 24 and 32.
            //30        4       the compression method being used. See the next table for a list of possible values.
            //34        4       the image size. This is the size of the raw bitmap data (see below), and should not be confused with the file size.
            //38        4       the horizontal resolution of the image. (pixel per meter, signed integer)
            //42        4       the vertical resolution of the image. (pixel per meter, signed integer)
            //46        4       the number of colors in the color palette, or 0 to default to 2n.
            //50        4       the number of important colors used, or 0 when every color is important; generally ignored.

            //Open file for writing
            
            System.IO.BinaryWriter file = new System.IO.BinaryWriter(File.Open(path, FileMode.Create));
            

            //FILE* file = fopen(path, "w");
            //if (file == NULL)
            //    return;
            
            //Define header data
            ushort magicNumber = 0x4D42;
            ushort reserved0 = 0;//0x4D41;
            ushort reserved1 = 0;//0x5454;
            uint dataOffset = 54;
            uint infoHeaderSize = 40;
            uint width = rasterWidth;
            uint height = rasterHeight;
            ushort colorPlanes = 1;
            ushort bitsPerPixel = 32;
            uint compression = 0;
            uint dataSize = width * height * bitsPerPixel / 8;
            uint horizontalResolution = 2835;
            uint verticalResolution = 2835;
            uint paletteColorCount = 0;
            uint importantPaletteColorCount = 0;
            uint fileSize = 54 + dataSize;

            
            //Write BMP header (Windows V3, 32bbp)
            file.Write(magicNumber);
            file.Write(fileSize);
            file.Write(reserved0);
            file.Write(reserved1);
            file.Write(dataOffset);
            file.Write(infoHeaderSize);
            file.Write(width);
            file.Write(height);
            file.Write(colorPlanes);
            file.Write(bitsPerPixel);
            file.Write(compression);
            file.Write(dataSize);
            file.Write(horizontalResolution);
            file.Write(verticalResolution);
            file.Write(paletteColorCount);
			file.Write(importantPaletteColorCount);

            //Write BMP body (XXRRGGBB)
			for(int i = 0; i < rasterPixels.Length; i++) {
				file.Write(rasterPixels[i]);
			}
				//             for (uint y = rasterHeight - 1; y >= 0; y--)
				//             {
				//                 for (uint x = 0; x < rasterWidth; x++)
				//                 {
				//                     uint color = rasterPixels[y * rasterWidth + x];
				// 					file.Write(color);
				//                 }
				//             }

				//Cleanup
			file.Close();
        }
		/*
		 * draw - Uses OpenGL to render this sprite.  Requires that
		 *		  the modelview matrix has been properly set up previously
		 *		  to scale and translate to proper location, but pops matrix
		 *		  to remove those transformations.
		 *		  
		 *		  Takes as parameters a cycle number (indicating which animation
		 *		  cycle is to be drawn) and a frame number (indicating which frame
		 *		  the animation is on).
		 *		  
		 *		  Returns the frameNumber passed, possibly modding it first
		 *		  to bring it back into the expected range.
		 */
		public double draw(bool viewIs3d, int cycleNumber = 0, double frameTime = 0.0) {
 			//If possible, move some of these state changes into GameEngine to improve performance
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new OpenTK.Graphics.Color4(1, 1, 1, 0)); //transparent

			int frameNum = (int)(frameTime * framesPerSecond);
			if(frameNum >= tex[cycleNumber].Length) {
				frameTime -= tex[cycleNumber].Length / framesPerSecond;
				frameNum %= tex[cycleNumber].Length;
			}

			GL.BindTexture(TextureTarget.Texture2D, texID);
			if(frameNum != prevFrameNum || cycleNumber != prevCycleNum) {
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, texw, texh, 0, PixelFormat.Rgba, PixelType.UnsignedByte, tex[cycleNumber][frameNum]);
				prevFrameNum = frameNum;
				prevCycleNum = cycleNumber;
			}

			GL.Scale(1, -1, 1);
			if(viewIs3d) {
				GL.Rotate(270, Vector3d.UnitY);
			}
			quad.Render();

			return frameTime;
		}
	}
}
