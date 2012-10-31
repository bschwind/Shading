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
        private GraphicsDevice device;
        private SpriteBatch spriteBatch;
        private ContentManager content;
        private List<PPEffect> ppEffects;

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

        public void DrawFullScreenQuad(Texture2D tex, Effect effect)
        {
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(tex, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
            spriteBatch.End();
        }

        public void DrawFullScreenQuad(Texture2D tex)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(tex, new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height), Color.White);
            spriteBatch.End();
        }

        public Texture2D Process(Texture2D image, Texture2D color, Texture2D depth, Texture2D normal)
        {
            Texture2D currentResult = image;

            foreach (PPEffect p in ppEffects)
            {
                currentResult = p.Process(currentResult, color, depth, normal, target1, target2);
            }

            return currentResult;
        }
    }
}
