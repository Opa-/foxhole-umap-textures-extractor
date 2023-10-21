# Foxhole Umap Textures Extractor

Extract textures of all umap files for Foxhole video game.

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
dotnet run --input "/Path/to/foxhole/pak" --output "/Path/to/export/tiles"
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
