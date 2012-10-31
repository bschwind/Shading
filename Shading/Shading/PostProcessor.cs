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

        public PostProcessor(ContentManager content, GraphicsDevice device, SpriteBatch batch)
        {
            this.content = content;
            this.device = device;
            this.spriteBatch = batch;
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
    }
}
