using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace PhysicsTestOnSlimDX
{

    interface IRenderObject
    {
        bool OnInitialize(Device device);
        void OnRender(DeviceContext context);

        //秒単位 通常小数点以下 ex. 0.016
        void OnPreRender(Device device, float diffTime);
    }

    class Particle
    {
        const string imagePath = "cobblestone_mossy.png";
        const string fxPath = "";
        SamplerState sampler;
        ShaderResourceView textureView;
        Effect effect;
        EffectPass effectPass;


        bool OnInitialize(Device device)
        {

            return true;
        }

        void OnPreRender(Device device, float diffTime)
        {

        }

        void OnRender(DeviceContext context)
        {
            effectPass.Apply(context);
            context.PixelShader.SetSampler(sampler,0);
            context.PixelShader.SetShaderResource(textureView, 0);

        }
    }
}
