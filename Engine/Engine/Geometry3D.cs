using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK.Graphics.OpenGL;

/* DEPRECATED - This class will be deleted soon.  It has been
 * replaced by ObjMesh.  It is being kept temporarily until
 * the draw() code has been moved elsewhere.
 */

namespace Engine {
	class Geometry3D {

		private double[] vertices;
		private double[] normals;
		private double[] texverts;
		private uint[] indices;
		private byte[] tex;
		private int texw, texh;

		public Geometry3D(double[] _vertices, double[] _normals, double[] _texverts, uint[] _indices, byte[] _tex, int _texw, int _texh) {
			vertices = _vertices;
			normals = _normals;
			texverts = _texverts;
			indices = _indices;
			tex = _tex;
			texw = _texw;
			texh = _texh;
		}

		/*
		 * draw - Uses OpenGL to render this geometry.  Requires that
		 *		  the modelview matrix has been properly set up previously
		 *		  to scale and translate to proper location, but pops matrix
		 *		  to remove those transformations.
		 */
		public void draw() {
			//If possible, move some of these state changes into GameEngine to improve performance
			GL.EnableClientState(ArrayCap.VertexArray);
			GL.EnableClientState(ArrayCap.NormalArray);
			GL.EnableClientState(ArrayCap.TextureCoordArray);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
			GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Modulate);

			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, texw, texh, 0, PixelFormat.Rgb, PixelType.UnsignedByte, tex);
			GL.VertexPointer(3, VertexPointerType.Double, 0, vertices);
			GL.NormalPointer(NormalPointerType.Double, 0, normals);
			GL.TexCoordPointer(2, TexCoordPointerType.Double, 0, texverts);
			GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedInt, indices);
			GL.PopMatrix(); //this matches to the push in RenderObject.doScaleAndTranslate()
		}
	}
}
