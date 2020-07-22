namespace DawnlikeProcessor
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Newtonsoft.Json;

    public class Category
    {
        public string Name;
        public List<SubCategory> Files;
    }

    public class SubCategory
    {
        public string Name;
        public List<Tile> Tiles;
    }

    public class Tile
    {
        public int Index;
        public Point TopLeft;
    }

    public class AnimatedTile : Tile
    {
        public new List<Point> TopLeft;
    }

    internal static class Program
    {
        private static void Main(string[] args)
        {
            const int tileX = 16;
            const int tileY = 16;
            var res = new List<Category>();
            var nodes = new List<Packer.Node<(Image source, Point loc, int frame, Tile tile)>>();

            foreach (var categoryFull in Directory.EnumerateDirectories("dawnlike"))
            {
                var category = Path.GetFileName(categoryFull);
                if (category == "Examples") continue;
                var cat = new Category();
                cat.Name = category;
                cat.Files = new List<SubCategory>();
                foreach (var fileFull in Directory.EnumerateFiles(categoryFull, "*.png"))
                {
                    var file = Path.GetFileName(fileFull);
                    var name = Path.GetFileNameWithoutExtension(file);
                    var anim = false;
                    var frame = -1;
                    if (name.EndsWith('0') || name.EndsWith('1'))
                    {
                        anim = true;
                        if (name.EndsWith('0'))
                        {
                            frame = 0;
                        }
                        else if (name.EndsWith('1'))
                        {
                            frame = 1;
                        }
                        name = name.Substring(0, name.Length - 1);
                    }

                    SubCategory subCat;
                    var add = true;
                    if (anim && frame == 1)
                    {
                        subCat = cat.Files.First(x => x.Name == name);
                        add = false;
                    }
                    else
                    {
                        subCat = new SubCategory { Tiles = new List<Tile>(), Name = name };
                    }

                    var image = new Image(fileFull);
                    var tiles_x = image.Width / tileX;
                    var tiles_y = image.Height / tileY;
                    var i = 0;
                    for (var y = 0; y < tiles_y; y++)
                    {
                        for (var x = 0; x < tiles_x; x++)
                        {
                            if (!image.KeepSlice(x * tileX, y * tileY, tileX, tileY)) continue;
                            Tile tile;
                            if (anim && frame == 0)
                            {
                                tile = new AnimatedTile() { Index = i, TopLeft = new List<Point>() };
                            }
                            else if (anim && frame == 1)
                            {
                                tile = subCat.Tiles.First(x => x.Index == i);
                            }
                            else
                            {
                                tile = new Tile() { Index = i };
                            }
                            i++;
                            subCat.Tiles.Add(tile);
                            nodes.Add(new Packer.Node<(Image source, Point loc, int frame, Tile tile)>((image, new Point(x * tileX, y * tileY), frame, tile), tileX, tileY));
                        }
                    }
                    if (subCat.Tiles.Any() && add == true) cat.Files.Add(subCat);
                }
                if (cat.Files.Any()) res.Add(cat);
            }

            var packer = new Packer();
            packer.Fit(nodes);

            var maxX = nodes.Max(x => x.fit.X);
            var maxY = nodes.Max(x => x.fit.Y);

            var result = new Image(maxX + tileX, maxY + tileY);
            foreach (var node in nodes)
            {
                if (node.data.frame == -1)
                {
                    node.data.tile.TopLeft = node.fit;
                }
                else
                {
                    ((AnimatedTile)node.data.tile).TopLeft.Add(node.fit);
                }
                result.Blit(node.data.source, node.data.loc.X, node.data.loc.Y, tileX, tileY, node.fit.X, node.fit.Y);
            }
            result.Save("test.png");

            var json = JsonConvert.SerializeObject(res, Formatting.Indented);
            File.WriteAllText("test.json", json);
        }
    }
}
