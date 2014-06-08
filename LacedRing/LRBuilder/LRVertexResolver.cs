using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Structures;
using Structures.CTStructure;
using Structures.LRStructure;

namespace LacedRing.LRBuilder
{
    public class LRVertexResolver
    {
        private CTTriangleMesh ctTriangleMesh;
        private LRTriangleMesh lrTriangleMesh;
        private bool[] vMarked;
        private bool[] tMarked;

        public LRVertexResolver(CTTriangleMesh ctTriangleMesh, LRTriangleMesh lrTriangleMesh, bool[]vMarked, bool[]tMarked)
        {
            this.ctTriangleMesh = ctTriangleMesh;
            this.lrTriangleMesh = lrTriangleMesh;
            this.vMarked = vMarked;
            this.tMarked = tMarked;
        }

        public LRTriangleMesh ResolveVertices(out int[] vertexMapping)
        {
            //init vertex-map with -1
            vertexMapping = new int[ctTriangleMesh.Points.Length];
            for (int i = 0; i < vertexMapping.Length; i++)
            {
                vertexMapping[i] = -1;
            }

            lrTriangleMesh.V = new Point3D[ctTriangleMesh.Points.Length];

            //Get vertex which is on the ring (any)
            int startCtVertex = 0;
            while (true)
            {//Find first marked vertex -> startvertex
                if (vMarked[startCtVertex] == true)
                {
                    break;
                }
                startCtVertex++;
            }

            lrTriangleMesh.V[0] = ctTriangleMesh.Points[startCtVertex]; //Save start vertex
            vertexMapping[startCtVertex] = 0; //Create entry in map -> index =  pos of vertex in corner table, value = pos of vertex in lr

            int index = 1;
            for (int currCtVertex = startCtVertex; ; index++)
            {
                int[] faces = ctTriangleMesh.VF(currCtVertex); //Get faces incident with vertex
                int[][] facesSharingRingEdge = LRUtils.GetFacesSharingRingEdge(faces, tMarked);
                currCtVertex = GetNextRingVertex(currCtVertex, facesSharingRingEdge); //Get next ring-vertex left to the current one

                if (currCtVertex == startCtVertex)
                    break; //Back at the start of the ring
                
                lrTriangleMesh.V[index] = ctTriangleMesh.Points[currCtVertex]; //Save that vertex
                vertexMapping[currCtVertex] = index;
            }

            lrTriangleMesh.MR = index;

            return lrTriangleMesh;
        }

        private int GetNextRingVertex(int startVertex, int[][] facesSharingRingEdge)
        {
            int c = ctTriangleMesh.GetSpecificCorner(facesSharingRingEdge[0][0], startVertex);
            return ctTriangleMesh.V(ctTriangleMesh.N(c));
        }
    }
}
