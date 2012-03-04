using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.DevIl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;

namespace U5Designs
{
    class TextureManager : IDisposable
    {
        /** DLL P/Invoke **/
        [DllImport("../../Resources/lib/DevIL.dll")]
        public static extern void ilInit();
        [DllImport("../../Resources/lib/ILU.dll")]
        public static extern void iluInit();
        [DllImport("../../Resources/lib/ILUT.dll")]
        public static extern void ilutInit();
        // CAN BE REPLACED WITH AN XML LATER ON
        Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();


        public void InitializeTextureManager()
        {
            ilInit();
            iluInit();
            ilutInit();
            Tao.DevIl.Ilut.ilutRenderer(Ilut.ILUT_OPENGL);
        }

        public void RenderSetup()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public Texture GetTexture(string id)
        {
            return Textures[id];

        }

        public void Dispose()
        {
            foreach (Texture t in Textures.Values)
            {
                // TO DO: REMOVE TEXTURES THAT ARE NO LONGER IN USE
                GL.DeleteTextures(1, new int[] { t.Id });
            }
        }

        private bool HasTexture(string id)
        {
           return this.Textures.ContainsKey(id);
        }

        public void LoadTexture(string id, string path)
        {

           
                var bitmap = new Bitmap(path);
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                 ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

                int textureId = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bitmap.Width, bitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bitmapData.Scan0);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
                //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Blend);
                //GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvColor, new OpenTK.Graphics.Color4(1, 1, 1, 0)); //transparent
                GL.BindTexture(TextureTarget.Texture2D, textureId);
                Textures.Add(id, new Texture(textureId, bitmap.Width, bitmap.Height));
                bitmap.UnlockBits(bitmapData);
                bitmap.Dispose();
            //int devID = 0;
            //Il.ilGenImages(1, out devID);
            //// Set the texture
            //Il.ilBindImage(devID);

            //// If we cannot load the texture
            //if (!Il.ilLoadImage(path))
            //{
            //    System.Diagnostics.Debug.Assert(false, "Error Opening File, [" + path + "].");
            //}

            //Ilu.iluFlipImage();


            //int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            //int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);

            //int openGLID = Ilut.ilutGLBindTexImage();

            //System.Diagnostics.Debug.Assert(openGLID != 0);

            //Textures.Add(id, new Texture(openGLID, width, height));
        }



    }
}
