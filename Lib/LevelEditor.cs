using System;
using System.Text;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

class LevelDataForm
{
    public string[,] TileMap;
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

struct LineStruct
{
    public Vector2 position;
    public int length;
    public Color lineColor;
    public LineStruct(Vector2 position, int length)
    {
        this.position = position;
        this.length = length;
        this.lineColor = Color.White;
    }
}

namespace ActionGameExample
{
    public class LevelEditor: IDisposable
    {
        public static bool LevelEditorMode = true;
        private const int LAYER_NUMBER = 4;
        private const int LAYER_REPEAT = 4;
        private const int scrollSpeed = 2;
        private static readonly Rectangle tileSize = new Rectangle(0,0,32,32);
        private int levelNumber;
        private ContentManager _content;
        private List<LayerStuct> _layers;
        private List<LineStruct> _horizontalLineObjs;
        private int[,] Tiles;
        private LevelDataForm _levelData;
        private Texture2D _pencil;
        
        public LevelEditor(IServiceProvider serviceProvider)
        {
            this._content = new ContentManager(serviceProvider, "Content");
            this._layers = new List<LayerStuct>{};
            this._horizontalLineObjs = new List<LineStruct>{};
            this.levelNumber = 0;
        }

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            this._pencil = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            this._pencil.SetData(new []{Color.White});
        }

        private void HandleLevelTextData(FileStream levelStrem)
        {
            var fileLength = levelStrem.Length;
            byte[] buffer = new byte[1024];
            UTF8Encoding u8obj = new UTF8Encoding(true);
            string JsonString = "";
            while(levelStrem.Read(buffer, 0, buffer.Length)>0)
            {
                JsonString += u8obj.GetString(buffer);
            }
            if(fileLength!=0)
                this._levelData = JsonSerializer.Deserialize<LevelDataForm>(JsonString);
            else
                this._levelData = new LevelDataForm();
        }

        public void LoadContent()
        {
            string levelJsonPath = $"Content/Level/{this.levelNumber}.json";
            FileStream levelJsonStream;
            if(File.Exists(levelJsonPath))
                levelJsonStream = File.Open(levelJsonPath, FileMode.Open);
            else
                levelJsonStream = File.Create(levelJsonPath);
            
            using(levelJsonStream)
            {
                this.HandleLevelTextData(levelJsonStream);   
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
            
            // load Horizontal line
            for(int rowy=tileSize.Width;rowy<= Game.originalScreenSize.Y; rowy += tileSize.Width)
            {
                LineStruct lineObj = new LineStruct(new Vector2(0, rowy), lastLayerWidth * LAYER_REPEAT);
                this._horizontalLineObjs.Add(lineObj);
            }
        }

        public void HandleScreenScrool()
        {
            var borderLeft = this._layers[0];
            var borderRight = this._layers[_layers.Count-1];
            var surfaceLayerScrollSpeed = scrollSpeed + (LAYER_NUMBER-1);
            if(Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if(borderLeft.position.X<0){
                    for(int i=0;i<this._layers.Count;i++)
                    {
                        var copyLay = this._layers[i];
                        copyLay.position += copyLay.scrollVector;
                        this._layers[i] = copyLay;
                    }
                    for(int i=0;i<this._horizontalLineObjs.Count;i++)
                    {
                        var copyLineObj = this._horizontalLineObjs[i];
                        copyLineObj.position += new Vector2(surfaceLayerScrollSpeed, 0);
                        this._horizontalLineObjs[i] = copyLineObj;
                    }
                }
            }
            if(Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if(borderRight.rectangle().Right>Game.originalScreenSize.X)
                {
                    for(int i=0;i<this._layers.Count;i++)
                    {
                        var copyLay = this._layers[i];
                        copyLay.position -= copyLay.scrollVector;
                        this._layers[i] = copyLay;
                    }
                    for(int i=0;i<this._horizontalLineObjs.Count;i++)
                    {
                        var copyLineObj = this._horizontalLineObjs[i];
                        copyLineObj.position -= new Vector2(surfaceLayerScrollSpeed, 0);
                        this._horizontalLineObjs[i] = copyLineObj;
                    }
                }
            }
        }

        public void Update()
        {
            this.HandleScreenScrool();
        }

        public void DrawHorizontalLine(SpriteBatch sb)
        {
            float angleOfLine = (float)(2*Math.PI);
            foreach(var lineObj in this._horizontalLineObjs)
            {
                sb.Draw(this._pencil, new Rectangle((int)lineObj.position.X, (int)lineObj.position.Y, lineObj.length, 1), null, lineObj.lineColor, angleOfLine, Vector2.Zero, SpriteEffects.None, 0);
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach(var item in this._layers)
            {
                sb.Draw(item.texture, item.position, Color.White);
            }
            this.DrawHorizontalLine(sb);
        }

        public void Dispose()
        {
            this._content.Unload();
        }
    }
}