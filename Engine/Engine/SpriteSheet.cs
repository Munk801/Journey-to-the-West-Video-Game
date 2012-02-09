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

		public SpriteSheet(Bitmap texbmp, int[] cycleStartNums, int[] cycleLengths, int _texw, int _texh, bool switchColorOrder) {
			texw = _texw;
			texh = _texh;
			
			BitmapData bmp_data = texbmp.LockBits(new Rectangle(0, 0, texbmp.Width, texbmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			tex = new byte[cycleStartNums.Length][][];
			for(int cycleNum = 0; cycleNum < cycleStartNums.Length; cycleNum++) {
				tex[cycleNum] = new byte[cycleLengths[cycleNum]][];
				for(int frameNum = 0; frameNum < cycleLengths[cycleNum]; frameNum++) {
					IntPtr tex_addr = IntPtr.Add(bmp_data.Scan0, (cycleStartNums[cycleNum]+frameNum) * texw * texh * 4);
					tex[cycleNum][frameNum] = new byte[texw * texh * 4];
/*					Marshal.Copy(tex_addr, tex[cycleNum][frameNum], 0, tex[cycleNum][frameNum].Length);*/
					for(int y = 0; y < texh; y++) {
						for(int x = 0; x < texw; x++) {
							if(switchColorOrder) {
								int pixel = ((texh - y - 1) * texw + x) * 4;
								tex[cycleNum][frameNum][pixel + 2] = (byte)(255 - (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, (y * texw + x) * 4 + 0), typeof(byte)));
								tex[cycleNum][frameNum][pixel + 1] = (byte)(255 - (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, (y * texw + x) * 4 + 1), typeof(byte)));
								tex[cycleNum][frameNum][pixel + 0] = (byte)(255 - (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, (y * texw + x) * 4 + 2), typeof(byte)));
								tex[cycleNum][frameNum][pixel + 3] = (byte)(255 - (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, (y * texw + x) * 4 + 3), typeof(byte))); //+ 3 is alpha
							} else {
								for(int i = 0; i < 4; i++) {
									tex[cycleNum][frameNum][((texh - y - 1) * texw + x) * 4 + i] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, (y * texw + x) * 4 + i), typeof(byte));
								}
							}
						}
					}
// 						for(int i = 0; i < texw * texh * 4; i += 4) {
// 							tex[cycleNum][frameNum][i + 0] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i), typeof(byte));
// 							tex[cycleNum][frameNum][i + 1] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i + 1), typeof(byte));
// 							tex[cycleNum][frameNum][i + 2] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i + 2), typeof(byte));
// 							tex[cycleNum][frameNum][i + 3] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i + 3), typeof(byte));
// 						}
				}
			}
			texbmp.UnlockBits(bmp_data);

			//CORasterSaveToBMP(tex[0][2], (uint)texw, (uint)texh, "C:\\Users\\Kendal\\Desktop\\test.bmp");
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
		public int draw(int cycleNumber, int frameNumber) {
			//If possible, move some of these state changes into GameEngine to improve performance
// 			GL.EnableClientState(ArrayCap.VertexArray);
// 			GL.DisableClientState(ArrayCap.NormalArray);
// 			GL.EnableClientState(ArrayCap.TextureCoordArray);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new OpenTK.Graphics.Color4(1, 1, 1, 0)); //transparent

			if(frameNumber >= tex[cycleNumber].Length) {
				frameNumber %= tex[cycleNumber].Length;
			}

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, texw, texh, 0, PixelFormat.Rgba, PixelType.UnsignedByte, tex[cycleNumber][frameNumber]);
// 			GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
// 			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, texverts);
			//CORasterSaveToBMP(tex[cycleNumber][frameNumber], (uint)texw, (uint)texh, "C:\\Users\\Kendal\\Desktop\\test.bmp");
//			GL.DrawElements(BeginMode.Quads, indices.Length, DrawElementsType.UnsignedByte, indices);
			quad.Render();
			//GL.PopMatrix(); //this matches to the push in RenderObject.doScaleAndTranslate()

			return frameNumber;
		}
	}
}
