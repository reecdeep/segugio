using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace dnYara.Interop
{
    public static class ObjRefHelper
    {
        private static int POINTER_SIZE = Marshal.SizeOf(IntPtr.Zero);

        /// calculates the C-array offset for a struct of type `T` at index `index`
        private static int OffsetFor<T>(int index) => index * Marshal.SizeOf(typeof(T));

        private static T IndexedGet<T>(IntPtr array_start, int index) {
            var array_offset = OffsetFor<T>(index);
            var struct_at_index = (T)Marshal.PtrToStructure(array_start + array_offset, typeof(T));
            return struct_at_index;
        }

        private static bool IsNull(IntPtr p) => p == IntPtr.Zero;

        /// iterates over a linked-list of YR_STRINGs, starting from a given location.
        /// performs the equivalent of `yr_rule_strings_foreach`.
        public static IEnumerable<YR_STRING> GetYaraStrings(IntPtr ref_obj) =>
            EachStructOfTInObjRef<YR_STRING>(
                ref_obj,
                Yes<YR_STRING>,
                ((ptr, yrString) => NullIfPredicateElseStructSize(StringIsLastInRule, ptr, yrString))
            );

        private static bool MetaIsLastInRule(YR_META m) => (m.flags & Constants.META_FLAGS_LAST_IN_RULE) != 0;

        private static bool StringIsLastInRule(YR_STRING str) => (str.flags & Constants.STRING_FLAGS_LAST_IN_RULE) != 0;

        public static IEnumerable<string> IterateCStrings(IntPtr ref_obj)
        {
            string currentString;
            for (
                IntPtr stringPtr = ref_obj;
                SafeMarshalString(stringPtr, out currentString);
                stringPtr += currentString.Length + 1)
            {
                yield return currentString;
            }
        }

        /// Incrementer that bumps an IntPtr by the size of the struct it represents
        private static IntPtr IncrementByStructSize<T>(IntPtr prev, T instance) => prev + Marshal.SizeOf(typeof(T));

        private static IntPtr NullIfPredicateElseStructSize<T>(Func<T, bool> predicate, IntPtr basePtr, T instance) {
            if(predicate(instance)){
                return IntPtr.Zero;
            }
            return IncrementByStructSize<T>(basePtr, instance);
        }

        /// helper function that is true for all input
        private static bool Yes<T>(T item) => true;

        /// walks a variable-sized array of pointers of type T, marshalling and running a custom validation function on each iteration of the pointer
        /// This is an abstraction around specialized loops like `ForEachYaraMetaInObjRef`
        public static IEnumerable<T> EachStructOfTInObjRef<T>(IntPtr ref_obj, Func<T, bool> validityChecker, Func<IntPtr, T, IntPtr> incrementer) where T : struct
        {
            T structPtr;
            for (
                IntPtr structArrayPtr = ref_obj;
                MarshalAndValidate(structArrayPtr, validityChecker, out structPtr);
                structArrayPtr = incrementer(structArrayPtr, structPtr)
            )
            {
                yield return structPtr;
            }
        }

        public static IEnumerable<YR_RULE> GetRules(IntPtr rulesPtr) =>
            EachStructOfTInObjRef<YR_RULE>(
                rulesPtr, 
                (rule => !RuleIsNull(rule)),
                IncrementByStructSize<YR_RULE>
            );


        private static bool MetaIsNull(YR_META m) => m.type == (int)META_TYPE.META_TYPE_NULL;
        
        public static IEnumerable<YR_META> GetMetas(IntPtr yrMetasPtr) =>
            EachStructOfTInObjRef<YR_META>(
                yrMetasPtr, 
                Yes<YR_META>,
                (ptr, meta) => NullIfPredicateElseStructSize(MetaIsLastInRule, ptr, meta)
            );

        public static string ReadYaraString(YR_STRING s)
        {
            string outStr;
            SafeMarshalString(s.identifier, out outStr);
            return outStr;
        }

        /// implements the header-only function`yr_string_matches_foreach` for iterating through
        /// matches in a scan.
        public static IEnumerable<YR_MATCH> GetStringMatches(IntPtr matches, YR_STRING str)
        {
            var string_matches = IndexedGet<YR_MATCHES>(matches, (int)str.idx);

            return
                EachStructOfTInObjRef<YR_MATCH>(string_matches.head,
                    Yes<YR_MATCH>,
                    ((ptr, m) => m.next)
                )
                .Where(m => !m.is_private);
        }

        private static YR_MATCH GetMatchFromObjRef(IntPtr objRef)
        {
            try
            {
                YR_MATCH yrMatch = (YR_MATCH)Marshal.PtrToStructure(objRef, typeof(YR_MATCH));
                return yrMatch;
            }
            catch
            {
                Debug.WriteLine($"Error for Match : {objRef}");
                return default;
            }
        }

        private static bool MarshalAndValidate<T>(IntPtr struct_ptr, Func<T, bool> validityChecker, out T destination_ptr) where T : struct
        {
            destination_ptr = default(T);
            if (IsNull(struct_ptr))
            {
                return false;
            }

            destination_ptr = (T)Marshal.PtrToStructure(struct_ptr, typeof(T));
            return validityChecker(destination_ptr);
        }

        private static bool SafeMarshalString(IntPtr cstring_ptr, out string stringContent)
        {

            stringContent = null;
            if (IsNull(cstring_ptr))
                return false;

            stringContent = Marshal.PtrToStringAnsi(cstring_ptr);
            if (string.IsNullOrEmpty(stringContent))
                return false;

            return true;
        }

        // replicates the RULE_IS_NULL check from the types.h module of yara.
        // used in rule iteration.
        private static bool RuleIsNull(YR_RULE rule) => (rule.flags & Constants.RULE_FLAGS_NULL) != 0;

        public static Nullable<YR_PROFILING_INFO> TryGetProfilingInfoForRule(IntPtr profilingInfoPtr, int rule_index) {
            if(IsNull(profilingInfoPtr)) return null;

            return IndexedGet<YR_PROFILING_INFO>(profilingInfoPtr, rule_index);
        }
    }
}
