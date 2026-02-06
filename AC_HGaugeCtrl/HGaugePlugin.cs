#nullable enable
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using H;

using LogLevel = BepInEx.Logging.LogLevel;


namespace AC_HGaugeCtrl
{
	[BepInProcess(PROCESS_NAME)]
	[BepInPlugin(GUID, PLUGIN_NAME, VERSION)]
	public partial class HGaugePlugin : BasePlugin
	{
		#region PLUGIN_INFO

		/*PLUGIN INFO*/
		public const string PLUGIN_NAME = "AC_HGaugeCtrl";
		public const string COPYRIGHT = "";
		public const string COMPANY = "https://github.com/TonWonton/AC_HGaugeCtrl";

		public const string PROCESS_NAME = "Aicomi";
		public const string GUID = "AC_HGaugeCtrl";
		public const string VERSION = "1.0.0";

		#endregion



		/*VARIABLES*/
		//Instance
		public static HGaugePlugin Instance { get; private set; } = null!;
		private static ManualLogSource _log = null!;



		/*METHODS*/
		public static bool TryGetHGaugeComponent([MaybeNullWhen(false)] out HGaugeComponent hGaugeComponent)
		{
			hGaugeComponent = HGaugeComponent.Instance;
			return hGaugeComponent != null;
		}

		public static HGaugeComponent GetOrAddHGaugeComponent()
		{
			HGaugeComponent? hGaugeComponent = HGaugeComponent.Instance;

			if (hGaugeComponent != null)
			{
				return hGaugeComponent;
			}
			else
			{
				return Instance.AddComponent<HGaugeComponent>();
			}
		}



		/*EVENT HANDLING*/
		private void OnSettingsChanged(object? sender, EventArgs args)
		{
			if (TryGetHGaugeComponent(out HGaugeComponent? hGaugeComponent))
			{
				hGaugeComponent.SetPriorities();
				hGaugeComponent.UpdateGaugeGain();
			}
		}



		/*PLUGIN LOAD*/
		public override void Load()
		{
			//Instance
			Instance = this;
			_log = Log;

			//Initialization
			InitializeConfig();

			//Create hooks
			Harmony.CreateAndPatchAll(typeof(HGaugeComponent.Hooks), GUID);
			Logging.Info("Loaded");
		}



		//Logging
		public static class Logging
		{
			public static void Log(LogLevel level, string message)
			{
				_log.Log(level, message);
			}

			public static void Fatal(string message)
			{
				_log.LogFatal(message);
			}

			public static void Error(string message)
			{
				_log.LogError(message);
			}

			public static void Warning(string message)
			{
				_log.LogWarning(message);
			}

			public static void Message(string message)
			{
				_log.LogMessage(message);
			}

			public static void Info(string message)
			{
				_log.LogInfo(message);
			}

			public static void Debug(string message)
			{
				_log.LogDebug(message);
			}
		}
	}
}