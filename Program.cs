using CommandLine;
using System.Runtime.InteropServices;
using CUE4Parse.Encryption.Aes;
using CUE4Parse.FileProvider;
using CUE4Parse.UE4.Objects.Core.Misc;
using CUE4Parse.UE4.Assets.Exports;
using CUE4Parse.UE4.Assets.Exports.Texture;
using CUE4Parse_Conversion.Textures;
using CUE4Parse.FileProvider.Objects;
using CUE4Parse.UE4.Versions;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using SkiaSharp;

namespace FoxholeUmapTexturesExtractor
{

    public static class Program
    {
        private const string _mapDirectory = "War/Content/Maps/Master";
        private const string _umapExtension = ".umap";


        private class TextureExtractor
        {
            private const string _aesKey = "0x0000000000000000000000000000000000000000000000000000000000000000";
            private readonly DefaultFileProvider _provider;

            public TextureExtractor(string gameDirectory)
            {
                _provider = new DefaultFileProvider(gameDirectory, SearchOption.TopDirectoryOnly, true, new VersionContainer(EGame.GAME_UE4_24));
                _provider.Initialize();
                _provider.SubmitKey(new FGuid(), new FAesKey(_aesKey));
            }

            public void ProcessUmap(GameFile gameFile, FileInfo exportDirectory)
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var textures = _provider.LoadAllObjects(gameFile.Path).OfType<UTexture2D>();
                foreach (UObject texture in textures)
                {
                    if (texture.Outer == null)
                    {
                        Log.Logger.Warning("Ignoring {0} : no Outer found.", texture.Name);
                        continue;
                    }

                    var path = Path.Combine(exportDirectory.FullName, gameFile.NameWithoutExtension, texture.Outer.Name);
                    var output = new FileInfo(Path.Combine(path, Path.ChangeExtension(texture.Name, "png")));
                    var bitmap = GenerateBitmap((UTexture2D)texture);
                    try
                    {
                        Directory.CreateDirectory(path);
                        using var stream = output.OpenWrite();
                        bitmap.SaveTo(stream);
                    }
                    catch (IOException e)
                    {
                        Log.Logger.Error("Error writing {0} : {1}", texture.Name, e.Message);
                    }
                }
                watch.Stop();
                Log.Logger.Information("✅ Processed {0} in {1} seconds", gameFile.NameWithoutExtension, watch.Elapsed.TotalSeconds);
            }

            public IEnumerable<KeyValuePair<string, GameFile>> GetAllUmapFiles()
            {
                return _provider.Files.Where(f => f.Value.Path.StartsWith(_mapDirectory) && f.Value.Path.EndsWith(_umapExtension));
            }

            private static SKData GenerateBitmap(UTexture2D o)
            {
                var mip = o.GetFirstMip();
                TextureDecoder.DecodeTexture(mip, o.Format, false, ETexturePlatform.DesktopMobile, out var data, out _);
                var bitmap = new SKBitmap();
                var gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                var info = new SKImageInfo(checked((int)o.ImportedSize.X), checked((int)o.ImportedSize.Y), SKColorType.Bgra8888, SKAlphaType.Unpremul);
                bitmap.InstallPixels(info, gcHandle.AddrOfPinnedObject(), info.RowBytes, delegate { gcHandle.Free(); }, null);
                return bitmap.Encode(SKEncodedImageFormat.Png, 100);
            }

        }

        class Options
        {
            [Option('i', "input", Required = true, HelpText = "Game directory containing PAK file(s).")]
            public FileInfo GameDirectory { get; set; }

            [Option('o', "output", Required = true, HelpText = "Export directory to write the PNG tiles.")]
            public FileInfo ExportDirectory { get; set; }

            [Option('p', "parallel", Required = false, HelpText = "Max degree of parallellism.", Default = 8)]
            public int ParallelMax { get; set; }
        }

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console(theme: AnsiConsoleTheme.Literate).CreateLogger();

            CommandLine.Parser.Default.ParseArguments<Options>(args)
            .WithParsed<Options>(o => {
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var textureExtractor = new TextureExtractor(o.GameDirectory.FullName);
                Parallel.ForEach(textureExtractor.GetAllUmapFiles(), new ParallelOptions { MaxDegreeOfParallelism = o.ParallelMax }, file =>
                {
                    textureExtractor.ProcessUmap(file.Value, o.ExportDirectory);
                });
                watch.Stop();
                Log.Logger.Information("Exported tiles in {0} in {1} minutes", o.ExportDirectory, watch.Elapsed.TotalMinutes);
            });
        }
    }
}
