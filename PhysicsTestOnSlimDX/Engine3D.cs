using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

namespace PhysicsTestOnSlimDX
{
    public class Engine3D : IDisposable
    {
        static string[] CS_PROFILE = { "cs_main", "cs_5_0" };
        static string[] VS_PROFILE = { "vs_main", "vs_5_0" };
        static string[] GS_PROFILE = { "gs_main", "gs_5_0" };
        static string[] PS_PROFILE = { "ps_main", "ps_5_0" };


        StringBuilder debugString = new StringBuilder();
        private SlimDX.Direct3D11.Device device = null;
        private SlimDX.DXGI.SwapChain swapChain = null;
        private RenderTargetView renderTarget = null;


        Dictionary<string, Effect> effectDictionary
            = new Dictionary<string, Effect>();
        Dictionary<string, ShaderResourceView> textureDictionary
            = new Dictionary<string, ShaderResourceView>();
        List<SamplerState> samplerList = new List<SamplerState>();

        Dictionary<string, ShaderBytecode> shaderBytecodeDictionary = new Dictionary<string, ShaderBytecode>();
        Dictionary<string, ComputeShader> computeShaderDictionary = new Dictionary<string, ComputeShader>();

        DepthStencilView depthStencil;
        
        public Camera camera = new Camera();
        
        System.Drawing.Size clientSize;

        float fpsLimit = 60.0f;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        Queue<long> fpsQueue = new Queue<long>();
        System.Windows.Forms.Form form;

        List<IRenderObject> renderObjectArray = new List<IRenderObject>();

        ~Engine3D()
        {
            swapChain.Dispose();
        }
        public void Dispose()
        {
            foreach(ShaderResourceView srv in textureDictionary.Values)
            {
                srv.Dispose();
            }
            device.Dispose();
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
        public virtual void MainLoop()
        {
            float diffTime = 0;

            long nowTime = sw.ElapsedMilliseconds;

            debugString.Append(camera.TEXT);

            if (!sw.IsRunning)
            {
                sw.Start();
                oldTime = nowTime;
                diffTime = nowTime;
            }
            else
            {
                if (fpsQueue.Count < 60)
                {
                    fpsQueue.Enqueue(nowTime);
                }
                else
                {
                    fpsQueue.Enqueue(nowTime);
                    float fps = nowTime - fpsQueue.Dequeue();
                    fps = 1000 * 60 / fps;
                    debugString.Append("fps = " + fps);
                }

                diffTime = nowTime - oldTime;
                debugString.Append("diffTime = " + diffTime);
                oldTime = nowTime;
            }

            OnPreRender(diffTime * camera.Speed / 1000);

            float sleepTime = 1000f / 60f - diffTime;
            if (sleepTime > 1)
            {
                //System.Threading.Thread.Sleep((int)sleepTime);
            }
            OnRender();
            form.Text = debugString.ToString();
            debugString.Clear();


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
                ShaderFlags.Debug,
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

        public ComputeShader Load(string filePath)
        {
            if (computeShaderDictionary.ContainsKey(filePath))
            {
                return computeShaderDictionary[filePath];
            }
            ShaderBytecode sb = ShaderBytecode.CompileFromFile(
                filePath,
                CS_PROFILE[0],
                CS_PROFILE[1],
#if DEBUG
 ShaderFlags.Debug,
#else
                SharderFlags.None,
#endif
 EffectFlags.None
                );
            ComputeShader cs = new ComputeShader(device, sb);
            if( cs != null)
            {
                computeShaderDictionary.Add(filePath, cs);
            }
            return cs;
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
