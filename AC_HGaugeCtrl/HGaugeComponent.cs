#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using HarmonyLib;
using H;

using Logging = AC_HGaugeCtrl.HGaugePlugin.Logging;
using Il2CppCollections = Il2CppSystem.Collections.Generic;
using ButtonClickedEvent = UnityEngine.UI.Button.ButtonClickedEvent;
using JetBrains.Annotations;


namespace AC_HGaugeCtrl
{
	public class HGaugeComponent : MonoBehaviour
	{
		/*VARIABLES*/
		//Instance
		public static HGaugeComponent? Instance { get; private set; }

		//HScene
		private HScene _hScene = null!;
		private FlagControl _ctrlFlag = null!;
		private FinishCategory _finishCategory = null!;
		private Gauge _femaleGauge = null!;
		private Gauge _maleGauge = null!;
		private Toggle _femaleFinishLock = null!;
		private Toggle _maleFinishLock = null!;
		private List<Button> _finishButtonList = new List<Button>();
		private int[] _finishPriorities = new int[3];
		private int[] _finishPrioritiesHoushi = new int[3];


		private int _mode = -1;
		private int _modeCtrl = -1;
		private int _loopType = -1;
		private float _speed = 0f;
		private bool _isNowFFeelProc = false;
		private bool _isNowMFeelProc = false;
		private bool _isNowInLoop = false;
		private bool _isNowFemaleGaugeLock = false;
		private bool _isNowMaleGaugeLock = false;
		private bool _isPositionChangeProc = false;

		private float _gaugeMultiplierF = 0.68f;
		private float _gaugeHitGainMultiplierF = 1.088f;
		private float _gaugeMultiplierM = 1f;
		private float _gaugeHitGainMultiplierM = 1.1f;
		private float _positionChangeProcSpeed = 0f;


		/*METHODS*/
		public void SetPriorities()
		{
			//Set priorities
			_finishPriorities.SetFinishPrioritiesFromFinishPriority(HGaugePlugin.finishPriority.Value);
			_finishPrioritiesHoushi.SetFinishPrioritiesFromFinishPriorityHoushi(HGaugePlugin.finishPriorityHoushi.Value);
		}

		public void UpdateGaugeGain()
		{
			//Cache gauge gain values
			float gaugeSpeedMultiplierF = HGaugePlugin.gaugeSpeedMultiplierF.Value;
			_gaugeMultiplierF = gaugeSpeedMultiplierF;
			_gaugeHitGainMultiplierF = gaugeSpeedMultiplierF * HGaugePlugin.gaugeHitMultiplierF.Value;

			float gaugeSpeedMultiplierM = HGaugePlugin.gaugeSpeedMultiplierM.Value;
			_gaugeMultiplierM = gaugeSpeedMultiplierM;
			_gaugeHitGainMultiplierM = gaugeSpeedMultiplierM * HGaugePlugin.gaugeHitMultiplierM.Value;
		}

		private void OnFFeelProc()
		{
			_isNowFFeelProc = true;
			_isNowMFeelProc = false;
		}

		private void OnMFeelProc()
		{
			_isNowMFeelProc = true;
			_isNowFFeelProc = false;
		}

		private void OnBothFeelProc()
		{
			_isNowFFeelProc = true;
			_isNowMFeelProc = true;
		}

		private void OnFeelEndProc()
		{
			_isNowFFeelProc = false;
			_isNowMFeelProc = false;
		}

		private void OnPositionChangeProc()
		{
			_isPositionChangeProc = true;
			_positionChangeProcSpeed = _ctrlFlag.Speed;
		}

