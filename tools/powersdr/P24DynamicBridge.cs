using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;

namespace PowerSDR
{
    internal sealed class P24DynamicValue : DynamicObject
    {
        internal static readonly P24DynamicValue Instance = new P24DynamicValue();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = this;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) { return true; }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            result = this;
            return true;
        }

        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            result = this;
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value) { return true; }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            Type t = binder.Type;
            if (t == typeof(string)) { result = String.Empty; return true; }
            if (t == typeof(bool)) { result = false; return true; }
            if (t.IsEnum) { result = Enum.ToObject(t, 0); return true; }
            if (t == typeof(byte)) { result = (byte)0; return true; }
            if (t == typeof(sbyte)) { result = (sbyte)0; return true; }
            if (t == typeof(short)) { result = (short)0; return true; }
            if (t == typeof(ushort)) { result = (ushort)0; return true; }
            if (t == typeof(int)) { result = 0; return true; }
            if (t == typeof(uint)) { result = 0u; return true; }
            if (t == typeof(long)) { result = 0L; return true; }
            if (t == typeof(ulong)) { result = 0UL; return true; }
            if (t == typeof(float)) { result = 0f; return true; }
            if (t == typeof(double)) { result = 0d; return true; }
            if (t == typeof(decimal)) { result = 0m; return true; }
            if (!t.IsValueType) { result = null; return true; }
            try { result = Activator.CreateInstance(t); return true; }
            catch { result = null; return false; }
        }

        public override bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
        {
            if (binder.Operation == ExpressionType.Not) { result = true; return true; }
            result = 0;
            return true;
        }

        public override bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
        {
            switch (binder.Operation)
            {
                case ExpressionType.Equal:
                    result = arg == null || Object.ReferenceEquals(arg, this);
                    return true;
                case ExpressionType.NotEqual:
                    result = !(arg == null || Object.ReferenceEquals(arg, this));
                    return true;
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    result = false;
                    return true;
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    result = true;
                    return true;
                case ExpressionType.Add:
                    result = arg is string ? Convert.ToString(arg, CultureInfo.InvariantCulture) : (object)0d;
                    return true;
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                    result = 0d;
                    return true;
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    result = false;
                    return true;
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    result = Convert.ToBoolean(arg, CultureInfo.InvariantCulture);
                    return true;
                default:
                    result = this;
                    return true;
            }
        }

        public override string ToString() { return String.Empty; }
    }

    internal class P24ReflectiveDynamic : DynamicObject
    {
        protected readonly object Target;
        protected readonly Type TargetType;
        private readonly Dictionary<string, object> _shadow =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        internal P24ReflectiveDynamic(object target)
        {
            Target = target;
            TargetType = target as Type ?? (target == null ? null : target.GetType());
        }

        protected virtual BindingFlags Flags
        {
            get
            {
                return BindingFlags.Public | BindingFlags.NonPublic |
                       (Target is Type ? BindingFlags.Static : BindingFlags.Instance);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_shadow.TryGetValue(binder.Name, out result)) return true;
            if (TargetType != null)
            {
                try
                {
                    PropertyInfo p = TargetType.GetProperty(binder.Name, Flags);
                    if (p != null && p.CanRead)
                    {
                        result = p.GetValue(Target is Type ? null : Target, null);
                        return true;
                    }
                    FieldInfo f = TargetType.GetField(binder.Name, Flags);
                    if (f != null)
                    {
                        result = f.GetValue(Target is Type ? null : Target);
                        return true;
                    }
                }
                catch { }
            }
            result = P24DynamicValue.Instance;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (TargetType != null)
            {
                try
                {
                    PropertyInfo p = TargetType.GetProperty(binder.Name, Flags);
                    if (p != null && p.CanWrite)
                    {
                        p.SetValue(Target is Type ? null : Target, ConvertValue(value, p.PropertyType), null);
                        return true;
                    }
                    FieldInfo f = TargetType.GetField(binder.Name, Flags);
                    if (f != null && !f.IsInitOnly)
                    {
                        f.SetValue(Target is Type ? null : Target, ConvertValue(value, f.FieldType));
                        return true;
                    }
                }
                catch { }
            }
            _shadow[binder.Name] = value;
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (TargetType != null)
            {
                MethodInfo[] methods = TargetType.GetMethods(Flags);
                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo m = methods[i];
                    if (!String.Equals(m.Name, binder.Name, StringComparison.OrdinalIgnoreCase)) continue;
                    ParameterInfo[] ps = m.GetParameters();
                    if (ps.Length != args.Length) continue;
                    try
                    {
                        object[] converted = new object[args.Length];
                        for (int n = 0; n < args.Length; n++)
                            converted[n] = ConvertValue(args[n], ps[n].ParameterType);
                        result = m.Invoke(Target is Type ? null : Target, converted);
                        return true;
                    }
                    catch { }
                }
            }

            result = InvokeFallback(binder.Name, args);
            return true;
        }

        protected virtual object InvokeFallback(string name, object[] args)
        {
            return P24DynamicValue.Instance;
        }

        protected static object ConvertValue(object value, Type targetType)
        {
            if (targetType == null) return value;
            Type t = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (value == null)
                return t.IsValueType ? Activator.CreateInstance(t) : null;
            if (t.IsInstanceOfType(value)) return value;
            if (t.IsEnum)
            {
                if (value is string) return Enum.Parse(t, (string)value, true);
                return Enum.ToObject(t, Convert.ToInt32(value, CultureInfo.InvariantCulture));
            }
            try { return Convert.ChangeType(value, t, CultureInfo.InvariantCulture); }
            catch { return value; }
        }
    }

    internal sealed class P24ConsoleDynamic : P24ReflectiveDynamic
    {
        private static readonly Dictionary<PowerSDR.Console, P24ConsoleDynamic> Cache =
            new Dictionary<PowerSDR.Console, P24ConsoleDynamic>();

        private readonly PowerSDR.Console _console;

        private P24ConsoleDynamic(PowerSDR.Console console) : base(console)
        {
            _console = console;
        }

        internal static dynamic Wrap(PowerSDR.Console console)
        {
            if (console == null) return null;
            lock (Cache)
            {
                P24ConsoleDynamic p;
                if (!Cache.TryGetValue(console, out p))
                {
                    p = new P24ConsoleDynamic(console);
                    Cache[console] = p;
                }
                return p;
            }
        }

        protected override object InvokeFallback(string name, object[] args)
        {
            // Map common Thetis helper methods onto native PowerSDR state without
            // adding Thetis backend code to Console.
            try
            {
                switch (name)
                {
                    case "GetANF":
                        return ReadBool("RX1ANF", "ANF");
                    case "GetAVG":
                        return ReadBool("DisplayAVG", "AVGOn");
                    case "GetPeak":
                        return ReadBool("DisplayPeak", "PeakOn");
                    case "GetCTUN":
                        return ReadBool("CTUN");
                    case "GetMute":
                        return ReadBool("MUT", "Mute");
                    case "GetBin":
                        return ReadBool("BIN");
                    case "GetSubRX":
                        return ReadBool("RX1SubRX", "SubRX");
                    case "GetSelectedNR":
                        return ReadBool("NR", "NR1", "RX1NR");
                    case "GetSelectedNB":
                        return ReadBool("NB", "NB1", "RX1NB");
                    case "GetSNB":
                        return ReadBool("SNB");
                    case "GetPanSwap":
                        return ReadBool("PanSwap");
                    case "GetAGCMode":
                        return ReadMember("RX1AGCMode", "CurrentAGCMode");
                    case "GetDisplayMode":
                        return ReadMember("CurrentDisplayMode", "DisplayMode");
                    case "GetSqlMode":
                        return SquelchState.SQL;
                    case "GetAGCAuto":
                        return false;
                    case "GetGeneralSetting":
                        return false;
                    case "GetQuickSplitEnabled":
                        return ReadBool("VFOSplit");
                    case "GetXPAStatus":
                        return false;
                    case "ThreadSafeCatParse":
                        return String.Empty;
                    case "StopAllTx":
                        TryWrite("MOX", false);
                        return null;
                    case "GetOtherButtonState":
                        return false;
                    case "DoOtherButtonAction":
                        return null;
                    case "GetVHFEnabled":
                        return false;
                    case "GetVHFText":
                        return String.Empty;
                }
            }
            catch { }
            return P24DynamicValue.Instance;
        }

        private object ReadMember(params string[] names)
        {
            Type t = _console.GetType();
            BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            for (int i = 0; i < names.Length; i++)
            {
                try
                {
                    PropertyInfo p = t.GetProperty(names[i], f);
                    if (p != null && p.CanRead) return p.GetValue(_console, null);
                    FieldInfo fi = t.GetField(names[i], f);
                    if (fi != null) return fi.GetValue(_console);
                }
                catch { }
            }
            return P24DynamicValue.Instance;
        }

        private bool ReadBool(params string[] names)
        {
            object o = ReadMember(names);
            try { return Convert.ToBoolean(o, CultureInfo.InvariantCulture); }
            catch { return false; }
        }

        private bool TryWrite(string name, object value)
        {
            try
            {
                Type t = _console.GetType();
                BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                PropertyInfo p = t.GetProperty(name, f);
                if (p != null && p.CanWrite)
                {
                    p.SetValue(_console, ConvertValue(value, p.PropertyType), null);
                    return true;
                }
                FieldInfo fi = t.GetField(name, f);
                if (fi != null && !fi.IsInitOnly)
                {
                    fi.SetValue(_console, ConvertValue(value, fi.FieldType));
                    return true;
                }
            }
            catch { }
            return false;
        }
    }

    internal static class P24Statics
    {
        private static dynamic For(string name)
        {
            Type t = typeof(P24Statics).Assembly.GetType("PowerSDR." + name, false, true);
            return t == null ? (dynamic)P24DynamicValue.Instance : new P24ReflectiveDynamic(t);
        }

        internal static dynamic Display { get { return For("Display"); } }
        internal static dynamic Common { get { return For("Common"); } }
        internal static dynamic BandStackManager { get { return For("BandStackManager"); } }
        internal static dynamic ThetisBotDiscord { get { return For("ThetisBotDiscord"); } }
        internal static dynamic OtherButtonIdHelpers { get { return For("OtherButtonIdHelpers"); } }
        internal static dynamic HardwareSpecific { get { return For("HardwareSpecific"); } }
        internal static dynamic MNotchDB { get { return For("MNotchDB"); } }
        internal static dynamic SpecHPSDRDLL { get { return For("SpecHPSDRDLL"); } }
        internal static dynamic Alex { get { return For("Alex"); } }
        internal static dynamic ColorInterpolator { get { return For("ColorInterpolator"); } }
    }

    internal static class P24StringExtensions
    {
        internal static string Left(this string s, int count)
        {
            if (String.IsNullOrEmpty(s) || count <= 0) return String.Empty;
            return s.Length <= count ? s : s.Substring(0, count);
        }

        internal static string ReplaceIgnoreTokenCase(this string s, string token, string replacement)
        {
            if (String.IsNullOrEmpty(s) || String.IsNullOrEmpty(token)) return s ?? String.Empty;
            int start = 0;
            while (true)
            {
                int i = s.IndexOf(token, start, StringComparison.OrdinalIgnoreCase);
                if (i < 0) return s;
                s = s.Substring(0, i) + (replacement ?? String.Empty) + s.Substring(i + token.Length);
                start = i + (replacement ?? String.Empty).Length;
            }
        }

        internal static bool Contains(this string s, string value, StringComparison comparison)
        {
            return s != null && value != null && s.IndexOf(value, comparison) >= 0;
        }
    }

    internal sealed class P24HiPerfTimer
    {
        private readonly System.Diagnostics.Stopwatch _sw = new System.Diagnostics.Stopwatch();
        internal void Start() { _sw.Start(); }
        internal void Stop() { _sw.Stop(); }
        internal void Reset() { _sw.Reset(); }
        internal double ElapsedMsec { get { return _sw.Elapsed.TotalMilliseconds; } }
    }
}
