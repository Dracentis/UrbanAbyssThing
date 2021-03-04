using System;
using RWCustom;
using UnityEngine;

public class PowerCycle : UpdatableAndDeletable
{
	public PowerCycle(Room room) : base()
	{
		//Range of cycle length
		this.cycleMin = 20;
		this.cycleMax = 30;

		//RainWorldGame
		this.game = room.game;
		this.counter = 10;
		this.progress = 0f;

		this.on = (UnityEngine.Random.value < 0.5f);
		this.from = ((!this.on) ? 0f : 1f);
		this.to = ((!this.on) ? 0f : 1f);
	}

	public void Update()
	{
		if (this.game == null || this.game.cameras == null)
			return;
		this.counter--;
		if (this.counter < 1)
		{
			this.on = !this.on;
			this.counter = UnityEngine.Random.Range(this.cycleMin * 40, this.cycleMax * 40);
			this.from = this.to;
			this.to = ((!this.on) ? 0f : 1f);
			this.progress = 0f;

			// This next block is for playing a sound when powering on or off
			for (int i = 0; i < this.game.cameras.Length; i++)
			{
				if (this.game.cameras[i].room != null && this.game.cameras[i].room.roomSettings.GetEffectAmount(EnumExt_UrbanAbyss.PowerCycle) > 0f)
				{
					this.game.cameras[i].room.PlaySound((!this.on) ? EnumExt_UrbanAbyss.GeneratorPowerDown : EnumExt_UrbanAbyss.GeneratorPowerUp, 0f, this.game.cameras[i].room.roomSettings.GetEffectAmount(EnumExt_UrbanAbyss.PowerCycle), 1f);
				}
			}
		}

		// Switch on or off.
		if (this.progress < 1f)
		{
			this.progress = Mathf.Min(1f, this.progress + 0.008333334f);
		}

		if (this.game.cameras[0].room != null && this.game.cameras[0].room.roomSettings.GetEffectAmount(EnumExt_UrbanAbyss.PowerCycle) > 0f)
        {
			if (this.to == 1f)
			{
				if (this.progress > 0.2f)
				{
					this.game.cameras[0].ApplyEffectColorsToAllPaletteTextures(this.game.cameras[0].room.roomSettings.EffectColorA, this.game.cameras[0].room.roomSettings.EffectColorB);
				}
			}
            else
			{
				if (this.progress > 0.8f)
				{
					this.game.cameras[0].ApplyEffectColorsToAllPaletteTextures(22, 22);
				}
			}
		}
		

		//Camera shake:
		if (this.progress > 0f && this.progress < 1f)
		{
			for (int j = 0; j < this.game.cameras.Length; j++)
			{
				if (this.game.cameras[j].room.roomSettings.GetEffectAmount(EnumExt_UrbanAbyss.PowerCycle) > 0f)
				{
						this.game.cameras[j].room.ScreenMovement(null, new Vector2(0f, 0f), this.game.cameras[j].room.roomSettings.GetEffectAmount(EnumExt_UrbanAbyss.PowerCycle) * 0.2f * Mathf.Sin(this.progress * 3.1415927f));
				}
			}
		}
	}


	public float power
	{
		get
		{
			return Mathf.Lerp(this.from, this.to, Custom.SCurve(Mathf.InverseLerp(0.35f, 1f, this.progress), 0.5f));
		}
	}

	public int counter; // Countdown to power state change
	public bool on; // is power on
	public float from; 
	public float to;
	public float progress; // From 0f to 1f (1f is full power)
	public int cycleMin;
	public int cycleMax;
	public RainWorldGame game;
}
