﻿using VT2Lib.Core.IO;

namespace VT2Lib.Core.Stingray.Resources.Readers;

public abstract class ResourceReader<TResource> : IResourceReader
    where TResource : IResource, new()
{
    public bool CanRead(IDString64 resourceType)
    {
        return TResource.ResourceType == resourceType;
    }

    public bool CanRead<T>()
        where T : IResource
    {
        return typeof(TResource) == typeof(T);
    }

    public bool CanRead(Type? resourceType)
    {
        return typeof(TResource) == resourceType;
    }

    public abstract TResource Read(PrimitiveReader reader);

    public virtual unsafe TResource Read(ReadOnlySpan<byte> buffer)
    {
        fixed (byte* pBuffer = buffer)
        {
            using var stream = new UnmanagedMemoryStream(pBuffer, buffer.Length);
            using var reader = new PrimitiveReader(stream);
            return Read(reader);
        }
    }

    IResource IResourceReader.Read(PrimitiveReader reader)
    {
        return Read(reader);
    }

    IResource IResourceReader.Read(ReadOnlySpan<byte> buffer)
    {
        return Read(buffer);
    }
}