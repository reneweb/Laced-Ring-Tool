using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using LacedRing.Converter;
using LacedRing.Loader;
using LacedRing.Writer;
using Structures;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LRConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {    
            Console.WriteLine("----- LR Console Application -----");
            AbstractTriangleMesh mesh = LoadMesh();
            ExecOptions(mesh);
        }

        private static AbstractTriangleMesh LoadMesh()
        {
            while (true)
            {
                Console.WriteLine("Press (I) to load indexed face list, (L) to load laced ring or (E) to exit program");
                string line = Console.ReadLine();

                if (line.ToLower() == "i")
                {
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Path to mesh:");
                    line = Console.ReadLine();
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Start loading mesh");
                    AbstractTriangleMesh mesh = MeshLoader.LoadMesh<IFLTriangleMesh>(line);
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Mesh successfully loaded");
                    return mesh;

                }
                else if (line.ToLower() == "l")
                {
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Path to mesh:");
                    line = Console.ReadLine();
                    Console.WriteLine("Start loading mesh");
                    AbstractTriangleMesh mesh = MeshLoader.LoadMesh<LRTriangleMesh>(line);
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Mesh successfully loaded");
                    return mesh;
                }
                else if (line.ToLower() == "e")
                {
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Command not recognized");
                    Console.WriteLine("-------------------");
                }
            }
        }

        private static void ExecOptions(AbstractTriangleMesh mesh)
        {
            while (true)
            {
                if (mesh.GetType() == typeof(IFLTriangleMesh))
                {
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Press (C) to convert mesh to laced ring or (E) to exit program");
                    String line = Console.ReadLine();

                    if (line.ToLower() == "c")
                    {
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Start converting mesh");
                        IConverter<LRTriangleMesh> converter = Converter.GetSpecificConverter<IFLTriangleMesh, LRTriangleMesh>((IFLTriangleMesh)mesh);
                        mesh = converter.Convert();
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Mesh successfully converted ");
                    }
                    else if (line.ToLower() == "e")
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Command not recognized");
                    }
                }
                else if (mesh.GetType() == typeof(LRTriangleMesh))
                {
                    Console.WriteLine("-------------------");
                    Console.WriteLine("Press (S) to save mesh or (E) to exit program");
                    String line = Console.ReadLine();

                    if (line.ToLower() == "s")
                    {
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Path were mesh should be saved:");
                        String path = Console.ReadLine();
                        Console.WriteLine("Filename:");
                        String fileName = Console.ReadLine();
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Start writting mesh");
                        MeshWriter.WriteMesh(Path.Combine(path + fileName), mesh);
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Mesh successfully written");
                    }
                    else if (line.ToLower() == "e")
                    {
                        Environment.Exit(0);
                    }
                    else
                    {
                        Console.WriteLine("-------------------");
                        Console.WriteLine("Command not recognized");
                    }
                }
            }
        }
    }
}
