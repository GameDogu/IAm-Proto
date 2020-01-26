using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Enums
{

    public struct EnumEntry
    {
        public string EntryName;
        public string Value;
        public bool HasValue => Value != "";
    }

    public struct EnumInfo
    {
        public string NameSpace;
        public string EnumName ;
        public bool FlagsEnum;
        public List<EnumEntry> Entries;
        private bool entriesExist => Entries != null;
        public bool HasEntries => entriesExist && Entries.Count > 0;
        public bool IsEnumWithDefinedValues => entriesExist && Entries.Count(en => en.HasValue) > 0;
    }

    public class EnumBuilder
    {
        public string BuildEnum(EnumInfo info)
        {
            throw new NotImplementedException();
        }
    }
    

    public static class BaseEnumExtensions
    {
        public static T Next<T>(this T src) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }

        public static T Previous<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) - 1;
            return (j < 0) ? Arr[Arr.Length-1] : Arr[j];
        }
    }
}
