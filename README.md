[//]: <> (This readme is in the markdown format. Please preview in a markdown parser.)

# Tensai Bakabon (Sega Master System): RLE (De)Compressor

## About

This an RLE decompression and compression tool made for Tensai Bakabon for the Sega Master System, however, this compression scheme was used for other games as well.

## Usage

### Decompress
The arguments are as follows
1. `"Dump"` 
1. Rom Path
1. Address where the compressed data is at
1. Path to write the decompressed data to.
1. Interleave Factor
    * tilemap: `2`
    * graphics: `4`
    * level data: `12`

#### Example
```powershell
"Tensei Bakabon GFX.exe" "Dump" "%baseImage%" "0x0000C222" "%graphicsUncompressedOriginal%\font.bin" "4"
```

### Compress
1. `"Write"`
1. Path of file of data to compress
1. Path to write the compressed data to.
1. Interleave Factor
    * tilemap: `2`
    * graphics: `4`
    * level data: `12`

#### Examples
```powershell
"Tensei Bakabon GFX.exe" "Write" "%tileMapUncompressedNew%\title.bin" "%tileMapCompressedNew%\title.bin" "2"
"Tensei Bakabon GFX.exe" "Write" "%graphicsUncompressedNew%\title.bin" "%graphicsCompressedNew%\title.bin" "4"
"Tensei Bakabon GFX.exe" "Write" "%tileMapUncompressedNew%\underground.bin" "%tileMapCompressedNew%\underground.bin" "12"
```