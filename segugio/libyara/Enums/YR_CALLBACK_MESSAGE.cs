namespace dnYara.Interop
{
    public enum YR_CALLBACK_MESSAGE 
        : int
    {
        RuleMatching    = Constants.CALLBACK_MSG_RULE_MATCHING,
        RuleNotMatching = Constants.CALLBACK_MSG_RULE_NOT_MATCHING,
        ScanFinished    = Constants.CALLBACK_MSG_SCAN_FINISHED,
        ImportModule    = Constants.CALLBACK_MSG_IMPORT_MODULE,
        ModuleImported  = Constants.CALLBACK_MSG_MODULE_IMPORTED
    };
}