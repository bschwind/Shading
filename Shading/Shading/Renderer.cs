using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Shading
{
    public class Renderer
    {
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;
        private ContentManager content;
        private RenderTarget2D normalTarget, depthTarget, colorTarget;
        private List<Model> models;
        private PostProcessor postProcessor;

        private Effect drawNormalsEffect;
        private Effect renderGBufferEffect;
        private Effect ssaoEffect;

        public PostProcessor PostProcessor
        {
            get
            {
                return postProcessor;
            }
        }

        public Renderer(ContentManager content, GraphicsDevice device, SpriteBatch batch)
        {
            this.content = content;
            this.device = device;
            this.spriteBatch = batch;

            CreateRenderTargets();

            models = new List<Model>();
            postProcessor = new PostProcessor(content, device, batch);

            drawNormalsEffect = content.Load<Effect>("Effects/RenderNormals");
            renderGBufferEffect = content.Load<Effect>("Effects/RenderGBuffer");
            ssaoEffect = content.Load<Effect>("Effects/SSAO");
        }

        private void CreateRenderTargets()
        {
            normalTarget = new RenderTarget2D(device,
                                              device.Viewport.Width,
                                              device.Viewport.Height,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.None);

            depthTarget = new RenderTarget2D(device,
                                             device.Viewport.Width,
                                             device.Viewport.Height,
                                             false,
                                             SurfaceFormat.Single,
                                             DepthFormat.None);

            colorTarget = new RenderTarget2D(device,
                                             device.Viewport.Width,
                                             device.Viewport.Height,
                                             false,
                                             SurfaceFormat.Color,
                                             DepthFormat.Depth16);
        }

        public void AddModel(Model m)
        {
            models.Add(m);

            replaceModelEffect(m, renderGBufferEffect);
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

        public void Render(GameTime g, Camera cam)
        {
            float dt = (float)g.ElapsedGameTime.TotalSeconds;
            float elapsed = (float)g.TotalGameTime.TotalSeconds;

            device.SetRenderTargets(colorTarget, normalTarget, depthTarget);
            device.Clear(Color.Transparent);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            BoundingFrustum frustum = new BoundingFrustum(cam.View * cam.Projection);
            renderGBufferEffect.Parameters["frustumCorners"].SetValue(frustum.GetCorners());

            foreach (Model m in models)
            {
                foreach (ModelMesh mesh in m.Meshes)
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
            }

            //Texture2D result = postProcessor.Process(depthTarget, colorTarget, depthTarget, normalTarget);

            device.SetRenderTarget(null);
            device.Clear(Color.White);

            ssaoEffect.Parameters["frustumCorners"].SetValue(frustum.GetCorners());
            ssaoEffect.Parameters["depthMap"].SetValue(depthTarget);
            ssaoEffect.Parameters["normalMap"].SetValue(normalTarget);
            ssaoEffect.Parameters["halfPixel"].SetValue(new Vector2(0.5f / (float)device.PresentationParameters.BackBufferWidth, 0.5f / (float)device.PresentationParameters.BackBufferHeight));
            ssaoEffect.CurrentTechnique.Passes[0].Apply();
            PostProcessor.DrawFullScreenQuad();

            /*float[] data = new float[depthTarget.Width * depthTarget.Height];
            depthTarget.GetData<float>(data);
            System.Diagnostics.Debug.WriteLine(data.Average());*/

            //PostProcessor.DrawFullScreenQuad(depthTarget, ssaoEffect);

            float halfWidth = device.Viewport.Width / 2;
            float halfHeight = device.Viewport.Height / 2;

            /*spriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp, null, null);
            spriteBatch.Draw(colorTarget, new Rectangle(0, 0, (int)halfWidth, (int)halfHeight), Color.White);
            spriteBatch.Draw(depthTarget, new Rectangle((int)halfWidth, 0, (int)halfWidth, (int)halfHeight), Color.White);
            spriteBatch.Draw(normalTarget, new Rectangle(0, (int)halfHeight, (int)halfWidth, (int)halfHeight), Color.White);
            spriteBatch.End();*/
        }
    }
}
