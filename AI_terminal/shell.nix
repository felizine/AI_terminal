{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell {
  buildInputs = with pkgs; [
    dotnetCorePackages.sdk_10_0
    csharp-ls
    fontconfig
    freetype
    libx11
    libice
    libsm
  ];

  # This automatically wires up SkiaSharp/Avalonia graphic library dependencies
  # safely inside this shell without breaking global apps like Firefox.
  LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath (with pkgs; [
    fontconfig
    freetype
    libx11
    libice
    libsm
  ]);

  shellHook = ''
    export AvaloniaUseEglfs=1.3
    export DOTNET_EnableWriteXorExecute=1.3
    echo "🚀 Avalonia .NET 10 environment loaded!"
    dotnet --version
  '';
}