		private void OnPreUpdate()
		{
			//Process mode
			int mode = _hScene._mode;
			int modeCtrl = _hScene._modeCtrl;
			if (mode != _mode)
			{
				_mode = mode;
				_modeCtrl = modeCtrl;

				switch (mode)
				{
					case 0: OnFFeelProc(); break; //Caress
					case 1: OnMFeelProc(); break; //Service
					case 2: OnBothFeelProc(); break; //Insert
					case 4: OnFFeelProc(); break; //Masturbation
					case 6: OnFFeelProc(); break; //Les
					case 7: //1M2F
					{
						switch (modeCtrl)
						{
							case 1: OnMFeelProc(); break; //Service
							case 2: OnMFeelProc(); break; //Service
							case 4: OnBothFeelProc(); break; //Insert
							default: OnBothFeelProc(); Logging.Warning($"Unknown feel proc for mode {mode} modeCtrl {modeCtrl}"); break; //Default to both
						}
						break;
					}
					case 9: //5p
					{
						switch (modeCtrl)
						{
							case 1: OnMFeelProc(); break; //Service
							case 3: OnBothFeelProc(); break; //Insert
							default: OnBothFeelProc(); Logging.Warning($"Unknown feel proc for mode {mode} modeCtrl {modeCtrl}"); break; //Default to both
						}

						break;
					}

					default: OnBothFeelProc(); Logging.Warning($"Unknown feel proc for mode {mode} modeCtrl {modeCtrl}"); break; //Default to both
				}
			}

			//Process loop type
			bool rememberLoopSpeed = HGaugePlugin.rememberLoopSpeed.Value;
			FlagControl ctrlFlag = _ctrlFlag;
			int loopType = ctrlFlag.LoopType;
			int previousLoopType = _loopType;
			float previousSpeed = _speed;
			if (loopType != _loopType)
			{
				_loopType = loopType;

				if (loopType == -1) _isNowInLoop = false;
				else
				{
					_isNowInLoop = true;

					if (rememberLoopSpeed)
					{
						if (loopType == 0)
						{
							if (previousLoopType == 0) ctrlFlag.Speed = previousSpeed;
							else if (previousLoopType == 1) ctrlFlag.Speed = Mathf.Clamp(previousSpeed, 0f, 0.99999999f);
							else if (previousLoopType == 2) ctrlFlag.Speed = previousSpeed;
						}
						else if (loopType == 1)
						{
							//No transition into loop type 1
						}
						else if (loopType == 2)
						{
							if (previousLoopType == 1)
							{
								ctrlFlag.Speed = previousSpeed - 1f;
							}
							else if (previousLoopType == 0)
							{
								ctrlFlag.Speed = previousSpeed;
							}
						}
					}
				}
			}



			//Process position change proc
			else if (rememberLoopSpeed && _isPositionChangeProc)
			{
				ctrlFlag.Speed = _positionChangeProcSpeed;
				_isPositionChangeProc = false;
			}


			//Process speed
			float speed = ctrlFlag.Speed;
			float effectiveSpeedMultiplierF = 1f;
			float effectiveSpeedMultiplierM = 1f;
			_speed = speed;

			if (HGaugePlugin.speedScaling.Value)
			{
				const float min = 0.333f;
				float speedScalingWeightF = HGaugePlugin.gaugeSpeedScalingWeightF.Value;
				float speedScalingWeightM = HGaugePlugin.gaugeSpeedScalingWeightM.Value;
				if (HGaugePlugin.speedScalingConsiderLoopType.Value)
				{
					//Divide by 3 if consider loop type since loop type == 1 speed goes from 1f to 2f
					if (loopType != 2)
					{
						effectiveSpeedMultiplierF = ((speedScalingWeightF - min) * Mathf.Divide(speed, 3f)) + min;
						effectiveSpeedMultiplierM = ((speedScalingWeightM - min) * Mathf.Divide(speed, 3f)) + min;
					}
					else
					{
						effectiveSpeedMultiplierF = ((speedScalingWeightF - min) * speed) + min;
						effectiveSpeedMultiplierM = ((speedScalingWeightM - min) * speed) + min;
					}
				}
				else
				{
					//Subtract by 1f if not considering loop type since loop type == 1 speed goes from 1f to 2f
					if (loopType == 1)
					{
						effectiveSpeedMultiplierF = ((speedScalingWeightF - min) * (speed - 1f)) + min;
						effectiveSpeedMultiplierM = ((speedScalingWeightM - min) * (speed - 1f)) + min;
					}
					else
					{
						effectiveSpeedMultiplierF = ((speedScalingWeightF - min) * speed) + min;
						effectiveSpeedMultiplierM = ((speedScalingWeightM - min) * speed) + min;
					}
				}
			}

			//Process lock
			_isNowFemaleGaugeLock = _femaleFinishLock.isOn;
			_isNowMaleGaugeLock = _maleFinishLock.isOn;

			//Update
			float deltaTime = Time.deltaTime;
			Gauge femaleGauge = _femaleGauge;
			Gauge maleGauge = _maleGauge;
			List<Button> finishButtonList = _finishButtonList;

			bool isNowFFeelProc = _isNowFFeelProc;
			bool isNowMFeelProc = _isNowMFeelProc;
			//bool isBothFeelProc = isNowFFeelProc && isNowMFeelProc;
			bool isNowFemaleGaugeLock = _isNowFemaleGaugeLock;
			bool isNowMaleGaugeLock = _isNowMaleGaugeLock;
			bool isNowFemaleFeelNoLock = isNowFFeelProc && isNowFemaleGaugeLock == false;
			bool isNowMaleFeelNoLock = isNowMFeelProc && isNowMaleGaugeLock == false;

			if (_isNowInLoop)
			{
				//Add to gauge
				if (isNowFFeelProc && isNowFemaleGaugeLock == false)
				{
					if (femaleGauge.Hit) femaleGauge.Value += 0.03f * _gaugeHitGainMultiplierF * effectiveSpeedMultiplierF * deltaTime;
					else femaleGauge.Value += 0.03f * _gaugeMultiplierF * effectiveSpeedMultiplierF * deltaTime;
				}

				if (isNowMFeelProc && isNowMaleGaugeLock == false)
				{
					if (maleGauge.Hit) maleGauge.Value += 0.03f * _gaugeHitGainMultiplierM * effectiveSpeedMultiplierM * deltaTime;
					else maleGauge.Value += 0.03f * _gaugeMultiplierM * effectiveSpeedMultiplierM * deltaTime;
				}

				//Check finish
				if (HGaugePlugin.maleAutoFinish.Value && isNowMaleFeelNoLock && maleGauge.Value >= 0.99f)
				{
					PooledObject<Il2CppCollections.List<bool>> pooledObject = _finishCategory.GetActiveButton(out Il2CppCollections.List<bool> activeButtonList);

					try
					{
						//Try trigger normal finish
						if (isNowFFeelProc)
						{
							int[] finishPriorities = _finishPriorities;
							if (activeButtonList[finishPriorities[0]]) finishButtonList[finishPriorities[0]].onClick.Invoke();
							else if (activeButtonList[finishPriorities[1]]) finishButtonList[finishPriorities[1]].onClick.Invoke();
							else if (activeButtonList[finishPriorities[2]]) finishButtonList[finishPriorities[2]].onClick.Invoke();
						}

						//Try trigger houshi finish
						else
						{
							int[] finishPrioritiesHoushi = _finishPrioritiesHoushi;
							if (activeButtonList[finishPrioritiesHoushi[0]]) finishButtonList[finishPrioritiesHoushi[0]].onClick.Invoke();
							else if (activeButtonList[finishPrioritiesHoushi[1]]) finishButtonList[finishPrioritiesHoushi[1]].onClick.Invoke();
							else if (activeButtonList[finishPrioritiesHoushi[2]]) finishButtonList[finishPrioritiesHoushi[2]].onClick.Invoke();
						}
					}

					finally { pooledObject.System_IDisposable_Dispose(); }
				}

				else if (HGaugePlugin.femaleFinishTogether.Value && isNowFemaleFeelNoLock && femaleGauge.Value >= 0.99f)
				{
					Button finishSameButton = finishButtonList[5];
					if (_finishCategory._finishVisibles.Contains(FinishCategory.FinishKind.Same) && finishSameButton.isActiveAndEnabled) finishSameButton.onClick.Invoke();
				}
			}
		}



