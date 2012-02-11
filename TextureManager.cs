using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.DevIl;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;

namespace U5Designs
{
    class TextureManager : IDisposable
    {
        
        // CAN BE REPLACED WITH AN XML LATER ON
        Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();

        public void InitializeTextureManager()
        {
            Il.ilInit();
            Ilu.iluInit();
            Ilut.ilutInit();
            Ilut.ilutRenderer(Ilut.ILUT_OPENGL);
        }
        
        public Texture GetTexture(string id)
        {
            return Textures[id];
            
        }

        public void Dispose()
        {
            foreach (Texture texture in Textures.Values)
            {
                // TO DO: REMOVE TEXTURES THAT ARE NO LONGER IN USE
            }
        }

        public void LoadTexture(string id, string path)
        {
            int devID = 0;
            Il.ilGenImages(1, out devID);
            // Set the texture
            Il.ilBindImage(devID);

            // If we cannot load the texture
            if (!Il.ilLoadImage(path))
            {
                System.Diagnostics.Debug.Assert(false, "Error Opening File, [" + path + "].");
            }

            Ilu.iluFlipImage();


            int width = Il.ilGetInteger(Il.IL_IMAGE_WIDTH);
            int height = Il.ilGetInteger(Il.IL_IMAGE_HEIGHT);

            int openGLID = Ilut.ilutGLBindTexImage();

            System.Diagnostics.Debug.Assert(openGLID != 0);

            Textures.Add(id, new Texture(openGLID, width, height));
        }



    }
}
