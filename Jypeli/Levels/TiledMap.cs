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
    /// TODO summary tähän
    /// </summary>
    public class TiledMap
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public delegate void TileMethod(Vector position, double width, double height, Image tileImage);
        protected Dictionary<int, TileMethod> overrides = new Dictionary<int, TileMethod>();

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
            tilesets = new List<TiledTileset>();
            tilemap = TileMapLoader(tilemapFile);
            foreach (string f in tilesetFiles)
            {
                tilesets.Add(TilesetLoader(f));
            }
        }

        /// <summary>
        /// Sets an method to call when looping through the tile matrix
        /// </summary>
        /// <param name="tilenum">Tile ID in Tiled to assign a method for</param>
        /// <param name="method">The method to call</param>
        public void SetOverride(int tilenum, TileMethod method)
        {
            overrides[tilenum + 1] = method;
        }

        /// <summary>
        /// Creates the level.
        /// </summary>
        public void Execute()
        {
            double mapWidth = tilemap.Width * tilemap.TileWidth;
            double mapHeight = tilemap.Height * tilemap.TileHeight;

            Game.Instance.Level.Size = new Vector(mapWidth, mapHeight);
            Game.Instance.Level.CreateBorders();


            for (int l = 0; l < tilemap.Layers.Count; l++)
            {
                JArray data = (JArray)tilemap.Layers[l]["data"];
                object props = tilemap.Layers[l].TryGetValue("properties", out props) ? props : null;

                int? _layer = null;

                /*if (props != null) // if layer is manually set in props
                {
                    JToken _token = ((JArray)props).SelectToken("$[?(@.name == 'Layer')]"); // find token
                    _layer = (int?)_token["value"]; // get value
                    ((JArray)props).Remove(_token); // remove original token to avoid exceptions later
                }

                _layer = _layer != null ? (int)_layer : -1;*/

                _layer = l - 3; // Bottom layer in tiled corresponds to in-game layer -3

                // TODO: write check for more than 7 total layers

                /*Debug.WriteLine(l.ToString());
                Debug.WriteLine(_layer.ToString());*/

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
                            while (tilenum > _tileset.TileCount)
                            {
                                tilenum -= _tileset.TileCount;
                                _tileset = tilesets[j];
                                j++;
                            }

                            if (overrides.ContainsKey(tilenum))
                            {
                                overrides[tilenum].Invoke(new Vector(_x, _y), _tileset.TileWidth, _tileset.TileHeight, TiledMap.GetTileImage(tilenum, _tileset));
                            }
                            else
                            {
                                PhysicsObject t = TiledMap.CreateTile(new Vector(_x, _y), _tileset.TileWidth, _tileset.TileHeight, TiledMap.GetTileImage(tilenum, _tileset), (JArray)props);
                                Game.Instance.Add(t, (int)_layer);
                            }
                        }
                        i++;
                    }
                }
            }
        }

        static PhysicsObject CreateTile(Vector pos, double width, double height, Image tileImage, JArray props)
        {
            PhysicsObject tile = PhysicsObject.CreateStaticObject(width, height);
            tile.Position = pos;
            tile.Color = Color.Transparent;

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

            tile.Image = tileImage;
            tile.Shape = Shape.Rectangle; // Shape.FromImage(tileImg); 
            tileImage.Scaling = ImageScaling.Nearest;

            return tile;
        }

        static Image GetTileImage(int tilenum, TiledTileset tileset)
        {
            if (tilenum <= 0)
            {
                return null;
            }

            Image tiles = Game.LoadImage(tileset.Image);
            tilenum--;

            int col = tilenum % tileset.Columns + 1;
            int row = tilenum / tileset.Columns + 1;

            int left = (col - 1) * (tileset.TileWidth + tileset.Spacing);
            int top = (row - 1) * (tileset.TileHeight + tileset.Spacing);
            int right = left + tileset.TileWidth;
            int bottom = top + tileset.TileHeight;

            Image t = tiles.Area(left, top, right, bottom);

            return t;
        }

        /// <summary>
        /// Tileset data structure. Contains information how the tileset image should be split into individual tiles.  
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
        static TiledTileset TilesetLoader(string file)
        {
            string json = File.ReadAllText(String.Format("Content\\{0}", file));
            TiledTileset set = JsonConvert.DeserializeObject<TiledTileset>(json);
            return set;
        }

        /// <summary>
        /// Tilemap data structure. Contains the actual map data.
        /// </summary>
        public struct TiledTileMap
        {
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

        static TiledTileMap TileMapLoader(string file)
        {
            string json = File.ReadAllText(String.Format("Content\\{0}", file));
            TiledTileMap map = JsonConvert.DeserializeObject<TiledTileMap>(json);
            return map;
        }
    }
}