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

            Harmony.CreateAndPatchAll(typeof(BitchlandAllNPCsAreAlwaysFuckableBepInEx));

            Logger.LogInfo($"Plugin BitchlandAllNPCsAreAlwaysFuckableBepInEx BepInEx is loaded!");
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

        [HarmonyPatch(typeof(int_Person), "DefaultTalk_options")]
        [HarmonyPrefix] // call after the original method is called
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

        [HarmonyPatch(typeof(int_Person), "EndTheChat")]
        [HarmonyPostfix] // call after the original method is called
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
