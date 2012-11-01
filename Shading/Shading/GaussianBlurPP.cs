using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Shading
{
    public class GaussianBlurPP : PPEffect
    {
        private Effect gaussianEffect;
        private float sigma = 0.5f;

        public float Sigma
        {
            get
            {
                return sigma;
            }
            set
            {
                sigma = value;
                setupGaussianBlur(sigma);
            }
        }

        public GaussianBlurPP(PostProcessor processor, float sigma)
            : base(processor)
        {
            gaussianEffect = processor.Content.Load<Effect>("Effects/GaussianBlur");
            Sigma = sigma;
        }

        private void setupGaussianBlur(float sigma)
        {
            // Look up the sample weight effect parameters.
            EffectParameter weightsParameter;
            weightsParameter = gaussianEffect.Parameters["SampleWeights"];

            int samples = weightsParameter.Elements.Count;

            //Due to symmetry, we only store the sample weights for one side of
            //the middle pixel
            float[] sampleWeights = new float[samples];
            float totalWeight = 0f;

            for (int i = 0; i < samples; i++)
            {
                //We take samples halfway between two pixels to take advantage of
                //bilinear filtering
                sampleWeights[i] = computeGaussian(i * 2 + 1.5f, sigma);

                if (i > 0)
                {
                    totalWeight += sampleWeights[i] * 2;
                }
                else
                {
                    totalWeight += sampleWeights[i];
                }
            }

            //Normalize the weights so they sum to 1
            for (int i = 0; i < samples; i++)
            {
                sampleWeights[i] /= totalWeight;
            }

            weightsParameter.SetValue(sampleWeights);
            gaussianEffect.Parameters["dx"].SetValue(1f / processor.Device.Viewport.Width);
            gaussianEffect.Parameters["dy"].SetValue(1f / processor.Device.Viewport.Height);
        }

        public override Texture2D Process(Texture2D image, Texture2D color, Texture2D depth, Texture2D normal)
        {
            //Blur horizontally
            //processor.Device.SetRenderTarget(temp1);
            processor.SwapTargets();
            gaussianEffect.CurrentTechnique = gaussianEffect.Techniques["HorizontalGaussianBlur"];
            processor.DrawFullScreenQuad(image, gaussianEffect);

            //Blur vertically
            //processor.Device.SetRenderTarget(temp2);
            processor.SwapTargets();
            gaussianEffect.CurrentTechnique = gaussianEffect.Techniques["VerticalGaussianBlur"];
            processor.DrawFullScreenQuad(processor.GetResults(), gaussianEffect);

            processor.SwapTargets();

            return processor.GetResults();
        }

        private static float computeGaussian(float n, float sigma)
        {
            return (float)((1.0 / Math.Sqrt(2 * Math.PI * sigma * sigma)) *
                            Math.Exp(-(n * n) / (2 * sigma * sigma)));
        }
    }
}
