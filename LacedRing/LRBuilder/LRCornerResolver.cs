using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Structures.CTStructure;
using Structures.LRStructure;

namespace LacedRing.LRBuilder
{
    public class LRCornerResolver
    {
        public List<int> O { get; private set; }
        public List<int> OS { get; private set; }
        public List<int> IsolatedCorners { get; private set; }

        private LRTriangleMesh lrTriangleMesh;
        private CTTriangleMesh ctTriangleMesh;

        private int oCornerOffset = 0;

        private bool[] tMarked;
        private int[] vertexMapping;

        private Dictionary<int, Tuple<int, int>> ctCornerOppLrCornerMap = new Dictionary<int, Tuple<int, int>>();
        private Dictionary<int, Tuple<int, int>> ctOppCornerOsCornerMap = new Dictionary<int, Tuple<int, int>>();

        public LRCornerResolver(ref LRTriangleMesh lrTriangleMesh, ref CTTriangleMesh ctTriangleMesh, ref bool[] tMarked, ref int[] vertexMapping)
        {
            this.lrTriangleMesh = lrTriangleMesh;
            this.ctTriangleMesh = ctTriangleMesh;
            this.tMarked = tMarked;
            this.vertexMapping = vertexMapping;

            O = new List<int>();
            OS = new List<int>();
            IsolatedCorners = new List<int>();
        }

        public void UpdateLrMesh(LRTriangleMesh lrTriangleMesh)
        {
            this.lrTriangleMesh = lrTriangleMesh;
        }

        /// <summary>
        /// Resolves the corners which need to be explicitly stored in the O, OS and C-Structures for the given ct-corner c1.
        /// This is only necessary if the corner and respectivly the vertex is part of a T0
        /// </summary>
        /// <param name="c">The corner from the corner table structure to process</param>
        /// <param name="co">The opposite corner from the corner table structure</param>
        public void ResolveCorners(int c, int co)
        {
            //Used if theres a expensive triangle, with two adjacent T0-triangles
            if (ctOppCornerOsCornerMap.ContainsKey(c))
            {
                if (oCornerOffset % 8 == 3 || oCornerOffset % 8 == 7) //it's necessary to skip those entries
                    oCornerOffset++;

                OS[ctOppCornerOsCornerMap[c].Item2] = 8 * lrTriangleMesh.MR + oCornerOffset; //Add opposite corner of ET
                oCornerOffset++;

                O.Add(ctOppCornerOsCornerMap[c].Item1);
                return;
            }

            //Check if adjacent triangle is T2 triangle
            if (IsRingEdge(co, ctTriangleMesh.N(co)) && IsRingEdge(co, ctTriangleMesh.P(co)))
            {
                int sc;
                int sc1;

                if (tMarked[co / 3])
                {
                    sc = co; //Corner of start vertex from the adjacent triangle
                    sc1 = ctTriangleMesh.P(co); //Since it's a T2, there is a second ring edge and thus a second start vertex

                    if (oCornerOffset % 8 == 3 || oCornerOffset % 8 == 7) //it's necessary to skip those entries
                        oCornerOffset++;

                    OS.Add(8 * lrTriangleMesh.MR + oCornerOffset); //Add opposite corner of et

                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[ctTriangleMesh.N(sc)], -1);
                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[sc1], -1);

                    OS.Add(8 * lrTriangleMesh.MR + oCornerOffset); //Same entry twice, because one vertex is part of both ring edges
                    ResolveIsolatedVertexCorner(vertexMapping[ctTriangleMesh.V(c)], 8 * lrTriangleMesh.MR + oCornerOffset);
                    oCornerOffset++;

                    //Create opposite entry
                    O.Add(vertexMapping[ctTriangleMesh.V(co)] * 8);
                }
                else
                {
                    sc = co; //Corner of start vertex from the adjacent triangle
                    sc1 = ctTriangleMesh.N(co); //Since it's a T2, there is a second ring edge and thus a second start vertex

                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[sc1], -1);

                    if (oCornerOffset % 8 == 3 || oCornerOffset % 8 == 7) //it's necessary to skip those entries
                        oCornerOffset++;

