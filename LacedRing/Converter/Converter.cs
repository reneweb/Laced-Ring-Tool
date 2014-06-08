using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures;
using Structures.CTStructure;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LacedRing.Converter
{
    public class Converter
    {
        public static IConverter<M> GetSpecificConverter<T, M>(T inputMesh)
            where T : AbstractTriangleMesh
            where M : AbstractTriangleMesh
        {
            //Indexed face list to laced ring
            if (typeof(T) == typeof(IFLTriangleMesh) && typeof(M) == typeof(LRTriangleMesh))
            {
                AbstractTriangleMesh tempMesh = (AbstractTriangleMesh)inputMesh;
                IflToLrConverter iflToLr = new IflToLrConverter((IFLTriangleMesh)tempMesh);

                return (IConverter<M>)iflToLr;
            }
            //Indexed face list to corner table
            else if (typeof(T) == typeof(IFLTriangleMesh) && typeof(M) == typeof(CTTriangleMesh))
            {
                AbstractTriangleMesh tempMesh = (AbstractTriangleMesh)inputMesh;
                IflToCtConverter iflToCt = new IflToCtConverter((IFLTriangleMesh)tempMesh);

                return (IConverter<M>)iflToCt;
            }

            throw new ArgumentException("This conversion is not possible at this point");
        }
    }
}
