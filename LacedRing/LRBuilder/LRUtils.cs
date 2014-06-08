using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LacedRing.LRBuilder
{
    public static class LRUtils
    {
        /// <summary>
        /// Looks if adjacent faces share a ring edge, by looking if one is marked and the other unmarked.
        /// </summary>
        /// <param name="faces">Faces incident with a vertex in anti-clockwise order.</param>
        /// <param name="tMarked">Array indicating marked and unmarked faces.</param>
        /// <returns>First array contains the marked and umnarked triangle for the next ring edge (left one). The first entry is the marked the second the unmarked one.
        /// The second array contains the marked and unmarked triangle for the previous ring edge (right one) in the same manner.</returns>
        public static int[][] GetFacesSharingRingEdge(int[] faces, bool[] tMarked)
        {
            int[][] facesSharingRingEdge = new int[2][];

            for (int i = 0; i < faces.Length; i++)
            {
                int t = faces[i];
                int nextT;

                if (i == faces.Length - 1) nextT = faces[0];
                else nextT = faces[i + 1];

                //If the current triangle isn't marked and the next one is, the shared edge is a ring edge, specifically the one to the left of the current vertex
                if (!tMarked[t] && tMarked[nextT])
                {
                    int[] facesSharingNextRingEdge = new int[2];
                    facesSharingNextRingEdge[0] = nextT;
                    facesSharingNextRingEdge[1] = t;

                    facesSharingRingEdge[0] = facesSharingNextRingEdge;

                    if (facesSharingRingEdge[1] != null) return facesSharingRingEdge;
                }

                //If the current triangle is marked and the next one isn't, the shared edge is a ring edge, specifically the one to the right of the current vertex
                if (tMarked[t] && !tMarked[nextT])
                {
                    int[] facesSharingPreviousRingEdge = new int[2];
                    facesSharingPreviousRingEdge[0] = t;
                    facesSharingPreviousRingEdge[1] = nextT;

                    facesSharingRingEdge[1] = facesSharingPreviousRingEdge;

                    if (facesSharingRingEdge[0] != null) return facesSharingRingEdge;
                }
            }

            return facesSharingRingEdge;
        }
    }
}
