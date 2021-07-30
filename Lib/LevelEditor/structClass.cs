using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LevelEditorSpace
{
    public class LevelDataForm
    {
        public Dictionary<string, int>[] levelData{get; set;}
    }

    public class LayerStuct
    {
        public Texture2D texture;
        public Vector2 position;
        public Vector2 scrollVector;
        public LayerStuct(Texture2D texture, Vector2 position, Vector2 scrollVector )
        {
            this.texture = texture;
            this.position = position;
            this.scrollVector = scrollVector;
        }
        public Rectangle rectangle()
        {
            return new Rectangle((int)this.position.X, (int)this.position.X, this.texture.Width, this.texture.Height);
        }
    }

    public class LineStruct
    {
        public Vector2 position;
        public int length;
        public Color lineColor;
        public string lineType;
        public LineStruct(Vector2 position, int length, string lineType)
        {
            this.lineType = lineType;
            this.position = position;
            this.length = length;
            this.lineColor = Color.White;
        }

        public Rectangle rectangle()
        {
            if(this.lineType == "h")
                return new Rectangle((int)this.position.X, (int)this.position.Y, this.length, 1);
            else
                return new Rectangle((int)this.position.X, (int)this.position.Y, 1, this.length);
        }
    }

    public class MenuStuct
    {
        public Texture2D texture;
        public Vector2 position;
        public string type;

        public MenuStuct(Texture2D texture, Vector2 position, string type)
        {
            this.texture = texture;
            this.position = position;
            this.type = type;
        }

        public Rectangle rectangle()
        {
            return new Rectangle((int)this.position.X, (int)this.position.Y, this.texture.Width, this.texture.Height);
        }

        public void MenuContainerSwitch()
        {
            var showMenuContainer = PlatformShooter.LevelEditor.showMenuContainer;
            PlatformShooter.LevelEditor.showMenuContainer = showMenuContainer?false:true;
            if(PlatformShooter.LevelEditor.showMenuContainer)
                this.position += new Vector2(0, PlatformShooter.LevelEditor.menuContainer.Height);
            else
                this.position -= new Vector2(0, PlatformShooter.LevelEditor.menuContainer.Height);
        }

        public void TileSelect()
        {
            PlatformShooter.LevelEditor.inSelectTile = this.texture.Name;
            Console.WriteLine(PlatformShooter.LevelEditor.inSelectTile);
        }

        public void OnClickOnce(MouseState state)
        {
            if(this.rectangle().Contains(state.X/PlatformShooter.Game.horScaling, state.Y/PlatformShooter.Game.verScaling) && state.LeftButton == ButtonState.Pressed)
            {
                Console.WriteLine("menu click once");
                PlatformShooter.LevelEditor.onceLeftClick = true;
                switch(type)
                {
                    case "menuContainerSwitch":
                        this.MenuContainerSwitch();
                        break;
                    case "tileSelect":
                        this.TileSelect();
                        break;
                }
            }
        }
    }
}