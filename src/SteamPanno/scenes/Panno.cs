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
		private List<PannoNodeLeaf> pannoGamesInText;

		public override void _Ready()
		{
			pannoControl = GetNode<TextureRect>("./TextureRect");
			pannoImage = null;
		}

		public override void _Process(double delta)
		{
			if (pannoImage != null)
			{
				pannoControl.Texture = ImageTexture.CreateFromImage(pannoImage);
				pannoControl.Position = Vector2.Zero;

				foreach (var pannoControlChild in pannoControl.GetChildren())
				{
					if (pannoControlChild is Label)
					{
						pannoControl.RemoveChild(pannoControlChild);
					}
				}
				
				foreach (var textGame in pannoGamesInText)
				{
					var label = CreateTextRect(textGame.Area, textGame.Game.Name);
					pannoControl.AddChild(label);
				}
				
				pannoImage = null;
			}
		}

		public async Task Build(PannoNode pannoStructure, PannoLoader loader, PannoDrawer drawer)
		{
			var games = pannoStructure.AllLeaves().ToArray();
			pannoGamesInText = new List<PannoNodeLeaf>();
			
			foreach (var game in games)
			{
				var image = game.Area.PreferHorizontal()
					? await loader.GetGameLogoH(game.Game.Id)
					: await loader.GetGameLogoV(game.Game.Id);

				if (image != null)
				{
					drawer.Draw(image, game.Area);
				}

				pannoGamesInText.Add(game);
			}

			pannoImage = drawer.Dest;
		}

		private Label CreateTextRect(Rect2I area, string text)
		{
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
			
			return label;
		}
	}
}
