using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Shading
{
    public class PPEffect
    {
        protected PostProcessor processor;

        public PPEffect(PostProcessor processor)
        {
            this.processor = processor;
        }

        public virtual Texture2D Process(Texture2D image,
                                         Texture2D color,
                                         Texture2D depth,
                                         Texture2D normal)
        {
            return image;
        }
    }
}
