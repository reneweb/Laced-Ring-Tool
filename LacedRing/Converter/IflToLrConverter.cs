using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LacedRing.Converter
{
    internal class IflToLrConverter : IConverter<LRTriangleMesh>
    {
        private IFLTriangleMesh iflTriangleMesh;

        internal IflToLrConverter(IFLTriangleMesh iflTriangleMesh)
        {
            this.iflTriangleMesh = iflTriangleMesh;
        }

        public LRTriangleMesh Convert()
        {
            LRBuilder.LRBuilder lrBuilder = new LRBuilder.LRBuilder(iflTriangleMesh.V, iflTriangleMesh.Triangles);
            return lrBuilder.Build();
        }
    }
}
