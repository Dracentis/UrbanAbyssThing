using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class ColoredLightBeam : UpdatableAndDeletable, IDrawable
{
	public ColoredLightBeam(PlacedObject placedObject) : base()
	{
		this.gridDiv = 1;
		this.lastCamPos = -1;
		this.placedObject = placedObject;
		this.quad = new Vector2[4];
		this.quad[0] = placedObject.pos;
		this.quad[1] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[0];
		this.quad[2] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[1];
		this.quad[3] = placedObject.pos + (placedObject.data as PlacedObject.QuadObjectData).handles[2];
		this.gridDiv = this.GetIdealGridDiv();
		this.meshDirty = true;
		this.paletteEffectColor = new Color(1f,1f,1f);
	}

	public Color color
	{
		get
		{
			return this.c;
		}
		set
		{
			this.c = value;
			this.colorAlpha = 0f;
			if (this.c.r > this.colorAlpha)
			{
				this.colorAlpha = this.c.r;
			}
			if (this.c.g > this.colorAlpha)
			{
				this.colorAlpha = this.c.g;
			}
			if (this.c.b > this.colorAlpha)
			{
				this.colorAlpha = this.c.b;
			}
			this.c /= this.colorAlpha;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if ((this.placedObject.data as LightBeam.LightBeamData).colorB > 0f && this.room.game.cameras[0].room == this.room)
		{
			this.environmentColor = this.room.game.cameras[0].PixelColorAtCoordinate(this.quad[1]);
		}
	}

	public int GetIdealGridDiv()
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			if (Vector2.Distance(this.quad[i], this.quad[i + 1]) > num)
			{
				num = Vector2.Distance(this.quad[i], this.quad[i + 1]);
			}
		}
		if (Vector2.Distance(this.quad[0], this.quad[3]) > num)
		{
			num = Vector2.Distance(this.quad[0], this.quad[3]);
		}
		return Mathf.Clamp(Mathf.RoundToInt(num / 250f), 1, 20);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		TriangleMesh triangleMesh = TriangleMesh.MakeGridMesh("Futile_White", this.gridDiv);
		this.meshDirty = true;
		sLeaser.sprites[0] = triangleMesh;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["LightBeam"];
		this.verts = new Vector2[(sLeaser.sprites[0] as TriangleMesh).vertices.Length];
		this.AddToContainer(sLeaser, rCam, null);
	}

	private void UpdateVerts(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		this.quad[0] = this.placedObject.pos;
		this.quad[1] = this.placedObject.pos + (this.placedObject.data as PlacedObject.QuadObjectData).handles[0];
		this.quad[2] = this.placedObject.pos + (this.placedObject.data as PlacedObject.QuadObjectData).handles[1];
		this.quad[3] = this.placedObject.pos + (this.placedObject.data as PlacedObject.QuadObjectData).handles[2];
		int idealGridDiv = this.GetIdealGridDiv();
		if (idealGridDiv != this.gridDiv)
		{
			this.gridDiv = idealGridDiv;
			sLeaser.sprites[0].RemoveFromContainer();
			this.InitiateSprites(sLeaser, rCam);
		}
		for (int i = 0; i <= this.gridDiv; i++)
		{
			for (int j = 0; j <= this.gridDiv; j++)
			{
				Vector2 a = Vector2.Lerp(this.quad[0], this.quad[1], (float)j / (float)this.gridDiv);
				Vector2 b = Vector2.Lerp(this.quad[1], this.quad[2], (float)i / (float)this.gridDiv);
				Vector2 b2 = Vector2.Lerp(this.quad[3], this.quad[2], (float)j / (float)this.gridDiv);
				Vector2 a2 = Vector2.Lerp(this.quad[0], this.quad[3], (float)i / (float)this.gridDiv);
				this.verts[j * (this.gridDiv + 1) + i] = Custom.LineIntersection(a, b2, a2, b);
			}
		}
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (this.meshDirty)
		{
			this.UpdateVerts(sLeaser, rCam);
			this.UpdateColor(sLeaser, rCam, this.lastAlpha);
			this.meshDirty = false;
		}
		for (int i = 0; i < this.verts.Length; i++)
		{
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i, this.verts[i] - camPos);
		}
		int num = Mathf.FloorToInt((this.placedObject.data as LightBeam.LightBeamData).alpha * 3f);
		float num2 = Mathf.InverseLerp(0.33333334f * (float)num, 0.33333334f * (float)(num + 1), (this.placedObject.data as LightBeam.LightBeamData).alpha);
		if ((this.placedObject.data as LightBeam.LightBeamData).sun)
		{
			num2 *= Mathf.Pow(Mathf.InverseLerp(-0.2f, 0f, rCam.room.world.rainCycle.ShaderLight), 1.2f);
		}
		num2 = Mathf.Lerp(0.33333334f * (float)num, 0.33333334f * (float)(num + 1), num2);
		if (num2 != this.lastAlpha)
		{
			this.UpdateColor(sLeaser, rCam, num2);
			this.lastAlpha = num2;
		}
		if (rCam.currentCameraPosition != this.lastCamPos)
		{
			this.lastCamPos = rCam.currentCameraPosition;
			this.UpdateColor(sLeaser, rCam, num2);
		}
		if (base.slatedForDeletetion || this.room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void UpdateColor(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float a)
	{
		if (this.colorFromEnvironment)
        {
			this.color = Color.Lerp(Color.Lerp(this.paletteLitColor, Color.white, (this.placedObject.data as LightBeam.LightBeamData).colorA), this.environmentColor, (this.placedObject.data as LightBeam.LightBeamData).colorB);
		}
        else
		{
			this.color = this.paletteEffectColor;
        }
		Color color = Custom.RGB2RGBA(this.color, a);
		for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++)
		{
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = color;
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.paletteLitColor = palette.texture.GetPixel(8, 5);
		if (this.effectColor >= 0)
		{
			this.paletteEffectColor = palette.texture.GetPixel(30, 5 - this.effectColor * 2);
		}
		this.meshDirty = true;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.sprites[0].RemoveFromContainer();
		rCam.ReturnFContainer("ForegroundLights").AddChild(sLeaser.sprites[0]);
	}
	public PlacedObject placedObject;
	public Vector2[] quad;
	public Vector2[] verts;
	public bool meshDirty;
	private float lastAlpha;
	private int gridDiv;
	public int lastCamPos;
	private Color c;
	private float colorAlpha;
	public Color environmentColor;
	public Color paletteLitColor;
	public Color paletteEffectColor;
	public bool colorFromEnvironment;
	public int effectColor = -1;

	public class ColoredLightBeamData : LightBeam.LightBeamData
	{
		public ColoredLightBeamData(PlacedObject owner) : base(owner)
		{
			this.colorType = ColoredLightBeam.ColoredLightBeamData.ColorType.Environment;
		}

		public override void FromString(string s)
		{
			base.FromString(s);
			string[] array = Regex.Split(s, "~");
			if (array.Length > 12)
			{
				this.colorType = Custom.ParseEnum<ColoredLightBeam.ColoredLightBeamData.ColorType>(array[12]);
			}
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				base.ToString(),
				"~",
				this.colorType.ToString()
			});
		}

		public ColoredLightBeam.ColoredLightBeamData.ColorType colorType;

		public enum ColorType
		{
			Environment,
			White,
			EffectColor1,
			EffectColor2
		}
	}
}

