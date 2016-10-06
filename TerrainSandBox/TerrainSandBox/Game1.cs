using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TerrainSandBox
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        BasicEffect effect;

        MouseState currentMouseState;
        MouseState previousMouseState;

        KeyboardState currentKeyboardState;
        KeyboardState previousKeyboardState;

        VertexPositionNormalTexture[] vertices;

        VertexPositionColor[] overLayVertices;

        Rectangle backgroundRect;

        List<int> selectedVertices;

        VertexBuffer buffer;
        VertexBuffer overlayBuffer;
        IndexBuffer indexBuffer;
        int[] indices;
        int primitivestoDraw;
        PrimitiveType primitiveType;

        Texture2D texture;

        Texture2D background;
        Texture2D river;
        Texture2D heightMap;

        RenderTarget2D renderTarget;

        Camera camera;

        Plane groundPlane;

        RasterizerState solid;
        RasterizerState wireframe;

        bool landScapeEnabled = true;
        public bool wireFrameEnabled = false;

        int refreshRate = 100;
        int refreshCounter = 0;

        float rotation = 0;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            this.IsMouseVisible = true;

            solid = new RasterizerState();
            solid.FillMode = FillMode.Solid;

            wireframe = new RasterizerState();
            wireframe.FillMode = FillMode.WireFrame;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            camera = new Camera(this, new Vector3(0, 100, -100), Vector3.Zero, Vector3.Up);

            selectedVertices = new List<int>();

            groundPlane = new Plane(Vector3.Up, 0);

            effect = new BasicEffect(GraphicsDevice);

            primitivestoDraw = 29;

            primitiveType = PrimitiveType.LineStrip;

            backgroundRect = new Rectangle(0, 0, Window.ClientBounds.Width, Window.ClientBounds.Height);

            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            renderTarget = new RenderTarget2D(GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, true, GraphicsDevice.DisplayMode.Format, DepthFormat.Depth24);
           
            //setVertices(200,200);

            background = Content.Load<Texture2D>("Textures/heightMap");
            river = Content.Load<Texture2D>("Textures/River");
            heightMap = Content.Load<Texture2D>("Textures/heightmap");

            setVertices(heightMap);

            // Represents a list of 3D vertices to be streamed to the graphics device
            buffer = new VertexBuffer(GraphicsDevice,                         // this is the actual graphics device
                                            typeof(VertexPositionNormalTexture),            // defines the container holding data
                                            vertices.Length,                         // number of verticies
                                            BufferUsage.WriteOnly);                      // Behavior options; it is good practice for this to match the createOptions parameter in the GraphicsDevice constructor

            // Represents a list of 3D vertices to be streamed to the graphics device
            overlayBuffer = new VertexBuffer(GraphicsDevice,                         // this is the actual graphics device
                                            typeof(VertexPositionColor),            // defines the container holding data
                                            vertices.Length,                         // number of verticies
                                            BufferUsage.WriteOnly);                      // Behavior options; it is good practice for this to match the createOptions parameter in the GraphicsDevice constructor

            indexBuffer = new IndexBuffer(GraphicsDevice,
                                          typeof(int),
                                          indices.Length,
                                          BufferUsage.WriteOnly);

            indexBuffer.SetData(indices);



            // copies the data
            buffer.SetData(vertices);

            overlayBuffer.SetData(overLayVertices);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                SelectVertices();
            }

            if (currentKeyboardState.IsKeyUp(Keys.W) && previousKeyboardState.IsKeyDown(Keys.W))
            {
                if (landScapeEnabled)
                {
                    landScapeEnabled = false;
                    wireFrameEnabled = true;
                }
                else
                {
                    landScapeEnabled = true;
                    wireFrameEnabled = false;
                }
            }
            if (currentKeyboardState.IsKeyDown(Keys.Up))
            {
                for (int i = 0; i < selectedVertices.Count; i++)
                {
                    vertices[selectedVertices[i]].Position.Y += .1f;
                    overLayVertices[selectedVertices[i]].Position.Y += .1f;
                }
            }

            if (currentKeyboardState.IsKeyDown(Keys.Down))
            {
                for (int i = 0; i < selectedVertices.Count; i++)
                {
                    vertices[selectedVertices[i]].Position.Y -= .1f;
                    overLayVertices[selectedVertices[i]].Position.Y -= .1f;
                }
            }

            if (landScapeEnabled)
            {
                refreshCounter += gameTime.ElapsedGameTime.Milliseconds;
                if (refreshCounter >= refreshRate)
                {
                    refreshCounter -= refreshRate;
                    for (int i = 0; i < indices.Length; i += 3)
                    {
                        Vector3 v1 = (vertices[indices[i + 1]].Position - vertices[indices[i]].Position);
                        Vector3 v2 = (vertices[indices[i + 2]].Position - vertices[indices[i]].Position);
                        Vector3 normal = Vector3.Cross(v1, v2);

                        vertices[indices[i]].Normal = normal;
                        vertices[indices[i + 1]].Normal = normal;
                        vertices[indices[i + 2]].Normal = normal;
                    }
                }
            }

            GraphicsDevice.SetVertexBuffer(null);
            buffer.SetData(vertices);
            overlayBuffer.SetData(overLayVertices);

            if (currentKeyboardState.IsKeyDown(Keys.Right))
            {
                rotation += 0.02f;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left))
            {
                rotation -= 0.02f;
            }

            camera.position.Z = -(float)Math.Cos(rotation) * 100;

            camera.position.X = -(float)Math.Sin(rotation) * 100;

            camera.Update(gameTime);


            if (!landScapeEnabled)
            {
                wireFrameEnabled = true;
            }

            previousMouseState = currentMouseState;
            previousKeyboardState = currentKeyboardState;

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            GraphicsDevice.SetRenderTarget(renderTarget);

            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();

            spriteBatch.Draw(background, backgroundRect, Color.White);
            //spriteBatch.Draw(river, backgroundRect, Color.White);

            spriteBatch.End();

            GraphicsDevice.SetRenderTarget(null);

            texture = (Texture2D)renderTarget;

            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            effect.World = Matrix.Identity;                       // this is where I'd like to draw the object
            effect.View = camera.View;
            effect.Projection = camera.Projection;

            GraphicsDevice.Indices = indexBuffer;
            if (landScapeEnabled)
            {

                GraphicsDevice.RasterizerState = solid;

                effect.Texture = texture;
                effect.TextureEnabled = true;

                effect.VertexColorEnabled = false;
                effect.EnableDefaultLighting();

                GraphicsDevice.SetVertexBuffer(buffer);

                //  draw for each pass
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    // draw each side seperatly 
                    for (int i = 0; i < 4; i += 4)
                    {
                        GraphicsDevice.DrawIndexedPrimitives(
                                primitiveType,        // primitive type we intend to draw
                                0,
                                0,
                                vertices.Length,                            // reference to the array of data
                                0,                                  // offset into the array
                                primitivestoDraw);                                 // primitive count
                    }
                }

            }
            if (wireFrameEnabled)
            {
                effect.LightingEnabled = false;

                GraphicsDevice.RasterizerState = wireframe;

                effect.TextureEnabled = false;

                effect.VertexColorEnabled = true;

                GraphicsDevice.SetVertexBuffer(overlayBuffer);

                //  draw for each pass
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();
                    // draw each side seperatly 
                    for (int i = 0; i < 4; i += 4)
                    {
                        GraphicsDevice.DrawIndexedPrimitives(
                                primitiveType,        // primitive type we intend to draw
                                0,
                                0,
                                vertices.Length,                            // reference to the array of data
                                0,                                  // offset into the array
                                primitivestoDraw);                                 // primitive count
                    }
                }
            }

            base.Draw(gameTime);
        }

        /*
        protected void setWireFrameVertices(int squaresAcross, int squaresDown)
        {
            vertices = new VertexPositionColor[((squaresAcross * 2) + 2) + ((squaresDown - 1) * (squaresAcross + 1))];

            vertices[0] = new VertexPositionColor(new Vector3(0, 0, 1), Color.Blue);
            vertices[1] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);

            for (int i = 0; i < squaresDown; i++)
            {
                if (i > 0)
                {
                    vertices[((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1))] = new VertexPositionColor(new Vector3(0, 0, i + 1), Color.PapayaWhip);
                    int k = ((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1));
                }
                for (int j = 1; j <= squaresAcross; j++)
                {
                    if (i == 0)
                    {
                        vertices[(j * 2)] = new VertexPositionColor(new Vector3(j, 0, 1), Color.AliceBlue);
                        vertices[(j * 2) + 1] = new VertexPositionColor(new Vector3(j, 0, 0), Color.Chartreuse);
                    }
                    else if (i == 1)
                    {
                        vertices[((squaresAcross * 2) + 2) + j] = new VertexPositionColor(new Vector3(j, 0, 2), Color.Yellow);
                    }
                    else
                    {
                        int l = ((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1)) + j;
                        vertices[((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1)) + j] = new VertexPositionColor(new Vector3(j, 0, i + 1), Color.Purple);
                    }
                }
            }

           indices = new short[((squaresAcross * 4) + 2)*squaresDown];

           indices[0] = 1; 
           indices[1] = 0;

           for (int i = 0; i < squaresDown; i++)
           {
               if (i == 1)
               {
                   indices[((squaresAcross * 4) + 2)] = 0;
                   indices[((squaresAcross * 4) + 2) + 1] = (short)((squaresAcross * 2) + 2);
               }
               else if (i > 1)
               {
                   indices[((squaresAcross * 4) + 2) * i] = (short)(((squaresAcross * 2) + 2)+((squaresAcross+1)*(i-2)));
                   indices[(((squaresAcross * 4) + 2) * i) + 1] = (short)(((squaresAcross * 2) + 2) + ((squaresAcross + 1) * (i - 1)));
               }
               for (int j = 1; j <= squaresAcross; j++)
               {
                   if (i == 0)
                   {
                       indices[(j * 4) - 2] = (short)(j * 2);
                       indices[(j * 4) - 1] = (short)((j * 2) + 1);
                       indices[(j * 4)] = (short)((j * 2) - 1);
                       indices[(j * 4) + 1] = (short)(j * 2);
                   }
                   else if (i == 1)
                   {
                       indices[((squaresAcross * 4) + 2) + (j * 4) - 2] = (short)(((squaresAcross * 2) + 2)+j);
                       indices[((squaresAcross * 4) + 2) + (j * 4) - 1] = (short)((j * 2));
                       indices[((squaresAcross * 4) + 2) + (j * 4)] = (short)((j * 2) - 2);
                       indices[((squaresAcross * 4) + 2) + (j * 4) + 1] = (short)(((squaresAcross * 2) + 2) + j);
                   }
                   else
                   {
                       indices[(i * ((squaresAcross * 4) + 2)) + (j * 4) - 2] = (short)((((squaresAcross * 2) + 2) + ((i-1)*(squaresAcross + 1))) + j);
                       indices[(i * ((squaresAcross * 4) + 2)) + (j * 4) - 1] = (short)(((((squaresAcross * 2) + 2) + ((i - 2) * (squaresAcross + 1))) + (j-1))+1);
                       indices[(i * ((squaresAcross * 4) + 2)) + (j * 4)] = (short)((((squaresAcross * 2) + 2) + ((i - 2) * (squaresAcross + 1))) + (j-1));
                       indices[(i * ((squaresAcross * 4) + 2)) + (j * 4) + 1] = (short)((((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1))) + j);
                   }
               }
           }

           primitivestoDraw = 1+((4*squaresAcross)*squaresDown)+((squaresDown-1)*2);
           primitiveType = PrimitiveType.LineStrip;

           Vector3 offSet = new Vector3(squaresAcross/2, 0, squaresDown/2);
           for (int i = 0; i < vertices.Length; i++)
           {
               vertices[i].Position -= offSet;
           }
        }
        */

        protected void setVertices(int squaresAcross, int squaresDown)
        {
            vertices = new VertexPositionNormalTexture[((squaresAcross * 2) + 2)+((squaresDown-1)*(squaresAcross+1))];
            overLayVertices = new VertexPositionColor[((squaresAcross * 2) + 2)+((squaresDown-1)*(squaresAcross+1))];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, 0, 1),Vector3.Up, new Vector2(1,1-1/(float)squaresDown));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), Vector3.Up, Vector2.One);

            for (int i = 0; i < squaresDown; i++)
            {
                if (i > 0)
                {
                    vertices[((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1))] = new VertexPositionNormalTexture(new Vector3(0, 0, i + 1), Vector3.Up, new Vector2(1,1- ((i + 1)/(float)squaresDown)));
                    int k = ((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1));
                }
                for (int j = 1; j <= squaresAcross; j++)
                {
                    if (i == 0)
                    {
                        vertices[(j * 2)] = new VertexPositionNormalTexture(new Vector3(j, 0, 1), Vector3.Up, new Vector2(1-j / (float)squaresAcross,1- 1 / (float)squaresDown));
                        vertices[(j * 2) + 1] = new VertexPositionNormalTexture(new Vector3(j, 0, 0), Vector3.Up, new Vector2(1-j / (float)squaresAcross, 1));
                    }
                    else if (i == 1)
                    {
                        vertices[((squaresAcross * 2) + 2) + j] = new VertexPositionNormalTexture(new Vector3(j, 0, 2), Vector3.Up, new Vector2(1-j / (float)squaresAcross,1- 2 / (float)squaresDown));
                    }
                    else
                    {
                        int l = ((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1)) + j;
                        vertices[((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1)) + j] = new VertexPositionNormalTexture(new Vector3(j, 0, i + 1), Vector3.Up, new Vector2(1-j / (float)squaresAcross,1- ((i + 1) / (float)squaresDown)));
                    }
                }
            }

            indices = new int[squaresDown*squaresAcross * 6];

            for (int i = 0; i < squaresDown; i++)
            {
                for (int j = 0; j < squaresAcross; j++)
                {
                    if (i == 0)
                    {
                        indices[(j * 6)] = (int)(j * 2);
                        indices[(j * 6) + 1] = (int)(j * 2 + 1);
                        indices[(j * 6) + 2] = (int)(j * 2 + 2);
                        indices[(j * 6) + 3] = (int)(j * 2 + 3);
                        indices[(j * 6) + 4] = (int)(j * 2 + 2);
                        indices[(j * 6) + 5] = (int)(j * 2 + 1);
                    }
                    else if (i == 1)
                    {
                        indices[((squaresAcross * 6)) + (j * 6)] = (int)((2 + (squaresAcross * 2)) + j);
                        indices[((squaresAcross * 6)) + (j * 6) + 1] = (int)(j * 2);
                        indices[((squaresAcross * 6)) + (j * 6) + 2] = (int)((2 + (squaresAcross * 2)) + j + 1);
                        indices[((squaresAcross * 6)) + (j * 6) + 3] = (int)(j * 2 + 2);
                        indices[((squaresAcross * 6)) + (j * 6) + 4] = (int)((2 + (squaresAcross * 2)) + j + 1);
                        indices[((squaresAcross * 6)) + (j * 6) + 5] = (int)(j * 2);
                    }
                    else
                    {
                        indices[(i*(squaresAcross * 6)) + (j * 6)] = (int)(((2 + (squaresAcross * 2))+((i-1)*(squaresAcross+1))) + j);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 1] = (int)(((2 + (squaresAcross * 2)) + ((i - 2) * (squaresAcross + 1))) + j);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 2] = (int)(((2 + (squaresAcross * 2)) + ((i - 1) * (squaresAcross + 1))) + j+1);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 3] = (int)(((2 + (squaresAcross * 2)) + ((i - 2) * (squaresAcross + 1))) + j + 1);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 4] = (int)(((2 + (squaresAcross * 2)) + ((i - 1) * (squaresAcross + 1))) + j + 1);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 5] = (int)(((2 + (squaresAcross * 2)) + ((i - 2) * (squaresAcross + 1))) + j);
                    }
                }
            }

            primitivestoDraw = squaresAcross*2*squaresDown;
            primitiveType = PrimitiveType.TriangleList;

            Vector3 offSet = new Vector3(squaresAcross/2,0,squaresDown/2);
            for(int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position -= offSet;
                overLayVertices[i].Position = vertices[i].Position;
                overLayVertices[i].Color = Color.White;
            }
        }

        protected void setVertices(Texture2D heightmap)
        {
            int squaresAcross = heightmap.Width;
            int squaresDown = heightmap.Height;

            Color[] mapColors = new Color[squaresAcross * squaresDown];
            heightmap.GetData(mapColors);

            float[,] data = new float[squaresAcross, squaresDown];

            for (int x = 0; x < squaresAcross; x++)
            {
                for (int y = 0; y < squaresDown; y++)
                {
                    data[x, y] = mapColors[x + y * squaresAcross].R/25;
                }
            }

            vertices = new VertexPositionNormalTexture[((squaresAcross * 2) + 2) + ((squaresDown - 1) * (squaresAcross + 1))];
            overLayVertices = new VertexPositionColor[((squaresAcross * 2) + 2) + ((squaresDown - 1) * (squaresAcross + 1))];

            vertices[0] = new VertexPositionNormalTexture(new Vector3(0, 0, 1), Vector3.Up, new Vector2(1, 1 - 1 / (float)squaresDown));
            vertices[1] = new VertexPositionNormalTexture(new Vector3(0, 0, 0), Vector3.Up, Vector2.One);

            for (int i = 0; i < squaresDown; i++)
            {
                if (i > 0)
                {
                    vertices[((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1))] = new VertexPositionNormalTexture(new Vector3(0, 0, i + 1), Vector3.Up, new Vector2(1, 1 - ((i + 1) / (float)squaresDown)));
                    int k = ((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1));
                }
                for (int j = 1; j <= squaresAcross; j++)
                {
                    if (i == 0)
                    {
                        vertices[(j * 2)] = new VertexPositionNormalTexture(new Vector3(j, 0, 1), Vector3.Up, new Vector2(1 - j / (float)squaresAcross, 1 - 1 / (float)squaresDown));
                        vertices[(j * 2) + 1] = new VertexPositionNormalTexture(new Vector3(j, 0, 0), Vector3.Up, new Vector2(1 - j / (float)squaresAcross, 1));
                    }
                    else if (i == 1)
                    {
                        vertices[((squaresAcross * 2) + 2) + j] = new VertexPositionNormalTexture(new Vector3(j, 0, 2), Vector3.Up, new Vector2(1 - j / (float)squaresAcross, 1 - 2 / (float)squaresDown));
                    }
                    else
                    {
                        int l = ((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1)) + j;
                        vertices[((squaresAcross * 2) + 2) + ((i - 1) * (squaresAcross + 1)) + j] = new VertexPositionNormalTexture(new Vector3(j, 0, i + 1), Vector3.Up, new Vector2(1 - j / (float)squaresAcross, 1 - ((i + 1) / (float)squaresDown)));
                    }
                }
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                int x = (int)(squaresAcross - vertices[i].Position.X);
                int y = (int)(squaresDown - vertices[i].Position.Z);
                if (vertices[i].Position.Z >= squaresDown && vertices[i].Position.X >= squaresAcross)
                {
                    vertices[i].Position.Y = data[0, 0];
                }
                else if (vertices[i].Position.Z >= squaresDown)
                {
                    vertices[i].Position.Y = data[(int)(squaresAcross - vertices[i].Position.X - 1), 0];
                }
                else if (vertices[i].Position.X >= squaresAcross)
                {
                    vertices[i].Position.Y = data[0, (int)(squaresDown - vertices[i].Position.Z - 1)];
                }
                else
                {
                    vertices[i].Position.Y = data[(int)(squaresAcross - vertices[i].Position.X-1), (int)(squaresDown - vertices[i].Position.Z-1)];
                }
            }

            indices = new int[squaresDown * squaresAcross * 6];

            for (int i = 0; i < squaresDown; i++)
            {
                for (int j = 0; j < squaresAcross; j++)
                {
                    if (i == 0)
                    {
                        indices[(j * 6)] = (int)(j * 2);
                        indices[(j * 6) + 1] = (int)(j * 2 + 1);
                        indices[(j * 6) + 2] = (int)(j * 2 + 2);
                        indices[(j * 6) + 3] = (int)(j * 2 + 3);
                        indices[(j * 6) + 4] = (int)(j * 2 + 2);
                        indices[(j * 6) + 5] = (int)(j * 2 + 1);
                    }
                    else if (i == 1)
                    {
                        indices[((squaresAcross * 6)) + (j * 6)] = (int)((2 + (squaresAcross * 2)) + j);
                        indices[((squaresAcross * 6)) + (j * 6) + 1] = (int)(j * 2);
                        indices[((squaresAcross * 6)) + (j * 6) + 2] = (int)((2 + (squaresAcross * 2)) + j + 1);
                        indices[((squaresAcross * 6)) + (j * 6) + 3] = (int)(j * 2 + 2);
                        indices[((squaresAcross * 6)) + (j * 6) + 4] = (int)((2 + (squaresAcross * 2)) + j + 1);
                        indices[((squaresAcross * 6)) + (j * 6) + 5] = (int)(j * 2);
                    }
                    else
                    {
                        indices[(i * (squaresAcross * 6)) + (j * 6)] = (int)(((2 + (squaresAcross * 2)) + ((i - 1) * (squaresAcross + 1))) + j);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 1] = (int)(((2 + (squaresAcross * 2)) + ((i - 2) * (squaresAcross + 1))) + j);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 2] = (int)(((2 + (squaresAcross * 2)) + ((i - 1) * (squaresAcross + 1))) + j + 1);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 3] = (int)(((2 + (squaresAcross * 2)) + ((i - 2) * (squaresAcross + 1))) + j + 1);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 4] = (int)(((2 + (squaresAcross * 2)) + ((i - 1) * (squaresAcross + 1))) + j + 1);
                        indices[(i * (squaresAcross * 6)) + (j * 6) + 5] = (int)(((2 + (squaresAcross * 2)) + ((i - 2) * (squaresAcross + 1))) + j);
                    }
                }
            }

            for (int i = 0; i < indices.Length; i += 3)
            {
                Vector3 v1 = (vertices[indices[i + 1]].Position - vertices[indices[i]].Position);
                Vector3 v2 = (vertices[indices[i + 2]].Position - vertices[indices[i]].Position);
                Vector3 normal = Vector3.Cross(v1, v2);

                vertices[indices[i]].Normal = normal;
                vertices[indices[i+1]].Normal = normal;
                vertices[indices[i+2]].Normal = normal;
            }

            primitivestoDraw = squaresAcross * 2 * squaresDown;
            primitiveType = PrimitiveType.TriangleList;

            Vector3 offSet = new Vector3(squaresAcross / 2, 0, squaresDown / 2);
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position -= offSet;
                overLayVertices[i].Position = vertices[i].Position;
                overLayVertices[i].Color = Color.White;
            }
        }

        protected void SelectVertices()
        {
            //clears the selected vertices list(which stores the indices of the vertices)
            selectedVertices = new List<int>();

            //the near point 2D is the mouse position on the screen that is projected on the near plane
            Vector3 nearPoint2D = new Vector3(currentMouseState.X,currentMouseState.Y,0);

            //the far point 2D is the mouse position on the screen that is projected on the far plane
            Vector3 farPoint2D = new Vector3(currentMouseState.X,currentMouseState.Y,1);

            //nearPoint is the mouse position on the near plane in 3D space
            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearPoint2D, camera.Projection, camera.View, Matrix.Identity);

            //farPoint is the mouse position on the far plane in 3D space
            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farPoint2D, camera.Projection, camera.View, Matrix.Identity);

            //this is the ray that is cast from the point that is clicked on the screen
            //the starting position is the near point and the ray points towards the far point
            Ray selectRay = new Ray(nearPoint, Vector3.Normalize(farPoint-nearPoint));

            //this is the distance from the ray's origin to where the ray intersects the ground plane(where y = 0)
            float? intersectPlane = selectRay.Intersects(groundPlane);

            //if the ray does not intersect the plane, the method exits
            if (intersectPlane == null || intersectPlane <= 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    overLayVertices[i].Color = Color.White;
                }
                return;
            }

            //the point of intersection on the plane
            Vector3 point = nearPoint + ((float)intersectPlane * selectRay.Direction);

            for(int i = 0; i < vertices.Length;i++)
            {
                //the vertex projected onto the plane
                Vector3 point2D = new Vector3(vertices[i].Position.X, 0, vertices[i].Position.Z);

                //if the vertex is within a certain distance from the intersection point, the vertex is selected
                if (Vector3.Distance(point2D, point) <= 1.5f)
                {
                    overLayVertices[i].Color = Color.Red;
                    //add the index to selected vertices
                    selectedVertices.Add(i);
                }
                else
                {
                    overLayVertices[i].Color = Color.White;
                }
            }


        }
    }
}
