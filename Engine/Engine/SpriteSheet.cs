using System;
using System.Collections.Generic;
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

		private byte[][][] tex; //tex[cycleNumber][frameNumber][y*w + x]
		private int texw, texh;

		public SpriteSheet(Bitmap texbmp, int[] cycleStartNums, int[] cycleLengths, int _texw, int _texh) {
			texw = _texw;
			texh = _texh;

			BitmapData bmp_data = texbmp.LockBits(new Rectangle(0, 0, texbmp.Width, texbmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			tex = new byte[cycleStartNums.Length][][];
			for(int cycleNum = 0; cycleNum < cycleStartNums.Length; cycleNum++) {
				tex[cycleNum] = new byte[cycleLengths[cycleNum]][];
				for(int frameNum = 0; frameNum < cycleLengths[cycleNum]; frameNum++) {
					IntPtr tex_addr = IntPtr.Add(bmp_data.Scan0, (cycleStartNums[cycleNum]+frameNum) * texw * texh * 4);
					tex[cycleNum][frameNum] = new byte[texw * texh * 4];
					for(int i = 0; i < texw * texh*4; i += 4) {
						tex[cycleNum][frameNum][i + 3] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i), typeof(byte));
						tex[cycleNum][frameNum][i + 0] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i+1), typeof(byte));
						tex[cycleNum][frameNum][i + 1] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i+2), typeof(byte));
						tex[cycleNum][frameNum][i + 2] = (byte)Marshal.PtrToStructure(IntPtr.Add(tex_addr, i+3), typeof(byte));
					}
				}
			}
			texbmp.UnlockBits(bmp_data);
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
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.DisableClientState(ArrayCap.NormalArray);
			GL.EnableClientState(ArrayCap.TextureCoordArray);
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
			GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
			GL.TexCoordPointer(2, TexCoordPointerType.Float, 0, texverts);
			GL.DrawElements(BeginMode.Quads, indices.Length, DrawElementsType.UnsignedByte, indices);
			GL.PopMatrix(); //this matches to the push in RenderObject.doScaleAndTranslate()

			return frameNumber;
		}
	}
}
