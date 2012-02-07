using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.DevIl;

namespace U5Designs
{
    class TextureManager : IDisposable
    {
        Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();

        public void InitializeTextureManager()
        {

        }
        
        public Texture GetTexture(string id)
        {
            return Textures[id];
            
        }

        public void Dispose()
        {

        }

        public void LoadTexture(string id, string path)
        {

        }


    }
}
