using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Structures.FBStructure
{
    public class IFLTriangleMesh : AbstractTriangleMesh
    {
        public Point3D[] V { get; set; }
        public Triangle[] Triangles { get; set; }
    }
}
