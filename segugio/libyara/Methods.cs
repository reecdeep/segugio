using System;
using System.Runtime.InteropServices;

namespace dnYara.Interop
{
    public class Methods
    {
        public const string YaraLibName = "libyara.dll";


        /// Return Type: int
        ///rules: YR_RULES*
        [DllImport(YaraLibName, EntryPoint = "yr_rules_destroy")]
        //public static extern int yr_rules_destroy(ref YR_RULES rules);
        public static extern int yr_rules_destroy(IntPtr rules);

        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///file_name: char*
        [DllImport(YaraLibName, EntryPoint = "_yr_compiler_push_file_name")]
        public static extern int _yr_compiler_push_file_name(
            IntPtr compiler,
            [In, MarshalAs(UnmanagedType.LPStr)] string file_name);


        /// Return Type: void
        ///compiler: YR_COMPILER*
        [DllImport(YaraLibName, EntryPoint = "_yr_compiler_pop_file_name")]
        public static extern void _yr_compiler_pop_file_name(IntPtr compiler);


        /// Return Type: char*
        ///include_name: char*
        ///calling_rule_filename: char*
        ///calling_rule_namespace: char*
        ///user_data: void*
        [DllImport(YaraLibName, EntryPoint = "_yr_compiler_default_include_callback")]
        public static extern IntPtr _yr_compiler_default_include_callback(
            [In, MarshalAs(UnmanagedType.LPStr)] string include_name,
            [In, MarshalAs(UnmanagedType.LPStr)] string calling_rule_filename,
            [In, MarshalAs(UnmanagedType.LPStr)] string calling_rule_namespace,
            IntPtr user_data);


        /// Return Type: int
        ///compiler: YR_COMPILER**
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_create")]
        public static extern YARA_ERROR yr_compiler_create(out IntPtr compiler);


        /// Return Type: void
        ///compiler: YR_COMPILER*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_destroy")]
        public static extern void yr_compiler_destroy(IntPtr compiler);


        /// Return Type: void
        ///compiler: YR_COMPILER*
        ///callback: YR_COMPILER_CALLBACK_FUNC
        ///user_data: void*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_set_callback")]
        public static extern void yr_compiler_set_callback(
            IntPtr compiler,
            YR_COMPILER_CALLBACK_FUNC callback,
            IntPtr user_data);


        /// Return Type: void
        ///compiler: YR_COMPILER*
        ///include_callback: YR_COMPILER_INCLUDE_CALLBACK_FUNC
        ///include_free: YR_COMPILER_INCLUDE_FREE_FUNC
        ///user_data: void*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_set_include_callback")]
        public static extern void yr_compiler_set_include_callback(
            IntPtr compiler,
            YR_COMPILER_INCLUDE_CALLBACK_FUNC include_callback,
            YR_COMPILER_INCLUDE_FREE_FUNC include_free,
            IntPtr user_data);


        /// Return Type: void
        ///compiler: YR_COMPILER*
        ///re_ast_callback: YR_COMPILER_RE_AST_CALLBACK_FUNC
        ///user_data: void*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_set_re_ast_callback")]
        public static extern void yr_compiler_set_re_ast_callback(
            IntPtr compiler,
            YR_COMPILER_RE_AST_CALLBACK_FUNC re_ast_callback,
            IntPtr user_data);


        /// Return Type: void
        ///compiler: YR_COMPILER*
        ///table: void*
        ///entries: int
        ///warning_threshold: unsigned char
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_set_atom_quality_table")]
        public static extern void yr_compiler_set_atom_quality_table(IntPtr compiler, IntPtr table, int entries, byte warning_threshold);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///filename: char*
        ///warning_threshold: unsigned char
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_load_atom_quality_table")]
        public static extern int yr_compiler_load_atom_quality_table(
            IntPtr compiler,
            [In, MarshalAs(UnmanagedType.LPStr)] string filename,
            byte warning_threshold);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///rules_file: FILE*
        ///namespace_: char*
        ///file_name: char*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_add_file", SetLastError = false)]
        public static extern int yr_compiler_add_file(
            IntPtr compilerPtr,
            IntPtr filePtr,
            [In, MarshalAs(UnmanagedType.LPStr)] string namespace_,
            [In, MarshalAs(UnmanagedType.LPStr)] string file_name);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///rules_fd: HANDLE->void*
        ///namespace_: char*
        ///file_name: char*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_add_fd")]
        public static extern int yr_compiler_add_fd(
            IntPtr compiler,
            IntPtr rules_fd,
            [In, MarshalAs(UnmanagedType.LPStr)] string namespace_,
            [In, MarshalAs(UnmanagedType.LPStr)] string file_name);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///rules_string: char*
        ///namespace_: char*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_add_string")]
        public static extern int yr_compiler_add_string(
            IntPtr compilerPtr,
            [In, MarshalAs(UnmanagedType.LPStr)] string rules_string,
            [In, MarshalAs(UnmanagedType.LPStr)] string namespace_);


