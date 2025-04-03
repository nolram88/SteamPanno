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
		private TextureRect pannoControl;
		private PannoImage pannoImage;
		private bool pannoImageNeedsUpdate;
		private DateTime pannoImageDate;
		private List<PannoNodeLeaf> pannoGamesInText;

		public override void _Ready()
		{
			pannoControl = GetNode<TextureRect>("./TextureRect");
		}

		public bool Save()
		{
			if (pannoImage != null)
			{
				var dateText = pannoImageDate.ToString("yyyy-MM-dd-HH-mm-ss");
				var savePath = Path.Combine(Settings.GetDataPath(), $"panno_{dateText}.png");
				if (!File.Exists(savePath))
				{
					pannoImage.SavePng(savePath);
					return true;
				}
			}

			return false;
		}

		public override void _Process(double delta)
		{
			if (pannoImageNeedsUpdate)
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
				
				pannoImageNeedsUpdate = false;
			}
		}

		public async Task Build(PannoNode pannoStructure, PannoLoader loader, PannoDrawer drawer, IPannoProgress progress)
		{
			var games = pannoStructure.AllLeaves().ToArray();
			pannoGamesInText = new List<PannoNodeLeaf>();

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
			pannoImageNeedsUpdate = true;
			pannoImageDate = DateTime.Now;
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
