using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK.Graphics.OpenGL;

namespace Engine {
	public class MeshTexture {
		int[] texIDs;

// 		public MeshTexture(Bitmap bmp) {
// 			texIDs = new int[1];
// 			texIDs[0] = GL.GenTexture();
// 			GL.BindTexture(TextureTarget.Texture2D, texIDs[0]);
// 			BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
// 			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
// 				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
// 			bmp.UnlockBits(bmp_data);
// 		}

		public MeshTexture(List<Bitmap> bmps) {
			texIDs = new int[bmps.Count];
			for(int i = 0; i < bmps.Count; i++) {
				texIDs[i] = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, texIDs[i]);
				BitmapData bmp_data = bmps[i].LockBits(new Rectangle(0, 0, bmps[i].Width, bmps[i].Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
				bmps[i].UnlockBits(bmp_data);
			}
		}

		public void doTexture(int frame=0) {
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

			GL.BindTexture(TextureTarget.Texture2D, texIDs[frame]);
		}
	}
}
