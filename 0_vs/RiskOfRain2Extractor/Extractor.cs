using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IllusionPlugin;

namespace RiskOfRain2Extractor
{
    public class Extractor : IPlugin
    {
        public string Name => "Extractor";

        public string Version => "0.0.1";

        public void OnApplicationQuit()
        {
        }

        public void OnApplicationStart()
        {
        }

        public void OnFixedUpdate()
        {
        }

        public void OnLevelWasInitialized(int level)
        {
        }

        static IEnumerable<Type> GetClasses(System.Reflection.Assembly asm, string nameSpace)
        {
            return asm.GetTypes()
                .Where(type => type.Namespace == nameSpace)
                .Select(type => type);
        }
        private InvokeNamespaceClassStaticMethodResult[] _InvokeNamespaceClassStaticMethod(string namespaceName, string methodName, bool throwExceptions, params object[] parameters)
        {
            var results = new List<InvokeNamespaceClassStaticMethodResult>();
            foreach (var _a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var _t in _a.GetTypes())
                {
                    if ((_t.Namespace == namespaceName) && _t.IsClass)
                    {
                        var method_t = _t.GetMethod(methodName, parameters.Select(_ => _.GetType()).ToArray());
                        if ((method_t != null) && method_t.IsPublic && method_t.IsStatic)
                        {
                            var details_t = new InvokeNamespaceClassStaticMethodResult();
                            details_t.Namespace = _t.Namespace;
                            details_t.Class = _t.Name;
                            details_t.Method = method_t.Name;
                            try
                            {
                                if (method_t.ReturnType == typeof(void))
                                {
                                    method_t.Invoke(null, parameters);
                                    details_t.Void = true;
                                }
                                else
                                {
                                    details_t.Return = method_t.Invoke(null, parameters);
                                }
                            }
                            catch (Exception ex)
                            {
                                if (throwExceptions)
                                {
                                    throw;
                                }
                                else
                                {
                                    details_t.Exception = ex;
                                }
                            }
                            results.Add(details_t);
                        }
                    }
                }
            }
            return results.ToArray();
        }

        public class BuffDef
        {
            public int      buffIndex;
            public string   iconPath = "NONE";
            public float    r;
            public float    g;
            public float    b;
            public float    a;
            public bool     canStack;
            public bool     isElite;

            public override string ToString()
            {
                return
                    "buffIndex" + buffIndex + "\r\n" +
                    "iconPath" + iconPath + "\r\n" +
                    "r" + r + "\r\n" +
                    "g" + g + "\r\n" +
                    "b" + b + "\r\n" +
                    "a" + a + "\r\n" +
                    "canStack" + canStack + "\r\n" +
                    "isElite" + isElite + "\r\n";
            }
        }

        private class InvokeNamespaceClassStaticMethodResult
        {
            public string Namespace;
            public string Class;
            public string Method;
            public object Return;
            public bool Void;
            public Exception Exception;
        }

        public void OnLevelWasLoaded(int level)
        {
            string outS = "";

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var namespaces = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
                         .Select(t => t.Namespace)
                         .Distinct();
            List<string> s = new List<string>();
            System.Reflection.Assembly unityASM = null;
            foreach (var b in assemblies)
            {
                if (b.GetName().Name == "Assembly-CSharp")
                {
                    unityASM = b;
                }
                s.Add(b.GetName().Name);
            }
            var a = GetClasses(unityASM, "RoR2");
            var buffDef = a.Where(type => type.Name == "BuffDef").First();
            var buffCatalog = a.Where(type => type.Name == "BuffCatalog").First();
            for (int i = 0; i < 30; i++)
            {
                try
                {
                    object methodValue = buffCatalog.GetMethod("GetBuffDef").Invoke(null, new object[] { (object)i });
                    FieldInfo[] fields = methodValue.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in fields)
                    {
                        outS += "["+i+"] ";
                        outS += field.Name;
                        outS += ": ";
                        outS += field.GetValue(methodValue).ToString();
                        outS += "\r\n";
                    }
                    outS += "\r\n";
                } catch (Exception e) { }
            }
            System.IO.File.WriteAllLines(@"C:\Users\ViMaSter\Desktop\out.txt", new string[] { outS });
        }

        public void OnUpdate()
        {
        }
    }
}
