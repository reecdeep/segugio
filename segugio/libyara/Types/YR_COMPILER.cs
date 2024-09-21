using System;
using System.Runtime.InteropServices;


namespace dnYara.Interop
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct YR_COMPILER
    {
        public int errors;
        public int current_line;
        public int last_error;
        public int last_error_line;

        /// _SETJMP_FLOAT128[16]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public SETJMP_FLOAT128[] error_recovery;

        public IntPtr sz_arena;
        public IntPtr rules_arena;
        public IntPtr strings_arena;
        public IntPtr code_arena;
        public IntPtr re_code_arena;
        public IntPtr compiled_rules_arena;
        public IntPtr externals_arena;
        public IntPtr namespaces_arena;
        public IntPtr metas_arena;
        public IntPtr matches_arena;
        public IntPtr automaton_arena;

        public IntPtr automaton;
        public IntPtr rules_table;
        public IntPtr objects_table;
        public IntPtr strings_table;
        public IntPtr current_namespace;
        public IntPtr current_rule;

        public IntPtr fixup_stack_head;

        /// int
        public int namespaces_count;

        /// unsigned char*[4]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.YR_MAX_LOOP_NESTING, ArraySubType = UnmanagedType.SysUInt)]
        public IntPtr[] loop_address;

        /// char*[4]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.YR_MAX_LOOP_NESTING, ArraySubType = UnmanagedType.SysUInt)]
        public IntPtr[] loop_identifier;

        /// int
        public int loop_depth;

        /// int
        public int loop_for_of_mem_offset;

        /// char*[16]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Constants.YR_MAX_INCLUDE_DEPTH, ArraySubType = UnmanagedType.SysUInt)]
        public IntPtr[] file_name_stack;

        /// int
        public int file_name_stack_ptr;

        /// char[256]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.YR_MAX_COMPILER_ERROR_EXTRA_INFO)]
        public string last_error_extra_info;

        /// char[8192]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.YR_LEX_BUF_SIZE)]
        public string lex_buf;

        /// char*
        public IntPtr lex_buf_ptr;

        /// unsigned short
        public ushort lex_buf_len;

        /// char[260]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Constants.MAX_PATH)]
        public string include_base_dir;

        /// void*
        public IntPtr user_data;

        /// void*
        public IntPtr incl_clbk_user_data;

        /// void*
        public IntPtr re_ast_clbk_user_data;




        /// YR_COMPILER_CALLBACK_FUNC
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public YR_COMPILER_CALLBACK_FUNC callback;
        //public int func1;

        /// YR_COMPILER_INCLUDE_CALLBACK_FUNC
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public YR_COMPILER_INCLUDE_CALLBACK_FUNC include_callback;

        /// YR_COMPILER_INCLUDE_FREE_FUNC
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public YR_COMPILER_INCLUDE_FREE_FUNC include_free;

        /// YR_COMPILER_RE_AST_CALLBACK_FUNC
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public YR_COMPILER_RE_AST_CALLBACK_FUNC re_ast_callback;

        /// YR_ATOMS_CONFIG
        public YR_ATOMS_CONFIG atoms_config;
        
    }

}
