using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

namespace SlimDX_WINDOWS
{
    public class D3D11ENGINE
    {
        private SlimDX.Direct3D11.Device device = null;
        private SlimDX.DXGI.SwapChain swapChain = null;
        private RenderTargetView renderTarget = null;
        DepthStencilView depthStencil;
        System.Windows.Forms.Form form;
        System.Drawing.Size clientSize;

        System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();


        
        public D3D11ENGINE()
        {
        
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

            /*
            foreach (IRenderObject itr in renderObjectArray)
            {
                itr.OnInitialize(device);
            }

            */
            return;
        }

        static int frameCount = 0;
        public void MainLoop()
        {
            float fps;
            frameCount++;
            if( !sw.IsRunning)
            {
                sw.Start();
            }
            float elspsed = (float)sw.ElapsedMilliseconds;
            if( elspsed >= 500f)
            {
                fps = (float)frameCount * 500f / elspsed;
                form.Text = "FPS=[" + fps + "] frameCount" + frameCount;
                sw.Restart();
                frameCount = 0;
            }
            OnRender();
        }

        public void OnRender()
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
            /*
            updateCamera();

            foreach (IRenderObject itr in renderObjectArray)
            {
                itr.OnRender(device.ImmediateContext);
            }
            */
            swapChain.Present(0, SlimDX.DXGI.PresentFlags.None);

        }

    }
}