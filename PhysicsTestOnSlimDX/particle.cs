using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;
using System.Runtime.InteropServices;

namespace PhysicsTestOnSlimDX
{

    interface IRenderObject
    {
        bool OnInitialize(Device device);
        void OnRender(DeviceContext context);

        //秒単位 通常小数点以下 ex. 0.016
        void OnPreRender(Device device, float diffTime);
    }

    class Particle : IRenderObject
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
            public Vector3 Position;
            public Vector3 Speed;
            public Vector3 SpinAxis;
            public float Spin;

            public static int SizeInBytes { get { return Marshal.SizeOf(typeof(Status)); } }
        }

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

        const string imagePath = "cobblestone_mossy.png";
        const string fxPath = "physics.fx";

        SlimDX.Direct3D11.Buffer vertexBuffer;
        ShaderResourceView statusView;
        SlimDX.Direct3D11.Buffer statusBuffer;
        List<Status> statusArray = new List<Status>();
        SamplerState sampler;
        ShaderResourceView textureView;
        Effect effect;
        EffectPass effectPass;
        InputLayout vertexLayout;
        Engine3D engine;

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
                        Position = new Vector3(x, (float)r.Next(30), z),
                        Speed = new Vector3(1f/(float)r.Next(10), 1f/(float)r.Next(10), 1f/(float)r.Next(10)),
                        SpinAxis = Vector3.Normalize(new Vector3(0,1,0)),
                        Spin = 1f/ (float)r.Next(10),

                    };
                    statusArray.Add(s);
                    
                }
            }
        }


        public bool OnInitialize(Device device)
        {
            effect = engine.LoadEffect(fxPath);
            effectPass = effect.GetTechniqueByName("Textured_HW_Instancing").GetPassByIndex(0);

            textureView = engine.LoadTexture(imagePath);

            try
            {
                vertexLayout = new InputLayout(
                    device,
                    effectPass.Description.Signature,
                    VertexDefinition.VertexElements
                );
            }
            catch(Exception e)
            {
                return false;
            }

            DataStream stream;
            
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
            catch(Exception e)
            {
                return false;
            }
            stream.Dispose();

            try
            {
                stream = new DataStream(statusArray.ToArray(), true, true);
                var statusBuffer
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

                statusView
                    = new ShaderResourceView(
                        device,
                        statusBuffer,
                        new ShaderResourceViewDescription
                        {
                           Dimension = ShaderResourceViewDimension.ExtendedBuffer,
                           //FirstElement = 0,
                           Format = SlimDX.DXGI.Format.Unknown,
                           //ArraySize = statusArray.Count,
                           ElementCount = statusArray.Count,
                        }
                        );
            }
            catch(Exception e)
            {
                return false;
            }
            stream.Dispose();

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

            /*
            VertexBufferBinding[] binds = new VertexBufferBinding[]
            {
                new VertexBufferBinding(vertexBuffer, Marshal.SizeOf(typeof(VertexDefinition)),0),
            };
            context.InputAssembler.SetVertexBuffers(0, binds);
            */
            context.InputAssembler.SetVertexBuffers(
                0,
                new VertexBufferBinding(
                    vertexBuffer,
                    VertexDefinition.SizeInBytes,
                    0
                    )
               );

            effectPass.Apply(context);
            context.PixelShader.SetSampler(sampler,0);
            context.PixelShader.SetShaderResource(textureView, 0);
            context.VertexShader.SetShaderResource(statusView, 1);
            

            

            context.DrawInstanced(faces.Length, statusArray.Count, 0, 0);

        }
    }
}
