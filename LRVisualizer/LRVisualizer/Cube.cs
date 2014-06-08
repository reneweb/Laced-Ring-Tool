using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LRVisualizer
{
    public class Cube
    {
        private Vector3 size = Vector3.One;

        private Vector3 position = Vector3.Zero;

        private const int cornerTopFrontLeft = 0;
        private const int cornerTopBackLeft = 1;
        private const int cornerTopBackRight = 2;
        private const int cornerTopFrontRight = 3;

        private const int cornerBottomFrontLeft = 4;
        private const int cornerBottomBackLeft = 5;
        private const int cornerBottomBackRight = 6;
        private const int cornerBottomFrontRight = 7;

        private Triangle[] triangles = new Triangle[12];

        public Cube(Vector3 s, Vector3 pos)
        {
            size = s;
            position = pos;

            initialize();
        }

        private void initialize()
        {
            Vector3[] baseCorners = new Vector3[8];

            baseCorners[cornerTopFrontLeft] = position + new Vector3(-1, 1, 1) * size;
            baseCorners[cornerTopBackLeft] = position + new Vector3(-1, 1, -1) * size;
            baseCorners[cornerTopBackRight] = position + new Vector3(1, 1, -1) * size;
            baseCorners[cornerTopFrontRight] = position + new Vector3(1, 1, 1) * size;

            baseCorners[cornerBottomFrontLeft] = position + new Vector3(-1, -1, 1) * size;
            baseCorners[cornerBottomBackLeft] = position + new Vector3(-1, -1, -1) * size;
            baseCorners[cornerBottomBackRight] = position + new Vector3(1, -1, -1) * size;
            baseCorners[cornerBottomFrontRight] = position + new Vector3(1, -1, 1) * size;

            Color c = Color.Yellow;

            // top
            triangles[0] = new Triangle(baseCorners[cornerTopBackLeft], baseCorners[cornerTopBackRight], baseCorners[cornerTopFrontRight], c);
            triangles[1] = new Triangle(baseCorners[cornerTopBackLeft], baseCorners[cornerTopFrontRight], baseCorners[cornerTopFrontLeft], c);

            //front
            triangles[2] = new Triangle(baseCorners[cornerTopFrontLeft], baseCorners[cornerTopFrontRight], baseCorners[cornerBottomFrontLeft], c);
            triangles[3] = new Triangle(baseCorners[cornerBottomFrontLeft], baseCorners[cornerTopFrontRight], baseCorners[cornerBottomFrontRight], c);

            // back 
            triangles[4] = new Triangle(baseCorners[cornerBottomBackLeft], baseCorners[cornerBottomBackRight], baseCorners[cornerTopBackLeft], c);
            triangles[5] = new Triangle(baseCorners[cornerBottomBackLeft], baseCorners[cornerTopBackLeft], baseCorners[cornerTopBackRight], c);

            // bottom
            triangles[6] = new Triangle(baseCorners[cornerBottomBackLeft], baseCorners[cornerBottomBackRight], baseCorners[cornerBottomFrontLeft], c);
            triangles[7] = new Triangle(baseCorners[cornerBottomFrontLeft], baseCorners[cornerBottomBackRight], baseCorners[cornerBottomFrontRight], c);

            // left
            triangles[8] = new Triangle(baseCorners[cornerBottomBackLeft], baseCorners[cornerBottomFrontLeft], baseCorners[cornerTopBackLeft], c);
            triangles[9] = new Triangle(baseCorners[cornerTopBackLeft], baseCorners[cornerBottomFrontLeft], baseCorners[cornerTopFrontLeft], c);

            // right
            triangles[10] = new Triangle(baseCorners[cornerBottomBackRight], baseCorners[cornerBottomFrontRight], baseCorners[cornerTopFrontRight], c);
            triangles[11] = new Triangle(baseCorners[cornerBottomBackRight], baseCorners[cornerTopFrontRight], baseCorners[cornerTopBackRight], c);
        }

        public void Draw(GraphicsDevice d)
        {
            foreach (Triangle t in triangles)
                t.Draw(d);
        }

        internal class Triangle
        {
            internal VertexPositionColor[] vertices = new VertexPositionColor[3];

            internal Triangle(Vector3 v1, Vector3 v2, Vector3 v3, Color c)
            {
                vertices[0] = new VertexPositionColor(v1, c);
                vertices[1] = new VertexPositionColor(v2, c);
                vertices[2] = new VertexPositionColor(v3, c);
            }

            public void Draw(GraphicsDevice d)
            {
                d.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, 1);
            }
        }
    }

}
