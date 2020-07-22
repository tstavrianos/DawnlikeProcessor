namespace DawnlikeProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using StbImageSharp;

    using StbImageWriteSharp;

    using ColorComponents = StbImageSharp.ColorComponents;

    public class Image
    {
        private readonly byte[] _data;

        public readonly int Width;

        public readonly int Height;

        public Image(string path)
        {
            using var stream = File.OpenRead(path);
            var res = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
            this._data = res.Data;
            this.Width = res.Width;
            this.Height = res.Height;
        }

        public Image(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this._data = new byte[width * height * 4];
        }

        public void Save(string path)
        {
            using Stream stream = File.OpenWrite(path);
            var writer = new ImageWriter();
            writer.WritePng(
                this._data,
                this.Width,
                this.Height,
                StbImageWriteSharp.ColorComponents.RedGreenBlueAlpha,
                stream);
        }

        public void Blit(Image source, int srcX, int srcY, int srcW, int srcH, int dstX, int dstY)
        {
            if (source == null) return;
            this.Blit(source._data, source.Width, source.Height, srcX, srcY, srcW, srcH, dstX, dstY);
        }

        private void Blit(
            IReadOnlyList<byte> raw,
            int rawWidth,
            int rawHeight,
            int srcX,
            int srcY,
            int srcW,
            int srcH,
            int dstX,
            int dstY)
        {
            if (raw == null) return;
            if (srcX == -1) srcX = 0;
            if (srcY == -1) srcY = 0;
            if (srcW == -1) srcW = rawWidth;
            if (srcH == -1) srcH = rawHeight;
            if (dstX == -1) dstX = 0;
            if (dstY == -1) dstY = 0;

            var minx = Math.Max(srcX, 0);
            var miny = Math.Max(srcY, 0);
            var maxx = Math.Min(srcX + srcW, rawWidth);
            var maxy = Math.Min(srcY + srcH, rawHeight);

            for (var y = miny; y < maxy; y++)
            {
                for (var x = minx; x < maxx; x++)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        this._data[4 * (dstX + x - minx + (dstY + y - miny) * this.Width) + i] =
                            raw[4 * (x + y * rawWidth) + i];
                    }
                }
            }
        }

        public bool KeepSlice(int x, int y, int width, int height)
        {
            byte r = 0;
            byte g = 0;
            byte b = 0;
            byte a = 0;

            var first = true;

            for (var j = y; j < y + height; j++)
            {
                for (var i = x; i < x + width; i++)
                {
                    var idx = 4 * (i + j * this.Width);
                    if (first)
                    {
                        first = false;
                        r = this._data[idx];
                        g = this._data[idx + 1];
                        b = this._data[idx + 2];
                        a = this._data[idx + 3];
                    }
                    else
                    {
                        if (r != this._data[idx] ||
                            g != this._data[idx + 1] ||
                            b != this._data[idx + 2] ||
                            a != this._data[idx + 3])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}