        /// Return Type: char*
        ///compiler: YR_COMPILER*
        ///buffer: char*
        ///buffer_size: int
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_get_error_message")]
        public static extern IntPtr yr_compiler_get_error_message(IntPtr compiler, IntPtr buffer, int buffer_size);


        /// Return Type: char*
        ///compiler: YR_COMPILER*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_get_current_file_name")]
        public static extern IntPtr yr_compiler_get_current_file_name(IntPtr compiler);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///identifier: char*
        ///value: int64_t
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_define_integer_variable")]
        public static extern YARA_ERROR yr_compiler_define_integer_variable(
            IntPtr compiler,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            long value);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///identifier: char*
        ///value: int
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_define_boolean_variable")]
        public static extern YARA_ERROR yr_compiler_define_boolean_variable(
            IntPtr compiler,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            int value);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///identifier: char*
        ///value: double
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_define_float_variable")]
        public static extern YARA_ERROR yr_compiler_define_float_variable(
            IntPtr compiler,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            double value);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///identifier: char*
        ///value: char*
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_define_string_variable")]
        public static extern YARA_ERROR yr_compiler_define_string_variable(
            IntPtr compiler,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            [In, MarshalAs(UnmanagedType.LPStr)] string value);


        /// Return Type: int
        ///compiler: YR_COMPILER*
        ///rules: YR_RULES**
        [DllImport(YaraLibName, EntryPoint = "yr_compiler_get_rules")]
        public static extern YARA_ERROR yr_compiler_get_rules(
            IntPtr compilerPtr,
            ref IntPtr rules);


