using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Jypeli
{
    /// <summary>
    /// Tilemap which can load JSON data from maps created with Tiled (https://www.mapeditor.org/).
    /// Allows assigning a custom function for individual tile numbers, overriding the default tile creation function.
    /// </summary>
    public class TiledMap
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public delegate void TileMethod(Vector position, double width, double height, Image tileImage);
        protected Dictionary<int, TileMethod> overrides = new Dictionary<int, TileMethod>();
        private Dictionary<TiledTileset, Dictionary<int, Image>> tileImages = new Dictionary<TiledTileset, Dictionary<int, Image>>();

        public List<TiledTileset> tilesets;
        public TiledTileMap tilemap;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <summary>
        /// Loads the map data for creating the level.
        /// </summary>
        /// <param name="tilemapFile">The map file in JSON format</param>
        /// <param name="tilesetFiles">The tileset files in JSON format</param>
        public TiledMap(string tilemapFile, params string[] tilesetFiles)
        {
            // TODO: load tileset files automatically (relative paths defined in map files)

            tilesets = new List<TiledTileset>();
            tilemap = TileMapLoader(tilemapFile);
            foreach (string f in tilesetFiles)
            {
                tilesets.Add(TilesetLoader(f));
            }
        }

        /// <summary>
        /// Override the default tile creation function for a tile.
        /// </summary>
        /// <param name="tilenum">Tile ID (in Tiled) to assign a method for</param>
        /// <param name="tileMethod">Callable TileMethod in the form of <c>void TileMethod(Vector position, double width, double height, Image tileImage)</c></param>
        public void SetOverride(int tilenum, TileMethod tileMethod)
        {
            overrides[tilenum + 1] = tileMethod;
        }

        /// <summary>
        /// Creates the level.
        /// </summary>
        public void Execute()
        {
            double mapWidth = tilemap.Width * tilemap.TileWidth;
            double mapHeight = tilemap.Height * tilemap.TileHeight;

            Game.Instance.Level.Size = new Vector(mapWidth, mapHeight);
            //Game.Instance.Level.CreateBorders();


            for (int l = 0; l < tilemap.Layers.Count; l++)
            {
                JArray data = (JArray)tilemap.Layers[l]["data"];
                object props = tilemap.Layers[l].TryGetValue("properties", out props) ? props : null;

                int? _layer = null;

                _layer = l - 2; // Bottom layer in Tiled corresponds to in-game layer -2

                // TODO: write check for more than 7 total layers

                int i = 0;
                for (int row = 0; row < tilemap.Height; row++)
                {
                    for (int col = 0; col < tilemap.Width; col++)
                    {
                        double _x = -mapWidth / 2 + col * tilemap.TileWidth + tilemap.TileWidth / 2;
                        double _y = mapHeight / 2 + -row * tilemap.TileHeight - tilemap.TileHeight / 2;

                        int tilenum = (int)data[i];
                        if (tilenum != 0)
                        {
                            TiledTileset _tileset = tilesets[0];
                            int j = 1;
                            int _num = tilenum;
                            while (_num > _tileset.TileCount)
                            {
                                _num -= _tileset.TileCount;
                                _tileset = tilesets[j];
                                j++;
                            }

                            if (overrides.ContainsKey(tilenum))
                            {
                                overrides[tilenum].Invoke(new Vector(_x, _y), _tileset.TileWidth, _tileset.TileHeight, GetTileImage(tilenum, _tileset));
                            }
                            else
                            {
                                PhysicsObject t = CreateTile(new Vector(_x, _y), _tileset.TileWidth, _tileset.TileHeight, GetTileImage(tilenum, _tileset), (JArray)props);
                                Game.Instance.Add(t, (int)_layer);
                            }
                        }
                        i++;
                    }
                }
            }
        }

        PhysicsObject CreateTile(Vector pos, double width, double height, Image tileImage, JArray props)
        {
            PhysicsObject tile = PhysicsObject.CreateStaticObject(width, height);
            tile.Position = pos;
            tile.Color = Color.Transparent;
            tile.Image = tileImage;
            tile.Shape = Shape.Rectangle;
            tileImage.Scaling = ImageScaling.Nearest;

            // layer properties
            if (props != null)
                foreach (var prop in props)
                {
                    PropertyInfo propertyInfo = tile.GetType().GetProperty(prop["name"].ToString());

                    // testing if calling MakeOneWay works when there is a true bool with the same name
                    if (prop["name"].ToString() == "MakeOneWay" && (bool)prop["value"] && prop["type"].ToString() == "bool")
                    {
                        tile.MakeOneWay();
                    }

                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        try
                        {
                            propertyInfo.SetValue(tile, Convert.ChangeType(prop["value"], propertyInfo.PropertyType));
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine(e.Message);
                        }
                    }
                }

            return tile;
        }

        /// <summary>
        /// Gets a tile image from a tileset
        /// </summary>
        /// <param name="tilenum"></param>
        /// <param name="tileset"></param>
        /// <returns>Image of the specified tile</returns>
        public Image GetTileImage(int tilenum, TiledTileset tileset)
        {
            if (tilenum <= 0)
            {
                return null;
            }

            if (tileImages.TryGetValue(tileset, out Dictionary<int, Image> ts))
            {
                if (ts.TryGetValue(tilenum, out Image img))
                {
                    return img;
                }
                else
                {
                    return LoadTileImage(tilenum, tileset);
                }
            }
            else
            {
                tileImages[tileset] = new Dictionary<int, Image>();
                return LoadTileImage(tilenum, tileset);
            }
        }
        private Image LoadTileImage(int tilenum, TiledTileset tileset)
        {
            Image tiles = Game.LoadImage(tileset.Image);
            tilenum--;

            int col = tilenum % tileset.Columns + 1;
            int row = tilenum / tileset.Columns + 1;

            int left = (col - 1) * (tileset.TileWidth + tileset.Spacing);
            int top = (row - 1) * (tileset.TileHeight + tileset.Spacing);
            int right = left + tileset.TileWidth;
            int bottom = top + tileset.TileHeight;

            Image t = tiles.Area(left, top, right, bottom);
            tileImages[tileset][tilenum + 1] = t;

            return t;
        }

        /// <summary>
        /// Reads text from a file.
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns>File contents</returns>
        public string LoadText(string file)
        {
            string data = String.Empty;
            string path = Game.Device.IsPhone ? file : Game.Device.ContentPath + "\\" + file; // Tested with Windows 10 and Android 12

            using (StreamReader input = new StreamReader(Game.Device.StreamContent(path)))
            {
                string line;
                while ((line = input.ReadLine()) != null)
                {
                    data += line;
                }
            }
            return data;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        /// <summary>
        /// Tileset data structure. 
        /// Contains information how the tileset image should be split into individual tiles.  
        /// </summary>
        public struct TiledTileset
        {
            public int Columns;
            public string Image;
            public int ImageHeight;
            public int ImageWidth;
            public int Margin;
            public string Name;
            public int Spacing;
            public int TileCount;
            public string TiledVersion;
            public int TileHeight;
            public int TileWidth;
            public string Type;
            public string Version;
        }
        TiledTileset TilesetLoader(string file)
        {
            string json = LoadText(file);
            TiledTileset set = JsonConvert.DeserializeObject<TiledTileset>(json);
            return set;
        }

        /// <summary>
        /// Tilemap data structure. 
        /// Contains the actual map data.
        /// </summary>
        public struct TiledTileMap
        {
            public string BackgroundColor;
            public int CompressionLevel;
            public int Height;
            public bool Infinite;
            public List<Dictionary<string, object>> Layers;
            public int NextLayerID;
            public int NextObjectID;
            public string Orientation;
            public string RenderOrder;
            public string TiledVersion;
            public int TileHeight;
            public List<Dictionary<string, object>> Tilesets;
            public int TileWidth;
            public string Type;
            public string Version;
            public int Width;

        }

        TiledTileMap TileMapLoader(string file)
        {
            string json = LoadText(file);
            TiledTileMap map = JsonConvert.DeserializeObject<TiledTileMap>(json);
            return map;
        }
        /*
        // TODO
        public struct TileMapLayer
        {
            public List<int> Data;
            public int Height;
            public int ID;
            public string Name;
            public double Opacity;
            public string Type;
            public bool Visible;
            public int Width;
            public double X;
            public double Y;
        }*/
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}