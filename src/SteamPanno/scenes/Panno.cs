using Godot;
using SteamPanno.panno;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class Panno : Control
	{
		private TextureRect pannoControl;
		private PannoImage pannoImage;
		private List<Label> pannoLabels;

		public override void _Ready()
		{
			pannoControl = GetNode<TextureRect>("./TextureRect");
			pannoImage = null;
			pannoLabels = new List<Label>();
		}

		public override void _Process(double delta)
		{
			if (pannoImage != null)
			{
				pannoControl.Texture = ImageTexture.CreateFromImage(pannoImage);
				foreach (var label in pannoLabels)
				{
					AddChild(label);
				}

				pannoImage = null;
			}
		}

		public async Task Build(PannoNode pannoStructure, PannoLoader loader, PannoDrawer drawer)
		{
			var games = pannoStructure.AllLeaves().ToArray();

			foreach (var game in games)
			{
				var image = game.Area.PreferHorizontal()
					? await loader.GetGameLogoH(game.Game.Id)
					: await loader.GetGameLogoV(game.Game.Id);

				if (image != null)
				{
					drawer.Draw(image, game.Area);
				}
				
				AddTextRect(game.Area, game.Game.Name);
				
			}

			pannoImage = drawer.Dest;
		}

		private void AddTextRect(Rect2I area, string text)
		{
			//var view = new SubViewport();
			//view.Size = new Vector2I(100, 100);
			//view.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
			//await ToSignal(GetTree(), "idle_frame");

			var label = new Label();
			label.Text = text;
			label.ClipText = true;
			label.LabelSettings = new LabelSettings()
			{
				FontSize = 1 * area.Size.X / 15,
				LineSpacing = 0,
			};
			label.AutowrapMode = TextServer.AutowrapMode.Word;
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			label.Position = area.Position;
			label.Size = area.Size;
			pannoLabels.Add(label);
		}
	}
}
