using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace LevelEditorSpace
{
    class LevelDataForm
    {
        public int[,] tileMap;
    }

    class LayerStuct
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

    class LineStruct
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

    class MenuStuct
    {
        public Texture2D texture;
        public Vector2 position;
        public bool menuRelease;

        public MenuStuct(Texture2D texture, Vector2 position)
        {
            this.texture = texture;
            this.position = position;
            this.menuRelease = true;
        }

        public Rectangle rectangle()
        {
            return new Rectangle((int)this.position.X, (int)this.position.Y, this.texture.Width, this.texture.Height);
        }

        public bool OnClick(MouseState state)
        {
            if(state.LeftButton == ButtonState.Released)
                this.menuRelease = true;
            
            // on pressing
            if(!this.menuRelease && state.LeftButton == ButtonState.Pressed)
                return true;
            
            if(state.LeftButton == ButtonState.Pressed && this.rectangle().Contains(state.X/PlatformShooter.Game.horScaling, state.Y/PlatformShooter.Game.verScaling))
            {
                // Console.WriteLine("menu click");
                this.menuRelease = false;
                var showMenuContainer = PlatformShooter.LevelEditor.showMenuContainer;
                PlatformShooter.LevelEditor.showMenuContainer = showMenuContainer?false:true;
                if(PlatformShooter.LevelEditor.showMenuContainer)
                    this.position += new Vector2(0, PlatformShooter.LevelEditor.menuContainer.Height);
                else
                    this.position -= new Vector2(0, PlatformShooter.LevelEditor.menuContainer.Height);
                return true;
            }
            return false;
        }
    }

}