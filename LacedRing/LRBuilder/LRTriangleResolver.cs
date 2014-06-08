using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Structures;
using Structures.CTStructure;
using Structures.LRStructure;

namespace LacedRing.LRBuilder
{
    public class LRTriangleResolver
    {
        private CTTriangleMesh ctTriangleMesh;
        private LRTriangleMesh lrTriangleMesh;
        private bool[] vMarked;
        private bool[] tMarked;

        private List<Triangle> t = new List<Triangle>();
        //private List<int> o = new List<int>();
        private List<int> ts = new List<int>();
        //private List<int> os = new List<int>();
        //private List<int> isolatedCorners = new List<int>();

        private int isolatedVIndex = 0;
        private int[] vertexMapping;

        private bool[] tProcessed;

        private LRCornerResolver lrCornerResolver;

        public LRTriangleResolver(CTTriangleMesh ctTriangleMesh, LRTriangleMesh lrTriangleMesh, bool[] vMarked, bool[] tMarked)
        {
            this.ctTriangleMesh = ctTriangleMesh;
            this.lrTriangleMesh = lrTriangleMesh;
            this.vMarked = vMarked;
            this.tMarked = tMarked;

            lrTriangleMesh.LR = new int[2 * lrTriangleMesh.MR];
            tProcessed = new bool[ctTriangleMesh.Triangles.Length];
        }

        public LRTriangleMesh ResolveTriangles(int[] vertexMapping)
        {
            this.vertexMapping = vertexMapping;
            lrCornerResolver = new LRCornerResolver(ref lrTriangleMesh, ref ctTriangleMesh, ref tMarked, ref vertexMapping);

            //Loop through all vertices
            for (int i = 0; i < vertexMapping.Length; i++)
            {
                int[] faces = ctTriangleMesh.VF(i);
                int[][] facesSharingRingEdge = LRUtils.GetFacesSharingRingEdge(faces, tMarked);

                if (vMarked[i]) //If vertex is marked then its not isolated and therefore has a ring edge (is start vertex of a ring edge)
                {
                    if (facesSharingRingEdge[0] != null && !tProcessed[facesSharingRingEdge[0][0]])
                    {
                        //Resolve "left" triangle
                        int c = ctTriangleMesh.GetSpecificCorner(facesSharingRingEdge[0][0], i); //Get the corner of the vertex, which is incident with the marked triangle of the ring edge, where the vertex is the start vertex
                        int pc = ctTriangleMesh.P(c); //Get the next corner around the triangle, which is the one facing the ring edge
                        
                        lrTriangleMesh.LR[(2 * vertexMapping[ctTriangleMesh.V(c)])] = vertexMapping[ctTriangleMesh.V(pc)]; //Store the mapped vertex index (laced ring vertex index) in the LR table at the correct pos.
                    }

                    if (facesSharingRingEdge[0] != null && !tProcessed[facesSharingRingEdge[0][1]])
                    {
                        //Resolve "right" triangle
                        int c = ctTriangleMesh.GetSpecificCorner(facesSharingRingEdge[0][1], i); //Get the corner of the vertex, which is incident with the marked triangle of the ring edge, where the vertex is the start vertex
                        int nc = ctTriangleMesh.N(c); //Get the previous corner around the triangle, which is the one facing the ring edge
                        
                        lrTriangleMesh.LR[(2 * vertexMapping[ctTriangleMesh.V(c)]) + 1] = vertexMapping[ctTriangleMesh.V(nc)]; //Store the mapped vertex index (laced ring vertex index) in the LR table at the correct pos.
                    }

                    T0LookUp(i, faces); //Look up if vertex is part of a T0 and handle accordingly
                }
                else //Isolated vertex
                {
                    CheckAndResolveIsolatedVertex(i); //Create entry for isolated vertex
                    T0LookUp(i, faces); //Look up if it is part of a T0 and handle accordingly
                }
            }

            //Store results in lr object
            lrTriangleMesh.T = t.ToArray();
            lrTriangleMesh.O = lrCornerResolver.O.ToArray();
            lrTriangleMesh.TS = ts.ToArray();
            lrTriangleMesh.OS = lrCornerResolver.OS.ToArray();
            lrTriangleMesh.C = lrCornerResolver.IsolatedCorners.ToArray();

            return lrTriangleMesh;
        }

