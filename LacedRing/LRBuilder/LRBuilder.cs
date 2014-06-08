using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LacedRing.Converter;
using Structures;
using Structures.CTStructure;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LacedRing.LRBuilder
{
    public class LRBuilder
    {
        private CTTriangleMesh ctTriangleMesh;
        private LRTriangleMesh lrTriangleMesh;

        private bool[] vMarked;
        private bool[] tMarked;

        public LRBuilder(Point3D[] vertices, Triangle[] triangles)
        {
            IFLTriangleMesh iflTriangleMesh = new IFLTriangleMesh();
            iflTriangleMesh.V = vertices;
            iflTriangleMesh.Triangles = triangles;

            IConverter<CTTriangleMesh> iflToCt = Converter.Converter.GetSpecificConverter<IFLTriangleMesh, CTTriangleMesh>(iflTriangleMesh);
            DateTime startTime = DateTime.Now;

            this.ctTriangleMesh = iflToCt.Convert();

            DateTime endTime = DateTime.Now;
            TimeSpan span = endTime.Subtract(startTime);

            Init();
        }

        public LRBuilder(CTTriangleMesh ctTriangleMesh)
        {
            this.ctTriangleMesh = ctTriangleMesh;

            Init();
        }

        private void Init()
        {
            vMarked = new bool[ctTriangleMesh.Points.Length];
            tMarked = new bool[ctTriangleMesh.Triangles.Length];
        }

        public LRTriangleMesh Build()
        {
            int[] vertexMapping;
            lrTriangleMesh = new LRTriangleMesh();

            //Run ring expander
            RingExpander();

            //Resolve vertices
            LRVertexResolver lrVertexResolver = new LRVertexResolver(ctTriangleMesh, lrTriangleMesh, vMarked, tMarked);
            lrTriangleMesh = lrVertexResolver.ResolveVertices(out vertexMapping);

            //Resolve triangles
            LRTriangleResolver lrTriangleResolver = new LRTriangleResolver(ctTriangleMesh, lrTriangleMesh, vMarked, tMarked);
            lrTriangleMesh = lrTriangleResolver.ResolveTriangles(vertexMapping);

            return lrTriangleMesh;
        }

        private void RingExpander()
        {
            int s = ctTriangleMesh.IncidentCorner[new Random().Next(ctTriangleMesh.IncidentCorner.Length)];

            int c = s; // start at the seed corner s
            vMarked[ctTriangleMesh.V(ctTriangleMesh.N(c))] = vMarked[ctTriangleMesh.V(ctTriangleMesh.P(c))] = true; // mark vertices as visited
            do
            {
                if (!vMarked[ctTriangleMesh.V(c)])
                {
                    vMarked[ctTriangleMesh.V(c)] = tMarked[c / 3] = true; // invade c.T
                }
                else if (!tMarked[c / 3])
                {
                    c = ctTriangleMesh.Opposite[c]; // go back one triangle
                }

                c = ctTriangleMesh.Opposite[ctTriangleMesh.N(c)]; // advance to next ring edge on the right
            }
            while (c != ctTriangleMesh.Opposite[s]); // until back at the beginning
        }
    }
}
