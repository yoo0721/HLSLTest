using System.Collections.Generic;
using System.Linq;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.D3DCompiler;

class Program
{
    static void Main(string[] args)
    {
        Device device = new Device(DriverType.Hardware);
        Buffer onlyGpuBuffer = createStructuredBuffer(device, Enumerable.Range(0, 10).ToArray());
        Buffer onlyGpuBuffer2 = createStructuredBuffer(device, Enumerable.Range(10, 20).ToArray());

        initComputeShader(device, onlyGpuBuffer, onlyGpuBuffer2);
        device.ImmediateContext.Dispatch(10, 1, 1);
        writeBuffer(device, onlyGpuBuffer);
        System.Console.ReadLine();
    }

    static void initComputeShader(Device device, Buffer onlyGpuBuffer, Buffer onlyGpuBuffer2)
    {
        device.ImmediateContext.ComputeShader.SetUnorderedAccessView(new UnorderedAccessView(device, onlyGpuBuffer), 0);
        device.ImmediateContext.ComputeShader.SetUnorderedAccessView(new UnorderedAccessView(device, onlyGpuBuffer2), 1);

        ShaderBytecode shaderBytecode = ShaderBytecode.CompileFromFile(
            "MyShader.fx",
            "MyComputeShader",
            "cs_5_0",
            ShaderFlags.None,
            EffectFlags.None
            );
        device.ImmediateContext.ComputeShader.Set(
            new ComputeShader(device, shaderBytecode)
            );

        Effect effect = new Effect(device, shaderBytecode);
        
    }

    static void writeBuffer(Device device, Buffer buffer)
    {
        Buffer cpuAccessibleBuffer = createCpuAccessibleBuffer(device, Enumerable.Range(0, 10).ToArray());
        device.ImmediateContext.CopyResource(buffer, cpuAccessibleBuffer);
        int[] readBack = readBackFromGpu(cpuAccessibleBuffer);

        foreach (var number in readBack)
        {
            System.Console.WriteLine(number);
        }
    }

    static Buffer createStructuredBuffer(Device device, int[] initialData)
    {
        DataStream initialDataStream = new DataStream(initialData, true, true);
        return new Buffer(
            device,
            initialDataStream,
            new BufferDescription
            {
                SizeInBytes = (int)initialDataStream.Length,
                BindFlags = BindFlags.UnorderedAccess,
                OptionFlags = ResourceOptionFlags.StructuredBuffer,
                StructureByteStride = sizeof(int)
            }
            );
    }

    static Buffer createCpuAccessibleBuffer(Device device, int[] initialData)
    {
        DataStream initialDataStream = new DataStream(initialData, true, true);
        return new Buffer(
            device,
            initialDataStream,
            new BufferDescription
            {
                SizeInBytes = (int)initialDataStream.Length,
                CpuAccessFlags = CpuAccessFlags.Read,
                Usage = ResourceUsage.Staging
            }
            );
    }

    static int[] readBackFromGpu(Buffer from)
    {
        DataBox data = from.Device.ImmediateContext.MapSubresource(
            from,
            MapMode.Read,
            MapFlags.None
            );
        return getArrayInt32(data.Data);
    }

    static int[] getArrayInt32(DataStream stream)
    {
        int[] buffer = new int[stream.Length / sizeof(int)];
        stream.ReadRange(buffer, 0, buffer.Length);
        return buffer;
    }
}