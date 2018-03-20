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

            app.Description = "Zipper is a tiny app that can compress and extract zip files.";

            Action<CommandLineApplication> compressConfig = (a) => {
                var inputDir = a.Option("-i | --input <directory>", "The input directory to be compressed. (optional) Defaults to current directory.", CommandOptionType.SingleValue);
                var outputFile = a.Option("-o | --output <zipFile>", "The zip file to output. (required) Must not target input directory.", CommandOptionType.SingleValue);
                a.HelpOption("-? | --help");
                a.OnExecute(() => Compress(inputDir, outputFile));
            };

            Action<CommandLineApplication> extractConfig = (a) => {
                var inputFile = a.Option("-i | --input <zipFile>", "The input file to be extracted. (required)", CommandOptionType.SingleValue);
                var outputDir = a.Option("-o | --output <directory>", "The directory to output. (optional) Defaults to current directory.", CommandOptionType.SingleValue);
                a.HelpOption("-? | --help");
                a.OnExecute(() => Extract(inputFile, outputDir));
            };

            app.Syntax = "zipper compress -o ../myfile.zip";
            app.Command("compress", compressConfig);
            app.Command("extract", extractConfig);
            
            if (args.Length == 0 || (args.Length == 1 && args[0] == "-?") || (args.Length == 1 && args[0] == "--help"))
            {
                Console.WriteLine();
                Console.WriteLine("Usage: zipper compress -o ../myFile.zip");
                Console.WriteLine("       zipper extract -i myFile.zip");
                Console.WriteLine();
                Console.WriteLine("Help:  zipper compress --help");
                Console.WriteLine("       zipper extract --help");
                return -1;
            }
            
            try
            {
                return app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        private static int Compress(CommandOption inputDir, CommandOption outputFile)
        {
            try
            {
                var inDir = inputDir.HasValue() ? Path.GetFullPath(inputDir.Value()) : Environment.CurrentDirectory;
                
                if (!outputFile.HasValue()) throw new ArgumentException("The <zipFile> is missing.");
                var outFile = Path.GetFullPath(outputFile.Value());
                Console.WriteLine($"Compressing '{inDir}' into '{outFile}'");

                if (!Directory.Exists(Path.GetDirectoryName(outFile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(outFile));
                
                ZipFile.CreateFromDirectory(inDir, outFile, CompressionLevel.Optimal, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            return 0;
        }

        private static int Extract(CommandOption inputFile, CommandOption outputDir)
        {
            try
            {
                if (!inputFile.HasValue()) throw new ArgumentException("The <zipFile> is missing.");
                var inFile = Path.GetFullPath(inputFile.Value());
                
                var outDir = outputDir.HasValue() ? Path.GetFullPath(outputDir.Value()) : Environment.CurrentDirectory;

                Console.WriteLine($"Extracting '{inFile}' to '{outDir}'");

                if (!Directory.Exists(outDir))
                    Directory.CreateDirectory(outDir);
                
                ZipFile.ExtractToDirectory(inFile, outDir, false);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            return 0;
        }
    }
}