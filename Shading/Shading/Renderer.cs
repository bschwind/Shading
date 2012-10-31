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
        }

        private void CreateRenderTargets()
        {
            normalTarget = new RenderTarget2D(device,
                                              device.Viewport.Width,
                                              device.Viewport.Height,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.Depth16);

            depthTarget = new RenderTarget2D(device,
                                             device.Viewport.Width,
                                             device.Viewport.Height,
                                             false,
                                             SurfaceFormat.Single,
                                             DepthFormat.Depth16);

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

            replaceModelEffect(m, drawNormalsEffect);
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

            device.SetRenderTarget(colorTarget);
            device.Clear(Color.Transparent);

            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

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

            Texture2D result = postProcessor.Process(colorTarget, colorTarget, depthTarget, normalTarget);

            device.SetRenderTarget(null);
            device.Clear(Color.White);
            postProcessor.DrawFullScreenQuad(result);
        }
    }
}
