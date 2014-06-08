using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LacedRing.Writer
{
    public class MeshWriter
    {
        public static void WriteMesh(String filePath, AbstractTriangleMesh lrTriangleMesh)
        {
            if (lrTriangleMesh.GetType() == typeof(LRTriangleMesh))
            {
                LRWriter lrWriter = new LRWriter((LRTriangleMesh)lrTriangleMesh);
                lrWriter.Execute(filePath);
                return;
            }

            throw new NotImplementedException("There is no writer for this type of mesh");
        }
    }
}