        private void T0LookUp(int currIndex, int[] faces)
        {
            //Look through all the faces, the vertex is incident with and find all T0-triangles
            for (int j = 0; j < faces.Length; j++)
            {
                if (!tProcessed[faces[j]]) //Only look at faces, which haven't been already processed
                {
                    //Get the specific corner for the vertex and current face and get the next and previous corners of the triangle
                    int c = ctTriangleMesh.GetSpecificCorner(faces[j], currIndex);
                    int cn = ctTriangleMesh.N(c);
                    int cp = ctTriangleMesh.P(c);

                    //Happens if theres a vertex defined which is not used in the mesh
                    if (c == -1) return;

                    //Check if face has no ring edge at all -> then it's a T0
                    if (!IsRingEdge(c, cn) && !IsRingEdge(c, cp) && !IsRingEdge(cn, cp))
                    {
                        CheckAndResolveIsolatedVertex(ctTriangleMesh.V(cn));
                        CheckAndResolveIsolatedVertex(ctTriangleMesh.V(cp));
                        
                        //Create and add triangle
                        Triangle triangle = new Triangle
                        {
                            Vertex1 = vertexMapping[ctTriangleMesh.V(c)],
                            Vertex2 = vertexMapping[ctTriangleMesh.V(cp)],
                            Vertex3 = vertexMapping[ctTriangleMesh.V(cn)]
                        };

                        t.Add(triangle);

                        //Get opposite entries, for each corner
                        int co = ctTriangleMesh.Opposite[c];
                        int cno = ctTriangleMesh.Opposite[cn];
                        int cpo = ctTriangleMesh.Opposite[cp];

                        //Check if any of those opposite vertices is an isolated one and create an entry if necessary
                        CheckAndResolveIsolatedVertex(ctTriangleMesh.V(co));
                        CheckAndResolveIsolatedVertex(ctTriangleMesh.V(cno));
                        CheckAndResolveIsolatedVertex(ctTriangleMesh.V(cpo));

                        //Get all adjacent, expensive Triangles of the T0 (Expensive triangles are those adjacent with the T0, which are not a T0 themselves)
                        if(!tProcessed[co/3]) ExpensiveTriangleLookup(co);
                        if (!tProcessed[cpo / 3]) ExpensiveTriangleLookup(cpo);
                        if (!tProcessed[cno / 3]) ExpensiveTriangleLookup(cno);

                        //resolve the corners, which need to be stored explicitly
                        //lrCornerResolver.UpdateLrMesh(lrTriangleMesh);
                        lrCornerResolver.ResolveCorners(c, co);
                        lrCornerResolver.ResolveCorners(cp, cpo);
                        lrCornerResolver.ResolveCorners(cn, cno);  

                        //Mark this triangle as processed
                        tProcessed[faces[j]] = true;
                    }
                }
            }
        }

        private void ExpensiveTriangleLookup(int c)
        {
            //If the triangle is marked it's to the "left" of the ring edge and won't have an offset in the LR-table. Otherwise it's a "right" one and will have and offset of 1.
            int offset = tMarked[c / 3] ? 0 : 1;

            bool isT0 = true;

            //If c and c.n form a ring edge, it's not a T0
            if (IsRingEdge(c, ctTriangleMesh.N(c)))
            {
                //Find the corner of the start vertex, by looking if the vertex is marked (left) or not marked (right)
                int sc = tMarked[c / 3] ? c : ctTriangleMesh.N(c);

                lrTriangleMesh.LR[2 * vertexMapping[ctTriangleMesh.V(sc)] + offset] = -ts.Count - 1; //Enter special negative value in LR table, which leads to entries in the TS and OS tables.
                ts.Add(vertexMapping[ctTriangleMesh.V(ctTriangleMesh.P(c))]); //Enter value for ts

                isT0 = false;
            }

            //If c and c.p form a ring edge, it's not a T0
            if (IsRingEdge(c, ctTriangleMesh.P(c)))
            {
                //Find the corner of the start vertex, by looking if the vertex is marked (left) or not marked (right)
                int sc = tMarked[c / 3] ? ctTriangleMesh.P(c) : c;

                lrTriangleMesh.LR[2 * vertexMapping[ctTriangleMesh.V(sc)] + offset] = -ts.Count - 1; //Enter special negative value in LR table, which leads to entries in the TS and OS tables.
                ts.Add(vertexMapping[ctTriangleMesh.V(ctTriangleMesh.N(c))]); //Enter value for ts

                isT0 = false;
            }

            if (!isT0) //Only if triangle is not a T0 it got processed
                tProcessed[c / 3] = true; //Mark triangle as processed
        }

        /// <summary>
        /// Checks if given vertex (from ct structure) is a isolated vertex.
        /// If it is isolated an entry will be made in the V-Table of the LR-Structure and in the vertex-map
        /// </summary>
        /// <param name="vertex">The vertex from the ct structure to process</param>
        /// <returns>True, if it is a isolated vertex, false otherwise</returns>
        private bool CheckAndResolveIsolatedVertex(int vertex)
        {
            if (vertexMapping[vertex] != -1) //If the entry is not -1, it's not an isolated vertex, thus just return the vertex of the lr structure
            {
                return false;
            }
            else
            {
                //Create entry for isolated vertex in V-array and calculate a corner and store in C-array
                int index = lrTriangleMesh.MR + isolatedVIndex; //Calc entry (consecutive index)
                lrTriangleMesh.V[index] = ctTriangleMesh.Points[vertex]; //Add entry in V-Table of lr-structure
                vertexMapping[vertex] = index; //Adjust value in vertex map
                isolatedVIndex++; //+1, to calc the next index for a isolated vertex

                return true;
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
