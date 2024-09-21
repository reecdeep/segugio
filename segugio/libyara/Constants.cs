namespace dnYara.Interop
{
    public class Constants
    {
        public const int CHAR_BIT = 8;

        public const int YR_MAX_THREADS = 32;
        public const int tidx_mask_size = (((YR_MAX_THREADS) + (CHAR_BIT - 1)) / CHAR_BIT);

        // This is a placeholder for the raw struct data of a YR_MUTEX, which is a HANDLE on windows and a pthread_mutex_t.
        // These have varying sizes, and we don't actually care about the contents, so we block out the size to ensure
        // bit offsets are maintained.
        public const int yr_mutex_blob_size = 56;

        public const int YR_MAX_LOOP_NESTING  = 4;
        public const int YR_MAX_INCLUDE_DEPTH = 16;


        public const int YR_MAX_COMPILER_ERROR_EXTRA_INFO = 256;
        public const int YR_LEX_BUF_SIZE = 8192;
        public const int MAX_PATH = 260;

        public const int CALLBACK_CONTINUE = 0;
        public const int CALLBACK_ABORT    = 1;
        public const int CALLBACK_ERROR    = 2;

        public const int SCAN_FLAGS_FAST_MODE      = 1;
        public const int SCAN_FLAGS_PROCESS_MEMORY = 2;
        public const int SCAN_FLAGS_NO_TRYCATCH    = 4;

        public const int CALLBACK_MSG_RULE_MATCHING     = 1;
        public const int CALLBACK_MSG_RULE_NOT_MATCHING = 2;
        public const int CALLBACK_MSG_SCAN_FINISHED     = 3;
        public const int CALLBACK_MSG_IMPORT_MODULE     = 4;
        public const int CALLBACK_MSG_MODULE_IMPORTED   = 5;

        public const int RULE_GFLAGS_NULL = 0x1000;

        public const int RULE_FLAGS_NULL = 0x04;

        public const int STRING_FLAGS_LAST_IN_RULE = 0x1000;

        public const int META_FLAGS_LAST_IN_RULE = 1;
    }
}
