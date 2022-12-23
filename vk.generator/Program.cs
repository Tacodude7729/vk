using System;
using System.IO;
using System.Collections.Generic;

namespace Vk.Generator
{
    public class Program
    {
        public static int Main(string[] args)
        {
            string outputPath;

            if (args.Length == 0)
            {
                outputPath = AppContext.BaseDirectory;
            }
            else
            {
                if (args.Length != 1)
                {
                    Console.Error.WriteLine("Too many arguments. Expected one for the output folder.");
                    return 1;
                }
                outputPath = args[0];
            }

            Configuration.CodeOutputPath = outputPath;

            if (File.Exists(outputPath))
            {
                Console.Error.WriteLine("The given path is a file, not a folder.");
                return 1;
            }
            else if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            VulkanSpecification vs;

            using (var fs = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "vk.xml")))
            {
                vs = VulkanSpecification.LoadFromXmlStream(fs);
            }
            using (var fs = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "video.xml")))
            {
                vs.Merge(VulkanSpecification.LoadFromXmlStream(fs));
            }

            TypeNameMappings tnm = new TypeNameMappings();
            foreach (var typedef in vs.Typedefs)
            {
                if (typedef.Requires != null)
                {
                    tnm.AddMapping(typedef.Requires, typedef.Name);
                }
                else
                {
                    tnm.AddMapping(typedef.Name, "uint");
                }
            }

            HashSet<string> definedBaseTypes = new HashSet<string>
                {
                    "VkBool32"
                };

            if (Configuration.MapBaseTypes)
            {
                foreach (var baseType in vs.BaseTypes)
                {
                    if (!definedBaseTypes.Contains(baseType.Key))
                    {
                        tnm.AddMapping(baseType.Key, baseType.Value);
                    }
                }
            }

            CodeGenerator.GenerateCodeFiles(vs, tnm, Configuration.CodeOutputPath);

            return 0;
        }
    }
}