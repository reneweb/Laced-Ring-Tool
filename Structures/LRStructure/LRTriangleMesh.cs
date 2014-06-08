using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.LRStructure
{
    public class LRTriangleMesh : AbstractTriangleMesh
    {
        /// <summary>
        /// Number of ring vertices
        /// </summary>
        public int MR;

        /// <summary>
        /// Vertices, 0-mr (ring vertices) holds the ring vertices, rest is used for isolated vertices
        /// </summary>
        public Point3D[] V;

        /// <summary>
        /// T1 and T2 Triangles (represented by a reference to the vertex which is not on the ring edge, the other two are given implicitly)
        /// </summary>
        public int[] LR;

        /// <summary>
        /// Stores reference to one corner for each isolated vertex.
        /// </summary>
        public int[] C;

        //---- Corner table representation for T0-triangles
        public Triangle[] T;
        /// <summary>
        /// Opposite vertices of T0-Triangles (since for T0-Triangles the usual Corner Table representation is used)
        /// </summary>
        public int[] O;

        //---- (Condensed) Corner table representation for expensive T1/T2-triangles
        public int[] TS;
        /// <summary>
        /// Opposite vertices for expensive T1/T2-triangles
        /// </summary>
        public int[] OS;

        /// <summary>
        /// next vertex on ring
        /// </summary>
        public int VN(int v)
        {
            return ((v + 1) % MR);
        }

        /// <summary>
        /// previous vertex on ring
        /// </summary>
        public int VP(int v)
        {
            return ((v + MR - 1) % MR);
        }

        /// <summary>
        /// left triangle (return vertex which is not on the ring from the triangle on the left of the ring edge)
        /// </summary>
        /// <param name="v">The start-vertex of the ring-edge</param>
        public int VL(int v)
        {
            int l = LR[2 * v];
            if (l >= 0)
            {
                return l;
            }
            else
            {
                int index = l * (-1) - 1;
                return TS[index];
            }
        }

        /// <summary>
        /// right triangle (return vertex which is not on the ring from the triangle on the right of the ring edge)
        /// </summary>
        /// <param name="v">The start-vertex of the ring-edge</param>
        public int VR(int v)
        {
            int r = LR[2 * v + 1];
            if (r >= 0)
            {
                return r;
            }
            else
            {
                int index = r * (-1) - 1;
                return TS[index];
            }
        }

        /// <summary>
        /// vertex of corner c
        /// </summary>
        public int CV(int c)
        {
            if (c >= 8 * MR)
            {
                int index = (c - c / 4 - 6 * MR);
                int t = index / 3;

                if ((index % 3) == 0) return (T[t].Vertex1);
                if ((index % 3) == 1) return (T[t].Vertex2);
                if ((index % 3) == 2) return (T[t].Vertex3);
                return -1;
            }
            else
            {
                int v = c / 8;

                if (c % 8 == 0 || c % 8 == 6) return v;
                if (c % 8 == 2 || c % 8 == 4) return VN(v);
                if (c % 8 == 1) return VL(v);
                if (c % 8 == 5) return VR(v);
                return -1;
            }
        }

        /// <summary>
        /// triangle of corner c
        /// </summary> 
        public int CT(int c)
        {
            return (c / 4);
        }

        /// <summary>
        /// next corner around c.t
        /// </summary> 
        public int CN(int c)
        {
            if (c % 4 == 2)
                return (c - 2);
            else
                return (c + 1);
        }

        /// <summary>
        /// previous corner around c.t
        /// </summary> 
        public int CP(int c)
        {
            if (c % 4 == 0)
                return (c + 2);
            else
                return (c - 1);
        }

        /// <summary>
        /// next corner around c.v
        /// </summary> 
        public int CS(int c)
        {
            return CN(CL(c));
        }

        /// <summary>
        /// corner opposite of c
        /// </summary> 
        public int CO(int c)
        {
            if (c >= 8 * MR)
            {
                return O[(c - c / 4 - 6 * MR)];
            }
            else
            {
                int v = c / 8;

                if (((c % 8 == 0 || c % 8 == 2) && LR[2 * v] < 0)) //Expensive triangle
                {
                    int index = LR[2 * v] * (-1) - 1; //Calc index

                    if (c % 8 == 0) return OS[2 * index];
                    else return OS[2 * index + 1];
                }
                else if (((c % 8 == 4 || c % 8 == 6) && LR[2 * v + 1] < 0)) //Expensive triangle
                {
                    int index = LR[2 * v + 1] * (-1) - 1; //Calc index

                    if (c % 8 == 6) return OS[2 * index];
                    else return OS[2 * index + 1];
                }
                else
                {
                    //For v.1 and v.5 (corners are seperated through ring)
                    if (c % 8 == 1) return 8 * v + 5;
                    if (c % 8 == 5) return 8 * v + 1;

                    //Cases where corner is infered through ring operations
                    if (c % 8 == 0 && VL(v) == VL(VN(v))) return 8 * VN(v) + 2; //with curve
                    if (c % 8 == 6 && VR(v) == VR(VN(v))) return 8 * VN(v) + 4;

                    if (c % 8 == 2 && VL(v) == VL(VP(v))) return 8 * VP(v);
                    if (c % 8 == 4 && VR(v) == VR(VP(v))) return 8 * VP(v) + 6;

                    if (c % 8 == 2 && VP(v) == VL(v)) return 8 * VP(v) + 5; //T2
                    if (c % 8 == 4 && VP(v) == VR(v)) return 8 * VP(v) + 1;

                    if (c % 8 == 0 && v == VL(VN(v))) return 8 * VN(v) + 5;
                    if (c % 8 == 6 && v == VR(VN(v))) return 8 * VN(v) + 1;

                    if (c % 8 == 0 && VL(VP(VL(v))) == VN(v)) return 8 * VP(VL(v)); // ring edges parallel
                    if (c % 8 == 6 && VR(VP(VR(v))) == VN(v)) return 8 * VP(VR(v)) + 6;

                    if (c % 8 == 2 && v == VL(VL(v))) return 8 * VL(v) + 2;
                    if (c % 8 == 4 && v == VR(VR(v))) return 8 * VR(v) + 4;

                    return -1;
                }
            }
        }

        /// <summary>
        /// left neighbor c.n.o of c
        /// </summary> 
        public int CL(int c)
        {
            return CO(CN(c));
        }

        /// <summary>
        /// right neighbor c.p.o of c
        /// </summary> 
        public int CR(int c)
        {
            return CO(CP(c));
        }

        /// <summary>
        /// a corner of vertex v
        /// </summary> 
        public int VC(int v)
        {
            if (v >= MR)
                return C[v - MR];
            else if (VL(v) == VN(VN(v)))
                return (8 * VN(v) + 1);
            else if (VR(v) == VN(VN(v)))
                return (8 * VN(v) + 5);
            else
                return (8 * v);
        }

        /// <summary>
        /// a corner of triangle t
        /// </summary> 
        public int TC(int t)
        {
            return 4 * t;
        }

        public bool IsRingEdge(int v1, int v2)
        {
            if (v1 >= MR || v2 >= MR) return false;

            if (v1 == ((v2 - 1) + MR) % MR || v2 == ((v1 - 1) + MR) % MR)
            {
                return true;
            }

            return false;
        }

        public int GetStartVertexOfRingEdge(int v1, int v2)
        {
            if (v1 == ((v2 - 1) + MR) % MR)
            {
                return v1;
            }
            else
            {
                return v2;
            }
        }
    }
}
