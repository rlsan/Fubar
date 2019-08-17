﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FebEngine.Tiles
{
  public class TileSet
  {
    public string name = "Tileset";
    public Texture2D Texture { get; }

    private Tile[] rawTiles;
    public List<Tile> TileSwatches { get; }

    public int TileWidth { get; }

    public int TileHeight { get; }

    public readonly int rows;
    public readonly int columns;

    public int SwatchCount { get { return TileSwatches.Count; } }

    public TileSet(Texture2D texture, int tileWidth, int tileHeight)
    {
      Texture = texture;

      TileWidth = tileWidth;
      TileHeight = tileHeight;

      rows = Texture.Width / TileWidth;
      columns = Texture.Height / tileHeight;

      rawTiles = new Tile[rows * columns];
      TileSwatches = new List<Tile>();

      for (int i = 0; i < rawTiles.Length; i++)
      {
        AddTile(new Tile { id = i, frame = i });
      }
    }

    public Vector2 GetTilePositionFromIndex(int index)
    {
      int tileX = index % rows;
      int tileY = index / rows;

      return new Vector2(tileX, tileY);
    }

    public Tile GetTileFromIndex(int index)
    {
      if (index < SwatchCount)
      {
        return TileSwatches[index];
      }
      else
      {
        return TileSwatches[SwatchCount - 1];
      }
    }

    public Tile AddTile(Tile tile, string name = null)
    {
      var t = tile;
      t.id = SwatchCount;

      if (name != null)
      {
        t.Name = name;
      }
      else
      {
        t.Name = t.GetType().Name + SwatchCount;
      }

      TileSwatches.Add(t);

      return t;
    }
  }
}