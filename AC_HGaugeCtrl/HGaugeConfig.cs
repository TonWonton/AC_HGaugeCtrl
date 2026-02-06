#nullable enable
using System;
using System.ComponentModel;
using BepInEx.Configuration;

using Logging = AC_HGaugeCtrl.HGaugePlugin.Logging;
using Order = AC_HGaugeCtrl.ConfigurationManagerAttributes;
using Range = BepInEx.Configuration.AcceptableValueRange<float>;
using Desc = BepInEx.Configuration.ConfigDescription;


namespace AC_HGaugeCtrl
{
	public enum FinishPriority
	{
		[Description("Together, inside, outside")]
		TogetherInsideOutside521 = 0,

		[Description("Together, outside, inside")]
		TogetherOutsideInside512 = 1,

		[Description("Inside, together, outside")]
		InsideTogetherOutside251 = 2,

		[Description("Inside, outside, together")]
		InsideOutsideTogether215 = 3,

		[Description("Outside, together, inside")]
		OutsideTogetherInside152 = 4,

		[Description("Outside, inside, together")]
		OutsideInsideTogether125 = 5
	}

	public enum FinishPriorityHoushi
	{
		[Description("Swallow, spit, outside")]
		SwallowSpitOutside341 = 0,

		[Description("Swallow, outside, spit")]
		SwallowOutsideSpit314 = 1,

		[Description("Spit, swallow, outside")]
		SpitSwallowOutside431 = 2,

		[Description("Spit, outside, swallow")]
		SpitOutsideSwallow413 = 3,

		[Description("Outside, swallow, spit")]
		OutsideSwallowSpit134 = 4,

		[Description("Outside, spit, swallow")]
		OutsideSpitSwallow143 = 5
	}
	public partial class HGaugePlugin
	{
		//Climax together
		public const string CLIMAX = "Climax";
		public static ConfigEntry<bool> femaleFinishTogether = null!;
		public static ConfigEntry<bool> maleAutoFinish = null!;
		public static ConfigEntry<FinishPriority> finishPriority = null!;
		public static ConfigEntry<FinishPriorityHoushi> finishPriorityHoushi = null!;

		//Gauge
		public const string GAUGE = "Gauge";
		public static ConfigEntry<float> gaugeSpeedMultiplierF = null!;
		public static ConfigEntry<float> gaugeHitMultiplierF = null!;
		public static ConfigEntry<float> gaugeSpeedMultiplierM = null!;
		public static ConfigEntry<float> gaugeHitMultiplierM = null!;

		//Speed scaling
		public const string SPEED = "Speed";
		public static ConfigEntry<bool> rememberLoopSpeed = null!;
		public static ConfigEntry<bool> speedScaling = null!;
		public static ConfigEntry<bool> speedScalingConsiderLoopType = null!;
		public static ConfigEntry<float> gaugeSpeedScalingWeightF = null!;
		public static ConfigEntry<float> gaugeSpeedScalingWeightM = null!;

		//Description
		private static Range Range(float min, float max) { return new Range(min, max); }
		private static Desc Desc(int order, string? description = null) { return new Desc(description ?? string.Empty, null, new Order() { Order = order }); }
		private static Desc RangeDesc(Range range, int order, string? description = null) { return new Desc(description ?? string.Empty, range, new Order() { Order = order }); }

		//Initialization
		private void InitializeConfig()
		{
			femaleFinishTogether = Config.Bind(CLIMAX, "Female finish together", true, Desc(0));
			maleAutoFinish = Config.Bind(CLIMAX, "Male auto finish", true, Desc(-1));
			finishPriority = Config.Bind(CLIMAX, "Finish priority", FinishPriority.TogetherInsideOutside521, Desc(-2));
			finishPriorityHoushi = Config.Bind(CLIMAX, "Finish priority (Houshi)", FinishPriorityHoushi.SwallowSpitOutside341, Desc(-3));

			gaugeSpeedMultiplierF = Config.Bind(GAUGE, "Female base gauge speed multiplier", 0.68f, RangeDesc(Range(-6f, 6f), 0));
			gaugeHitMultiplierF = Config.Bind(GAUGE, "Female gauge hit multiplier", 2.2f, RangeDesc(Range(-6f, 6f), -1));
			gaugeSpeedMultiplierM = Config.Bind(GAUGE, "Male base gauge speed multiplier", 1f, RangeDesc(Range(-6f, 6f), -2));
			gaugeHitMultiplierM = Config.Bind(GAUGE, "Male gauge hit multiplier", 1.1f, RangeDesc(Range(-6f, 6f), -3));

			rememberLoopSpeed = Config.Bind(SPEED, "Remember loop speed", true, Desc(0));
			speedScaling = Config.Bind(SPEED, "Scale gauge gain with speed", false, Desc(-1));
			speedScalingConsiderLoopType = Config.Bind(SPEED, "Consider loop type for speed scaling", true, Desc(-2));
			gaugeSpeedScalingWeightF = Config.Bind(SPEED, "Female gauge speed scaling weight", 1.04f, RangeDesc(Range(-6f, 6f), -3));
			gaugeSpeedScalingWeightM = Config.Bind(SPEED, "Male gauge speed scaling weight", 1.16f, RangeDesc(Range(-6f, 6f), -4));

			femaleFinishTogether.SettingChanged += OnSettingsChanged;
			maleAutoFinish.SettingChanged += OnSettingsChanged;
			finishPriority.SettingChanged += OnSettingsChanged;
			finishPriorityHoushi.SettingChanged += OnSettingsChanged;

			speedScaling.SettingChanged += OnSettingsChanged;
			speedScalingConsiderLoopType.SettingChanged += OnSettingsChanged;
			gaugeSpeedMultiplierF.SettingChanged += OnSettingsChanged;
			gaugeHitMultiplierF.SettingChanged += OnSettingsChanged;
			gaugeSpeedScalingWeightF.SettingChanged += OnSettingsChanged;
			gaugeSpeedMultiplierM.SettingChanged += OnSettingsChanged;
			gaugeHitMultiplierM.SettingChanged += OnSettingsChanged;
			gaugeSpeedScalingWeightM.SettingChanged += OnSettingsChanged;
		}
	}
}