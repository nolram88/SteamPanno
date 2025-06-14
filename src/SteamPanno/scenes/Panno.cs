using Godot;
using SteamPanno.global;
using SteamPanno.panno;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class Panno : Control
	{
		private enum PannoState
		{
			Empty = 0,
			Ready = 1,
			Drawn = 2,
			Visible = 3,
		}

		private SubViewport subViewport;
		private TextureRect textureIn;
		private TextureRect textureOut;
		private PannoImage pannoImage;
		private DateTime pannoImageDate;
		private PannoState pannoState;
		private List<PannoGameLayout> pannoGamesInText;

		public override void _Ready()
		{
			subViewport = GetNode<SubViewport>("./SubViewport");
			textureIn = GetNode<TextureRect>("./SubViewport/TextureIn");
		}

		public bool Save()
		{
			if (pannoImage != null)
			{
				var dateText = pannoImageDate.ToString("yyyy-MM-dd-HH-mm-ss");
				var savePath = Path.Combine(Settings.GetDataPath(), $"panno_{dateText}.png");
				if (!File.Exists(savePath))
				{
					textureOut.Texture.GetImage().SavePng(savePath);
					return true;
				}
			}

			return false;
		}

		public override void _Process(double delta)
		{
			switch (pannoState)
			{
				case PannoState.Ready:
					subViewport.Size = pannoImage.Size;
					textureIn.Position = Vector2.Zero;
					textureIn.Size = pannoImage.Size;
					textureIn.Texture = ImageTexture.CreateFromImage(pannoImage);

					foreach (var pannoControlChild in textureIn.GetChildren())
					{
						if (pannoControlChild is Label)
						{
							textureIn.RemoveChild(pannoControlChild);
						}
					}

					foreach (var textGame in pannoGamesInText)
					{
						var label = CreateTextRect(textGame.Area, textGame.Game.Name);
						textureIn.AddChild(label);
					}

					pannoState = PannoState.Drawn;
					break;

				case PannoState.Drawn:
					if (textureOut != null)
					{
						RemoveChild(textureOut);
					}

					textureOut = new TextureRect();
					textureOut.Position = Vector2.Zero;
					textureOut.Size = GetTree().Root.Size;
					var texture = subViewport.GetTexture();
					var textureSize = texture.GetSize();
					textureOut.Texture = texture;
					textureOut.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
					textureOut.StretchMode = textureSize.X <= textureOut.Size.X && textureSize.Y <= textureOut.Size.Y
						? TextureRect.StretchModeEnum.KeepCentered
						: TextureRect.StretchModeEnum.KeepAspectCentered;
					textureOut.SetAnchorsPreset(LayoutPreset.FullRect);

					AddChild(textureOut);

					pannoState = PannoState.Visible;
					break;

				default:
					break;
			}
		}

		public async Task LoadAndDraw(PannoGameLayout[] games, PannoLoader loader, PannoDrawer drawer, IPannoProgress progress)
		{
			pannoGamesInText = new List<PannoGameLayout>();

			var current = 0;
			foreach (var game in games)
			{
				progress.ProgressSet(((double)current / games.Length) * 100, $"{game.Game.Name} ({current}/{games.Length})");

				var image = game.Area.PreferHorizontal()
					? await loader.GetGameLogoH(game.Game.Id)
					: await loader.GetGameLogoV(game.Game.Id);

				if (image != null)
				{
					drawer.Draw(image, game.Area);
				}
				pannoGamesInText.Add(game);

				current++;
			}

			pannoImage = drawer.Dest;
			pannoImageDate = DateTime.Now;
			pannoState = PannoState.Ready;
		}

		private Label CreateTextRect(Rect2I area, string text)
		{
			var label = new Label();
			label.LabelSettings = new LabelSettings()
			{
				Font = ThemeDB.FallbackFont,
				FontSize = Math.Max(area.Size.X / 15, 1),
				LineSpacing = 0,
			};
			label.Text = text;
			label.ClipText = true;
			label.AutowrapMode = TextServer.AutowrapMode.Word;
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			label.Position = area.Position;
			label.Size = area.Size;

			return label;
		}
	}
}
