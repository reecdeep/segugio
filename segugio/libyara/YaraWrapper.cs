using dnYara.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace libYaraWrapper.libyara
{

    /// <summary>
    /// RAII wrapper for GCHandle.
    /// </summary>
    public class GCHandleHandler
        : IDisposable
    {
        public GCHandle Handle { get; }

        public GCHandleHandler(object value)
        {
            Handle = GCHandle.Alloc(value);
        }

        public GCHandleHandler(
            object value,
            GCHandleType handleType)
        {
            Handle = GCHandle.Alloc(value, handleType);
        }

        public void Dispose()
        {
            if (Handle != null)
                Handle.Free();
        }

        public IntPtr GetPointer()
        {
            return GCHandle.ToIntPtr(Handle);
        }
    }

    public sealed class CompilationException
        : Exception
    {
        public List<string> Errors;

        public CompilationException(List<string> errors)
            : base(string.Format(
                        "Error compiling rules.\n{0}", string.Join("\n", errors)))
        {
            Errors = new List<string>(errors);
        }
    }

    //[Serializable]
    //internal class FileIOException
    //    : Exception
    //{
    //    public Errno Error { get; set; }
    //    public string Path { get; set; }
    //    public FileIOException()
    //    {
    //    }

    //    public FileIOException(Errno error, string path) : base($"An error occured on file {path}: {error}.")
    //    {
    //        Error = error;
    //        Path = path;
    //    }
    //}


    public sealed class YaraException
        : Exception
    {
        public YARA_ERROR YRError { get; set; }
        public YaraException(YARA_ERROR error)
                : base(string.Format("Yara error code {0}", Enum.GetName(typeof(YARA_ERROR), error)))
        {
            YRError = error;
        }
    }

    /// <summary>
    /// RAII wrapper for Yara context, must be used with 'using' keyword. 
    /// </summary>
    public sealed class YaraContext
        : IDisposable
    {
        public YaraContext()
        {
            ErrorUtility.ThrowOnError(Methods.yr_initialize());
        }

        ~YaraContext()
        {
            Dispose();
        }

        public void Dispose()
        {
            Methods.yr_finalize();
        }
    }


    public static class DictionaryExtensions
    {
        public static IDictionary<Key, Value> ToDictionary<Key, Value>(this IEnumerable<(Key, Value)> values)
        {
            var dict = new Dictionary<Key, Value>();
            foreach (var (key, value) in values)
            {
                dict[key] = value;
            }
            return dict;
        }
    }

    public class ScanHelper
    {
        public static YARA_ERROR CheckRule(string ruleFile)
        {
            YARA_ERROR error = YARA_ERROR.SUCCESS;
            Compiler comp = new Compiler();

            try
            {
                comp.AddRuleFile(ruleFile);
            }
            catch (YaraException e)
            {
                error = e.YRError;
            }
            catch
            {
                error = YARA_ERROR.ERROR_INVALID_FILE;
            }

            comp.Dispose();
            return error;
        }
    }

    /// <summary>
    /// Data structure representing a string match.
    /// </summary>
    public sealed class Match
    {
        /// <summary>
        /// Base offset/address for the match. While scanning a file this field is usually zero, while
        /// scanning a process memory space this field is the virtual address of the memory block where the match was found.
        /// </summary>
        public long Base { get; set; }

        /// <summary>
        /// Offset of the match relative to base.
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        /// Buffer containing a portion of the matching string.
        /// </summary>
        public byte[] Data { get; set; }


        public Match(YR_MATCH match)
        {
            Base = match.@base;
            Offset = match.offset;

            Data = new byte[match.data_length];
            Marshal.Copy(match.dataPtr, Data, 0, Data.Length);
        }

        public override string ToString()
        {
            if (Data.Length == 0)
                return string.Empty;

            if (Data.Length > 1)
            {
                if (Data[0] == 0)
                    return Encoding.BigEndianUnicode.GetString(Data);

                else if (Data[1] == 0)
                    return Encoding.Unicode.GetString(Data);
            }

            return Encoding.ASCII.GetString(Data);
        }
    }


    public class ProfilingInfo
    {
        public ProfilingInfo()
        {

        }

        public ProfilingInfo(YR_PROFILING_INFO prof)
        {
            AtomMatches = prof.atom_matches;
            MatchTimeNanos = prof.match_time;
            ExecTimeNanos = prof.exec_time;
        }

        /// Number of times that some atom belonging to the rule matched. Each
        /// matching atom means a potential string match that needs to be verified.
        public uint AtomMatches { get; private set; }

        // Amount of time (in nanoseconds) spent verifying atom matches for
        // determining if the corresponding string actually matched or not. This
        // time is not measured for all atom matches, only 1 out of 1024 matches
        // are actually measured.
        public ulong MatchTimeNanos { get; private set; }

        // Amount of time (in nanoseconds) spent evaluating the rule condition.
        public ulong ExecTimeNanos { get; private set; }
    }

    /// <summary>
    /// Data structure representing a single rule.
    /// </summary>
    public sealed class Rule
    {
        /// <summary>
        /// Rule identifier.
        /// </summary>
        public string Identifier { get; set; }

        /// <summary>
        /// Pointer to a sequence of null terminated strings with tag names. An additional null character
        /// marks the end of the sequence. Example: tag1\0tag2\0tag3\0\0.
        /// To iterate over the tags you can use yr_rule_tags_foreach().
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// Key-Value pairs associated with the rule. The value portion can be one of
        /// <list type="bullet">
        /// <term>string</term>
        /// <term>long (int64)</term>
        /// <term>boolean</term>
        /// <term>null</term>
        /// </list>
        /// </summary>
        public IDictionary<string, object> Metas { get; private set; }

        public int AtomsCount { get; private set; }

        public Rule()
        {
            Identifier = string.Empty;
            Tags = new List<string>();
            Metas = new Dictionary<string, object>();
        }

        private static (string name, object value) ExtractMetaValue(YR_META meta)
        {
            var name = Marshal.PtrToStringAnsi(meta.identifier);
            object v = null;
            switch ((META_TYPE)meta.type)
            {
                case META_TYPE.META_TYPE_NULL:
                    break;
                case META_TYPE.META_TYPE_INTEGER:
                    v = meta.integer;
                    break;
                case META_TYPE.META_TYPE_BOOLEAN:
                    v = meta.integer == 0 ? false : true;
                    break;
                case META_TYPE.META_TYPE_STRING:
                    v = Marshal.PtrToStringAnsi(meta.strings);
                    break;
            }
            return (name, v);
        }

        public Rule(YR_RULE rule)
        {
            IntPtr ptr = rule.identifier;
            Identifier = Marshal.PtrToStringAnsi(ptr);
            Tags = ObjRefHelper.IterateCStrings(rule.tags).ToList();
            Metas = ObjRefHelper.GetMetas(rule.metas).Select(ExtractMetaValue).ToDictionary();
            AtomsCount = rule.num_atoms;
        }
    }


    /// <summary>
    /// Data structure containing the different types of external variables passed to a custom scanner
    /// </summary>
    public class ExternalVariables
    {
        public Dictionary<string, string> StringVariables = new Dictionary<string, string>();

        public Dictionary<string, long> IntVariables = new Dictionary<string, long>();

        public Dictionary<string, double> FloatVariables = new Dictionary<string, double>();

        public Dictionary<string, bool> BoolVariables = new Dictionary<string, bool>();

        public void ClearAll()
        {
            StringVariables.Clear();
            IntVariables.Clear();
            FloatVariables.Clear();
            BoolVariables.Clear();
        }

        public int CountAll()
        {
            return StringVariables.Count + IntVariables.Count + FloatVariables.Count + BoolVariables.Count;
        }

    }

    public abstract class ErrorUtility
    {
        public static void ThrowOnError(YARA_ERROR error)
        {
            switch (error)
            {
                case YARA_ERROR.SUCCESS:
                    break;
                default:
                    throw new YaraException(error);
            }
        }
    }



    /// <summary>
    /// Yara compiled rules.
    /// </summary>
    public sealed class CompiledRules
        : IDisposable
    {
        internal IntPtr BasePtr { get; set; }

        public List<Rule> Rules { get; private set; }

        public uint RuleCount { get; private set; }
        public uint StringsCount { get; private set; }
        public uint NamespacesCount { get; private set; }

        public CompiledRules(IntPtr rulesPtr)
        {
            BasePtr = rulesPtr;
            ExtractData();
        }

        private void ExtractData()
        {
            var ruleStruct = Marshal.PtrToStructure<YR_RULES>(BasePtr);
            Rules = ObjRefHelper
                .GetRules(ruleStruct.rules_list_head)
                .Select(rule => new Rule(rule))
                .ToList();
            RuleCount = ruleStruct.num_rules;
            StringsCount = ruleStruct.num_strings;
            NamespacesCount = ruleStruct.num_namespaces;
        }
        public CompiledRules(string filename)
        {
            IntPtr ptr = IntPtr.Zero;
            ErrorUtility.ThrowOnError(Methods.yr_rules_load(filename, ref ptr));
            BasePtr = ptr;
            ExtractData();
        }

        ~CompiledRules()
        {
            if (BasePtr != default)
                Release();
            Dispose();
        }

        public bool Save(string filename)
        {
            ErrorUtility.ThrowOnError(Methods.yr_rules_save(BasePtr, filename));
            return true;
        }

        public void Dispose()
        {
            if (!BasePtr.Equals(IntPtr.Zero))
            {
                IntPtr ptr = BasePtr;
                BasePtr = IntPtr.Zero;
                Methods.yr_rules_destroy(ptr);
            }
        }

        public IntPtr Release()
        {
            var temp = BasePtr;
            BasePtr = default;
            return temp;
        }
    }


    public class Compiler
        : IDisposable
    {
        private IntPtr compilerPtr;

        private List<string> compilationErrors;
        private YR_COMPILER_CALLBACK_FUNC compilerCallback;

        public Compiler()
        {
            ErrorUtility.ThrowOnError(Methods.yr_compiler_create(out compilerPtr));

            compilationErrors = new List<string>();

            compilerCallback = new YR_COMPILER_CALLBACK_FUNC(this.HandleError);

            Methods.yr_compiler_set_callback(compilerPtr, compilerCallback, IntPtr.Zero);

        }

        ~Compiler()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!compilerPtr.Equals(IntPtr.Zero))
            {
                var ptr = compilerPtr;
                compilerPtr = IntPtr.Zero;
                Methods.yr_compiler_destroy(ptr);
            }

        }

        public void AddRuleFile(string path)
        {
            compilationErrors.Clear();

            try
            {
                //PosixFileHandler fw = new PosixFileHandler(path, "r");

                string nullstr = string.Empty;

                string rule = File.ReadAllText(path);

                //var errors = Methods.yr_compiler_add_file(
                //    compilerPtr,
                //    fw.FileHandle,
                //    null,
                //    path);

                var errors = Methods.yr_compiler_add_string(
                    compilerPtr,
                    rule,
                    nullstr);

                if (errors != 0)
                    throw new CompilationException(compilationErrors);
            }
            catch (Exception e)
            {

                MessageBox.Show($"YARA file not matching formal requirements: {path} ",
                      "Error during YARA file compilation", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Environment.Exit(0);

                //throw new Win32Exception(e.HResult, e.Message);
            }
        }

        public void AddRuleString(string rule)
        {
            compilationErrors.Clear();

            var errors = Methods.yr_compiler_add_string(
                compilerPtr,
                rule,
                string.Empty);

            if (errors != 0)
                throw new CompilationException(compilationErrors);
        }

        public void DeclareExternalStringVariable(string name, string defaultValue = "")
        {
            var errors = Methods.yr_compiler_define_string_variable(
                compilerPtr,
                name,
                defaultValue);

            if (errors != 0)
                throw new InvalidDataException($"Error {errors} in DeclareExternalStringVariable '{name}'='{defaultValue}'");
        }

        public void DeclareExternalIntVariable(string name, long defaultValue = 0)
        {
            var errors = Methods.yr_scanner_define_integer_variable(
                compilerPtr,
                name,
                defaultValue);

            if (errors != 0)
                throw new InvalidDataException($"Error {errors} in DeclareExternalIntVariable '{name}'={defaultValue}");
        }

        public void DeclareExternalFloatVariable(string name, double defaultValue = 0)
        {
            var errors = Methods.yr_scanner_define_float_variable(
                compilerPtr,
                name,
                defaultValue);

            if (errors != 0)
                throw new InvalidDataException($"Error {errors} in DeclareExternalFloatVariable setting '{name}'={defaultValue}");
        }

        public void DeclareExternalBooleanVariable(string name, bool defaultValue = false)
        {
            var errors = Methods.yr_compiler_define_boolean_variable(
                compilerPtr,
                name,
                defaultValue == true ? 1 : 0);

            if (errors != 0)
                throw new InvalidDataException($"Error {errors} in DeclareExternalBooleanVariable setting '{name}'={defaultValue}");
        }

        public CompiledRules Compile()
        {
            IntPtr rulesPtr = new IntPtr();

            ErrorUtility.ThrowOnError(
                Methods.yr_compiler_get_rules(compilerPtr, ref rulesPtr));

            return new CompiledRules(rulesPtr);
        }

        public static CompiledRules CompileRulesFile(string path)
        {
            Compiler yc = new Compiler();
            yc.AddRuleFile(path);

            return yc.Compile();
        }

        public static CompiledRules CompileRulesString(string rule)
        {
            Compiler yc = new Compiler();
            yc.AddRuleString(rule);

            return yc.Compile();
        }

        public void HandleError(
            int errorLevel,
            string fileName,
            int lineNumber,
            IntPtr rule,
            string message,
            IntPtr userData)
        {

            var marshaledRule = rule == IntPtr.Zero
                ? new System.Nullable<YR_RULE>()
                : Marshal.PtrToStructure<YR_RULE>(rule);
            var ruleName = marshaledRule.HasValue ? "No Rule" : Marshal.PtrToStringAnsi(marshaledRule.Value.identifier);
            var msg = string.Format("rule {3}, Line {1}, file: {2}: {0}",
                message,
                lineNumber,
                string.IsNullOrWhiteSpace(fileName) ? fileName : "[none]",
                ruleName);

            compilationErrors.Add(msg);
        }
    }

    public class CustomScanner
    {
        private const int YR_TIMEOUT = 10000;

        private IntPtr customScannerPtr = IntPtr.Zero;

        public CustomScanner(CompiledRules rules, int flags = 0, int timeout = YR_TIMEOUT)
        {
            CreateNewScanner(rules, (YR_SCAN_FLAGS)flags, timeout);
        }

        ~CustomScanner()
        {
            if (customScannerPtr != IntPtr.Zero)
            {
                Release();
            }
        }

        //must be called before the context is destroyed (ie: falling out of a using())
        public void Release()
        {
            Methods.yr_scanner_destroy(customScannerPtr);
            customScannerPtr = IntPtr.Zero;
        }

        private void CreateNewScanner(CompiledRules rules, YR_SCAN_FLAGS flags, int timeout)
        {
            ErrorUtility.ThrowOnError(
                Methods.yr_scanner_create(rules.BasePtr, out IntPtr newScanner));

            customScannerPtr = newScanner;

            SetFlags(flags);
            SetTimeout(timeout);
        }

        public virtual void SetFlags(YR_SCAN_FLAGS flags) => Methods.yr_scanner_set_flags(customScannerPtr, (int)flags);
        public virtual void SetTimeout(int timeout) => Methods.yr_scanner_set_timeout(customScannerPtr, timeout);

        private bool TestAllVariablesUnique(ExternalVariables externalVariables, out string duplicatesListString)
        {
            duplicatesListString = "";

            List<string> allKeys = externalVariables.StringVariables.Keys.ToList();
            allKeys.AddRange(externalVariables.IntVariables.Keys.ToList());
            allKeys.AddRange(externalVariables.FloatVariables.Keys.ToList());
            allKeys.AddRange(externalVariables.BoolVariables.Keys.ToList());

            var duplicates = allKeys.GroupBy(_ => _).Where(_ => _.Count() > 1).ToList();

            if (duplicates.Count == 0) return true;

            for (var i = 0; i < duplicates.Count; i++)
            {
                duplicatesListString += $"{duplicates[i].Key}";
                if (i < (duplicates.Count - 1))
                    duplicatesListString += ", ";
            }

            return false;
        }

        private void SetExternalVariables(ExternalVariables externalVariables)
        {
            if (!TestAllVariablesUnique(externalVariables, out string duplicates))
            {
                throw new InvalidDataException("Duplicate external variable names declared: " + duplicates);
            }

            foreach (KeyValuePair<string, string> variable in externalVariables.StringVariables)
                ErrorUtility.ThrowOnError(
                     Methods.yr_scanner_define_string_variable(customScannerPtr, variable.Key, variable.Value));

            foreach (KeyValuePair<string, long> variable in externalVariables.IntVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_integer_variable(customScannerPtr, variable.Key, variable.Value));

            foreach (KeyValuePair<string, double> variable in externalVariables.FloatVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_float_variable(customScannerPtr, variable.Key, variable.Value));

            foreach (KeyValuePair<string, bool> variable in externalVariables.BoolVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_boolean_variable(customScannerPtr, variable.Key, variable.Value == true ? 1 : 0));
        }

        //YARA doesnt allow deletion of variables, this cleans them us as much as practical but
        //a new scanner should be created if it's imporant for them not to exist
        private void ClearExternalVariables(ExternalVariables externalVariables)
        {
            foreach (KeyValuePair<string, string> variable in externalVariables.StringVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_string_variable(customScannerPtr, variable.Key, String.Empty));

            foreach (KeyValuePair<string, long> variable in externalVariables.IntVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_integer_variable(customScannerPtr, variable.Key, long.MinValue));

            foreach (KeyValuePair<string, double> variable in externalVariables.FloatVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_float_variable(customScannerPtr, variable.Key, float.NegativeInfinity));

            foreach (KeyValuePair<string, bool> variable in externalVariables.BoolVariables)
                ErrorUtility.ThrowOnError(
                    Methods.yr_scanner_define_boolean_variable(customScannerPtr, variable.Key, 0));
        }


        public virtual List<ScanResult> ScanFile(string path, ExternalVariables externalVariables)
        {
            if (customScannerPtr == IntPtr.Zero)
                throw new NullReferenceException("Custom Scanner has not been initialised");

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            SetExternalVariables(externalVariables);

            YR_CALLBACK_FUNC scannerCallback = new YR_CALLBACK_FUNC(HandleMessage);
            List<ScanResult> scanResults = new List<ScanResult>();
            GCHandleHandler resultsHandle = new GCHandleHandler(scanResults);
            Methods.yr_scanner_set_callback(customScannerPtr, scannerCallback, resultsHandle.GetPointer());

            ErrorUtility.ThrowOnError(
                Methods.yr_scanner_scan_file(
                    customScannerPtr,
                    path
                    ));

            ClearExternalVariables(externalVariables);

            return scanResults;
        }

        public virtual List<ScanResult> ScanString(
            string text,
            ExternalVariables externalVariables,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.ASCII;

            byte[] buffer = encoding.GetBytes(text);

            return ScanMemory(ref buffer, externalVariables, YR_SCAN_FLAGS.None);
        }

        public virtual List<ScanResult> ScanStream(
            Stream stream,
            ExternalVariables externalVariables)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                byte[] buffer = ms.ToArray();

                return ScanMemory(ref buffer, externalVariables, YR_SCAN_FLAGS.None);
            }
        }

        public virtual List<ScanResult> ScanMemory(
            ref byte[] buffer,
            ExternalVariables externalVariables)
        {
            return ScanMemory(ref buffer, externalVariables, YR_SCAN_FLAGS.None);
        }

        public List<ScanResult> ScanMemory(
            ref byte[] buffer,
            ExternalVariables externalVariables,
            YR_SCAN_FLAGS flags)
        {
            if (buffer.Length == 0)
                return new List<ScanResult>();

            return ScanMemory(ref buffer, buffer.Length, externalVariables, flags);
        }

        internal List<ScanResult> ScanMemory(
            IntPtr buffer,
            int length,
            ExternalVariables externalVariables)
        {
            return ScanMemory(buffer, length, externalVariables, YR_SCAN_FLAGS.None);
        }

        internal List<ScanResult> ScanMemory(
            IntPtr buffer,
            int length,
            ExternalVariables externalVariables,
            YR_SCAN_FLAGS flags)
        {
            byte[] res = new byte[length - 1];
            Marshal.Copy(buffer, res, 0, length);
            return ScanMemory(ref res, length, externalVariables, flags);
        }

        public virtual List<ScanResult> ScanMemory(
            ref byte[] buffer,
            int length,
            ExternalVariables externalVariables,
            YR_SCAN_FLAGS flags)
        {
            YR_CALLBACK_FUNC scannerCallback = new YR_CALLBACK_FUNC(HandleMessage);
            List<ScanResult> scanResults = new List<ScanResult>();
            GCHandleHandler resultsHandle = new GCHandleHandler(scanResults);
            Methods.yr_scanner_set_callback(customScannerPtr, scannerCallback, resultsHandle.GetPointer());

            SetFlags(flags);
            SetExternalVariables(externalVariables);

            IntPtr btCpy = Marshal.AllocHGlobal(buffer.Length); ;
            Marshal.Copy(buffer, 0, btCpy, (int)buffer.Length);

            ErrorUtility.ThrowOnError(
                Methods.yr_scanner_scan_mem(
                    customScannerPtr,
                    btCpy,
                    (ulong)length
                    ));

            ClearExternalVariables(externalVariables);

            return scanResults;
        }

        private YR_CALLBACK_RESULT HandleMessage(
            IntPtr context,
            int message,
            IntPtr message_data,
            IntPtr user_data)
        {

            if (message == Constants.CALLBACK_MSG_RULE_MATCHING)
            {
                var resultsHandle = GCHandle.FromIntPtr(user_data);
                var results = (List<ScanResult>)resultsHandle.Target;

                YR_RULE rule = Marshal.PtrToStructure<YR_RULE>(message_data);
                results.Add(new ScanResult(context, rule));
            }

            return YR_CALLBACK_RESULT.Continue;
        }
    }

    public class Scanner
    {
        private const int YR_TIMEOUT = 10000;

        private YR_CALLBACK_FUNC callbackPtr;

        public Scanner()
        {
            callbackPtr = new YR_CALLBACK_FUNC(HandleMessage);
        }

        public virtual List<ScanResult> ScanFile(string path, CompiledRules rules)
        {
            return ScanFile(path, rules, YR_SCAN_FLAGS.None);
        }

        public virtual List<ScanResult> ScanFile(
            string path,
            CompiledRules rules,
            YR_SCAN_FLAGS flags)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            var results = new List<ScanResult>();
            var nativePath = path;

            GCHandleHandler resultsHandle = new GCHandleHandler(results);

            ErrorUtility.ThrowOnError(
                Methods.yr_rules_scan_file(
                    rules.BasePtr,
                    nativePath,
                    (int)flags,
                    callbackPtr,
                    resultsHandle.GetPointer(),
                    YR_TIMEOUT));

            resultsHandle.Dispose();

            return results;
        }

        public virtual List<ScanResult> ScanProcess(int processId, CompiledRules rules)
        {
            return ScanProcess(processId, rules, YR_SCAN_FLAGS.None);
        }

        public virtual List<ScanResult> ScanProcess(
            int processId,
            CompiledRules rules,
            YR_SCAN_FLAGS flags)
        {
            var results = new List<ScanResult>();
            GCHandleHandler resultsHandle = new GCHandleHandler(results);

            ErrorUtility.ThrowOnError(
                Methods.yr_rules_scan_proc(
                    rules.BasePtr,
                    processId,
                    (int)flags,
                    callbackPtr,
                    resultsHandle.GetPointer(),
                    YR_TIMEOUT));

            return results;
        }

        public virtual List<ScanResult> ScanString(
            string text,
            CompiledRules rules,
            Encoding encoding = null)
        {
            if (encoding == null)
                encoding = Encoding.ASCII;

            byte[] buffer = encoding.GetBytes(text);

            return ScanMemory(ref buffer, rules, YR_SCAN_FLAGS.None);
        }

        public virtual List<ScanResult> ScanStream(
            Stream stream,
            CompiledRules rules)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                byte[] buffer = ms.ToArray();

                return ScanMemory(ref buffer, rules, YR_SCAN_FLAGS.None);
            }
        }

        public virtual List<ScanResult> ScanMemory(
            ref byte[] buffer,
            CompiledRules rules)
        {
            return ScanMemory(ref buffer, rules, YR_SCAN_FLAGS.None);
        }

        public List<ScanResult> ScanMemory(
            ref byte[] buffer,
            CompiledRules rules,
            YR_SCAN_FLAGS flags)
        {
            if (buffer.Length == 0)
                return new List<ScanResult>();

            return ScanMemory(ref buffer, buffer.Length, rules, flags);
        }

        internal List<ScanResult> ScanMemory(
            IntPtr buffer,
            int length,
            CompiledRules rules)
        {
            return ScanMemory(buffer, length, rules, YR_SCAN_FLAGS.None);
        }

        internal List<ScanResult> ScanMemory(
            IntPtr buffer,
            int length,
            CompiledRules rules,
            YR_SCAN_FLAGS flags)
        {
            byte[] res = new byte[length - 1];
            Marshal.Copy(buffer, res, 0, length);
            return ScanMemory(ref res, length, rules, flags);
        }

        public virtual List<ScanResult> ScanMemory(
            ref byte[] buffer,
            int length,
            CompiledRules rules,
            YR_SCAN_FLAGS flags)
        {
            var results = new List<ScanResult>();
            GCHandleHandler resultsHandle = new GCHandleHandler(results);

            IntPtr btCpy = Marshal.AllocHGlobal(buffer.Length); ;
            Marshal.Copy(buffer, 0, btCpy, (int)buffer.Length);

            ErrorUtility.ThrowOnError(
                Methods.yr_rules_scan_mem(
                    rules.BasePtr,
                    btCpy,
                    (ulong)length,
                    (int)flags,
                    callbackPtr,
                    resultsHandle.GetPointer(),
                    YR_TIMEOUT));

            return results;
        }

        private YR_CALLBACK_RESULT HandleMessage(
            IntPtr context,
            int message,
            IntPtr message_data,
            IntPtr user_data)
        {
            if (message == Constants.CALLBACK_MSG_RULE_MATCHING)
            {
                var resultsHandle = GCHandle.FromIntPtr(user_data);
                var results = (List<ScanResult>)resultsHandle.Target;

                YR_RULE rule = Marshal.PtrToStructure<YR_RULE>(message_data);
                results.Add(new ScanResult(context, rule));
            }

            return YR_CALLBACK_RESULT.Continue;
        }
    }

    public class ScanResult
    {
        public Rule MatchingRule;
        public Dictionary<string, List<Match>> Matches;
        public ProfilingInfo ProfilingInfo;

        public ScanResult()
        {
            MatchingRule = null;
            Matches = new Dictionary<string, List<Match>>();
            ProfilingInfo = null;
        }

        public ScanResult(IntPtr scanContext, YR_RULE matchingRule)
        {
            IntPtr matchesPtr = GetMatchesPtr(scanContext);
            IntPtr profilingInfoPtr = GetProfilingInfoPtr(scanContext);

            MatchingRule = new Rule(matchingRule);
            Matches = new Dictionary<string, List<Match>>();

            var matchingStrings = ObjRefHelper.GetYaraStrings(matchingRule.strings);
            foreach (var str in matchingStrings)
            {
                var identifier = str.identifier;

                if (identifier == IntPtr.Zero)
                    return;

                var matches = ObjRefHelper.GetStringMatches(matchesPtr, str);

                foreach (var match in matches)
                {
                    string matchText = ObjRefHelper.ReadYaraString(str);

                    if (!Matches.ContainsKey(matchText))
                        Matches.Add(matchText, new List<Match>());

                    Matches[matchText].Add(new Match(match));
                    if (ProfilingInfo == null)
                    {
                        var profInfo = ObjRefHelper.TryGetProfilingInfoForRule(profilingInfoPtr, (int)str.rule_idx);
                        if (profInfo.HasValue)
                        {
                            ProfilingInfo = new ProfilingInfo(profInfo.Value);
                        }
                    }
                }
            }
        }

        private IntPtr GetProfilingInfoPtr(IntPtr scanContext)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                YR_SCAN_CONTEXT_WIN scan_context = Marshal.PtrToStructure<YR_SCAN_CONTEXT_WIN>(scanContext);
                return scan_context.profiling_info;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                YR_SCAN_CONTEXT_LINUX scan_context = Marshal.PtrToStructure<YR_SCAN_CONTEXT_LINUX>(scanContext);
                return scan_context.profiling_info;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                YR_SCAN_CONTEXT_OSX scan_context = Marshal.PtrToStructure<YR_SCAN_CONTEXT_OSX>(scanContext);
                return scan_context.profiling_info;
            }
            return IntPtr.Zero;
        }

        private IntPtr GetMatchesPtr(IntPtr scanContext)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                YR_SCAN_CONTEXT_WIN scan_context = Marshal.PtrToStructure<YR_SCAN_CONTEXT_WIN>(scanContext);
                return scan_context.matches;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                YR_SCAN_CONTEXT_LINUX scan_context = Marshal.PtrToStructure<YR_SCAN_CONTEXT_LINUX>(scanContext);
                return scan_context.matches;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                YR_SCAN_CONTEXT_OSX scan_context = Marshal.PtrToStructure<YR_SCAN_CONTEXT_OSX>(scanContext);
                return scan_context.matches;
            }
            return IntPtr.Zero;
        }
    }

}
