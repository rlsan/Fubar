﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Fubar
{
  public class Physics
  {
    /// <summary>
    /// The downward force of gravity.
    /// </summary>
    public float gravity = 0.5f;

    /// <summary>
    /// How far from a body to check for collision.
    /// </summary>
    public int queryRange = 400;

    /// <summary>
    /// The size of the buffer between colliding objects.
    /// </summary>
    public int skinWidth = 1;

    private static WorldManager World { get; set; }
    private static QuadTree QuadTree { get; set; }

    public Physics(WorldManager world, int quadTreeDepth = 4)
    {
      World = world;
      QuadTree = new QuadTree(new Rectangle(-4000, -4000, 8000, 8000), quadTreeDepth);
    }

    private void RebuildQuadTree()
    {
      QuadTree.Reset();

      foreach (var sprite in World.GetEntities<Sprite>())
        QuadTree.Insert(sprite.Body);
    }

    public static bool Raycast(Vector2 origin, Vector2 direction, out RaycastHit hit, float distance = 1000, params string[] ignoreTags)
    {
      hit = new RaycastHit();
      var closestHit = new RaycastHit();
      var ray = new Ray(origin, Vector2.Normalize(direction));

      float shortestDistance = float.PositiveInfinity;
      bool hasHit = false;

      Rectangle rangeRect = new Rectangle((int)origin.X - (int)distance, (int)origin.Y - (int)distance, (int)distance * 2, (int)distance * 2);
      List<Body> Fbodies = QuadTree.Query(rangeRect);

      var rayLine = new Line(ray.Origin, ray.Origin + ray.Direction * distance);
      //Debug.DrawLine(rayLine);
      foreach (var body in Fbodies)
      {
        if (body.isTrigger) continue;
        if (body.Bounds.Contains(origin)) continue;
        if (ignoreTags.Contains(body.Parent.tag)) continue;

        if (Line.Intersect(rayLine, body.Bounds, out Vector2 intersectionPoint))
        {
          hasHit = true;
          float d = Vector2.Distance(intersectionPoint, ray.Origin);

          if (d < shortestDistance)
          {
            shortestDistance = d;
            closestHit = new RaycastHit(body, intersectionPoint, d);
          }
        }
      }

      foreach (var mapGroup in World.GetEntities<MapGroup>())
      {
        var layer = mapGroup.CurrentMap.GetLayer(1);

        int i = 0;
        foreach (var tile in layer.Tiles)
        {
          float x = (i % layer.Tilemap.Width) * layer.Tilemap.TileWidth;
          float y = (i / layer.Tilemap.Width) * layer.Tilemap.TileHeight;

          if (tile >= 0)
          {
            var rect = new Rectangle((int)x, (int)y, layer.Tilemap.TileWidth, layer.Tilemap.TileHeight);

            if (Line.Intersect(rayLine, rect, out Vector2 intersectionPoint))
            {
              hasHit = true;
              float d = Vector2.Distance(intersectionPoint, ray.Origin);

              if (d < shortestDistance)
              {
                shortestDistance = d;
                closestHit = new RaycastHit(null, intersectionPoint, d);
              }
            }
          }
          i++;
        }
      }

      if (hasHit)
      {
        hit = closestHit;
        return true;
      }

      return false;
    }

    private void Collide(Body bodyA, Body bodyB)
    {
      var e = new CollisionArgs();

      var AABB1 = bodyA.Bounds;
      var AABB2 = bodyB.Bounds;

      #region Trigger Collision

      if (bodyB.isTrigger)
      {
        if (AABB1.Intersects(AABB2))
        {
          e.Primary = bodyA.Parent;
          e.Other = bodyB.Parent;
          bodyA.Parent.OnTriggerStay(e);

          e.Primary = bodyB.Parent;
          e.Other = bodyA.Parent;
          bodyB.Parent.OnCollision(e);
        }

        return;
      }

      #endregion Trigger Collision

      #region X Collision

      // If the both AABBs are overlapping on the X axis...
      if (AABB1.Top < AABB2.Bottom && AABB1.Bottom > AABB2.Top)
      {
        // Set the maximum distance this object can move to its current X velocity.
        float maxDistanceX = bodyA.velocity.X;

        // If moving right...
        if (bodyA.velocity.X > 0)
        {
          // Does the object have collision on the left side?
          if (bodyB.collidesLeft)
          {
            // Limit the maximum distance to the distance between the object's right and the second object's left.
            maxDistanceX = Math.Min(Math.Abs(bodyA.velocity.X), Math.Abs(AABB2.Left - AABB1.Right) - 1);

            // If the objects are touching...
            if (maxDistanceX == 0)
            {
              e = new CollisionArgs();
              e.Primary = bodyA.Parent;
              e.Other = bodyB.Parent;

              bodyA.blocked.Right = true;
              bodyA.Parent.OnCollision(e);
            }
          }
        }

        // If moving left...
        else if (bodyA.velocity.X < 0)
        {
          // Does the object have collision on the right side?
          if (bodyB.collidesRight)
          {
            // Limit the maximum distance to the distance between the object's left and the second object's right.
            maxDistanceX = -Math.Min(Math.Abs(bodyA.velocity.X), Math.Abs(AABB2.Right - AABB1.Left) - 1);

            // If the objects are touching...
            if (maxDistanceX == 0)
            {
              e = new CollisionArgs();
              e.Primary = bodyA.Parent;
              e.Other = bodyB.Parent;
              bodyA.blocked.Left = true;

              bodyA.Parent.OnCollision(e);
            }
          }
        }

        // Update the body's velocity to the new, limited velocity.
        bodyA.velocity.X = maxDistanceX;
      }

      #endregion X Collision

      #region Y Collision

      // If the both AABBs are overlapping on the Y axis...
      if (AABB1.Left < AABB2.Right && AABB1.Right > AABB2.Left)
      {
        // Set the maximum distance this object can move to its current Y velocity.
        float maxDistanceY = bodyA.velocity.Y;

        // If moving down...
        if (bodyA.velocity.Y > 0)
        {
          // Does the object have collision on the top?
          if (bodyB.collidesUp)
          {
            // Limit the maximum distance to the distance between the object's bottom and the second object's top.
            maxDistanceY = Math.Min(Math.Abs(bodyA.velocity.Y), Math.Abs(AABB2.Top - AABB1.Bottom) - skinWidth);
          }
        }

        // If moving up...
        else if (bodyA.velocity.Y < 0)
        {
          // Does the object have collision on the bottom?
          if (bodyB.collidesDown)
          {
            // Limit the maximum distance to the distance between the object's top and the second object's bottom.
            maxDistanceY = -Math.Min(Math.Abs(bodyA.velocity.Y), Math.Abs(AABB2.Bottom - AABB1.Top) - skinWidth);
          }
        }

        // Update the body's velocity to the new, limited velocity.
        bodyA.velocity.Y = maxDistanceY;

        if (maxDistanceY == 0)
        {
          if (!bodyB.isTrigger && !bodyA.isTrigger)
          {
            e = new CollisionArgs();
            e.Primary = bodyA.Parent;
            e.Other = bodyB.Parent;

            bodyA.blocked.Down = true;
            bodyA.Parent.OnCollision(e);

            e = new CollisionArgs();
            e.Primary = bodyB.Parent;
            e.Other = bodyA.Parent;

            bodyB.blocked.Up = true;
            bodyB.Parent.OnCollision(e);
          }
        }
      }

      #endregion Y Collision
    }

    public void Update()
    {
      // Rebuild the quadtree with updated values.
      RebuildQuadTree();

      // Iterate through each sprite in the world.
      foreach (var sprite in World.GetEntities<Sprite>())
      {
        var body = sprite.Body;

        // Reset the body's collision values.
        body.blocked.Reset();

        // Skip if the sprite is dead or if its body is disabled.
        if (!sprite.isAlive) continue;
        if (!body.enabled) continue;

        // Apply gravity.
        if (body.hasGravity) body.AddVelocity(0, gravity);

        // Limit velocity for each axis.
        body.velocity = body.velocity.Limit(body.maxVelocity);

        // A non-dynamic body does not personally check for collision.
        if (!body.isDynamic) continue;

        // Set the area that the quadtree should query.
        Rectangle queryArea = new Rectangle(
          (int)sprite.Position.X - queryRange / 2,
          (int)sprite.Position.Y - queryRange / 2,
          queryRange, queryRange);

        // Check collision against each body detected by the quadtree.
        foreach (var queriedBody in QuadTree.Query(queryArea))
        {
          // Skip if it's checking collision on itself.
          if (queriedBody == body) continue;

          // Skip if there are no shared collision layers.
          bool hasMatch =
            body.collisionLayers.Any
            (x => queriedBody.collisionLayers.Any(y => y == x));
          if (!hasMatch) continue;

          // Adjust the body's velocity.
          Collide(body, queriedBody);
        }

        // Check collision against all tilemaps.
        foreach (var mapGroup in World.GetEntities<MapGroup>())
        {
          CollideTilemap(body, mapGroup.CurrentMap);
        }

        // Apply the velocity to the sprite.
        sprite.Position += body.velocity;
      }
    }

    private void CollideTilemap(Body body, Tilemap map)
    {
      if (map == null) return;

      var e = new CollisionArgs();
      e.Primary = body.Parent;
      e.Other = Sprite.Empty;

      var layer = map.GetLayer(1);
      int size = 64;
      int dist = 4;

      Rectangle bb = body.Bounds;

      int leftSide = Mathf.FloorToGrid(bb.Left, size) / size;
      int rightSide = Mathf.FloorToGrid(bb.Right, size) / size;

      int topSide = Mathf.FloorToGrid(bb.Top, size) / size;
      int bottomSide = Mathf.FloorToGrid(bb.Bottom, size) / size;

      float moveY = body.velocity.Y;

      if (body.velocity.Y > 0)
      {
        int hitPoint = 10000;
        for (int x = leftSide; x <= rightSide; x++)
        {
          for (int y = bottomSide; y <= bottomSide + dist; y++)
          {
            //Debug.DrawLine(new Vector2(x * size, y * size), new Vector2(x * size, bottomSide * size));
            var tile = layer.GetTile(x, y);

            if (tile > -1)
            {
              if (y < hitPoint) hitPoint = y;
              break;
            }
          }
        }

        //Debug.DrawPoint(leftSide * size, hitPoint * size);

        moveY = Math.Min(Math.Abs(body.velocity.Y), Math.Abs(hitPoint * size - body.Bounds.Bottom) - 1);

        if (moveY == 0)
        {
          body.blocked.Down = true;

          body.Parent.OnCollision(e);
        }

        //body.velocity.X = moveX;
      }
      else if (body.velocity.Y < 0)
      {
        int hitPoint = -10000;
        for (int x = leftSide; x <= rightSide; x++)
        {
          for (int y = topSide + 1; y > topSide - dist; y--)
          {
            //Debug.DrawLine(new Vector2(x * size, y * size), new Vector2(x * size, topSide * size));
            var tile = layer.GetTile(x, y);

            if (tile > -1)
            {
              if (y + 1 > hitPoint) hitPoint = y + 1;
              break;
            }
          }
        }

        //Debug.DrawPoint(leftSide * size, hitPoint * size);

        moveY = -Math.Min(Math.Abs(body.velocity.Y), Math.Abs(hitPoint * size - body.Bounds.Top) - 1);

        if (moveY == 0)
        {
          body.blocked.Up = true;

          body.Parent.OnCollision(e);
        }

        //body.velocity.X = moveX;
      }

      body.velocity.Y = moveY;

      float moveX = body.velocity.X;

      if (body.velocity.X > 0)
      {
        int hitPoint = 10000;
        for (int y = topSide; y <= bottomSide; y++)
        {
          for (int x = rightSide - 1; x <= rightSide + dist; x++)
          {
            //Debug.DrawLine(new Vector2((rightSide - 1) * size, y * size), new Vector2((rightSide + dist) * size, y * size));
            var tile = layer.GetTile(x, y);

            if (tile > -1)
            {
              if (x < hitPoint) hitPoint = x;
              break;
            }
          }
        }

        moveX = Math.Min(Math.Abs(body.velocity.X), Math.Abs(hitPoint * size - body.Bounds.Right) - 1);

        if (moveX == 0)
        {
          body.blocked.Right = true;

          body.Parent.OnCollision(e);
        }

        //body.velocity.X = moveX;
      }
      else if (body.velocity.X < 0)
      {
        int hitPoint = -10000;
        for (int y = topSide; y <= bottomSide; y++)
        {
          for (int x = leftSide + 1; x > leftSide - dist; x--)
          {
            //Debug.DrawLine(new Vector2((leftSide + 1) * size, y * size), new Vector2((leftSide - dist) * size, y * size));
            var tile = layer.GetTile(x, y);

            if (tile > -1)
            {
              if (x + 1 > hitPoint) hitPoint = x + 1;
              break;
            }
          }
        }

        moveX = -Math.Min(Math.Abs(body.velocity.X), Math.Abs(hitPoint * size - body.Bounds.Left) - 1);

        if (moveX == 0)
        {
          body.blocked.Left = true;

          body.Parent.OnCollision(e);
        }
      }

      body.velocity.X = moveX;
      return;
    }
  }
}