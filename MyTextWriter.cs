using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing.Imaging;
using OpenTK;

namespace U5Designs
{
    class MyTextWriter
    {
        private readonly Font TextFont = new Font(FontFamily.GenericSansSerif, 30);
        private readonly Bitmap TextBitmap;
        private List<PointF> _positions;
        private List<string> _lines;
        private List<Brush> _colours;
        private int _textureId;
        private Size _clientSize;

        public void Update(int ind, string newText)
        {
            if (ind < _lines.Count)
            {
                _lines[ind] = newText;
                UpdateText();
            }
        }


        public MyTextWriter(Size ClientSize, Size areaSize)
        {
            _positions = new List<PointF>();
            _lines = new List<string>();
            _colours = new List<Brush>();

            TextBitmap = new Bitmap(areaSize.Width, areaSize.Height);
            this._clientSize = ClientSize;
            _textureId = CreateTexture();
        }

        private int CreateTexture()
        {
            int textureId;
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Replace);//Important, or wrong color on some computers
            Bitmap bitmap = TextBitmap;
            GL.GenTextures(1, out textureId);
            GL.BindTexture(TextureTarget.Texture2D, textureId);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            //    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Nearest);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Nearest);
            GL.Finish();
            bitmap.UnlockBits(data);
            return textureId;
        }

        public void Dispose()
        {
            if (_textureId > 0)
                GL.DeleteTexture(_textureId);
        }

        public void Clear()
        {
            _lines.Clear();
            _positions.Clear();
            _colours.Clear();
        }

        public void AddLine(string s, PointF pos, Brush col)
        {
            _lines.Add(s);
            _positions.Add(pos);
            _colours.Add(col);
            UpdateText();
        }

        public void UpdateText()
        {
            if (_lines.Count > 0)
            {
                using (Graphics gfx = Graphics.FromImage(TextBitmap))
                {
                    gfx.Clear(Color.Black);
                    gfx.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    for (int i = 0; i < _lines.Count; i++)
                        gfx.DrawString(_lines[i], TextFont, _colours[i], _positions[i]);
                }

                System.Drawing.Imaging.BitmapData data = TextBitmap.LockBits(new Rectangle(0, 0, TextBitmap.Width, TextBitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, TextBitmap.Width, TextBitmap.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                TextBitmap.UnlockBits(data);
            }
        }

        public void Draw(double xPos = 0, double yPos = 0, float scaleX = 1.0f, float scaleY = 1.0f)
        {
            //GL.PushMatrix();
            //GL.LoadIdentity();

            //Matrix4 ortho_projection = Matrix4.CreateOrthographicOffCenter(0, _clientSize.Width, _clientSize.Height, 0, -1, 1);
            //GL.MatrixMode(MatrixMode.Projection);

            //GL.PushMatrix();//
            //GL.LoadMatrix(ref ortho_projection);

            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactorSrc.One, OpenTK.Graphics.OpenGL.BlendingFactorDest.SrcColor);
            GL.Disable(EnableCap.DepthTest);
            GL.MatrixMode(MatrixMode.Projection);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)TextureEnvMode.Replace);

            GL.PushMatrix();
            GL.LoadIdentity();
            GL.Ortho(0, 1280, 0, 720, 0, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();

            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _textureId);
            int width = TextBitmap.Width;
            int height = TextBitmap.Height;

            float scaledWidth = (width * scaleX);
            float scaledHeight = (height * scaleY);
            double halfHeight = scaledHeight / 2;
            double halfWidth = scaledWidth / 2;
            // Quad positions
            double x = xPos;
            double y = yPos;
            double z = 0;


            double XLoc = x - halfWidth;
            double YLoc = y + halfHeight;

            float topUV = 0;
            float bottomUV = 1;
            float leftUV = 0;
            float rightUV = 1;

            GL.Begin(BeginMode.Triangles);
            {
                GL.TexCoord2(leftUV, topUV);
                GL.Vertex3(x - halfWidth, y + halfHeight, z); // top left
                GL.TexCoord2(rightUV, topUV);
                GL.Vertex3(XLoc, y + halfHeight, z); // top right
                GL.TexCoord2(leftUV, bottomUV);
                GL.Vertex3(x - halfWidth, y - halfHeight, z); // bottom left


                GL.TexCoord2(rightUV, topUV);
                GL.Vertex3(XLoc, y + halfHeight, z); // top right
                GL.TexCoord2(rightUV, bottomUV);
                GL.Vertex3(XLoc, y - halfHeight, z); // bottom right
                GL.TexCoord2(leftUV, bottomUV);
                GL.Vertex3(x - halfWidth, y - halfHeight, z); // bottom left

            }
            GL.End();

            GL.Disable(EnableCap.Blend);
            GL.Disable(EnableCap.Texture2D);

            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Projection);
            GL.PopMatrix();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.Enable(EnableCap.DepthTest);
        }
    }
}
