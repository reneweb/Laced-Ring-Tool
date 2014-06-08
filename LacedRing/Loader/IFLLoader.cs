using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Structures;
using Structures.FBStructure;

namespace LacedRing.Loader
{
    public class IFLLoader
    {
        private bool paramsChanged = true;

        private bool reload = false;

        /// <summary>
        /// Determines whether or not to reload data each time the modul is asked for result. Set to true when the contens of the input files change, otherwise keep false.
        /// </summary>
        [Description("Determines whether or not to reload data each time the modul is asked for result. Set to true when the contens of the input files change, otherwise keep false.")]
        public bool Reload
        {
            get { return (reload); }
            set { reload = value; }
        }

        bool flipNormals = false;

        public bool FlipNormals
        {
            get { return flipNormals; }
            set { flipNormals = value; }
        }

        bool setDataSame = true;

        public bool SetDataSame
        {
            get { return setDataSame; }
            set { setDataSame = value; }
        }

        private string fileName = "mesh.obj";

        private IFLTriangleMesh output;

        private string trianglesCellName = "triangles";

        /// <summary>
        /// Sets the identifier for the triangle cells in the output TriangleMesh.
        /// </summary>
        [Description("Sets the identifier for the triangle cells in the output TriangleMesh")]
        public string TrianglesCellName
        {
            get { return (trianglesCellName); }
            set
            {
                if (value == "")
                {
                    throw new Exception("Cell identifier of triangle in the ObjLoader module cannot be an empty string.");
                }
                else
                {
                    this.trianglesCellName = value;
                    paramsChanged = true;
                }
            }
        }

        bool loadNormals = true;

        public bool LoadNormals
        {
            get { return loadNormals; }
            set { loadNormals = value; }
        }

        /// <summary>
        /// File to load data from. Should be in the .obj format. The property has no effect when there's a connection at the Filename port
        /// </summary>
        [Description("File to load data from. Should be in the .obj format. The property has no effect when there's a connection at the Filename port.")]
        public string FileName
        {
            get { return (fileName); }
            set
            {
                if (value != "")
                {
                    fileName = value;
                    paramsChanged = true;
                }
                else
                {
                    throw new Exception("FileName property cannot be set to an empty string!");
                }
            }
        }

        bool loadTextureCoordinates = false;

        public bool LoadTextureCoordinates
        {
            get { return loadTextureCoordinates; }
            set { loadTextureCoordinates = value; }
        }

        /// <summary>
        /// Loads the input file.
        /// I property values remain unchanged since the last run, then the file is not reloaded, unless the Realod property is set to true.
        /// </summary>
        public IFLTriangleMesh Execute(string fn)
        {

            // first pass determines the number of vertices and the number of triangles
            StreamReader sr;
            try
            {
                sr = new StreamReader(fn);
            }
            catch (FileNotFoundException fnfe)
            {
                throw new Exception(fnfe.Message);
            }

            int triangleCount = 0;
            int vertexCount = 0;
            int lineCount = 0;
            int textureCoordinateCount = 0;
            string line = sr.ReadLine();

            bool normalsFound = false;

            while (line != null)
            {
                line = line.Trim();

                if (line.StartsWith("v ")) vertexCount++;
                if (line.StartsWith("vt ")) textureCoordinateCount++;
                if (line.StartsWith("vn ")) normalsFound = true;
                if ((line.StartsWith("f ")) || (line.StartsWith("fo ")) || line.StartsWith("f\t"))
                {
                    triangleCount++;
                    if (line.Split(new char[] { ' ', '\t' }).Length == 5)
                        triangleCount++;
                }
                line = sr.ReadLine();
                lineCount++;
            }

            sr.Close();

            // second pass performs the actual parsing
            sr = new StreamReader(fn);
            Point3D[] vertices = new Point3D[vertexCount];
            Triangle[] triangles = new Triangle[triangleCount];

            int vi = 0;
            int ti = 0;
            int ni = 0;
            int tci = 0;
            Point3D point;
            Triangle triangle, textureTriangle;
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = ",";

            int linePos = 0;
            line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();

                //parsing of a vertex
                if (line.StartsWith("v "))
                {
                    string[] coords = line.Split(new char[] { ' ' }, 4, StringSplitOptions.RemoveEmptyEntries);

                    double c1 = double.Parse(coords[1], nfi);
                    double c2 = double.Parse(coords[2], nfi);
                    double c3 = double.Parse(coords[3], nfi);

                    point = new Point3D { X = c1, Y = c2, Z = c3 };
                    vertices[vi] = point;
                    vi++;
                }

                // parsing of a triangle
                if ((line.StartsWith("f ")) || (line.StartsWith("fo ")) || (line.StartsWith("f\t")))
                {
                    string[] indices = line.Split(new char[] { ' ', '\t' }, 5, StringSplitOptions.RemoveEmptyEntries);

                    int t1 = 0;
                    int t2 = 0;
                    int t3 = 0;

                    string[] parts = indices[1].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    int v1 = int.Parse(parts[0]) - 1;
                    if (parts.Length > 1)
                        t1 = int.Parse(parts[1]) - 1;

                    parts = indices[2].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    int v2 = int.Parse(parts[0]) - 1;
                    if (parts.Length > 1)
                        t2 = int.Parse(parts[1]) - 1;

                    parts = indices[3].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    int v3 = int.Parse(parts[0]) - 1;
                    if (parts.Length > 1)
                        t3 = int.Parse(parts[1]) - 1;

                    if (flipNormals)
                    {
                        triangle = new Triangle { Vertex1 = v1, Vertex2 = v3, Vertex3 = v2 };
                        textureTriangle = new Triangle { Vertex1 = t1, Vertex2 = t3, Vertex3 = t2 };
                    }
                    else
                    {
                        triangle = new Triangle { Vertex1 = v1, Vertex2 = v2, Vertex3 = v3 };
                        textureTriangle = new Triangle { Vertex1 = t1, Vertex2 = t2, Vertex3 = t3 };
                    }

                    if (indices.Length == 4)
                    {
                        triangles[ti] = triangle;
                        ti++;
                    }

                    if (indices.Length == 5)
                    {
                        parts = indices[4].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        v2 = int.Parse(parts[0]) - 1;
                        if (parts.Length > 1)
                            t2 = int.Parse(parts[1]) - 1;

                        if (flipNormals)
                        {
                            triangle = new Triangle { Vertex1 = v1, Vertex2 = v3, Vertex3 = v2 };
                            textureTriangle = new Triangle { Vertex1 = t1, Vertex2 = t2, Vertex3 = t3 };
                        }
                        else
                        {
                            triangle = new Triangle { Vertex1 = v1, Vertex2 = v3, Vertex3 = v2 };
                            textureTriangle = new Triangle { Vertex1 = t1, Vertex2 = t2, Vertex3 = t3 };
                        }
                        triangles[ti] = triangle;
                        ti++;
                    }
                }
                line = sr.ReadLine();
                linePos++;
            }

            // creating an output Mesh instance
            output = new IFLTriangleMesh();
            output.V = vertices;
            output.Triangles = triangles;
            return (output);
        }// Execute
    }
}
