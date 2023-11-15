namespace VT2Lib.Core.Stingray.Resources;

// EVAL: I kind of hate this. Should this be done this way? We'd like to treat resources as one 'thing' so we can
// easily manipulate them at a high level (extract, import, etc) without caring about the specific type but I'm not
// 100% on whether this is the best way to go about it.
// EVAL: Should this be named something like IStingrayResource instead?
/// <summary>
/// Represents a Stingray resource.
/// </summary>
public interface IResource
{
    // TODO: This _would_ be a 'static abstract' declaration, but 'static abstract' has issues with this use case;
    // interfaces that have a 'static abstract' member can't be used as type arguments, which makes them useless for quite a few
    // cases such as this. See https://github.com/dotnet/csharplang/issues/5955
    // 2023/11/10: this issue occurs when you return something like an IResource from a method and use it as a type argument.
    // This feels like something that should work, but nope. It means you can't have a list of INumber<int>, for instance.
    // Now we just have to remember to implement it or we runtime error.
    /// <summary>
    /// Gets the type of resource this is as an <see cref="IDString64"/>.
    /// </summary>
    static virtual IDString64 ResourceID => throw new NotImplementedException();

    IDString64 GetResourceID();

    int GetResourceVersion();
}