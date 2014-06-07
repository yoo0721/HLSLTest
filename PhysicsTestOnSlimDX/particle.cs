//#define COPY_BUFFER

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using System.Runtime.InteropServices;

namespace PhysicsTestOnSlimDX
{
    static class Disposer
    {
        // Viewに関連付けられているResourceもDispose
        public static void Dispose(ResourceView obj)
        {
            if (obj == null) return;
            if(!obj.Disposed)
            {
                Resource resource = obj.Resource;
                if( !resource.Disposed)
                {
                    resource.Dispose();
                }
                obj.Dispose();
            }
        }

        public static void Dispose(ComObject obj)
        {
            if (obj == null) return;
            if(!obj.Disposed)
            {
                obj.Dispose();
            }
        }
    }

    public interface IRenderObject
    {
        bool OnInitialize(Device device);
        void OnRender(DeviceContext context);

        //秒単位 通常小数点以下 ex. 0.016
        void OnPreRender(Device device, float diffTime);
    }

    class Particle : IRenderObject, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        struct VertexDefinition
        {
            public Vector3 Position;
            public Vector2 texel;

            public VertexDefinition(float x, float y, float z, float u, float v)
            {
                Position = new Vector3(x, y, z);
                texel = new Vector2(u, v);
            }

            public static readonly InputElement[] VertexElements = new[]
            {
                new InputElement
                {
                    SemanticName = "SV_Position",
                    Format = SlimDX.DXGI.Format.R32G32B32_Float
                
                },
                new InputElement
                {
                    SemanticName = "TEXCOORD",
                    Format = SlimDX.DXGI.Format.R32G32_Float,
                    AlignedByteOffset = InputElement.AppendAligned,
                },
            };

