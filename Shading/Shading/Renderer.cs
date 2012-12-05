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
        private Effect clearGBufferEffect;
        private Effect ssaoEffect;

        private Texture2D NoiseMap;
        public float radius = 2;

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
            clearGBufferEffect = content.Load<Effect>("Effects/ClearGBuffer");
            ssaoEffect = content.Load<Effect>("Effects/SSAO");

            SetupSSAOEffect();
            SetupSSAOEffect();
        }

        private void SetupClearGBufferEffect()
        {
            clearGBufferEffect.Parameters["farplane"].SetValue(10);
        }

        private void SetupSSAOEffect()
        {
            int sampleCount = ssaoEffect.Parameters["Samples"].Elements.Count;
            Vector3[] samples = new Vector3[sampleCount];
            Random rand = new Random();

            for (int i = 0; i < sampleCount; i++)
            {
                samples[i] = new Vector3((float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble() * 2 - 1, (float)rand.NextDouble());
                samples[i] = Vector3.Normalize(samples[i]);
                samples[i] *= (float)rand.NextDouble();
                /*float scale = (float)i / (float)sampleCount;
                scale = MathHelper.Lerp(0.1f, 1.0f, scale * scale);
                samples[i] *= scale;*/
            }

            ssaoEffect.Parameters["Samples"].SetValue(samples);

            int randomTexSize = 32;
            NoiseMap = new Texture2D(device, randomTexSize, randomTexSize);
            Color[] colors = new Color[randomTexSize * randomTexSize];

            for (int i = 0; i < randomTexSize * randomTexSize; i++)
            {
                colors[i] = new Color(new Vector3((float)rand.NextDouble(), (float)rand.NextDouble(), 0));
            }

            NoiseMap.SetData<Color>(colors);
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
            clearGBufferEffect.Parameters["farplane"].SetValue(cam.FarPlane);
            postProcessor.DrawFullScreenQuad(clearGBufferEffect);
            
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.Default;

            BoundingFrustum frustum = new BoundingFrustum(cam.View * cam.Projection);
            //renderGBufferEffect.Parameters["frustumCorners"].SetValue(frustum.GetCorners());

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

            //Texture2D result = postProcessor.Process(normalTarget, colorTarget, depthTarget, normalTarget);

            device.SetRenderTarget(colorTarget);
            device.Clear(Color.Red);

            //PostProcessor.DrawFullScreenQuad(depthTarget);
            Vector3[] corners = frustum.GetCorners();
            Vector3[] farCorners = new Vector3[4];
            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] = Vector3.Transform(corners[i], cam.View);
            }

            for (int i = 0; i < 4; i++)
            {
                farCorners[i] = corners[i + 4] - corners[i];
            }

            ssaoEffect.Parameters["frustumCorners"].SetValue(farCorners);
            ssaoEffect.Parameters["depthMap"].SetValue(depthTarget);
            ssaoEffect.Parameters["normalMap"].SetValue(normalTarget);
            ssaoEffect.Parameters["noiseMap"].SetValue(NoiseMap);
            ssaoEffect.Parameters["halfPixel"].SetValue(new Vector2(0.5f / (float)device.PresentationParameters.BackBufferWidth, 
                                                        0.5f / (float)device.PresentationParameters.BackBufferHeight));
            ssaoEffect.Parameters["Projection"].SetValue(cam.Projection);
            ssaoEffect.Parameters["invProjection"].SetValue(Matrix.Invert(cam.Projection));
            Matrix tempView = cam.View;
            tempView.M41 = 0;
            tempView.M42 = 0;
            tempView.M43 = 0;
            tempView.M44 = 0;
            ssaoEffect.Parameters["View"].SetValue(tempView);
            ssaoEffect.Parameters["radius"].SetValue(radius);

            PostProcessor.DrawFullScreenQuad(ssaoEffect);

            Texture2D result = postProcessor.Process(colorTarget, colorTarget, depthTarget, normalTarget);

            device.SetRenderTarget(null);
            device.Clear(Color.Red);
            postProcessor.DrawFullScreenQuad(result);
        }
    }
}
