using System;
using System.Collections.Generic;
using DevInterface;
using On;
using System.IO;
using Partiality.Modloader;
using UnityEngine;
using RWCustom;
using System.Collections;

public class UrbanAbyss : PartialityMod
{
    public static PowerCycle powerCycle;

    public static WWW generatorPowerUp;
    public static WWW generatorPowerDown;
    public static WWW generatorPowerUpFar;
    public static WWW generatorPowerDownFar;

    public override void Init()
    {
        this.ModID = "UrbanAbyss";
        Version = "0003";
        author = "Dracenis";
    }
    public override void OnLoad()
    {
        base.OnLoad();
        generatorPowerUp = new WWW("file://" + string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "LoadedSoundEffects",
                Path.DirectorySeparatorChar,
                "GeneratorPowerUp",
                ".wav"
            }));
        generatorPowerDown = new WWW("file://" + string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "LoadedSoundEffects",
                Path.DirectorySeparatorChar,
                "GeneratorPowerDown",
                ".wav"
            }));
        generatorPowerUpFar = new WWW("file://" + string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "LoadedSoundEffects",
                Path.DirectorySeparatorChar,
                "GeneratorPowerUpFar",
                ".wav"
            }));
        generatorPowerDownFar = new WWW("file://" + string.Concat(new object[]
            {
                Custom.RootFolderDirectory(),
                "Assets",
                Path.DirectorySeparatorChar,
                "Futile",
                Path.DirectorySeparatorChar,
                "Resources",
                Path.DirectorySeparatorChar,
                "LoadedSoundEffects",
                Path.DirectorySeparatorChar,
                "GeneratorPowerDownFar",
                ".wav"
            }));
        On.Room.Loaded += Room_LoadedHK;
        On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRepHK;
        On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyDataHK;
        On.RainCycle.ctor += RainCycle_ctorHK;
        On.RainCycle.Update += RainCycle_UpdateHK;
        On.SoundLoader.GetAudioClip += SoundLoader_GetAudioClip;
        On.LightSource.DrawSprites += LightSource_DrawSprites;
    }

    private void LightSource_DrawSprites(On.LightSource.orig_DrawSprites orig, LightSource self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        orig(self, sLeaser, rCam, timeStacker, camPos);
        if (!self.slatedForDeletetion && !self.fadeWithSun)
        {
            if (self.room != null && self.room.roomSettings.GetEffectAmount(EnumExt_UrbanAbyss.PowerCycle) > 0f && powerCycle != null)
            {
                if (powerCycle.linearPower < 1f && powerCycle.linearPower > 0f)
                {
                    float flicker = Mathf.Clamp(powerCycle.linearPower * 2f - Mathf.PerlinNoise(self.Pos.x*100f, self.Pos.y*100f),0f,1f);
                    flicker = flicker+(Mathf.Pow(flicker,0.1f)*(1f-flicker)*Mathf.Pow(Mathf.Sin(30f/(flicker+0.4f)),2f));
                    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        sLeaser.sprites[i].alpha = flicker * sLeaser.sprites[i].alpha;
                    }
                }
                else if (powerCycle.linearPower == 0f)
                {
                    for (int i = 0; i < sLeaser.sprites.Length; i++)
                    {
                        sLeaser.sprites[i].alpha = 0f;
                    }
                }
            }
        }
    }

    private AudioClip SoundLoader_GetAudioClip(On.SoundLoader.orig_GetAudioClip orig, SoundLoader self, int i)
    {
        if (i == self.GetSoundData(EnumExt_UrbanAbyss.GeneratorPowerUp).audioClip)
        {
            if (generatorPowerUp.audioClip.isReadyToPlay)
            {
                AudioClip clip = generatorPowerUp.GetAudioClip(false);
                clip.name = "GeneratorPowerUp";
                Debug.Log(clip != null);
                return clip;
            }
        }
        else if (i == self.GetSoundData(EnumExt_UrbanAbyss.GeneratorPowerDown).audioClip)
        {
            if (generatorPowerDown.audioClip.isReadyToPlay)
            {
                AudioClip clip = generatorPowerDown.GetAudioClip(false);
                clip.name = "GeneratorPowerDown";
                Debug.Log(clip != null);
                return clip;
            }
        }
        else if (i == self.GetSoundData(EnumExt_UrbanAbyss.GeneratorPowerUpFar).audioClip)
        {
            if (generatorPowerUpFar.audioClip.isReadyToPlay)
            {
                AudioClip clip = generatorPowerUpFar.GetAudioClip(false);
                clip.name = "GeneratorPowerUpFar";
                Debug.Log(clip != null);
                return clip;
            }
        }
        else if (i == self.GetSoundData(EnumExt_UrbanAbyss.GeneratorPowerDownFar).audioClip)
        {
            if (generatorPowerDownFar.audioClip.isReadyToPlay)
            {
                AudioClip clip = generatorPowerDownFar.GetAudioClip(false);
                clip.name = "GeneratorPowerDownFar";
                Debug.Log(clip != null);
                return clip;
            }
        }
        return orig(self, i);
    }


    private void RainCycle_ctorHK(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
    {
        orig(self, world, minutes);
        UrbanAbyss.powerCycle = null;
    }

    private void RainCycle_UpdateHK(On.RainCycle.orig_Update orig, RainCycle self)
    {
        orig(self);
        if (UrbanAbyss.powerCycle != null)
        {
            UrbanAbyss.powerCycle.Update();
        }
    }

    private void PlacedObject_GenerateEmptyDataHK(On.PlacedObject.orig_GenerateEmptyData orig, PlacedObject self)
    {
        orig(self);
        if (self.type == EnumExt_UrbanAbyss.ColoredLightBeam)
        {
            self.data = new ColoredLightBeam.ColoredLightBeamData(self);
        }
    }

    private void ObjectsPage_CreateObjRepHK(On.DevInterface.ObjectsPage.orig_CreateObjRep orig, ObjectsPage self, PlacedObject.Type tp, PlacedObject pObj)
    {
        if (tp == EnumExt_UrbanAbyss.ColoredLightBeam)
        {
            if (pObj == null)
            {
                pObj = new PlacedObject(tp, null);
                pObj.pos = self.owner.room.game.cameras[0].pos + Vector2.Lerp(self.owner.mousePos, new Vector2(-683f, 384f), 0.25f) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.2f;
                self.RoomSettings.placedObjects.Add(pObj);
            }
            PlacedObjectRepresentation placedObjectRepresentation = new ColoredLightBeamRepresentation(self.owner, tp.ToString() + "_Rep", self, pObj);
            if (placedObjectRepresentation != null)
            {
                self.tempNodes.Add(placedObjectRepresentation);
                self.subNodes.Add(placedObjectRepresentation);
            }
            return;
        }
        orig(self, tp, pObj);
    }

    public void Room_LoadedHK(On.Room.orig_Loaded orig, Room self)
    {
        orig(self); // same as this.orig_Loaded();
                    // then all the rest of the code, but using `self` instead of `this`
        for (int l = 0; l < self.roomSettings.placedObjects.Count; l++)
        {
            if (self.roomSettings.placedObjects[l].active)
            {
                if (self.roomSettings.placedObjects[l].type == EnumExt_UrbanAbyss.ColoredLightBeam)
                {
                    ColoredLightBeam LB = new ColoredLightBeam(self.roomSettings.placedObjects[l]);
                    self.AddObject(LB);
                    LB.colorFromEnvironment = ((self.roomSettings.placedObjects[l].data as ColoredLightBeam.ColoredLightBeamData).colorType == ColoredLightBeam.ColoredLightBeamData.ColorType.Environment);
                    LB.effectColor = Math.Max(-1, (self.roomSettings.placedObjects[l].data as ColoredLightBeam.ColoredLightBeamData).colorType - ColoredLightBeam.ColoredLightBeamData.ColorType.EffectColor1);
                }
            }
        }
        for (int k = 0; k < self.roomSettings.effects.Count; k++)
        { 
            if (self.roomSettings.effects[k].type == EnumExt_UrbanAbyss.PowerCycle)
            {
                if (UrbanAbyss.powerCycle == null || UrbanAbyss.powerCycle.game == null)
                {
                    UrbanAbyss.powerCycle = new PowerCycle(self);
                }
            }
        }
    }
}
