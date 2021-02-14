using System;
using RWCustom;
using UnityEngine;

namespace DevInterface
{
	public class ColoredLightBeamRepresentation : QuadObjectRepresentation
	{
		public ColoredLightBeamRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj) : base(owner, IDstring, parentNode, pObj, "Colored Light Beam")
		{
			this.controlPanel = new ColoredLightBeamRepresentation.ColoredLightBeamControlPanel(owner, "Colored_Light_Beam_Panel", this, new Vector2(0f, 100f));
			this.subNodes.Add(this.controlPanel);
			this.controlPanel.pos = (pObj.data as ColoredLightBeam.ColoredLightBeamData).panelPos;
			this.fSprites.Add(new FSprite("pixel", true));
			this.lineSprite = this.fSprites.Count - 1;
			owner.placedObjectsContainer.AddChild(this.fSprites[this.lineSprite]);
			this.fSprites[this.lineSprite].anchorY = 0f;
			for (int i = 0; i < owner.room.updateList.Count; i++)
			{
				if (owner.room.updateList[i] is ColoredLightBeam && (owner.room.updateList[i] as ColoredLightBeam).placedObject == pObj)
				{
					this.LB = (owner.room.updateList[i] as ColoredLightBeam);
					break;
				}
			}
			if (this.LB == null)
			{
				this.LB = new ColoredLightBeam(pObj);
				owner.room.AddObject(this.LB);
			}
		}

		public override void Refresh()
		{
			base.Refresh();
			base.MoveSprite(this.lineSprite, this.absPos);
			this.fSprites[this.lineSprite].scaleY = this.controlPanel.pos.magnitude;
			this.fSprites[this.lineSprite].rotation = Custom.AimFromOneVectorToAnother(this.absPos, this.controlPanel.absPos);
			(this.pObj.data as ColoredLightBeam.ColoredLightBeamData).panelPos = this.controlPanel.pos;
			this.LB.meshDirty = true;
			this.LB.colorFromEnvironment = ((this.pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType == ColoredLightBeam.ColoredLightBeamData.ColorType.Environment);
			this.LB.effectColor = Math.Max(-1, (this.pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType - ColoredLightBeam.ColoredLightBeamData.ColorType.EffectColor1);
		}

		public ColoredLightBeam LB;

		private ColoredLightBeamRepresentation.ColoredLightBeamControlPanel controlPanel;

		private int lineSprite;

		public class ColoredLightBeamControlPanel : Panel, IDevUISignals
		{
			public ColoredLightBeamControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos) : base(owner, IDstring, parentNode, pos, new Vector2(250f, 85f), "Colored Light Beam")
			{
				this.subNodes.Add(new ColoredLightBeamRepresentation.ColoredLightBeamControlPanel.ColoredLightBeamSlider(owner, "Alpha_Slider", this, new Vector2(5f, 65f), "Alpha/depth: "));
				this.subNodes.Add(new ColoredLightBeamRepresentation.ColoredLightBeamControlPanel.ColoredLightBeamSlider(owner, "ColA_Slider", this, new Vector2(5f, 45f), "White-Standard: "));
				this.subNodes.Add(new ColoredLightBeamRepresentation.ColoredLightBeamControlPanel.ColoredLightBeamSlider(owner, "ColB_Slider", this, new Vector2(5f, 25f), "Pickup: "));
				this.sunButton = new Button(owner, "Sun_Button", this, new Vector2(5f, 5f), 110f, string.Empty);
				this.subNodes.Add(this.sunButton);
				this.sunButton.Text = ((!((parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).sun) ? "STATIC" : "SUN");
				this.colorButton = new Button(owner, "Color_Button", this, new Vector2(120f, 5f), 100f, ((parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType.ToString());
				this.subNodes.Add(this.colorButton);
			}

			public void Signal(DevUISignalType type, DevUINode sender, string message)
			{
				string idstring = sender.IDstring;
				switch (idstring)
				{
					case "Color_Button":
						if (((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType >= (ColoredLightBeam.ColoredLightBeamData.ColorType)Enum.GetNames(typeof(ColoredLightBeam.ColoredLightBeamData.ColorType)).Length - 1)
						{
							((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType = ColoredLightBeam.ColoredLightBeamData.ColorType.Environment;
						}
						else
						{
							((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType++;
						}
						(sender as Button).Text = ((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType.ToString();
						(this.parentNode as ColoredLightBeamRepresentation).LB.colorFromEnvironment = (((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType == ColoredLightBeam.ColoredLightBeamData.ColorType.Environment);
						(this.parentNode as ColoredLightBeamRepresentation).LB.effectColor = Math.Max(-1, ((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorType - ColoredLightBeam.ColoredLightBeamData.ColorType.EffectColor1);
						break;
					default:
						((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).sun = !((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).sun;
						this.sunButton.Text = ((!((this.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).sun) ? "STATIC" : "SUN");
						break;
				}
			}

			public Button sunButton;
			public Button colorButton;

			public class ColoredLightBeamSlider : Slider
			{
				public ColoredLightBeamSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title) : base(owner, IDstring, parentNode, pos, title, false, 110f)
				{
				}

				public override void Refresh()
				{
					base.Refresh();
					float num = 0f;
					string idstring = this.IDstring;
					switch (idstring)
					{
						case "Alpha_Slider":
							num = ((this.parentNode.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).alpha;
							base.NumberText = ((int)(num * 100f)).ToString() + "%";
							break;
						case "ColA_Slider":
							num = ((this.parentNode.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorA;
							base.NumberText = ((int)(num * 100f)).ToString() + "%";
							break;
						case "ColB_Slider":
							num = ((this.parentNode.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorB;
							base.NumberText = ((int)(num * 100f)).ToString() + "%";
							break;
					}
					base.RefreshNubPos(num);
				}

				public override void NubDragged(float nubPos)
				{
					string idstring = this.IDstring;
					switch (idstring)
					{
						case "Alpha_Slider":
							((this.parentNode.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).alpha = nubPos;
							break;
						case "ColA_Slider":
							((this.parentNode.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorA = nubPos;
							break;
						case "ColB_Slider":
							((this.parentNode.parentNode as ColoredLightBeamRepresentation).pObj.data as ColoredLightBeam.ColoredLightBeamData).colorB = nubPos;
							break;
					}
					this.parentNode.parentNode.Refresh();
					this.Refresh();
				}
			}
		}
	}
}