        [DllImport(YaraLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern string _yr_compiler_default_include_callback(
            string include_name,
            string calling_rule_filename,
            string calling_rule_namespace,
            uint user_data);

        [DllImport(YaraLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern YARA_ERROR yr_initialize();

        [DllImport(YaraLibName, CallingConvention = CallingConvention.Cdecl)]
        public static extern void yr_finalize();


        /// Return Type: void
        [DllImport(YaraLibName, EntryPoint = "yr_finalize_thread")]
        public static extern void yr_finalize_thread();


        /// Return Type: int
        [DllImport(YaraLibName, EntryPoint = "yr_get_tidx")]
        public static extern int yr_get_tidx();


        /// Return Type: void
        ///param0: int
        [DllImport(YaraLibName, EntryPoint = "yr_set_tidx")]
        public static extern void yr_set_tidx(int tidx);


        /// Return Type: int
        ///param0: YR_CONFIG_NAME->_YR_CONFIG_NAME
        ///param1: void*
        [DllImport(YaraLibName, EntryPoint = "yr_set_configuration")]
        public static extern int yr_set_configuration(YR_CONFIG_NAME name, IntPtr src);


        /// Return Type: int
        ///param0: YR_CONFIG_NAME->_YR_CONFIG_NAME
        ///param1: void*
        [DllImport(YaraLibName, EntryPoint = "yr_get_configuration")]
        public static extern int yr_get_configuration(YR_CONFIG_NAME name, IntPtr src);

        /// Return Type: int
        ///rules: YR_RULES*
        ///buffer: uint8_t*
        ///buffer_size: size_t->unsigned __int64
        ///flags: int
        ///callback: YR_CALLBACK_FUNC
        ///user_data: void*
        ///timeout: int
        [DllImport(YaraLibName, EntryPoint = "yr_rules_scan_mem")]
        public static extern YARA_ERROR yr_rules_scan_mem(
            IntPtr rulesPtr,
            IntPtr buffer,
            ulong buffer_size,
            int flags,
            [MarshalAs(UnmanagedType.FunctionPtr)]
            YR_CALLBACK_FUNC callback,
            IntPtr user_data,
            int timeout);

        /// int yr_rules_save(YR_RULES* rules, const char* filename)
        ///timeout: int
        [DllImport(YaraLibName, EntryPoint = "yr_rules_save")]
        public static extern YARA_ERROR yr_rules_save(
            IntPtr rulesPtr,
            [In, MarshalAs(UnmanagedType.LPStr)] string filename);

        /// int yr_rules_save(YR_RULES* rules, const char* filename)
        ///timeout: int
        [DllImport(YaraLibName, EntryPoint = "yr_rules_load")]
        public static extern YARA_ERROR yr_rules_load(
            [In, MarshalAs(UnmanagedType.LPStr)] string filename,
            ref IntPtr rulesPtr);

        /// Return Type: int
        ///rules: YR_RULES*
        ///pid: int
        ///flags: int
        ///callback: YR_CALLBACK_FUNC
        ///user_data: void*
        ///timeout: int
        [DllImport(YaraLibName, EntryPoint = "yr_rules_scan_proc")]
        public static extern YARA_ERROR yr_rules_scan_proc(
            IntPtr rules,
            int pid, int flags,
            YR_CALLBACK_FUNC callback,
            IntPtr user_data,
            int timeout);


        /// Return Type: int
        ///rules: YR_RULES*
        ///filename: char*
        ///flags: int
        ///callback: YR_CALLBACK_FUNC
        ///user_data: void*
        ///timeout: int
        [DllImport(YaraLibName, EntryPoint = "yr_rules_scan_file")]
        public static extern YARA_ERROR yr_rules_scan_file(
            IntPtr rules,
            [In, MarshalAs(UnmanagedType.LPStr)] string filename,
            int flags,
            YR_CALLBACK_FUNC callback,
            IntPtr user_data,
            int timeout);



        /// Return Type: int
        ///rules: YR_RULES*
        ///scanner: YR_SCAN_CONTEXT**
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_create")]
        public static extern YARA_ERROR yr_scanner_create(
            IntPtr rules,
            out IntPtr scanner);


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_destroy")]
        public static extern YARA_ERROR yr_scanner_destroy(
            IntPtr scanner);


        /// Return Type: void
        ///scanner: YR_SCAN_CONTEXT*
        ///callback: YR_CALLBACK_FUNC
        ///user_data: void*
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_set_callback")]
        public static extern void yr_scanner_set_callback(
            IntPtr scanner,
            YR_CALLBACK_FUNC callback,
            IntPtr user_data
            );


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///timeout: int
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_set_timeout")]
        public static extern void yr_scanner_set_timeout(
            IntPtr scanner,
            int timeout);


        /// Return Type: void
        ///scanner: YR_SCAN_CONTEXT*
        ///flags: int
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_set_flags")]
        public static extern void yr_scanner_set_flags(
            IntPtr scanner,
            int flags);


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///identifier: char*
        ///value: long
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_define_integer_variable")]
        public static extern YARA_ERROR yr_scanner_define_integer_variable(
            IntPtr scanner,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            long value);


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///identifier: char*
        ///value: int
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_define_boolean_variable")]
        public static extern YARA_ERROR yr_scanner_define_boolean_variable(
            IntPtr scanner,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            int value);


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///identifier: char*
        ///value: double
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_define_float_variable")]
        public static extern YARA_ERROR yr_scanner_define_float_variable(
            IntPtr scanner,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            double value);


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///identifier: char*
        ///value: char*
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_define_string_variable")]
        public static extern YARA_ERROR yr_scanner_define_string_variable(
            IntPtr scanner,
            [In, MarshalAs(UnmanagedType.LPStr)] string identifier,
            [In, MarshalAs(UnmanagedType.LPStr)] string value
            );


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///buffer: const uint8_t*
        ///buffer_size: size_t
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_scan_mem")]
        public static extern YARA_ERROR yr_scanner_scan_mem(
            IntPtr scanner,
            IntPtr buffer,
            ulong buffer_size);


        /// Return Type: int
        ///scanner: YR_SCAN_CONTEXT*
        ///filename: char*
        [DllImport(YaraLibName, EntryPoint = "yr_scanner_scan_file")]
        public static extern YARA_ERROR yr_scanner_scan_file(
            IntPtr scanner,
            [In, MarshalAs(UnmanagedType.LPStr)] string filename);

    }
}
