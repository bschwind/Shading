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
        }

        public override Texture2D Process(Texture2D image, Texture2D color, Texture2D depth, Texture2D normal)
        {
            processor.DrawFullScreenQuad(image, negatizeEffect);
            processor.SwapTargets();

            return processor.GetResults();
        }
    }
}
