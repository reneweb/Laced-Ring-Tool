using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LacedRing.Loader
{
    public class MeshLoader
    {
        public static T LoadMesh<T>(String filePath) where T : AbstractTriangleMesh
        {
            if (typeof(T) == typeof(IFLTriangleMesh))
            {
                IFLLoader iflLoader = new IFLLoader();
                AbstractTriangleMesh atm = iflLoader.Execute(filePath);
                return (T)atm;
            }
            else if (typeof(T) == typeof(LRTriangleMesh))
            {
                LRLoader lrLoader = new LRLoader();
                AbstractTriangleMesh atm = lrLoader.Execute(filePath);
                return (T)atm;
            }

            throw new NotImplementedException("There is no loader for this type of mesh");
        }
    }
}
