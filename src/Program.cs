using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.CommandLineUtils;

namespace IllumiCare.Platform.CommandLineUtils
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var app = new CommandLineApplication(throwOnUnexpectedArg: true);

            var outputFile = app.Option("-o | --outputFile <zipFile>", "The file to output (required).", CommandOptionType.SingleValue);
            var inputPath = app.Option("-i | --inputPath <folder>", "The path to the application folder to be zipped (optional). If not supplied, the current folder is used.", CommandOptionType.SingleValue);

            app.HelpOption("-? | --help");

            app.OnExecute(() =>
            {
                if (outputFile.HasValue())
                {
                    var folder = inputPath.HasValue() ? Path.GetFullPath(inputPath.Value()) : Environment.CurrentDirectory;
                    var success = Execute(folder, outputFile.Value());
                    if (!success) return -1;
                }
                else
                {
                    app.ShowHelp();
                }

                return 0;
            });

            return app.Execute(args);
        }

        public static bool Execute(string inputPath, string outputFile)
        {
            try
            {
                Console.WriteLine("Zipping {0} into {1}...", inputPath, outputFile);

                if (!Directory.Exists(Path.GetDirectoryName(inputPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(inputPath));
                
                ZipFile.CreateFromDirectory(inputPath, outputFile, CompressionLevel.Optimal, false);                
            }
            catch (Exception e)
            {
                // We have 2 log calls because we want a nice error message but we also want to capture the callstack in the log.
                Console.WriteLine("An exception has occured while trying to zip '{0}' into '{1}'.", inputPath, outputFile);
                Console.WriteLine(e.ToString());
                return false;
            }

            return true;
        }
    }
}