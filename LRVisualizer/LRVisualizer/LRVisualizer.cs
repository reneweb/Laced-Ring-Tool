using System;
using System.Collections.Generic;
using System.Linq;
using LacedRing;
using LacedRing.Loader;
using LacedRing.Converter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Structures;
using Structures.FBStructure;
using Structures.LRStructure;

namespace LRVisualizer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class LRVisualizer : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont font;

        private String inputPath;

        public LRVisualizer(String inputPath)
        {
            this.inputPath = inputPath;

            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "LRVisualizerContent";
        }

        LRTriangleMesh lrTriangleMesh;

        BasicEffect effect;

        int currentCorner;

        Cube currentCube;

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            IFLTriangleMesh iflTriangleMesh = MeshLoader.LoadMesh<IFLTriangleMesh>(inputPath);
            IConverter<LRTriangleMesh> converter = Converter.GetSpecificConverter<IFLTriangleMesh, LRTriangleMesh>(iflTriangleMesh);
            lrTriangleMesh = converter.Convert();

            base.Initialize();

            BasicEffect e = new BasicEffect(GraphicsDevice);

            e.VertexColorEnabled = true;

            e.View = Matrix.CreateLookAt(new Vector3(0, 0, 5), new Vector3(0, 0, 0), Vector3.Up);

            float aspect = GraphicsDevice.Viewport.AspectRatio;

            e.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60f), aspect, 1, 100);
            effect = e;

            this.IsMouseVisible = true;
            mouseStatePrevious = Mouse.GetState();
            keyStateCurrent = Keyboard.GetState();

            this.Window.AllowUserResizing = true;

            RasterizerState s = new RasterizerState();
            s.CullMode = CullMode.None;
            GraphicsDevice.RasterizerState = s;

            int vertex = 0;
            currentCorner = lrTriangleMesh.VC(vertex);

            Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertex].X, (float)lrTriangleMesh.V[vertex].Y, (float)lrTriangleMesh.V[vertex].Z);

            currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>("fonts/Standard");
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

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        float currDist = 5f;

        MouseState mouseStateCurrent, mouseStatePrevious;

        KeyboardState keyStateCurrent, keyStatePrevious;

        private void checkInput()
        {
            if (mouseStateCurrent != null)
                mouseStatePrevious = mouseStateCurrent;

            mouseStateCurrent = Mouse.GetState();

            if (mouseStateCurrent.LeftButton == ButtonState.Pressed)
            {
                if (mouseStateCurrent.X < mouseStatePrevious.X)
                {
                    effect.World *= Matrix.CreateRotationY(MathHelper.ToRadians(1));
                }
                else if (mouseStateCurrent.X > mouseStatePrevious.X)
                {
                    //rotate right
                    effect.World *= Matrix.CreateRotationY(MathHelper.ToRadians(-1));
                }

                if (mouseStateCurrent.Y < mouseStatePrevious.Y)
                {
                    //rotate up
                    effect.World *= Matrix.CreateRotationX(MathHelper.ToRadians(1));
                }
                else if (mouseStateCurrent.Y > mouseStatePrevious.Y)
                {
                    //rotate down
                    effect.World *= Matrix.CreateRotationX(MathHelper.ToRadians(-1));
                }
            }

            if (mouseStateCurrent.ScrollWheelValue > mouseStatePrevious.ScrollWheelValue)
            {
                currDist += 0.2f;
                effect.View = Matrix.CreateLookAt(new Vector3(0, 0, currDist), new Vector3(0, 0, 0), Vector3.Up);
            }
            else if (mouseStateCurrent.ScrollWheelValue < mouseStatePrevious.ScrollWheelValue)
            {
                currDist -= 0.2f;
                effect.View = Matrix.CreateLookAt(new Vector3(0, 0, currDist), new Vector3(0, 0, 0), Vector3.Up);
            }

            if (keyStateCurrent != null)
                keyStatePrevious = keyStateCurrent;

            keyStateCurrent = Keyboard.GetState();

            if (keyStateCurrent.IsKeyDown(Keys.N) && !keyStatePrevious.IsKeyDown(Keys.N))
            {
                currentCorner = lrTriangleMesh.CN(currentCorner);
                int vertexIdx = lrTriangleMesh.CV(currentCorner);

                Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertexIdx].X, (float)lrTriangleMesh.V[vertexIdx].Y, (float)lrTriangleMesh.V[vertexIdx].Z);

                currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);
            }
            else if (keyStateCurrent.IsKeyDown(Keys.P) && !keyStatePrevious.IsKeyDown(Keys.P))
            {
                currentCorner = lrTriangleMesh.CP(currentCorner);
                int vertexIdx = lrTriangleMesh.CV(currentCorner);

                Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertexIdx].X, (float)lrTriangleMesh.V[vertexIdx].Y, (float)lrTriangleMesh.V[vertexIdx].Z);

                currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);
            }
            else if (keyStateCurrent.IsKeyDown(Keys.O) && !keyStatePrevious.IsKeyDown(Keys.O))
            {
                currentCorner = lrTriangleMesh.CO(currentCorner);
                int vertexIdx = lrTriangleMesh.CV(currentCorner);

                Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertexIdx].X, (float)lrTriangleMesh.V[vertexIdx].Y, (float)lrTriangleMesh.V[vertexIdx].Z);

                currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);
            }
            else if (keyStateCurrent.IsKeyDown(Keys.S) && !keyStatePrevious.IsKeyDown(Keys.S))
            {
                currentCorner = lrTriangleMesh.CS(currentCorner);
                int vertexIdx = lrTriangleMesh.CV(currentCorner);

                Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertexIdx].X, (float)lrTriangleMesh.V[vertexIdx].Y, (float)lrTriangleMesh.V[vertexIdx].Z);

                currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);
            }
            else if (keyStateCurrent.IsKeyDown(Keys.F) && !keyStatePrevious.IsKeyDown(Keys.F))
            {
                int v = lrTriangleMesh.CV(currentCorner);
                int vertexIdx = lrTriangleMesh.VN(v);
                currentCorner = lrTriangleMesh.VC(vertexIdx);

                Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertexIdx].X, (float)lrTriangleMesh.V[vertexIdx].Y, (float)lrTriangleMesh.V[vertexIdx].Z);

                currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);
            }
            else if (keyStateCurrent.IsKeyDown(Keys.B) && !keyStatePrevious.IsKeyDown(Keys.B))
            {
                int v = lrTriangleMesh.CV(currentCorner);
                int vertexIdx = lrTriangleMesh.VP(v);
                currentCorner = lrTriangleMesh.VC(vertexIdx);

                Vector3 cubePos = new Vector3((float)lrTriangleMesh.V[vertexIdx].X, (float)lrTriangleMesh.V[vertexIdx].Y, (float)lrTriangleMesh.V[vertexIdx].Z);

                currentCube = new Cube(new Vector3(0.01f, 0.01f, 0.01f), cubePos);
            }
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            
            GraphicsDevice.Clear(Color.DarkGray);
            c = new Color(15, 15, 15);

            checkInput();

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "N - Next corner, P - Previous corner, O - Opposite corner, S - Next corner around vertex", new Vector2(10, 10), Color.Black);
            spriteBatch.DrawString(font, "F - Next ring vertex, B - Previous ring vertex", new Vector2(10, 30), Color.Black);
            spriteBatch.End();

            // reset what spritebatch has changed
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;

            EffectPass pass = effect.CurrentTechnique.Passes[0];

            pass.Apply();
            

            foreach (Triangle t in lrTriangleMesh.T)
            {
                drawTriangle(t, lrTriangleMesh.V, GraphicsDevice, Color.Red);
            }

            int mr = lrTriangleMesh.MR;

            for (int i = 0; i < mr; i++)
            {
                Point3D[] p = new Point3D[3];

                //Left triangle
                p[0] = lrTriangleMesh.V[i];
                p[1] = lrTriangleMesh.V[lrTriangleMesh.VN(i)];
                p[2] = lrTriangleMesh.V[lrTriangleMesh.VL(i)];

                drawTriangle(p, GraphicsDevice, Color.Brown);

                drawLine(p, 1, 2, GraphicsDevice, Color.Black);
                drawLine(p, 2, 0, GraphicsDevice, Color.Black);

                //Right triangle
                p[2] = lrTriangleMesh.V[lrTriangleMesh.VR(i)];

                drawTriangle(p, GraphicsDevice, Color.Lavender);

                drawLine(p, GraphicsDevice);
                drawLine(p, 1, 2, GraphicsDevice, Color.Black);
                drawLine(p, 2, 0, GraphicsDevice, Color.Black);
            }

            currentCube.Draw(GraphicsDevice);

            base.Draw(gameTime);
        }

        private void drawTriangle(Triangle t, Point3D[] points, GraphicsDevice d)
        {
            drawTriangle(t, points, d, nextColor());
        }

        private void drawTriangle(Point3D[] p, GraphicsDevice d)
        {
            drawTriangle(p, d, nextColor());
        }

        private void drawTriangle(Triangle t, Point3D[] points, GraphicsDevice d, Color c)
        {
            VertexPositionColor[] tri = {new VertexPositionColor(Vector3Extension.CreateV3FromP3(points[t.Vertex1]), c),
                             new VertexPositionColor(Vector3Extension.CreateV3FromP3(points[t.Vertex2]), c),
                             new VertexPositionColor(Vector3Extension.CreateV3FromP3(points[t.Vertex3]), c)};

            d.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, tri, 0, 1);
        }

        private void drawTriangle(Point3D[] p, GraphicsDevice d, Color c)
        {
            VertexPositionColor[] tri = {new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[0]), c),
                             new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[1]), c),
                             new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[2]), c)};

            d.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, tri, 0, 1);
        }

        private void drawLine(Point3D[] p, int s, int e, GraphicsDevice d, Color c)
        {
            VertexPositionColor[] li = {new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[s]), c),
                             new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[e]), c)};
            d.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, li, 0, 1);
        }

        private void drawLine(Point3D[] p, GraphicsDevice d)
        {
            drawLine(p, d, Color.Red);
        }

        private void drawLine(Point3D[] p, GraphicsDevice d, Color c)
        {
            VertexPositionColor[] li = {new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[0]), c),
                             new VertexPositionColor(Vector3Extension.CreateV3FromP3(p[1]), c)};
            d.DrawUserPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, li, 0, 1);
        }


        Random r = new Random();

        private Color randColor()
        {
            return new Color(r.Next(255), r.Next(255), r.Next(255));
        }

        Color c = new Color(15, 15, 15);

        private Color nextColor()
        {
            byte r = c.R;
            byte g = c.G;
            byte b = c.B;

            if (r < 240)
                c.R += 15;
            else if (g < 240)
                c.G += 15;
            else if (b < 240)
                c.B += 15;
            else
                c = new Color(15, 15, 15);
            
            return c;
        }
    }

    public static class Vector3Extension
    {

        public static Vector3 CreateV3FromP3(this Point3D p)
        {
            return new Vector3((float)p.X, (float)p.Y, (float)p.Z);
        }
    }
}