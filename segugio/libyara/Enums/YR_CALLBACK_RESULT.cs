namespace dnYara.Interop
{
    public enum YR_CALLBACK_RESULT 
        : int
    {
        Continue = Constants.CALLBACK_CONTINUE,
        Abort    = Constants.CALLBACK_ABORT,
        Error    = Constants.CALLBACK_ERROR
    }
}