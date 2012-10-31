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
        Texture2D tex;
        Camera cam;
        Effect gaussianEffect;

        RenderTarget2D normalTarget;
        RenderTarget2D tempTarget1, tempTarget2;
        float sig = 12;

        Renderer renderer;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferWidth = 720;
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
            tex = Content.Load<Texture2D>("Models/lol");
            Effect normalEffect = Content.Load<Effect>("Effects/RenderNormals");
            gaussianEffect = Content.Load<Effect>("Effects/GaussianBlur");
            setupGaussianBlur(sig);
            replaceModelEffect(cathedral, normalEffect);

            normalTarget = new RenderTarget2D(GraphicsDevice,
                                              GraphicsDevice.Viewport.Width,
                                              GraphicsDevice.Viewport.Height,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.Depth16);

            tempTarget1 = new RenderTarget2D(GraphicsDevice,
                                              GraphicsDevice.Viewport.Width,
                                              GraphicsDevice.Viewport.Height,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.None);

            tempTarget2 = new RenderTarget2D(GraphicsDevice,
                                              GraphicsDevice.Viewport.Width,
                                              GraphicsDevice.Viewport.Height,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.None);

            renderer = new Renderer(Content, GraphicsDevice, spriteBatch);
            renderer.AddModel(cathedral);
        }

        private void setupGaussianBlur(float sigma)
        {
            // Look up the sample weight effect parameters.
            EffectParameter weightsParameter;
            weightsParameter = gaussianEffect.Parameters["SampleWeights"];

            int samples = weightsParameter.Elements.Count;

            //Due to symmetry, we only store the sample weights for one side of
            //the middle pixel
            float[] sampleWeights = new float[samples];
            float totalWeight = 0f;

            for (int i = 0; i < samples; i++)
            {
                //We take samples halfway between two pixels to take advantage of
                //bilinear filtering
                sampleWeights[i] = computeGaussian(i * 2 + 1.5f, sigma);

                if (i > 0)
                {
                    totalWeight += sampleWeights[i] * 2;
                }
                else
                {
                    totalWeight += sampleWeights[i];
                }
            }

            //Normalize the weights so they sum to 1
            for (int i = 0; i < samples; i++)
            {
                sampleWeights[i] /= totalWeight;
            }

            weightsParameter.SetValue(sampleWeights);
            gaussianEffect.Parameters["dx"].SetValue(1f / GraphicsDevice.Viewport.Width);
            gaussianEffect.Parameters["dy"].SetValue(1f / GraphicsDevice.Viewport.Height);
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

            sig += padState.Triggers.Right;
            sig -= padState.Triggers.Left;
            sig = MathHelper.Clamp(sig, 0.5f, 500);

            setupGaussianBlur(sig);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            renderer.Render(gameTime, cam);

            /*GraphicsDevice.SetRenderTarget(normalTarget);
            GraphicsDevice.Clear(Color.Transparent);

            float elapsed = (float)gameTime.TotalGameTime.TotalSeconds;

            foreach (ModelMesh mesh in cathedral.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    Effect effect = meshPart.Effect;

                    effect.Parameters["World"].SetValue(Matrix.Identity);
                    effect.Parameters["View"].SetValue(cam.View);
                    effect.Parameters["Projection"].SetValue(cam.Projection);
                }
                
                mesh.Draw();
            }

            //Blur horizontally
            GraphicsDevice.SetRenderTarget(tempTarget1);
            gaussianEffect.CurrentTechnique = gaussianEffect.Techniques["HorizontalGaussianBlur"];
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, gaussianEffect);
            spriteBatch.Draw(normalTarget, Vector2.Zero, Color.White);
            spriteBatch.End();

            //Blur vertically
            GraphicsDevice.SetRenderTarget(tempTarget2);
            gaussianEffect.CurrentTechnique = gaussianEffect.Techniques["VerticalGaussianBlur"];
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, gaussianEffect);
            spriteBatch.Draw(tempTarget1, Vector2.Zero, Color.White);
            spriteBatch.End();

            //Draw the composited blur
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            spriteBatch.Draw(tempTarget2, Vector2.Zero, Color.White);
            spriteBatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;*/

            base.Draw(gameTime);
        }

        private static float computeGaussian(float n, float sigma)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * sigma * sigma)) *
                            Math.Exp(-(n * n) / (2 * sigma * sigma)));
        }
    }
}
