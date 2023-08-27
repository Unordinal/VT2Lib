using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using VT2Lib.Core.Extensions;

namespace VT2Lib.Core.Stingray.Collections;

/// <summary>
/// A thread-safe repository for adding and retrieving IDStrings.
/// </summary>
public sealed class IDStringRepository : IIDStringProvider
{
    /// <summary>
    /// Gets the shared <see cref="IDStringRepository"/>.
    /// </summary>
    public static IDStringRepository Shared { get; } = new();

    public IDString32 this[uint id] => Get(id);

    public IDString64 this[ulong id] => Get(id);

    /// <summary>
    /// Gets a read-only collection that contains the <see cref="IDString32"/> values in this <see cref="IDStringRepository"/>.
    /// </summary>
    public IReadOnlyCollection<IDString32> IDString32Values => _idString32Repo.Values.AsReadOnly();

    /// <summary>
    /// Gets a read-only collection that contains the <see cref="IDString64"/> values in this <see cref="IDStringRepository"/>.
    /// </summary>
    public IReadOnlyCollection<IDString64> IDString64Values => _idString64Repo.Values.AsReadOnly();

    private readonly ConcurrentDictionary<uint, IDString32> _idString32Repo;
    private readonly ConcurrentDictionary<ulong, IDString64> _idString64Repo;

    public IDStringRepository()
    {
        _idString32Repo = new();
        _idString64Repo = new();
    }

    public IDStringRepository(IEnumerable<IDString32>? idString32Collection, IEnumerable<IDString64>? idString64Collection)
    {
        idString32Collection ??= Enumerable.Empty<IDString32>();
        idString64Collection ??= Enumerable.Empty<IDString64>();

        var idString32Kvps = idString32Collection.Select(idString => new KeyValuePair<uint, IDString32>(idString.ID, idString));
        var idString64Kvps = idString64Collection.Select(idString => new KeyValuePair<ulong, IDString64>(idString.ID, idString));

        _idString32Repo = new ConcurrentDictionary<uint, IDString32>(idString32Kvps);
        _idString64Repo = new ConcurrentDictionary<ulong, IDString64>(idString64Kvps);
    }

    public bool ContainsID(uint id)
    {
        return _idString32Repo.ContainsKey(id);
    }

    public bool ContainsID(ulong id)
    {
        return _idString64Repo.ContainsKey(id);
    }

    public void Add(IDString32 idString)
    {
        if (!InternalTryAdd(idString))
            ThrowHashAlreadyExistsException(idString);
    }

    public void Add(IDString64 idString)
    {
        if (!InternalTryAdd(idString))
            ThrowHashAlreadyExistsException(idString);
    }

    public IDString32 Get(uint id)
    {
        return _idString32Repo[id];
    }

    public IDString64 Get(ulong id)
    {
        return _idString64Repo[id];
    }

    public IDString32 GetOrCreate(uint id)
    {
        if (TryGet(id, out var idString))
            return idString;

        return new IDString32(id);
    }

    public IDString64 GetOrCreate(ulong id)
    {
        if (TryGet(id, out var idString))
            return idString;

        return new IDString64(id);
    }

    public bool TryAdd(IDString32 idString)
    {
        return InternalTryAdd(idString);
    }

    public bool TryAdd(IDString64 idString)
    {
        return InternalTryAdd(idString);
    }

    public bool TryGet(uint id, out IDString32 value)
    {
        return _idString32Repo.TryGetValue(id, out value);
    }

    public bool TryGet(ulong id, out IDString64 value)
    {
        return _idString64Repo.TryGetValue(id, out value);
    }

    private bool InternalTryAdd(IDString32 idString)
    {
        bool result = _idString32Repo.TryAdd(idString.ID, idString);
        if (!result)
        {
            var existing = _idString32Repo[idString.ID];
            if (existing.Value != idString.Value)
                Trace.WriteLine($"Hash collision: the specified 32-bit hash already exists with a different value in the repository. Existing: <{existing.ID:x8}: {existing}>. New: <{idString.ID:x8}: {idString}>.");
        }

        return result;
    }

    private bool InternalTryAdd(IDString64 idString)
    {
        bool result = _idString64Repo.TryAdd(idString.ID, idString);
        if (!result)
        {
            var existing = _idString64Repo[idString.ID];
            if (existing.Value != idString.Value)
                Trace.WriteLine($"Hash collision: the specified 64-bit hash already exists with a different value in the repository. Existing: <{existing.ID:x16}: {existing}>. New: <{idString.ID:x16}: {idString}>.");
        }

        return result;
    }

    private const string ExcHashAlreadyExistsSameValue = "An IDString with the same ID hash already exists in the repo with the same value. Existing: <{0}>.";
    private const string ExcHashAlreadyExistsDiffValue = "An IDString with the same ID hash already exists in the repo with a different value. Existing: <{0}>. New: <{1}>.";

    [DebuggerHidden, DoesNotReturn]
    private void ThrowHashAlreadyExistsException(IDString32 idString, [CallerArgumentExpression(nameof(idString))] string? argName = null)
    {
        bool doesAlreadyExist = _idString32Repo.TryGetValue(idString.ID, out var existing);
        Debug.Assert(doesAlreadyExist);

        string existingIDStr = existing.Value ?? "<null>";
        string newIDStr = idString.Value ?? "<null>";

        ThrowHashAlreadyExistsException(existingIDStr, newIDStr, argName);
    }

    [DebuggerHidden, DoesNotReturn]
    private void ThrowHashAlreadyExistsException(IDString64 idString, [CallerArgumentExpression(nameof(idString))] string? argName = null)
    {
        bool doesAlreadyExist = _idString64Repo.TryGetValue(idString.ID, out var existing);
        Debug.Assert(doesAlreadyExist);

        string existingIDStr = existing.Value ?? "<null>";
        string newIDStr = idString.Value ?? "<null>";

        ThrowHashAlreadyExistsException(existingIDStr, newIDStr, argName);
    }

    [DebuggerHidden, DoesNotReturn]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowHashAlreadyExistsException(string existingIDStr, string newIDStr, string? argName)
    {
        string excMessageFormat = existingIDStr == newIDStr
            ? ExcHashAlreadyExistsSameValue
            : ExcHashAlreadyExistsDiffValue;
        string excMessage = string.Format(excMessageFormat, existingIDStr, newIDStr);

        throw new ArgumentException(excMessage, argName);
    }
}