		//Initialization
		private void Start()
		{
			SetPriorities();
			UpdateGaugeGain();

			_ctrlFlag.SpeedGuageRate = 0f;
		}

		private void OnDestroy()
		{
			if (Instance == this)
			{
				Instance = null;
			}
		}

		private void Awake()
		{
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Destroy(this);
			}
		}



		//Component specific hooks
		public static class Hooks
		{
			[HarmonyPostfix]
			[HarmonyPatch(typeof(HScene), nameof(HScene.SetCameraLoad))]
			public static void HScenePostSetCameraLoad()
			{
				if (HGaugePlugin.TryGetHGaugeComponent(out HGaugeComponent? hGaugeComponent))
				{
					hGaugeComponent.OnPositionChangeProc();
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(HScene), nameof(HScene.Update_))]
			public static void HScenePreUpdate()
			{
				if (HGaugePlugin.TryGetHGaugeComponent(out HGaugeComponent? hGaugeComponent))
				{
					hGaugeComponent.OnPreUpdate();
				}
			}

			[HarmonyPrefix]
			[HarmonyPatch(typeof(HResult), "EvaluationResult")]
			[HarmonyPatch(typeof(HScene), "RestoreActors")]
			public static void HScenePreEnd()
			{
				if (HGaugePlugin.TryGetHGaugeComponent(out HGaugeComponent? hGaugeComponent))
				{
					Destroy(hGaugeComponent);
				}
			}

			[HarmonyPostfix]
			[HarmonyPatch(typeof(ProcBase), nameof(ProcBase.Initialize))]
			public static void ProcBasePostInitialize(ProcBase __instance)
			{
				if (Instance == null)
				{
					HGaugeComponent hGaugeComponent = HGaugePlugin.GetOrAddHGaugeComponent();
					hGaugeComponent._hScene = __instance._hScene;
					hGaugeComponent._ctrlFlag = __instance._hScene.CtrlFlag;
					hGaugeComponent._finishCategory = __instance._hScene._sprite.FinishCategory;
					hGaugeComponent._femaleGauge = __instance._gaugeF;
					hGaugeComponent._maleGauge = __instance._gaugeM;
					hGaugeComponent._femaleFinishLock = __instance._gaugeF._tglLock;
					hGaugeComponent._maleFinishLock = __instance._gaugeM._tglLock;

					Il2CppCollections.List<Button> finishButtonList = __instance._sprite.FinishCategory.LstButton;
					int finishButtonCount = finishButtonList.Count;
					for (int i = 0; i < finishButtonCount; i++)
					{
						hGaugeComponent._finishButtonList.Add(finishButtonList[i]);
					}
				}
			}
		}
	}
}