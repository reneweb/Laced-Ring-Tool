using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures;
using Structures.CTStructure;
using Structures.FBStructure;

namespace LacedRing.Converter
{
    // edge data structure
    struct Edge
    {
        // start and end vertex
        public int v1, v2;

        // hash code - needed for the dictionary
        public override int GetHashCode()
        {
            return (v1 + 10000 * v2);
        }

        //constructor - simplicity incarnate.
        public Edge(int v1, int v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        // equality operator. Edges are equal, when start and end point coiincide. 
        // Edge cannot be equal to anyhing than an edge
        public override bool Equals(object obj)
        {
            if (obj is Edge)
            {
                Edge other = (Edge)obj;
                if ((this.v1 == other.v1) && (this.v2 == other.v2))
                    return (true);
                else
                    return (false);
            }
            else
                return (false);
        }
    }

    internal class IflToCtConverter : IConverter<CTTriangleMesh>
    {
        private Point3D[] points;
        private Triangle[] triangles;

        private CTTriangleMesh ctTriangleMesh;

        internal IflToCtConverter(IFLTriangleMesh iflTriangleMesh)
        {
            points = iflTriangleMesh.V;
            triangles = iflTriangleMesh.Triangles;
        }

        public CTTriangleMesh Convert()
        {
            ctTriangleMesh = new CTTriangleMesh();
            ctTriangleMesh = InitCornerTable(ctTriangleMesh);
            ctTriangleMesh.Points = points;
            ctTriangleMesh.Triangles = triangles;

            return ctTriangleMesh;
        }

        // initializes the cornertable data structure
        // fills the incidentCorner and opposite 
        public CTTriangleMesh InitCornerTable(CTTriangleMesh ctTriangleMesh)
        {
            Dictionary<Edge, int> dictionary = new Dictionary<Edge, int>();
            ctTriangleMesh.Opposite = new int[this.triangles.Length * 3];
            ctTriangleMesh.IncidentCorner = new int[this.points.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle t = triangles[i];

                Edge e = new Edge(t.Vertex1, t.Vertex2);
                int cornerIndex = 3 * i;
                e = ProcessEdge(dictionary, e, cornerIndex);
                ctTriangleMesh.IncidentCorner[t.Vertex3] = cornerIndex;

                e = new Edge(t.Vertex2, t.Vertex3);
                cornerIndex = 3 * i + 1;
                e = ProcessEdge(dictionary, e, cornerIndex);
                ctTriangleMesh.IncidentCorner[t.Vertex1] = cornerIndex;

                e = new Edge(t.Vertex3, t.Vertex1);
                cornerIndex = 3 * i + 2;
                e = ProcessEdge(dictionary, e, cornerIndex);
                ctTriangleMesh.IncidentCorner[t.Vertex2] = cornerIndex;
            }

            return ctTriangleMesh;
        }

        // sees whether the edge is already in the dictionary. If it isn't, then it is added.
        private Edge ProcessEdge(Dictionary<Edge, int> dictionary, Edge e, int cornerIndex)
        {
            if (dictionary.ContainsKey(e))
            {
                int oppositeCornerIndex = dictionary[e];
                ctTriangleMesh.Opposite[cornerIndex] = oppositeCornerIndex;
                ctTriangleMesh.Opposite[oppositeCornerIndex] = cornerIndex;
            }
            else
            {
                Edge reversed = new Edge(e.v2, e.v1);
                dictionary.Add(reversed, cornerIndex);
            }
            return e;
        }
    }
}