                    OS.Add(8 * lrTriangleMesh.MR + oCornerOffset); //Add opposite corner of et
                    OS.Add(8 * lrTriangleMesh.MR + oCornerOffset); //Same entry twice, because one vertex is part of both ring edges
                    ResolveIsolatedVertexCorner(vertexMapping[ctTriangleMesh.V(c)], 8 * lrTriangleMesh.MR + oCornerOffset);
                    oCornerOffset++;

                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[ctTriangleMesh.P(sc)], -1);

                    //Create opposite entry
                    O.Add(vertexMapping[ctTriangleMesh.V(co)] * 8 + 6);
                }
            }
            //Check if adjacent triangle has oc and oc.n as a ring edge
            else if (IsRingEdge(co, ctTriangleMesh.N(co)))
            {
                int sc;
                int lrC;
                int lrC1;

                if (tMarked[co / 3])
                {
                    sc = co; //Corner of start vertex from the adjacent triangle
                    lrC = vertexMapping[ctTriangleMesh.V(sc)] * 8 + 2; //calc. laced ring corner
                    lrC1 = vertexMapping[ctTriangleMesh.V(sc)] * 8; //calc. laced ring corner of the other ring vertex

                    AddOsEntry(c);
                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[ctTriangleMesh.N(sc)], lrC);
                }
                else
                {
                    sc = ctTriangleMesh.N(co); //Corner of start vertex from the adjacent triangle
                    lrC = vertexMapping[ctTriangleMesh.V(sc)] * 8 + 6;  //calc. laced ring corner
                    lrC1 = vertexMapping[ctTriangleMesh.V(sc)] * 8 + 4; //calc. laced ring corner of the other ring vertex

                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[sc], lrC);
                    AddOsEntry(c);
                }

                O.Add(lrC1);
            }
            //Check if adjacent triangle has oc and oc.p as a ring edge
            else if (IsRingEdge(co, ctTriangleMesh.P(co)))
            {
                int sc;
                int lrC;
                int lrC1;

                if (tMarked[co / 3])
                {
                    sc = ctTriangleMesh.P(co); //Corner of start vertex from the adjacent triangle
                    lrC = vertexMapping[ctTriangleMesh.V(sc)] * 8; //calc. laced ring corner
                    lrC1 = vertexMapping[ctTriangleMesh.V(sc)] * 8 + 2; //calc. laced ring corner of the other ring vertex

                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[sc], lrC);
                    AddOsEntry(c);
                }
                else
                {
                    sc = co; //Corner of start vertex from the adjacent triangle
                    lrC = vertexMapping[ctTriangleMesh.V(sc)] * 8 + 4; //calc. laced ring corner
                    lrC1 = vertexMapping[ctTriangleMesh.V(sc)] * 8 + 6; //calc. laced ring corner of the other ring vertex

                    AddOsEntry(c);
                    ResolveOtherETOpposite(ctTriangleMesh.Opposite[ctTriangleMesh.P(sc)], lrC);
                }

                O.Add(lrC1);
            }
            //The adjacent triangle is a T0
            else
            {
                if (oCornerOffset % 8 == 3 || oCornerOffset % 8 == 7) //it's necessary to skip those entries
                    oCornerOffset++;

                ResolveT0AdjacentCorners(c, co);
                ResolveIsolatedVertexCorner(vertexMapping[ctTriangleMesh.V(c)], 8 * lrTriangleMesh.MR + oCornerOffset);
                oCornerOffset++;

                return;
            }
        }

        private void AddOsEntry(int c)
        {
            if (oCornerOffset % 8 == 3 || oCornerOffset % 8 == 7) //it's necessary to skip those entries
                oCornerOffset++;

            OS.Add(8 * lrTriangleMesh.MR + oCornerOffset); //Add opposite corner of et
            ResolveIsolatedVertexCorner(vertexMapping[ctTriangleMesh.V(c)], 8 * lrTriangleMesh.MR + oCornerOffset);
            oCornerOffset++;
        }

        /// <summary>
        /// Resolves the opposite corner for the other ring vertex in a expensive triangle (Since in the VO* table the corner for both ring vertices, sharing a ring edge, must be entered)
        /// </summary>
        /// <param name="ctOc">The opposite corner (ct structure) of the corner from the ring vertex</param>
        /// <param name="lrC">The corner (laced ring structure) of the ring vertex.</param>
        private void ResolveOtherETOpposite(int ctOc, int lrC)
        {
            //Check if T2
            if (IsRingEdge(ctOc, ctTriangleMesh.N(ctOc)) && IsRingEdge(ctOc, ctTriangleMesh.P(ctOc)))
                ctOc = (tMarked[ctOc / 3]) ? vertexMapping[ctTriangleMesh.V(ctOc)] * 8 : vertexMapping[ctTriangleMesh.V(ctOc)] * 8 + 6;
            //Check if T2 with other ring edges
            else if (IsRingEdge(ctOc, ctTriangleMesh.N(ctOc)) && IsRingEdge(ctTriangleMesh.N(ctOc), ctTriangleMesh.P(ctOc)))
                ctOc = (tMarked[ctOc / 3]) ? vertexMapping[ctTriangleMesh.V(ctTriangleMesh.N(ctOc))] * 8 + 1 : vertexMapping[ctTriangleMesh.V(ctTriangleMesh.P(ctOc))] * 8 + 5;
            //Check if c and c.n form a ring edge
            else if (IsRingEdge(ctOc, ctTriangleMesh.N(ctOc)))
                ctOc = (tMarked[ctOc / 3]) ? vertexMapping[ctTriangleMesh.V(ctOc)] * 8 : vertexMapping[ctTriangleMesh.V(ctTriangleMesh.N(ctOc))] * 8 + 4;
            //Check if c and c.p form a ring edge
            else if (IsRingEdge(ctOc, ctTriangleMesh.P(ctOc)))
                ctOc = (tMarked[ctOc / 3]) ? vertexMapping[ctTriangleMesh.V(ctTriangleMesh.P(ctOc))] * 8 + 2 : vertexMapping[ctTriangleMesh.V(ctOc)] * 8 + 6;
            //Check if c.n and c.p form a ring edge
            else if (IsRingEdge(ctTriangleMesh.N(ctOc), ctTriangleMesh.P(ctOc)))
                ctOc = (tMarked[ctOc / 3]) ? vertexMapping[ctTriangleMesh.V(ctTriangleMesh.N(ctOc))] * 8 + 1 : vertexMapping[ctTriangleMesh.V(ctTriangleMesh.P(ctOc))] * 8 + 5;
            //It's a T0
            else
            {
                ctOppCornerOsCornerMap[ctOc] = new Tuple<int, int>(lrC, OS.Count);
                ctOc = -1;
            }

            OS.Add(ctOc); //Add opposite corner of ET
        }

        private void ResolveT0AdjacentCorners(int c, int co)
        {
            if (ctCornerOppLrCornerMap.ContainsKey(c))
            {
                O[ctCornerOppLrCornerMap[c].Item1] = 8 * lrTriangleMesh.MR + oCornerOffset; //At the opposite corner index (which had the dummy -1 entry), set the correct corner (the current one c1)
                O.Add(ctCornerOppLrCornerMap[c].Item2);//Add the opposite corner of c1, which is stored in the map
            }
            else
            {
                ctCornerOppLrCornerMap.Add(co, new Tuple<int, int>(O.Count, 8 * lrTriangleMesh.MR + oCornerOffset)); //Get the laced ring corner index of the current corner (c1) and store at c2 (ct strcuture)
                O.Add(-1); //For now create a dummy entry at corner pos
            }
        }

        private void ResolveIsolatedVertexCorner(int lrV, int lrC)
        {
            //If v1 (vertex of c1) is a isolated vertex, it's necessary to store one corner (If it hasn't been done already).
            if (lrV >= lrTriangleMesh.MR && IsolatedCorners.Count <= (lrV - lrTriangleMesh.MR))
            {
                    IsolatedCorners.Add(lrC);
            }
        }

        /// <summary>
        /// Returns true if the first corner (from ct structure) and second corner (from ct structure) form a ring edge in lr structure.
        /// Note: if result is true, this doesn't say anything about the direction of the ring edge, i.e. the first corner could be the start vertex as well as the other one.
        /// </summary>
        private bool IsRingEdge(int ctC1, int ctC2)
        {
            return lrTriangleMesh.IsRingEdge(vertexMapping[ctTriangleMesh.V(ctC1)], vertexMapping[ctTriangleMesh.V(ctC2)]);
        }
    }
}
