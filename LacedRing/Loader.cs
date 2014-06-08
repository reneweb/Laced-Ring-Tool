using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures.FBStructure;

namespace Structures
{
    class Loader
    {
        static T LoadMesh<T>(String filePath, DataStructure dataStructure)
        {
            if (dataStructure == DataStructure.FBStructure)
            {
                
            }
            else if (dataStructure == DataStructure.LRStructure)
            {
                
            }

            throw new NotImplementedException();
        }
    }

    enum DataStructure
    {
        FBStructure,
        LRStructure,
    }
}
