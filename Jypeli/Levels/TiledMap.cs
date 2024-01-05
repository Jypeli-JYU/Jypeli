using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

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
        private Dictionary<TiledTileset, Image> tilesetImages = new Dictionary<TiledTileset, Image>();

        public readonly List<TiledTileset> tilesets;
        public readonly TiledTileMap tilemap;

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        private JsonSerializerOptions serOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };


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
                TiledTileset t = TilesetLoader(f);
                tilesets.Add(t);
                tilesetImages.Add(t, Game.LoadImage(t.Image));
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
        /// Objects are placed from in-game layer -2 upwards.
        /// </summary>
        public void Execute()
        {
            double mapWidth = tilemap.Width * tilemap.TileWidth;
            double mapHeight = tilemap.Height * tilemap.TileHeight;

            Game.Instance.Level.Size = new Vector(mapWidth, mapHeight);

            for (int l = 0; l < tilemap.Layers.Count; l++)
            {
                List<int> data = tilemap.Layers[l].Data;
                List<Property> props = tilemap.Layers[l].Properties;

                int? _layer = l - 2; // Bottom layer in Tiled corresponds to in-game layer -2

                // TODO: better logic for layers
                if (_layer > 3)
                {
                    _layer = 3;
                }

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
                            // TODO: multiple tilesets
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
                                PhysicsObject t = CreateTile(new Vector(_x, _y), _tileset.TileWidth, _tileset.TileHeight, GetTileImage(tilenum, _tileset), props);
                                Game.Instance.Add(t, (int)_layer);
                            }
                        }
                        i++;
                    }
                }
            }
        }

        PhysicsObject CreateTile(Vector pos, double width, double height, Image tileImage, List<Property> props = null)
        {
            PhysicsObject tile = PhysicsObject.CreateStaticObject(width, height);
            tile.Position = pos;
            tile.Color = Color.Transparent;
            tile.Image = tileImage;
            tile.Shape = Shape.Rectangle;
            tileImage.Scaling = ImageScaling.Nearest;

            // Layer properties
            if (props != null)
                foreach (var prop in props)
                {
                    PropertyInfo propertyInfo = tile.GetType().GetProperty(prop.Name);

                    // testing if calling MakeOneWay works when there is a true bool with the same name
                    if (prop.Name == "MakeOneWay")
                    {
                        if (bool.Parse(prop.Value.ToString()) && prop.Type.ToString() == "bool")
                            tile.MakeOneWay();
                    }

                    if (propertyInfo != null && propertyInfo.CanWrite)
                    {
                        try
                        {
                            propertyInfo.SetValue(tile, Convert.ChangeType(prop.Value.GetRawText(), propertyInfo.PropertyType));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.Message);
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

            // check if the same image was used before this tile
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
            int col = (tilenum - 1) % tileset.Columns + 1;
            int row = (tilenum - 1) / tileset.Columns + 1;

            int left = (col - 1) * (tileset.TileWidth + tileset.Spacing);
            int top = (row - 1) * (tileset.TileHeight + tileset.Spacing);
            int right = left + tileset.TileWidth;
            int bottom = top + tileset.TileHeight;

            Image t = tilesetImages[tileset].Area(left, top, right, bottom);
            tileImages[tileset][tilenum] = t;

            return t;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        // TODO: make immutable

        /// <summary>
        /// Tileset data structure. 
        /// Contains information how the tileset image should be split into individual tiles.  
        /// </summary>
        public struct TiledTileset
        {
            public int Columns { get; set; }
            public string Image { get; set; }
            public int ImageHeight { get; set; }
            public int ImageWidth { get; set; }
            public int Margin { get; set; }
            public string Name { get; set; }
            public int Spacing { get; set; }
            public int TileCount { get; set; }
            public string TiledVersion { get; set; }
            public int TileHeight { get; set; }
            public int TileWidth { get; set; }
            public string Type { get; set; }
            public string Version { get; set; }
        }
        TiledTileset TilesetLoader(string file)
        {
            string json = Game.LoadText(file);
            TiledTileset set = JsonSerializer.Deserialize<TiledTileset>(json, serOptions);

            return set;
        }

        /// <summary>
        /// Tilemap data structure. 
        /// Contains information about the map.
        /// </summary>
        public struct TiledTileMap
        {
            public string BackgroundColor { get; set; }
            public int CompressionLevel { get; set; }
            public int Height { get; set; }
            public bool Infinite { get; set; }
            public List<TileMapLayer> Layers { get; set; }
            public int NextLayerID { get; set; }
            public int NextObjectID { get; set; }
            public string Orientation { get; set; }
            public string RenderOrder { get; set; }
            public string TiledVersion { get; set; }
            public int TileHeight { get; set; }
            public List<Dictionary<string, object>> Tilesets { get; set; }
            public int TileWidth { get; set; }
            public string Type { get; set; }
            public string Version { get; set; }
            public int Width { get; set; }
        }

        TiledTileMap TileMapLoader(string file)
        {
            string json = Game.LoadText(file);
            TiledTileMap map = JsonSerializer.Deserialize<TiledTileMap>(json, serOptions);

            return map;
        }

        /// <summary>
        /// Tile layer data structure. 
        /// </summary>
        public struct TileMapLayer
        {
            public List<int> Data { get; set; }
            public int Height { get; set; }
            public int ID { get; set; }
            public string Name { get; set; }
            public double Opacity { get; set; }
            public List<Property> Properties { get; set; }
            public string Type { get; set; }
            public bool Visible { get; set; }
            public int Width { get; set; }
            public double X { get; set; }
            public double Y { get; set; }
        }

        /// <summary>
        /// Tile layer property structure. 
        /// </summary>
        public struct Property
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public JsonElement Value { get; set; }
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}