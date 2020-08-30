using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Tibia71SpriteExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            const int spriteWidth = 32;
            const int spriteHeight = 32;
			Stream file;
			try
			{
				Console.Write("Trying to read Tibia.spr...");
				file = File.OpenRead("Tibia.spr");
			}
			catch (Exception)
			{
				Console.WriteLine("\nFailed to open and read Tibia.spr");
				return;
			}
			Console.WriteLine(" done.");
			using var br = new BinaryReader(file);

            try
            {
				Console.Write("Trying to create folder 'Sprites'...");
                Directory.CreateDirectory("Sprites");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nFailed to create 'Sprites' folder: {ex}");
                return;
            }
			Console.WriteLine(" done.");

            var version = br.ReadUInt32();
            var spriteCount = br.ReadUInt16();
            Console.WriteLine($"Sprite version: {version}");
            Console.WriteLine($"Found {spriteCount} sprites.");
			Console.Write("Extracting sprites...");
            var spritePositions = new uint[spriteCount];
            for (var i = 0; i < spriteCount; i++)
            {
                spritePositions[i] = br.ReadUInt32();
            }

            for (var i = 0; i < spriteCount; i++)
            {
                var pos = spritePositions[i];
                if (pos < 1) continue;
                var transparentR = br.ReadByte(); // in case we want to draw the transparent colors as well
                var transparentG = br.ReadByte();
                var transparentB = br.ReadByte();
                var spriteEnd = br.BaseStream.Position + br.ReadUInt16();
                using (var sprite = new Bitmap(spriteWidth, spriteHeight))
                {
                    var sprData = sprite.LockBits(new Rectangle(0, 0, spriteWidth, spriteHeight),
                        ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                    var cPixel = 0;
                    unsafe // need for speed
                    {
                        var bmpPtr = (byte*) sprData.Scan0;
                        while (br.BaseStream.Position < spriteEnd)
                        {
                            var transparentPixels = br.ReadUInt16();
                            var colorfulPixels = br.ReadUInt16();
                            cPixel += transparentPixels;
                            bmpPtr += transparentPixels * 4;
                            for (var p = 0; p < colorfulPixels; p++)
                            {
                                var (red, green, blue) = (br.ReadByte(), br.ReadByte(), br.ReadByte());
                                bmpPtr[0] = blue; // blue
                                bmpPtr[1] = green; // green
                                bmpPtr[2] = red; // red
                                bmpPtr[3] = 255; // alpha
                                bmpPtr += 4;
                                cPixel++;
                            }
                        }
                    }

                    sprite.UnlockBits(sprData);
                    sprite.Save($"Sprites/{i+1}.bmp");
                }
            }
			Console.WriteLine(" done.");
			Console.ReadLine();
        }

        static void PrintBytes(byte[] bytes)
        {
            Console.WriteLine(string.Join(" ", bytes.Select(b=>$"{b:X2}")));
        }
    }
}
