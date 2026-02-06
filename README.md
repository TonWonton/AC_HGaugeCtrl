# AC_HGaugeCtrl

## Description
HGaugeCtrl plugin / MOD for `Aicomi` / `アイコミ`. Changes how the HGauge works. Changes how the HGauge works. Auto climax, gauge fill rates, speed/gauge hit scaling, etc.

## Prerequisites
- [BepInEx Unity Il2Cpp](https://github.com/BepInEx/BepInEx)
- [BepisPlugins](https://github.com/IllusionMods/BepisPlugins)
- [BepInEx.ConfigurationManager Il2Cpp](https://github.com/BepInEx/BepInEx.ConfigurationManager)

#### OR
- [AC-HF_Patch](https://github.com/ManlyMarco/AC-HF_Patch)

## Installation
1. Install prerequisites
2. Download from releases
3. Extract into game folder (.DLL file should be in `/GAME_FOLDER/BepInEx/plugins/`)

## Features
- Auto climax
  - Configurable priority for auto climax
- Set custom gauge fill rates for both female and male
  - Enable/disable speed scaling
  - Configurable gauge hit fill rates
- Remember animation speed when changing positions or loop types

## Notes

- Not all positions remembers the speed if the new position stops the animation/loop
