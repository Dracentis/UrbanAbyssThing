using System;
using System.Collections.Generic;
using DevInterface;
using On;
using Partiality.Modloader;
using UnityEngine;
using RWCustom;


public class UrbanAbyss : PartialityMod
{
    public static PowerCycle powerCycle;

    public override void Init()
    {
        this.ModID = "UrbanAbyss";
        Version = "0003";
        author = "Dracenis";
    }
    public override void OnLoad()
    {
        base.OnLoad();
        On.Room.Loaded += Room_LoadedHK;
        On.DevInterface.ObjectsPage.CreateObjRep += ObjectsPage_CreateObjRepHK;
        On.PlacedObject.GenerateEmptyData += PlacedObject_GenerateEmptyDataHK;
        On.RainCycle.ctor += RainCycle_ctorHK;
        On.RainCycle.Update += RainCycle_UpdateHK;
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
                if (UrbanAbyss.powerCycle == null)
                {
                    UrbanAbyss.powerCycle = new PowerCycle(self);
                }
            }
        }
    }
}
