using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

namespace PhysicsTestOnSlimDX
{
    class Engine3D
    {
        private SlimDX.Direct3D11.Device device = null;
        private SlimDX.DXGI.SwapChain swapChain = null;
        private RenderTargetView renderTarget = null;


        Dictionary<string, Effect> effectDictionary
            = new Dictionary<string, Effect>();
        Dictionary<string, ShaderResourceView> textureDictionary
            = new Dictionary<string, ShaderResourceView>();
        List<SamplerState> samplerList = new List<SamplerState>();

        DepthStencilView depthStencil;

        public Camera camera = new Camera();
        
        System.Drawing.Size clientSize;

        float fpsLimit = 60.0f;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        Queue<long> fpsQueue = new Queue<long>();
        System.Windows.Forms.Form form;

        List<IRenderObject> renderObjectArray = new List<IRenderObject>();

        public void Run()
        {
            SlimDX.Windows.MessagePump.Run(MainLoop);
        }

        public void SetRenderObject(IRenderObject renderObject)
        {
            renderObjectArray.Add(renderObject);
        }

        public void OnInitialize(System.Windows.Forms.Form form)
        {
            this.form = form;
            clientSize = new System.Drawing.Size(form.Width, form.Height);

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.Debug,
                new SlimDX.DXGI.SwapChainDescription
                {
                    BufferCount = 1,
                    OutputHandle = form.Handle,
                    IsWindowed = true,
                    SampleDescription = new SlimDX.DXGI.SampleDescription
                    {
                        Count = 1,
                        Quality = 0
                    },
                    ModeDescription = new SlimDX.DXGI.ModeDescription
                    {
                        Width = form.Width,
                        Height = form.Height,
                        RefreshRate = new SlimDX.Rational(60, 1),
                        Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm
                    },
                    Usage = SlimDX.DXGI.Usage.RenderTargetOutput,
                    
                },
                out device,
                out swapChain
                );

            using (Texture2D backBuffer =
                SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0))
            {
                renderTarget = new RenderTargetView(device, backBuffer);
                device.ImmediateContext.OutputMerger.SetTargets(renderTarget);
            }

            device.ImmediateContext.Rasterizer.SetViewports(
                new Viewport
                {
                    Width = form.Width,
                    Height = form.Height,
                    MaxZ = 1
                }
                );

            // 深度バッファの設定
            Texture2DDescription depthBufferDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil,
                Format = SlimDX.DXGI.Format.D32_Float,
                Width = clientSize.Width,
                Height = clientSize.Height,
                MipLevels = 1,
                SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0)
            };

            using (Texture2D depthBuffer = new Texture2D(device, depthBufferDesc))
            {
                depthStencil = new DepthStencilView(device, depthBuffer);
            }
            device.ImmediateContext.OutputMerger.SetTargets(depthStencil, renderTarget);

            foreach (IRenderObject itr in renderObjectArray)
            {
                itr.OnInitialize(device);
            }


            return;
        }

        private void updateCamera()
        {
            Matrix mat = Matrix.PerspectiveFovRH(
                (float)System.Math.PI / 4,
                (float)clientSize.Width / (float)clientSize.Height,
                0.1f, 1000
                );
            mat = camera.ViewMatrix * mat;

            foreach (Effect effect in effectDictionary.Values)
            {
                try
                {
                    effect.GetVariableByName("ViewProjection")
                        .AsMatrix().SetMatrix(mat);
                }
                catch(Exception e)
                {
                    return;
                }
            }

        }

        long oldTime = 0;
        protected virtual void MainLoop()
        {


            long nowTime = sw.ElapsedMilliseconds;
            float diffTime = 0;

            form.Text = camera.TEXT;
            if (!sw.IsRunning)
            {
                sw.Start();
                oldTime = sw.ElapsedMilliseconds;
            }
            if (fpsQueue.Count < 60)
            {
                fpsQueue.Enqueue(nowTime);
            }
            else
            {
                diffTime = nowTime - fpsQueue.Dequeue();
                float fps = 1000 * 60 / diffTime;
                form.Text += "fps = " + fps;
            }

            diffTime = nowTime - oldTime;
            oldTime = nowTime;

            OnPreRender(diffTime * camera.Speed / 1000);

            int sleepTime = 1000 / 60 - (int)diffTime;
            if (sleepTime > 1)
            {
          //      System.Threading.Thread.Sleep(sleepTime);
            }
            OnRender();


        }

        protected void OnPreRender(float diffTime)
        {
            foreach (IRenderObject itr in renderObjectArray)
            {
                itr.OnPreRender(device, diffTime);
            }

        }

        protected void OnRender()
        {


            device.ImmediateContext.ClearRenderTargetView(
                renderTarget,
                new Color4(System.Drawing.Color.CornflowerBlue));

            device.ImmediateContext.ClearDepthStencilView(
                depthStencil,
                DepthStencilClearFlags.Depth,
                1,
                0
                );

            updateCamera();

            foreach (IRenderObject itr in renderObjectArray)
            {
                itr.OnRender(device.ImmediateContext);
            }

            swapChain.Present(0, SlimDX.DXGI.PresentFlags.None);

        }


        public Effect LoadEffect(string filePath)
        {
            Effect effect;
            if (effectDictionary.ContainsKey(filePath))
            {
                return effectDictionary[filePath];
            }
            try
            {
                ShaderBytecode shaderBytecode = ShaderBytecode.CompileFromFile(
                filePath,
                "fx_5_0",
                ShaderFlags.None,
                EffectFlags.None);
                effect = new Effect(device, shaderBytecode);
                effectDictionary.Add(filePath, effect);

            }
            catch(Exception e)
            {
                return null;
            }
            return effect;
        }

       // Textureの読み込み
        public ShaderResourceView LoadTexture(string filePath)
        {
            ShaderResourceView srv;
            if(textureDictionary.ContainsKey(filePath))
            {
                return textureDictionary[filePath];
            }

            try
            {
                srv = ShaderResourceView.FromFile(
                    device,
                    filePath
                    );
                textureDictionary.Add(filePath, srv);
            }       
            catch(Exception e)
            {
                srv = null;
            }

            return srv;
        }

        public SamplerState getSamplerState()
        {
            SamplerState samplerState;
            if( samplerList.Count > 0)
            {
                return samplerList[0];
            }
            try
            {
                SamplerDescription description = new SamplerDescription
                {
                    Filter = SlimDX.Direct3D11.Filter.Anisotropic,
                    AddressU = SlimDX.Direct3D11.TextureAddressMode.Wrap,
                    AddressV = SlimDX.Direct3D11.TextureAddressMode.Wrap,
                    AddressW = TextureAddressMode.Wrap,

                };
                samplerState = SamplerState.FromDescription(device, description);
                samplerList.Add(samplerState);

            }
            catch (Exception e)
            {
                samplerState = null;
            }

            return samplerState;
        }









    }
}
