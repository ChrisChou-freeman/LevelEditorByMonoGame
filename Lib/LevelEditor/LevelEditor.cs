using System;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using LevelEditorSpace;

namespace PlatformShooter
{
    public class LevelEditor: IDisposable
    {
        public static bool LevelEditorMode = true;
        private const int LAYER_NUMBER = 4;
        private const int LAYER_REPEAT = 4;
        private const int scrollSpeed = 2;
        private static readonly Rectangle tileSize = new Rectangle(0,0,32,32);
        public static bool showMenuContainer = false;
        public static Rectangle menuContainer = new Rectangle(0, 0, 800, 100);
        public static string inSelectTile = null;
        public static bool onceLeftClick = false;
        private  List<MenuStuct> _tilesList;
        private int levelNumber;
        private ContentManager _content;
        private List<LayerStuct> _layers;
        private List<LineStruct> _LineObjs;
        private LevelDataForm _levelData;
        private Texture2D _pencil;
        private List<MenuStuct> _menuList;
        private List<Keys> _onceKey;
        private bool showGrid;
        
        public LevelEditor(IServiceProvider serviceProvider)
        {
            this._content = new ContentManager(serviceProvider, "Content");
            this._layers = new List<LayerStuct>{};
            this._LineObjs = new List<LineStruct>{};
            this._menuList = new List<MenuStuct>{};
            this._onceKey = new List<Keys>{};
            this._tilesList = new List<MenuStuct>{};
            this.levelNumber = 0;
            this.showGrid = true;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this._pencil = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            this._pencil.SetData(new []{Color.White});
        }

        private void LoadLevelJsonData(int[] gridsValue)
        {
            int columns = gridsValue[0];
            int rows = gridsValue[1];
            string levelJsonPath = $"Content/Level/{this.levelNumber}.json";
            FileStream levelJsonStream;
            if(File.Exists(levelJsonPath))
                levelJsonStream = File.Open(levelJsonPath, FileMode.Open);
            else
                levelJsonStream = File.Create(levelJsonPath);
            var fileLength = levelJsonStream.Length;
            byte[] buffer = new byte[1024];
            UTF8Encoding u8obj = new UTF8Encoding(true);
            string JsonString = "";
            while(levelJsonStream.Read(buffer, 0, buffer.Length)>0)
            {
                JsonString += u8obj.GetString(buffer);
            }
            levelJsonStream.Close();
            if(fileLength!=0)
                this._levelData = JsonSerializer.Deserialize<LevelDataForm>(JsonString);
            else
                this._levelData = new LevelDataForm(){tileMap=new int[rows, columns]};
            // Console.WriteLine(this._levelData.tileMap.GetLength(0));
            // Console.WriteLine(this._levelData.tileMap.GetLength(1));
        }

        private int[] LoadGrid(int lastLayerWidth)
        {
            int row = 0;
            int column = 0;
            // load Horizontal line
            for(int rowy=tileSize.Width;rowy<= Game.originalScreenSize.Y; rowy += tileSize.Width)
            {
                row ++;
                LineStruct lineObj = new LineStruct(new Vector2(0, rowy), lastLayerWidth * LAYER_REPEAT, "h");
                this._LineObjs.Add(lineObj);
            }
            // load Vertical line
            for (int columnx=tileSize.Height;columnx<=Game.originalScreenSize.X; columnx += tileSize.Height)
            {
                column ++;
                LineStruct lineObj = new LineStruct(new Vector2(columnx, 0), (int)Game.originalScreenSize.X, "v");
                this._LineObjs.Add(lineObj);
            }
            return new int[]{column, row};
        }

        public void LoadContent()
        { 
            var gridMenu = this._content.Load<Texture2D>("Menu/gridMenu");
            this._menuList.Add(new MenuStuct(gridMenu, new Vector2(10, 10), "menuContainerSwitch"));
            string [] tilesPath = Directory.GetFiles("Content/Tiles");
            int _width = tileSize.Width + 10;
            double MaxCol = Math.Floor(Game.originalScreenSize.X/_width);
            double col =  Math.Floor(tilesPath.Length / MaxCol);
            col = (tilesPath.Length%MaxCol > 0) ? col +1 : col;

            int currentCol =0;
            int currentRow = 0;
            for(int i=0;i<tilesPath.Length;i++)
            {
                var tp = tilesPath[i];
                string pathInContent = tp.Split("Content")[1].Substring(1);
                if(pathInContent.Split(".")[1] == "DS_Store")
                    continue;
                var tt = this._content.Load<Texture2D>(pathInContent.Split(".")[0]); 
                if(currentCol>=MaxCol){
                    currentCol =0;
                    currentRow += 1;
                }
                this._tilesList.Add(new MenuStuct(tt, new Vector2((int)_width * currentCol, tileSize.Height * currentRow), "tileSelect"));
                currentCol ++;
            }
            int lastLayerWidth = 0;
            for (int i=0;i<LAYER_REPEAT;i++){
                for(int j=0;j<LAYER_NUMBER;j++)
                {
                    string backgoundLayerPath = $"BackGrounds/Layer1{j}_{this.levelNumber}";
                    var layer = this._content.Load<Texture2D>(backgoundLayerPath);
                    this._layers.Add(new LayerStuct(layer, new Vector2(i* layer.Width, 70*j), new Vector2(scrollSpeed + j, 0)));
                    if(j==LAYER_NUMBER-1)
                        lastLayerWidth = layer.Width;
                }
            }
            int[] gridValue =  this.LoadGrid(lastLayerWidth);
            this.LoadLevelJsonData(gridValue);
        }

