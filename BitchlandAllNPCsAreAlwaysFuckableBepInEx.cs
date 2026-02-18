using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using SemanticVersioning;
using System;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine;

namespace BitchlandAllNPCsAreAlwaysFuckableBepInEx
{
    [BepInPlugin("com.wolfitdm.BitchlandAllNPCsAreAlwaysFuckableBepInEx", "BitchlandAllNPCsAreAlwaysFuckableBepInEx Plugin", "1.0.0.0")]
    public class BitchlandAllNPCsAreAlwaysFuckableBepInEx : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private ConfigEntry<bool> configEnableMe;

        public BitchlandAllNPCsAreAlwaysFuckableBepInEx()
        {
        }

        public static Type MyGetType(string originalClassName)
        {
            return Type.GetType(originalClassName + ",Assembly-CSharp");
        }

        private static string pluginKey = "General.Toggles";

        public static bool enableThisMod = false;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            configEnableMe = Config.Bind(pluginKey,
                                              "EnableThisMod",
                                              true,
                                             "Whether or not you want enable this mod (default true also yes, you want it, and false = no)");


            enableThisMod = configEnableMe.Value;

            PatchAllHarmonyMethods();

            Logger.LogInfo($"Plugin BitchlandAllNPCsAreAlwaysFuckableBepInEx BepInEx is loaded!");
        }

        public static void PatchAllHarmonyMethods()
        {
            if (!enableThisMod)
            {
                return;
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(int_Person), "DefaultTalk_options", "DefaultTalk_options", true, false);
                PatchHarmonyMethodUnity(typeof(int_Person), "EndTheChat", "EndTheChat", false, true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        public static void PatchHarmonyMethodUnity(Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            string uniqueId = "com.wolfitdm.BitchlandAllNPCsAreAlwaysFuckableBepInEx";
            Type uniqueType = typeof(BitchlandAllNPCsAreAlwaysFuckableBepInEx);

            // Create a new Harmony instance with a unique ID
            var harmony = new Harmony(uniqueId);

            if (originalClass == null)
            {
                Logger.LogInfo($"GetType originalClass == null");
                return;
            }

            MethodInfo patched = null;

            try
            {
                patched = AccessTools.Method(uniqueType, patchedMethodName);
            }
            catch (Exception ex)
            {
                patched = null;
            }

            if (patched == null)
            {
                Logger.LogInfo($"AccessTool.Method patched {patchedMethodName} == null");
                return;

            }

            // Or apply patches manually
            MethodInfo original = null;

            try
            {
                if (parameters == null)
                {
                    original = AccessTools.Method(originalClass, originalMethodName);
                }
                else
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
            }
            catch (AmbiguousMatchException ex)
            {
                Type[] nullParameters = new Type[] { };
                try
                {
                    if (patched == null)
                    {
                        parameters = nullParameters;
                    }

                    ParameterInfo[] parameterInfos = patched.GetParameters();

                    if (parameterInfos == null || parameterInfos.Length == 0)
                    {
                        parameters = nullParameters;
                    }

                    List<Type> parametersN = new List<Type>();

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo parameterInfo = parameterInfos[i];

                        if (parameterInfo == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name.StartsWith("__"))
                        {
                            continue;
                        }

                        Type type = parameterInfos[i].ParameterType;

                        if (type == null)
                        {
                            continue;
                        }

                        parametersN.Add(type);
                    }

                    parameters = parametersN.ToArray();
                }
                catch (Exception ex2)
                {
                    parameters = nullParameters;
                }

                try
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
                catch (Exception ex2)
                {
                    original = null;
                }
            }
            catch (Exception ex)
            {
                original = null;
            }

            if (original == null)
            {
                Logger.LogInfo($"AccessTool.Method original {originalMethodName} == null");
                return;
            }

            HarmonyMethod patchedMethod = new HarmonyMethod(patched);
            var prefixMethod = usePrefix ? patchedMethod : null;
            var postfixMethod = usePostfix ? patchedMethod : null;

            harmony.Patch(original,
                prefix: prefixMethod,
                postfix: postfixMethod);
        }

        private static List<string> persons = new List<string>();
        private static Dictionary<string,int> personsDict = new Dictionary<string,int>();

        private static void addPersonToDict(Person person)
        {
            string name = person.Name;
            if (!persons.Contains(name))
            {
                persons.Add(name);
                
                int psState = 0;

                try
                {
                    psState = (int)person.State;
                } catch (Exception e) {
                    try
                    {
                        psState = (int)Person_State.Work;
                    }
                    catch (Exception e2)
                    {
                        psState = 1;
                    }
                }

                if(!personsDict.ContainsKey(name))
                {
                    personsDict.Add(name, psState);
                }

                person.State = Person_State.Free;
            }
        }

        private static void setPersonStateToWork(Person person)
        {
            string name = person.Name;
            if (persons.Contains(name))
            {
                if (personsDict.ContainsKey(name))
                {
                    Person_State ps = Person_State.Work;
                    
                    try
                    {
                        ps = (Person_State)personsDict[name];
                    } catch (Exception ex) {
                        ps = Person_State.Work;
                    }

                    person.State = ps;

                    personsDict.Remove(name);
                }
                persons.Remove(name);
            }
        }
        public static bool DefaultTalk_options(object __instance)
        {
            if (!enableThisMod)
            {
                return true;
            }

            try
            {
                int_Person _this = (int_Person)__instance;

                if (_this.ThisPerson == null || _this.ThisPerson.Name == null)
                {
                    return true;
                }

                if (_this.ThisPerson.State != Person_State.Free)
                {
                    addPersonToDict(_this.ThisPerson);
                }

            } catch (Exception ex)
            {
            }

            return true;
        }
        public static void EndTheChat(object __instance)
        {
            if (!enableThisMod)
            {
                return;
            }

            try
            {
                int_Person _this = (int_Person)__instance;

                if (_this.ThisPerson == null || _this.ThisPerson.Name == null)
                {
                    return;
                }

                setPersonStateToWork(_this.ThisPerson);

            }
            catch (Exception ex)
            {
            }

            return;
        }
    }
}
