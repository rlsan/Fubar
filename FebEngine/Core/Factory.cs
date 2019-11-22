﻿using FebEngine.GUI;
using FebEngine.Tiles;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TexturePackerLoader;

namespace FebEngine
{
  public class Factory
  {
    private WorldManager World { get; set; }
    private GameState State { get; set; }
    private ContentManager Content { get; set; }
    private SpriteSheetLoader spriteSheetLoader { get; set; }

    public Factory(WorldManager world, GameState state, ContentManager content)
    {
      World = world;
      State = state;
      Content = content;

      spriteSheetLoader = new SpriteSheetLoader(Content, RenderManager.Instance.GraphicsDevice);
    }

    public Entity Entity(Entity entityToAdd, string name)
    {
      var entity = entityToAdd;
      entity.name = new StringBuilder(name);
      entity.world = World;

      World.entities.Add(entity as Entity, State);

      return entity;
    }

    public Sprite Sprite(string name, string path)
    {
      var entity = new Sprite(Content.Load<Texture2D>("missing"));
      entity.name = new StringBuilder(name);
      entity.world = World;

      entity.SetTexture(spriteSheetLoader.Load("sp1"));
      entity.Animations.Add("Base", path);

      World.entities.Add(entity as Entity, State);

      return entity;
    }

    public ParticleEmitter Emitter(string name, string path, int capacity = 1000, bool startEmitting = true, EmitterShape emitterShape = EmitterShape.Circle)
    {
      var entity = new ParticleEmitter(capacity, startEmitting, emitterShape);
      entity.name = new StringBuilder(name);
      //entity.SetTexture(spriteSheetLoader.Load(path));
      entity.world = World;

      World.entities.Add(entity as Entity, State);

      return entity;
    }

    public MapGroup MapGroup(string name)
    {
      var entity = new MapGroup();
      entity.name = new StringBuilder(name);
      entity.world = World;

      World.entities.Add(entity as Entity, State);

      return entity;
    }

    public Camera Camera(string name)
    {
      var entity = new Camera();
      entity.name = new StringBuilder(name);
      entity.world = World;

      World.entities.Add(entity as Entity, State);

      return entity;
    }

    public Timer Timer(string name)
    {
      var entity = new Timer();
      entity.name = new StringBuilder(name);
      entity.world = World;

      World.entities.Add(entity as Entity, State);

      return entity;
    }

    public GUICanvas Canvas(string name)
    {
      var entity = new GUICanvas(100, 100);
      entity.name = new StringBuilder(name);
      entity.world = World;

      World.entities.Add(entity as Entity, State);

      return entity;
    }
  }
}