        public void HandleScreenScrool(KeyboardState keyboardState)
        {
            var borderLeft = this._layers[0];
            var borderRight = this._layers[_layers.Count-1];
            var surfaceLayerScrollSpeed = scrollSpeed + (LAYER_NUMBER-1);
            if(keyboardState.IsKeyDown(Keys.A))
            {
                if(borderLeft.position.X<0){
                    for(int i=0;i<this._layers.Count;i++)
                    {
                        var copyLay = this._layers[i];
                        copyLay.position += copyLay.scrollVector;
                        this._layers[i] = copyLay;
                    }
                    for(int i=0;i<this._LineObjs.Count;i++)
                    {
                        var copyLineObj = this._LineObjs[i];
                        copyLineObj.position += new Vector2(surfaceLayerScrollSpeed, 0);
                        this._LineObjs[i] = copyLineObj;
                    }
                }
            }
            if(keyboardState.IsKeyDown(Keys.D))
            {
                if(borderRight.rectangle().Right>Game.originalScreenSize.X)
                {
                    for(int i=0;i<this._layers.Count;i++)
                    {
                        var copyLay = this._layers[i];
                        copyLay.position -= copyLay.scrollVector;
                        this._layers[i] = copyLay;
                    }
                    for(int i=0;i<this._LineObjs.Count;i++)
                    {
                        var copyLineObj = this._LineObjs[i];
                        copyLineObj.position -= new Vector2(surfaceLayerScrollSpeed, 0);
                        this._LineObjs[i] = copyLineObj;
                    }
                }
            }
        }

        public void HandleLeftMouseInput(MouseState state)
        {
            void _handleGridClick(MouseState state)
            {
                if(state.LeftButton == ButtonState.Pressed)
                {
                    double x = (double)state.X / Game.horScaling / (double)32;
                    double y = (double)state.Y / Game.verScaling/ (double)32;
                    Console.WriteLine($"{Math.Floor(x)},{Math.Floor(y)}");
                }
            }
            if(state.LeftButton == ButtonState.Released)
                onceLeftClick = false;
            foreach(var menu in this._menuList)
            {
                if(!onceLeftClick)
                    menu.OnClickOnce(state);
            }
            foreach(var tile in _tilesList)
            {
                if(!onceLeftClick)
                    tile.OnClickOnce(state);
            }
            if((!menuContainer.Contains(state.X / Game.horScaling, state.Y / Game.verScaling) || !showMenuContainer) && !onceLeftClick)
                _handleGridClick(state);
        }

        public void HandleOnceKey(KeyboardState keyboardState){
            if(keyboardState.IsKeyDown(Keys.G))
            {
                if(this._onceKey.Find(x=>x==Keys.G) == Keys.G)
                    return;
                this._onceKey.Add(Keys.G);
                this.showGrid = this.showGrid ? false : true;
            }
            if(keyboardState.IsKeyUp(Keys.G))
            {
                this._onceKey.Remove(Keys.G);
            }
        }

        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            this.HandleScreenScrool(keyboardState);
            this.HandleOnceKey(keyboardState);
            this.HandleLeftMouseInput(mouseState);
        }

        public void DrawGrid(SpriteBatch sb)
        {
            float angleOfLine = (float)(2*Math.PI);
            foreach(var lineObj in this._LineObjs)
            {
                sb.Draw(this._pencil, lineObj.rectangle(), null, lineObj.lineColor, angleOfLine, Vector2.Zero, SpriteEffects.None, 0);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach(var item in this._layers)
            {
                sb.Draw(item.texture, item.position, Color.White);
            }
            if(this.showGrid)
                this.DrawGrid(sb);
            
            foreach(var menu in this._menuList)
            {
                sb.Draw(menu.texture, menu.position, Color.White);
            }

            if(showMenuContainer)
            {
                sb.Draw(this._pencil, menuContainer, Color.White);
                foreach(var tile in _tilesList)
                {
                    sb.Draw(tile.texture, tile.position, Color.White);
                    if(tile.texture.Name == inSelectTile)
                    {
                        var r = tile.rectangle();
                        sb.Draw(this._pencil, new Rectangle(r.X, r.Y, r.Width, 2), Color.Red);
                        sb.Draw(this._pencil, new Rectangle(r.X, r.Y, 2, r.Height), Color.Red);
                        sb.Draw(this._pencil, new Rectangle(r.Left, r.Bottom, r.Width, 2), Color.Red);
                        sb.Draw(this._pencil, new Rectangle(r.Right, r.Top, 2, r.Height), Color.Red);
                    }
                }
            }

        }

        public void Dispose()
        {
            this._content.Unload();
        }
    }
}