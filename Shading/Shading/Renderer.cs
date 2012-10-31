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

        public Renderer(ContentManager content, GraphicsDevice device, SpriteBatch batch)
        {
            this.content = content;
            this.device = device;
            this.spriteBatch = batch;

            CreateRenderTargets();
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
    }
}
