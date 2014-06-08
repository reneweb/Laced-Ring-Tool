using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.CTStructure
{
    public class CTTriangleMesh : AbstractTriangleMesh
    {
        public Point3D[] Points { get; set; }
        public Triangle[] Triangles { get; set; }
        public int[] IncidentCorner { get; set; }   
        public int[] Opposite { get; set; }

        /// <summary>
        /// returns next corner
        /// </summary>
        public int N(int c)
        {
            if (c % 3 == 2)
                return (c - 2);
            else
                return (c + 1);
        }

        /// <summary>
        /// returns previous corner
        /// </summary> 
        public int P(int c)
        {
            if (c % 3 == 0)
                return (c + 2);
            else
                return (c - 1);
        }

        /// <summary>
        ///  returns vertex incident with a corner
        /// </summary>
        public int V(int c)
        {
            int t = c / 3;
            if ((c % 3) == 0)
                return (Triangles[t].Vertex3);
            if ((c % 3) == 1)
                return (Triangles[t].Vertex1);
            if ((c % 3) == 2)
                return (Triangles[t].Vertex2);
            return (-1);
        }

        /// <summary>
        /// returns vertices incident with a vertex
        /// </summary>
        public int[] VV(int vertex)
        {
            List<int> result = new List<int>();
            int initCorner = IncidentCorner[vertex];

            int o = Opposite[N(initCorner)];
            result.Add(V(o));
            int current = N(o);
            while (current != initCorner)
            {
                o = Opposite[N(current)];
                result.Add(V(o));
                current = N(o);
            }
            return (result.ToArray());
        }

        /// <summary>
        /// returns faces incident with a vertex
        /// </summary>
        public int[] VF(int vertex)
        {
            List<int> result = new List<int>();
            int initCorner = IncidentCorner[vertex];

            int o = Opposite[N(initCorner)];
            result.Add(o / 3);
            int current = N(o);
            while (current != initCorner)
            {
                o = Opposite[N(current)];
                result.Add(o / 3);
                current = N(o);
            }
            return (result.ToArray());
        }

        public int GetSpecificCorner(int t, int v)
        {
            int c = t * 3;
            if (Triangles[t].Vertex3 == v)
                return c;
            if (Triangles[t].Vertex1 == v)
                return c + 1;
            if (Triangles[t].Vertex2 == v)
                return c + 2;
            return (-1);
        }
    }
}
