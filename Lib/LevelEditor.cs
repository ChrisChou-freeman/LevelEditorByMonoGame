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

struct LayerStuct
{
    public Texture2D texture;
    public Vector2 position;
    public Rectangle rectangle;
    public Vector2 scrollVector;
    public LayerStuct(Texture2D texture, Vector2 position, Vector2 scrollVector )
    {
        this.texture = texture;
        this.position = position;
        this.rectangle = new Rectangle((int)this.position.X, (int)this.position.X, this.texture.Width, this.texture.Height);
        this.scrollVector = scrollVector;
    }
}

namespace ActionGameExample
{
    public class LevelEditor: IDisposable
    {
        public static bool LevelEditorMode = true;
        private const int LAYER_NUMBER = 4;
        private const int scrollSpeed = 1;
        private int levelNumber;
        private ContentManager _content;
        private List<LayerStuct> layers;
        private int[,] Tiles;
        private LevelDataForm _levelData;
        private const int LAYER_REPEAT = 4;
        
        public LevelEditor(IServiceProvider serviceProvider)
        {
            this._content = new ContentManager(serviceProvider, "Content");
            this.layers = new List<LayerStuct>{};
            this.levelNumber = 0;
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

            for (int i=0;i<LAYER_REPEAT;i++){
                for(int j=0;j<LAYER_NUMBER;j++)
                {
                    string backgoundLayerPath = $"BackGrounds/Layer1{j}_{this.levelNumber}";
                    var layer = this._content.Load<Texture2D>(backgoundLayerPath);
                    this.layers.Add(new LayerStuct(layer, new Vector2(i* layer.Width, 70*j), new Vector2(scrollSpeed + j, 0)));
                }
            }
        }

        public void HandleScrool()
        {
            var borderLeft = layers[0];
            var borderRight = layers[layers.Count-1];
            if(Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if(borderLeft.position.X<0){
                    for(int i=0;i<layers.Count;i++)
                    {
                        var copyLay = layers[i];
                        copyLay.position += copyLay.scrollVector;
                        layers[i] = copyLay;
                    }
                }
            }
            if(Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if(borderRight.position.X>0)
                {
                    for(int i=0;i<layers.Count;i++)
                    {
                        var copyLay = layers[i];
                        copyLay.position -= copyLay.scrollVector;
                        layers[i] = copyLay;
                    }
                }
            }
        }

        public void Update()
        {
            this.HandleScrool();
        }

        public void Draw(SpriteBatch sb)
        {
            foreach(var item in this.layers)
            {
                sb.Draw(item.texture, item.position, Color.White);
            }
        }

        public void Dispose()
        {
            this._content.Unload();
        }
    }
}