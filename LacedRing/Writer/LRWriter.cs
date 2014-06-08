using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures.LRStructure;

namespace LacedRing.Writer
{
    public class LRWriter
    {
        private LRTriangleMesh mesh;

        public LRWriter(LRTriangleMesh mesh)
        {
            this.mesh = mesh;
        }

        public void Execute(string fn)
        {
            using (StreamWriter sw = new StreamWriter(fn))
            {
                sw.Write("#-----Laced Ring Data Structure-----");
                sw.WriteLine();

                String line = "";

                //Write vertices from 0-mr and their connectivity information
                for (int i = 0; i < mesh.MR; i++)
                {
                    line = String.Format("V {0} {1} {2}", mesh.V[i].X, mesh.V[i].Y, mesh.V[i].Z);
                    if (mesh.LR[2 * i] >= 0)
                    {
                        line += String.Format(" L {0}", mesh.LR[2 * i]);
                    }
                    else
                    {
                        int index = mesh.LR[2 * i] * (-1) - 1;
                        line += String.Format(" ET {0} {1} {2}", mesh.TS[index], mesh.OS[2 * index], mesh.OS[2 * index + 1]);
                    }

                    if (mesh.LR[2 * i +1] >= 0)
                    {
                        line += String.Format(" R {0}", mesh.LR[2 * i + 1]);
                    }
                    else
                    {
                        int index = mesh.LR[2 * i + 1] * (-1) - 1;
                        line += String.Format(" ET {0} {1} {2}", mesh.TS[index], mesh.OS[2 * index], mesh.OS[2 * index + 1]);
                    }

                    sw.WriteLine(line);
                }

                //Seperator between ring vertices and isolated vertices
                if (mesh.V.Length > mesh.MR) sw.WriteLine("-");

                //Write isolated vertices and their connectivity information
                for (int i = mesh.MR; i < mesh.V.Length; i++)
                {
                    line = String.Format("V {0} {1} {2}", mesh.V[i].X, mesh.V[i].Y, mesh.V[i].Z);
                    line += String.Format(" C {0}", mesh.C[i - mesh.MR]);
                    sw.WriteLine(line);
                }

                //Write T0-triangles
                for (int i = 0; i < mesh.T.Length; i++)
                {
                    if (i == 0) sw.WriteLine();
                    line = String.Format("T {0} {1} {2}", mesh.T[i].Vertex1, mesh.T[i].Vertex2, mesh.T[i].Vertex3);
                    sw.WriteLine(line);
                }

                //Write Opposites
                if (mesh.O.Length > 0) sw.WriteLine();
                foreach (var item in mesh.O)
                {
                    line = String.Format("O {0}", item);
                    sw.WriteLine(line);
                }
            }
        }
    }
}
