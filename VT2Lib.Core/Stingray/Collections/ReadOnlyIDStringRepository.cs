namespace VT2Lib.Core.Stingray.Collections;

public sealed class ReadOnlyIDStringRepository : IIDStringProvider
{
    private readonly IDStringRepository _idStringRepo;

    public ReadOnlyIDStringRepository(IDStringRepository idStringRepo)
    {
        ArgumentNullException.ThrowIfNull(idStringRepo);

        _idStringRepo = idStringRepo;
    }

    public bool ContainsID(ulong id)
    {
        return _idStringRepo.ContainsID(id);
    }

    public bool ContainsID(uint id)
    {
        return _idStringRepo.ContainsID(id);
    }

    public IDString64 Get(ulong id)
    {
        return _idStringRepo.Get(id);
    }

    public IDString32 Get(uint id)
    {
        return _idStringRepo.Get(id);
    }

    public bool TryGet(ulong id, out IDString64 result)
    {
        return _idStringRepo.TryGet(id, out result);
    }

    public bool TryGet(uint id, out IDString32 result)
    {
        return _idStringRepo.TryGet(id, out result);
    }
}