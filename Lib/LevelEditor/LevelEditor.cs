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
        private const int LAYER_REPEAT = 3;
        private const int scrollSpeed = 2;
        private static readonly Rectangle tileSize = new Rectangle(0,0,32,32);
        public static bool showMenuContainer = false;
        public static Rectangle menuContainer = new Rectangle(0, 0, 800, 100);
        public static string inSelectTile = null;
        public static bool onceLeftClick = false;
        private  List<MenuStuct> _tilesList;
        private int _levelNumber;
        private ContentManager _content;
        private List<LayerStuct> _layers;
        private List<LineStruct> _LineObjs;
        private LevelDataForm _levelData;
        private Texture2D _pencil;
        private List<MenuStuct> _menuList;
        private List<Keys> _onceKey;
        private bool showGrid;
        private int _rowsInLevel;
        private int _colInLevel;
        private Vector2 _globleScroll;
        
        public LevelEditor(IServiceProvider serviceProvider)
        {
            this._content = new ContentManager(serviceProvider, "Content");
            this._layers = new List<LayerStuct>{};
            this._LineObjs = new List<LineStruct>{};
            this._menuList = new List<MenuStuct>{};
            this._onceKey = new List<Keys>{};
            this._tilesList = new List<MenuStuct>{};
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this._pencil = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            this._pencil.SetData(new []{Color.White});
        }

        #region LoadContent
        private void LoadJsonLevelData()
        {
            if(!Directory.Exists("Content/level"))
            {
                Directory.CreateDirectory("Content/level");
            }
            StreamReader levelJsonStream = new StreamReader($"Content/Level/{this._levelNumber}.json");
            string JsonString = "";
            var ln = levelJsonStream.ReadLine();
            while(ln != null)
            {
                JsonString += ln;
                ln = levelJsonStream.ReadLine();
            }
            levelJsonStream.Close();
            if(JsonString != "")
                this._levelData = JsonSerializer.Deserialize<LevelDataForm>(JsonString);
            else
                this._levelData = new LevelDataForm(){levelData=new Dictionary<string, int>[this._colInLevel * this._rowsInLevel]};
        }

        private void UnLoadJsonLevelData()
        {
            string jsonString = JsonSerializer.Serialize<LevelDataForm>(this._levelData);
            File.WriteAllText($"Content/Level/{this._levelNumber}.json", jsonString);
        }

        public Dictionary<string, int>[] LoadLevelData()
        {
            if(this._levelData.levelData[0] == null)
            {
                int count = 0;
                for(var row=0;row<this._rowsInLevel;row++)
                {
                    for(var col=0;col<this._colInLevel;col++)
                    {
                        Dictionary<string, int> d = new Dictionary<string, int>();
                        d.Add("x", col);
                        d.Add("y", row);
                        d.Add("n", -1);
                        this._levelData.levelData[count] = d;
                        count ++;
                    }
                }
            }
            return this._levelData.levelData;
        }

        private void LoadGrid(int lastLayerWidth)
        {
            int row = 0;
            int column = 0;
            // load Horizontal line
            for(int rowy=tileSize.Height;rowy<= Game.originalScreenSize.Y; rowy += tileSize.Height)
            {
                row ++;
                LineStruct lineObj = new LineStruct(new Vector2(0, rowy), lastLayerWidth * LAYER_REPEAT, "h");
                this._LineObjs.Add(lineObj);
            }
            // load Vertical line
            for (int columnx=tileSize.Width;columnx<=lastLayerWidth * LAYER_REPEAT; columnx += tileSize.Width)
            {
                column ++;
                LineStruct lineObj = new LineStruct(new Vector2(columnx, 0), (int)Game.originalScreenSize.X, "v");
                this._LineObjs.Add(lineObj);
            }
            this._rowsInLevel = row;
            this._colInLevel = column;
        }

        private void LoadTilesMenu()
        {
            string [] tilesPath = Directory.GetFiles("Content/Tiles");
            int _width = tileSize.Width + 10;
            int _hright = tileSize.Height + 10;
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
                this._tilesList.Add(new MenuStuct(tt, new Vector2(_width * currentCol, _hright * currentRow), "tileSelect"));
                currentCol ++;
            }
        }

        public int LoadLayers()
        {
            int lastLayerWidth = 0;
            for (int i=0;i<LAYER_REPEAT;i++){
                for(int j=0;j<LAYER_NUMBER;j++)
                {
                    string backgoundLayerPath = $"BackGrounds/Layer1{j}_{this._levelNumber}";
                    var layer = this._content.Load<Texture2D>(backgoundLayerPath);
                    this._layers.Add(new LayerStuct(layer, new Vector2(i* layer.Width, 70*j), new Vector2(scrollSpeed + j, 0)));
                    if(j==LAYER_NUMBER-1)
                        lastLayerWidth = layer.Width;
                }
            }
            return lastLayerWidth;
        }

        public void LoadBasicMenu()
        {
            var gridMenu = this._content.Load<Texture2D>("Menu/gridMenu");
            this._menuList.Add(new MenuStuct(gridMenu, new Vector2(Game.originalScreenSize.X - gridMenu.Width - 10, 10), "menuContainerSwitch"));
        }

        public void LoadContent()
        { 
            this.LoadBasicMenu();
            this.LoadTilesMenu();
            this.LoadGrid(this.LoadLayers());
            this.LoadJsonLevelData();
            this.LoadLevelData();
        }
        # endregion

        public void HandleScreenScrool(KeyboardState keyboardState)
        {
            var borderLeft = this._layers[LAYER_NUMBER-1];
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

                    this._globleScroll += new Vector2(surfaceLayerScrollSpeed, 0);
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
                    this._globleScroll -= new Vector2(surfaceLayerScrollSpeed, 0);
                }
            }
        }

        private void HandleGridClick(MouseState state, string function)
        {
            ButtonState mouseButton = function == "left"? state.LeftButton : state.RightButton;
            if(mouseButton == ButtonState.Pressed)
            {
                if(inSelectTile == null && function == "left") return;
                double unScalingX = state.X / Game.horScaling;
                double unScalingY = state.Y / Game.verScaling;
                double x = (unScalingX - this._globleScroll.X) / (double)tileSize.Width;
                double y = (unScalingY - this._globleScroll.Y) / (double)tileSize.Height;
                int xPoint = (int)Math.Floor(x);
                int yPoint = (int)Math.Floor(y);
                int levelDataIndex = yPoint * this._colInLevel + xPoint;
                Dictionary<string, int> ld = this._levelData.levelData[levelDataIndex];
                ld["x"] = xPoint;
                ld["y"] = yPoint;
                ld["n"] = mouseButton == state.LeftButton ? int.Parse(inSelectTile.Split("/")[1]) : -1;
            }
        }

        public void HandleLeftMouseInput(MouseState state)
        {

            if(state.LeftButton == ButtonState.Released)
                onceLeftClick = false;
            foreach(var menu in this._menuList)
            {
                if(!onceLeftClick)
                    menu.OnClickOnce(state);
            }
            foreach(var tile in _tilesList)
            {
                if(!onceLeftClick && showMenuContainer)
                    tile.OnClickOnce(state);
            }
            if((!menuContainer.Contains(state.X / Game.horScaling, state.Y / Game.verScaling) || !showMenuContainer) && !onceLeftClick)
                this.HandleGridClick(state, "left");
        }

        public void HandleRightMouseInpug(MouseState state)
        {
            if((!menuContainer.Contains(state.X / Game.horScaling, state.Y / Game.verScaling) || !showMenuContainer))
                this.HandleGridClick(state, "right");
        }

        public void HandleOnceKey(KeyboardState keyboardState){
            // Key G
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
            // Key S
            if(keyboardState.IsKeyDown(Keys.S))
            {
                if(this._onceKey.Find(x=>x==Keys.S) == Keys.S)
                    return;
                this._onceKey.Add(Keys.S);
                this.UnLoadJsonLevelData();
            }
            if(keyboardState.IsKeyUp(Keys.S))
            {
                this._onceKey.Remove(Keys.S);
            }
        }

        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            this.HandleScreenScrool(keyboardState);
            this.HandleOnceKey(keyboardState);
            this.HandleLeftMouseInput(mouseState);
            this.HandleRightMouseInpug(mouseState);
        }

        public void DrawGrid(SpriteBatch sb)
        {
            float angleOfLine = (float)(2*Math.PI);
            foreach(var lineObj in this._LineObjs)
            {
                Rectangle originRc = lineObj.rectangle();
                Rectangle finalPosition = new Rectangle(originRc.X + (int)this._globleScroll.X, originRc.Y + (int)this._globleScroll.Y, originRc.Width, originRc.Height);
                sb.Draw(this._pencil, finalPosition, null, lineObj.lineColor, angleOfLine, Vector2.Zero, SpriteEffects.None, 0);
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
                sb.Draw(this._pencil, menuContainer, Color.Gray);
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
            
            foreach(var tile in this._levelData.levelData)
            {
                if(tile["n"] == -1) continue;
                Texture2D t2 = this._tilesList.Find(item=>item.texture.Name.Split("/")[1] == tile["n"].ToString()).texture;
                sb.Draw(t2, new Vector2(tile["x"]*tileSize.Width, tile["y"]*tileSize.Height) + this._globleScroll, Color.White);
            }

        }

        public void Dispose()
        {
            this._content.Unload();
        }
    }
}