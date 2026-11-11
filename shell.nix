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
    libsecret
  ];

  LD_LIBRARY_PATH = pkgs.lib.makeLibraryPath (with pkgs; [
    fontconfig
    freetype
    libx11
    libice
    libsm
    libsecret
  ]);

  shellHook = ''
    echo "AI Terminal .NET 10 environment loaded"
    dotnet --version
  '';
}
