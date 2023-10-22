# Foxhole Umap Textures Extractor

Extract textures of all umap files for Foxhole video game.

## Why this rather than UEViewer or FModel ?

Some of the maps in Foxhole are split in sub-regions called *Outer*. Sometimes, different *Outers* shares the same heightmap texture names while they are different files. UEViewer/FModel exports will override them as they are being exported. This tool take *Outers* into account by creating a sub-folder for each of them.

# Usage

```
FoxholeUmapExporter 1.0.0
Copyright (C) 2023 FoxholeUmapExporter

  -i, --input       Required. Game directory containing PAK file(s).
  -o, --output      Required. Export directory to write the PNG tiles.
  -p, --parallel    (Default: 8) Max degree of parallellism.
  --help            Display this help screen.
  --version         Display version information.
```

# Run without building

```
dotnet run --input "C:\Program Files (x86)\Steam\steamapps\common\Foxhole\War\Content\Paks" --output ".\export"
```

# Build

```
# Linux
$ dotnet publish -r linux-x64 --self-contained true
# Windows
$ dotnet publish -r win-x64 --self-contained true
# OSX
$ dotnet publish -r osx-x64 --self-contained true
```
