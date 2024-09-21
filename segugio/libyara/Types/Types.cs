using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    public enum META_TYPE
    {
        META_TYPE_NULL = 0,
        META_TYPE_INTEGER = 1,
        META_TYPE_STRING = 2,
        META_TYPE_BOOLEAN = 3
    }

    /// Return Type: void
    ///error_level: int
    ///file_name: char*
    ///line_number: int
    ///rule: YR_RULE*
    ///message: char*
    ///user_data: void*
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void YR_COMPILER_CALLBACK_FUNC(
        int error_level,
        [In, MarshalAs(UnmanagedType.LPStr)] string file_name,
        int line_number,
        IntPtr rule,
        [In()] [MarshalAs(UnmanagedType.LPStr)] string message,
        IntPtr user_data
    );

    /// Return Type: char*
    ///include_name: char*
    ///calling_rule_filename: char*
    ///calling_rule_namespace: char*
    ///user_data: void*
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr YR_COMPILER_INCLUDE_CALLBACK_FUNC([In, MarshalAs(UnmanagedType.LPStr)] string include_name, [In()] [MarshalAs(UnmanagedType.LPStr)] string calling_rule_filename, [In()] [MarshalAs(UnmanagedType.LPStr)] string calling_rule_namespace, IntPtr user_data);

    /// Return Type: void
    ///callback_result_ptr: char*
    ///user_data: void*
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void YR_COMPILER_INCLUDE_FREE_FUNC([In, MarshalAs(UnmanagedType.LPStr)] string callback_result_ptr, IntPtr user_data);

    /// Return Type: void
    ///rule: YR_RULE*
    ///string_identifier: char*
    ///re_ast: RE_AST*
    ///user_data: void*
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void YR_COMPILER_RE_AST_CALLBACK_FUNC(ref YR_RULE rule, [In, MarshalAs(UnmanagedType.LPStr)] string string_identifier, ref RE_AST re_ast, IntPtr user_data);

    /// Return Type: int
    ///message: int
    ///message_data: void*
    ///user_data: void*
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate YR_CALLBACK_RESULT YR_CALLBACK_FUNC(System.IntPtr context, int message, System.IntPtr message_data, System.IntPtr user_data);

    /// Return Type: int
    ///config: YR_ATOMS_CONFIG*
    ///atom: unsigned char*
    ///atom_length: int
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate int YR_ATOMS_QUALITY_FUNC(ref YR_ATOMS_CONFIG config, IntPtr atom, int atom_length);

}
