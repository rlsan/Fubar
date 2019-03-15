﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace FebEngine.UI
{
  public class UIButton : UIElement
  {
    //public delegate OnClick Func<>;

    public bool Hover
    {
      get
      {
        var m = Mouse.GetState();
        if (isVisible)
        {
          return bounds.Contains(m.Position.ToVector2() / 2);
        }
        else
        {
          return false;
        }
      }
    }

    public bool Pressed
    {
      get
      {
        var m = Mouse.GetState();

        return Hover && m.LeftButton == ButtonState.Pressed || Hover && m.RightButton == ButtonState.Pressed;
      }
    }

    public bool Released
    {
      get
      {
        var m = Mouse.GetState();

        return Hover && m.LeftButton == ButtonState.Released || Hover && m.RightButton == ButtonState.Released;
      }
    }

    public bool Click
    {
      get
      {
        return isVisible && Pressed && Released;
      }
    }

    public override void Draw(SpriteBatch sb)
    {
      Color color = Color.White;
      if (Pressed)
      {
        color = Color.Gray;
      }

      sb.Draw(canvas.ThemeTexture,
        new Rectangle(bounds.X + 1, bounds.Y + 1, bounds.Width - 1, bounds.Height - 1),
        new Rectangle(0, 0, 16, 16),
        color
          );

      base.Draw(sb);
    }
  }
}