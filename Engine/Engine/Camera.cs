using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Engine
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 End;
        public int Width;
        public int Height;

        public Camera(Vector3 position, Vector3 end, int width, int height)
        {
            Position = position;
            End = end;
            Width = width;
            Height = height;
        }

        public void Set2DCamera()
        {
            SetOrthographic(Width, Height);
            End.X += 100;
            End.Y += 50;
            Position.X = End.X;
            Position.Y = End.Y;
            Position.Z = End.Z + 100;
        }

        public void Set3DCamera()
        {
            SetPerspective(Width, Height);
            End.X -= 100;
            End.Y -= 50;
            Position.X = End.X - 120;
            Position.Y = End.Y + 25;
            Position.Z = End.Z; ;

        }

        public void SetOrthographic(int width, int height)
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreateOrthographic(width / 4, height / 4, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);

        }

        public void SetPerspective(int width, int height)
        {
            GL.MatrixMode(MatrixMode.Projection);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI / 6, width / (float)height, 1.0f, 6400.0f);
            GL.LoadMatrix(ref projection);
        }

        public void SetModelView()
        {
            Matrix4 modelview = Matrix4.LookAt(Position, End, Vector3.UnitY);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

        public void Update(float playerDeltaX)
        {
            Position.X += playerDeltaX;
            End.X += playerDeltaX;
        }

    }
}
