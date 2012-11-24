using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Shading
{
    public class PostProcessor
    {
        private RenderTarget2D target1, target2;
        private RenderTarget2D availableTarget;
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;
        private ContentManager content;
        private List<PPEffect> ppEffects;
        private Effect drawImageEffect;

        //Screen-aligned quad code
        VertexBuffer quadVerts;
        IndexBuffer quadIndices;

        public ContentManager Content
        {
            get
            {
                return content;
            }
        }

        public GraphicsDevice Device
        {
            get
            {
                return device;
            }
        }

        public SpriteBatch SpriteBatch
        {
            get
            {
                return spriteBatch;
            }
        }

        public PostProcessor(ContentManager content, GraphicsDevice device, SpriteBatch batch)
        {
            this.content = content;
            this.device = device;
            this.spriteBatch = batch;

            ppEffects = new List<PPEffect>();

            CreateRenderTargets();
            availableTarget = target1;
            SetupScreenAlignedQuad();
            SetupDrawImageEffect();
        }

        private void SetupDrawImageEffect()
        {
            drawImageEffect = content.Load<Effect>("Effects/DrawImage");
            drawImageEffect.Parameters["halfPixel"].SetValue(new Vector2(0.5f / (float)device.PresentationParameters.BackBufferWidth,
                                                        0.5f / (float)device.PresentationParameters.BackBufferHeight));
        }

        private void SetupScreenAlignedQuad()
        {
            VertexPositionTexture[] verts = new VertexPositionTexture[]
                        {
                            new VertexPositionTexture(
                                new Vector3(-1,1,0),
                                new Vector2(0,0)),
                            new VertexPositionTexture(
                                new Vector3(1,1,0),
                                new Vector2(1,0)),
                            new VertexPositionTexture(
                                new Vector3(1,-1,0),
                                new Vector2(1,1)),
                            new VertexPositionTexture(
                                new Vector3(-1,-1,0),
                                new Vector2(0,1))
                        };

            short[] indices = new short[] { 0, 1, 3, 3, 1, 2 };

            quadVerts = new VertexBuffer(device, typeof(VertexPositionTexture), 4, BufferUsage.None);
            quadVerts.SetData<VertexPositionTexture>(verts);

            quadIndices = new IndexBuffer(device, IndexElementSize.SixteenBits, 6, BufferUsage.None);
            quadIndices.SetData<short>(indices);
        }

        private void CreateRenderTargets()
        {
            target1 = new RenderTarget2D(device,
                                              device.Viewport.Width,
                                              device.Viewport.Height,
                                              false,
                                              SurfaceFormat.Color,
                                              DepthFormat.None);

            target2 = new RenderTarget2D(device,
                                             device.Viewport.Width,
                                             device.Viewport.Height,
                                             false,
                                             SurfaceFormat.Color,
                                             DepthFormat.None);
        }

        public void AddPPEffect(PPEffect p)
        {
            ppEffects.Add(p);
        }

        public void DrawFullScreenQuad()
        {
            device.SetVertexBuffer(quadVerts);
            device.Indices = quadIndices;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2);
        }

        public void DrawFullScreenQuad(Effect effect)
        {
            effect.CurrentTechnique.Passes[0].Apply();
            DrawFullScreenQuad();
        }

        public void DrawFullScreenQuad(Texture2D tex)
        {
            drawImageEffect.Parameters["tex"].SetValue(tex);
            drawImageEffect.CurrentTechnique.Passes[0].Apply();
            DrawFullScreenQuad();
        }

        public void SwapTargets()
        {
            if (availableTarget.Equals(target1))
            {
                device.SetRenderTarget(target1);
                availableTarget = target2;
            }
            else
            {
                device.SetRenderTarget(target2);
                availableTarget = target1;
            }
        }

        public RenderTarget2D GetResults()
        {
            return availableTarget;
        }

        public Texture2D Process(Texture2D image, Texture2D color, Texture2D depth, Texture2D normal)
        {
            Texture2D currentResult = image;

            foreach (PPEffect p in ppEffects)
            {
                currentResult = p.Process(currentResult, color, depth, normal);
            }

            return currentResult;
        }
    }
}