            public static int SizeInBytes { get { return Marshal.SizeOf(typeof(VertexDefinition)); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        struct Status
        {
            public Vector3 Offset;
            public Vector3 Speed;
            public Vector3 SpinAxis;
            public float Spin;

            public static int SizeInBytes { get { return Marshal.SizeOf(typeof(Status)); } }
        }

        #region 固定頂点の作成
        static VertexDefinition[] faces =
        {
            new VertexDefinition( 0.5f, -0.5f,  0.5f, 0, 1),
            new VertexDefinition( 0.5f,  0.5f,  0.5f, 1, 1),
            new VertexDefinition( 0.5f,  0.5f, -0.5f, 1, 0),
            new VertexDefinition( 0.5f,  0.5f, -0.5f, 1, 0),
            new VertexDefinition( 0.5f, -0.5f, -0.5f, 0, 0),
            new VertexDefinition( 0.5f, -0.5f,  0.5f, 0, 1),

            new VertexDefinition(  0.5f, -0.5f, -0.5f, 1, 0),
            new VertexDefinition(  0.5f,  0.5f, -0.5f, 1, 1),
            new VertexDefinition( -0.5f,  0.5f, -0.5f, 0, 1),
            new VertexDefinition( -0.5f,  0.5f, -0.5f, 0, 1),
            new VertexDefinition( -0.5f, -0.5f, -0.5f, 0, 0),
            new VertexDefinition(  0.5f, -0.5f, -0.5f, 1, 0),


            new VertexDefinition( -0.5f, -0.5f, -0.5f, 0, 0),
            new VertexDefinition( -0.5f,  0.5f, -0.5f, 1, 0),
            new VertexDefinition( -0.5f,  0.5f,  0.5f, 1, 1),
            new VertexDefinition( -0.5f,  0.5f,  0.5f, 1, 1),
            new VertexDefinition( -0.5f, -0.5f,  0.5f, 0, 1),
            new VertexDefinition( -0.5f, -0.5f, -0.5f, 0, 0),

            new VertexDefinition( -0.5f, -0.5f, 0.5f, 0, 0),  
            new VertexDefinition( -0.5f,  0.5f, 0.5f, 0, 1),
            new VertexDefinition(  0.5f,  0.5f, 0.5f, 1, 1),
            new VertexDefinition(  0.5f,  0.5f, 0.5f, 1, 1),
            new VertexDefinition(  0.5f, -0.5f, 0.5f, 1, 0),
            new VertexDefinition( -0.5f, -0.5f, 0.5f, 0, 0),

            new VertexDefinition(  0.5f,  0.5f,  0.5f, 1, 1),  
            new VertexDefinition( -0.5f,  0.5f,  0.5f, 0, 1),
            new VertexDefinition( -0.5f,  0.5f, -0.5f, 0, 0),
            new VertexDefinition( -0.5f,  0.5f, -0.5f, 0, 0),
            new VertexDefinition(  0.5f,  0.5f, -0.5f, 1, 0),
            new VertexDefinition(  0.5f,  0.5f,  0.5f, 1, 1),


            new VertexDefinition(  0.5f, -0.5f,  0.5f, 1, 1),
            new VertexDefinition(  0.5f, -0.5f, -0.5f, 1, 0),
            new VertexDefinition( -0.5f, -0.5f, -0.5f, 0, 0),
            new VertexDefinition( -0.5f, -0.5f, -0.5f, 0, 0),
            new VertexDefinition( -0.5f, -0.5f,  0.5f, 0, 1),
            new VertexDefinition(  0.5f, -0.5f,  0.5f, 1, 1),  
        };
        #endregion

        const string imagePath = "../../../resource/cobblestone_mossy.png";
        const string fxPath = "physics.fx";
        const string csPath = "csParticle.fx";
        UnorderedAccessViewDescription uavDesc = new UnorderedAccessViewDescription
        {
            Format = SlimDX.DXGI.Format.Unknown,
            Dimension = UnorderedAccessViewDimension.Buffer,
        };
        ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription
        {
            Format = SlimDX.DXGI.Format.Unknown,
            Dimension = ShaderResourceViewDimension.ExtendedBuffer,
        };

        SlimDX.Direct3D11.Buffer vertexBuffer;
        SlimDX.Direct3D11.Buffer bufferUA;
        SlimDX.Direct3D11.Buffer bufferSR;
        ShaderResourceView statusSRV = null;
        UnorderedAccessView statusUAV = null;
        ShaderResourceView textureView;
        List<Status> statusArray = new List<Status>();
        SamplerState sampler;
        Effect effect;
        EffectPass effectPass;
        InputLayout vertexLayout;
        Engine3D engine;

        bool _disposed = false;

        public Particle(Engine3D engine)
        {
            this.engine = engine;

            var r = new Random();
            for(int z = 0; z < 100; z++)
            {
                for(int x = 0; x < 100; x++)
                {
                    Status s = new Status
                    {
                        Offset = new Vector3(x, (float)r.Next(30), z),
                        Speed = new Vector3((float)r.Next(10), (float)r.Next(10), (float)r.Next(10)) * 0.0001f,
                        SpinAxis = Vector3.Normalize(new Vector3(0,1,0)),
                        Spin = 1f/ (float)r.Next(10),

                    };
                    statusArray.Add(s);
                    
                }
            }
        }

        ~Particle()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed) return;
            this._disposed = true;
            if(disposing)
            {
                // マネージコードの処理

            }
                //アンマネージコードの処理
            Disposer.Dispose(vertexBuffer);
            //Disposer.Dispose(statusSRV);
            //Disposer.Dispose(statusUAV);
            Disposer.Dispose(textureView);
            Disposer.Dispose(sampler);
            Disposer.Dispose(effect);
            Disposer.Dispose(vertexLayout);

        }
 

