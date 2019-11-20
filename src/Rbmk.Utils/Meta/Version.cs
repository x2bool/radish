using System;

namespace Rbmk.Utils.Meta
{
    public struct Version : IComparable<Version>, IComparable
    {
        public readonly int Major;

        public readonly int Minor;

        public readonly int Patch;

        public readonly int Build;

        public Version(int major, int minor, int patch, int build)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Build = build;
        }

        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}.{Build}";
        }

        public string ToSemverString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
        
        public static Version Parse(string str)
        {
            var ver = str.Split('-')[0];
            
            int major = 0, minor = 0, patch = 0, build = 0;
            var parts = ver.Split('.');

            if (parts.Length > 0 && parts[0] != "*")
            {
                if (!int.TryParse(parts[0], out major))
                {
                    throw new FormatException($"Invalid version: {ver}");
                }
            }

            if (parts.Length > 1 && parts[1] != "*")
            {
                if (!int.TryParse(parts[1], out minor))
                {
                    throw new FormatException($"Invalid version: {ver}");
                }
            }

            if (parts.Length > 2 && parts[2] != "*")
            {
                if (!int.TryParse(parts[2], out patch))
                {
                    throw new FormatException($"Invalid version: {ver}");
                }
            }

            if (parts.Length > 3 && parts[3] != "*")
            {
                if (!int.TryParse(parts[3], out build))
                {
                    throw new FormatException($"Invalid version: {ver}");
                }
            }
            
            return new Version(major, minor, patch, build);
        }

        public static bool TryParse(string str, out Version version)
        {
            try
            {
                version = Parse(str);
                return true;
            }
            catch (FormatException)
            {
                version = default(Version);
                return false;
            }
        }


        public int CompareTo(Version other)
        {
            var majorComparison = Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;
            var minorComparison = Minor.CompareTo(other.Minor);
            if (minorComparison != 0) return minorComparison;
            var patchComparison = Patch.CompareTo(other.Patch);
            if (patchComparison != 0) return patchComparison;
            return Build.CompareTo(other.Build);
        }

        public int CompareTo(object obj)
        {
            if (ReferenceEquals(null, obj)) return 1;
            
            return obj is Version other
                ? CompareTo(other)
                : throw new ArgumentException($"Object must be of type {nameof(Version)}");
        }

        public static bool operator <(Version left, Version right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(Version left, Version right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(Version left, Version right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(Version left, Version right)
        {
            return left.CompareTo(right) >= 0;
        }
    }
}