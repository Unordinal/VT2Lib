namespace VT2Lib.Bundles.Resources;

// TODO: This almost seems like a flags value; unsure what exactly the deal is with this.
// Been a while, but iirc this seemed to change for the same language for some files.
// FIXME: Needs verification.
// update 2023/11/11: This may be a resource 'property variant', where a property can technically mean anything
// but can act like file-level overrides. See http://bitsquid.blogspot.com/2016/09/a-new-localization-system-for-stingray.html for more info.
public enum ResourceLanguage : uint
{
    English = 0,
    SimplifiedChinese = 1 << 1,
    Polish = 1 << 2,
    Russian = 1 << 3,
    Polish2 = 1 << 5,
    German = 1 << 6,
    Spanish = 1 << 7,
    BrazilianPortuguese = 1 << 8,
    French = 1 << 9,
    Italian = 1 << 10,
}