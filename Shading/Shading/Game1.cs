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

namespace Shading
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Model cathedral;
        Camera cam;
        float sigma = 12;
        Renderer renderer;
        GaussianBlurPP gaussian;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.PreferMultiSampling = true;
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            cam = new Camera(2.5f, 0.01f, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

            cathedral = Content.Load<Model>("Models/dragon");

            renderer = new Renderer(Content, GraphicsDevice, spriteBatch);
            renderer.AddModel(cathedral);
            gaussian = new GaussianBlurPP(renderer.PostProcessor, sigma);
            renderer.PostProcessor.AddPPEffect(gaussian);
        }

        private void replaceModelEffect(Model model, Effect effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = effect;
                }
            }
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            cam.Update(dt, PlayerIndex.One);

            GamePadState padState = GamePad.GetState(PlayerIndex.One);

            sigma += padState.Triggers.Right;
            sigma -= padState.Triggers.Left;
            sigma = MathHelper.Clamp(sigma, 0.5f, 100);

            gaussian.Sigma = sigma;

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            renderer.Render(gameTime, cam);

            base.Draw(gameTime);
        }
    }
}