        public bool OnInitialize(Device device)
        {
            effect = engine.LoadEffect(fxPath);
            effectPass = effect.GetTechniqueByName("Textured_HW_Instancing").GetPassByIndex(0);

            EffectPass cs = effect.GetTechniqueByIndex(0).GetPassByIndex(0);
            

            textureView = engine.LoadTexture(imagePath);

            try
            {
                vertexLayout = new InputLayout(
                    device,
                    effectPass.Description.Signature,
                    VertexDefinition.VertexElements
                );
            }
            catch (Exception e)
            {
                return false;
            }

            DataStream stream;

            #region vertexBufferの作成
            try
            {
                stream = new DataStream(faces, true, true);
                vertexBuffer = new SlimDX.Direct3D11.Buffer(
                    device,
                    stream,
                    new BufferDescription
                    {
                        SizeInBytes = (int)stream.Length,
                        BindFlags = SlimDX.Direct3D11.BindFlags.VertexBuffer,
                        OptionFlags = ResourceOptionFlags.DrawIndirect,
                    });
            }
            catch (Exception e)
            {
                return false;
            }
            stream.Dispose();
            #endregion

            #region statusBuffer作成
            /// <summary>
            /// Status構造体のバッファ(StructuredBuffer)のViewを作る
            /// </summary>
            /// <remarks>Viewをタイミングを見てリソースに設定</remarks>
            try
            {
                // Buffer の作成
                Status[] s = new Status[statusArray.Count];
                for (int i = 0; i < statusArray.Count; i++)
                {
                    s[i] = new Status
                    {
                        Offset = new Vector3(-2, -2, -2),
                        Speed = new Vector3(-1, -1, -1),
                        SpinAxis = new Vector3(0, 1, 0),
                        Spin = 1.0f,
                    };
                }
                    
                stream = new DataStream(s, true, true);
                bufferSR
                    = new SlimDX.Direct3D11.Buffer(
                       device,
                       stream,
                       new BufferDescription
                       {
                           SizeInBytes = (int)stream.Length,
                           OptionFlags = ResourceOptionFlags.StructuredBuffer,
                           StructureByteStride = Status.SizeInBytes,
                           BindFlags = BindFlags.ShaderResource,

                       });
                
                
                stream.Dispose();
                
                stream = new DataStream(statusArray.ToArray(), true, true);

                bufferUA
                    = new SlimDX.Direct3D11.Buffer(
                        device,
                        stream,
                        new BufferDescription
                        {
                           SizeInBytes = (int)stream.Length,
                           OptionFlags = ResourceOptionFlags.StructuredBuffer,
                           StructureByteStride = Status.SizeInBytes,
                           BindFlags = BindFlags.ShaderResource | BindFlags.UnorderedAccess,
                        });
                stream.Dispose();
                
                // View の作成
#if COPY_BUFFER
                statusSRV
                    = new ShaderResourceView(
                        device,
                        bufferSR,
                        new ShaderResourceViewDescription
                        {
                            Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                            //FirstElement = 0,
                            Format = SlimDX.DXGI.Format.Unknown,
                            //ArraySize = statusArray.Count,
                            ElementCount = statusArray.Count,
                        }
                        );
#else
                statusSRV
                    = new ShaderResourceView(
                        device,
                        bufferUA,
                        new ShaderResourceViewDescription
                        {
                            Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                            //FirstElement = 0,
                            Format = SlimDX.DXGI.Format.Unknown,
                            //ArraySize = statusArray.Count,
                            ElementCount = statusArray.Count,
                        }
                        );
                
#endif

                statusUAV
                    = new UnorderedAccessView(
                        device,
                        bufferUA,
                        new UnorderedAccessViewDescription
                        {
                            Dimension = UnorderedAccessViewDimension.Buffer,
                            ElementCount = statusArray.Count,
                            Format = SlimDX.DXGI.Format.Unknown,
                        });
            }
            catch (Exception e)
            {
                return false;
            }
            stream.Dispose();
            #endregion



            return true;
        }

        public void OnPreRender(Device device, float diffTime)
        {
            EffectVariable ev = effect.GetVariableByName("diffTime");
            EffectScalarVariable esv = ev.AsScalar();
            esv.Set(diffTime);
        }

        public void OnRender(DeviceContext context)
        {
            context.InputAssembler.InputLayout = vertexLayout;
            context.InputAssembler.PrimitiveTopology
                = PrimitiveTopology.TriangleList;

            context.InputAssembler.SetVertexBuffers(
                0,
                new VertexBufferBinding(
                    vertexBuffer,
                    VertexDefinition.SizeInBytes,
                    0
                    )
               );



            try
            {
                ComputeShader cs = engine.Load(csPath);
                context.ComputeShader.Set(cs);

                context.ComputeShader.SetUnorderedAccessView(statusUAV, 2);
                
                context.Dispatch(100, 1, 1);

                //context.CopyResource(bufferUA, bufferSR);

                context.ComputeShader.SetUnorderedAccessView(null, 2);// Slot2 をリセット


                effectPass.Apply(context);
                context.PixelShader.SetSampler(sampler, 0);
                context.PixelShader.SetShaderResource(textureView, 0);
                context.VertexShader.SetShaderResource(statusSRV, 1);

            }
            catch (Exception e)
            {
                return;
            }
            

            context.DrawInstanced(faces.Length, statusArray.Count, 0, 0);

            //context.VertexShader.SetShaderResource(null, 1);
        }
    }
}
