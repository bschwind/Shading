using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Shading
{
    public class NegatizeEffect : PPEffect
    {
        private Effect negatizeEffect;

        public NegatizeEffect(PostProcessor processor)
            : base(processor)
        {
            negatizeEffect = processor.Content.Load<Effect>("Effects/Negatize");
            negatizeEffect.Parameters["halfPixel"].SetValue(new Vector2(0.5f / (float)processor.Device.PresentationParameters.BackBufferWidth,
                                                            0.5f / (float)processor.Device.PresentationParameters.BackBufferHeight));
        }

        public override Texture2D Process(Texture2D image, Texture2D color, Texture2D depth, Texture2D normal)
        {
            negatizeEffect.Parameters["tex"].SetValue(image);
            processor.DrawFullScreenQuad(negatizeEffect);
            processor.SwapTargets();

            return processor.GetResults();
        }
    }
}
