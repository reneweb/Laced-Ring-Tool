using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures;
using Structures.LRStructure;

namespace LacedRing.Loader
{
    class LRLoader
    {
        public LRTriangleMesh Execute(string fn)
        {
            LRTriangleMesh lrTriangleMesh = new LRTriangleMesh();

            List<Point3D> v = new List<Point3D>();
            int mr = -1;
            List<int> lr = new List<int>();
            List<int> c = new List<int>();
            List<Triangle> t = new List<Triangle>();
            List<int> o = new List<int>();
            List<int> ts = new List<int>();
            List<int> os = new List<int>();

            bool isRingVertex = true;

            using (StreamReader sr = new StreamReader(fn))
            {
                String line = "";

                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (line.StartsWith("V"))
                    {
                        if (isRingVertex)
                        {
                            ParseRingVertex(line, v, lr, ts, os);
                        }
                        else
                        {
                            ParseIsolatedVertex(line, v, c);
                        }
                    }

                    if (line.StartsWith("-"))
                    {
                        mr = v.Count;
                        isRingVertex = false;
                    }

                    if (line.StartsWith("T"))
                    {
                        ParseTriangle(line, t);
                    }

                    if (line.StartsWith("O"))
                    {
                        ParseOpposites(line, o);
                    }
                }
            }

            lrTriangleMesh.V = v.ToArray();
            lrTriangleMesh.MR = mr == -1 ? v.Count : mr;
            lrTriangleMesh.LR = lr.ToArray();
            lrTriangleMesh.C = c.ToArray();
            lrTriangleMesh.T = t.ToArray();
            lrTriangleMesh.O = o.ToArray();
            lrTriangleMesh.TS = ts.ToArray();
            lrTriangleMesh.OS = os.ToArray();

            return lrTriangleMesh;
        }

        private void ParseRingVertex(String line, List<Point3D> v, List<int> lr, List<int> ts, List<int> os)
        {
            String[] lineSplit = line.Split(' ');
            v.Add(new Structures.Point3D { X = double.Parse(lineSplit[1]), Y = double.Parse(lineSplit[2]), Z = double.Parse(lineSplit[3]) });

            int nextEntryIndex;
            if (lineSplit[4] == "L")
            {
                lr.Add(int.Parse(lineSplit[5]));
                nextEntryIndex = 6;
            }
            else
            {
                int index = -(ts.Count) - 1;
                lr.Add(index);
                ts.Add(int.Parse(lineSplit[5]));
                os.Add(int.Parse(lineSplit[6]));
                os.Add(int.Parse(lineSplit[7]));

                nextEntryIndex = 8;
            }

            if (lineSplit[nextEntryIndex] == "R")
            {
                lr.Add(int.Parse(lineSplit[++nextEntryIndex]));
            }
            else
            {
                int index = -(ts.Count) - 1;
                lr.Add(index);
                ts.Add(int.Parse(lineSplit[++nextEntryIndex]));
                os.Add(int.Parse(lineSplit[++nextEntryIndex]));
                os.Add(int.Parse(lineSplit[++nextEntryIndex]));
            }
        }

        private void ParseIsolatedVertex(String line, List<Point3D> v, List<int> c)
        {
            String[] lineSplit = line.Split(' ');
            v.Add(new Structures.Point3D { X = double.Parse(lineSplit[1]), Y = double.Parse(lineSplit[2]), Z = double.Parse(lineSplit[3]) });
            c.Add(int.Parse(lineSplit[5]));
        }

        private void ParseTriangle(String line, List<Triangle> t)
        {
            String[] lineSplit = line.Split(' ');
            t.Add(new Triangle { Vertex1 = int.Parse(lineSplit[1]), Vertex2 = int.Parse(lineSplit[2]), Vertex3 = int.Parse(lineSplit[3]) });
        }

        private void ParseOpposites(string line, List<int> o)
        {
            String[] lineSplit = line.Split(' ');
            o.Add(int.Parse(lineSplit[1]));
        }
    }
}
