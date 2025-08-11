using Godot;
using SteamPanno.panno;
using System;
using System.Collections.Generic;
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
			Dirty = 4,
		}

		private SubViewport subViewport;
		private TextureRect textureIn;
		private TextureRect textureOut;
		private PannoImage pannoImage;
		private PannoState pannoState;
		private List<PannoGameLayout> pannoGamesInText;

		public override void _Ready()
		{
			subViewport = GetNode<SubViewport>("./SubViewport");
			textureIn = GetNode<TextureRect>("./SubViewport/TextureIn");
		}

		public void Save(string filename)
		{
			if (pannoImage != null)
			{
				textureOut.Texture.GetImage().SavePng(filename);
			}
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

					foreach (var textGame in pannoGamesInText)
					{
						if (textGame.Area.Size.X < Settings.Instance.MinimalGameAreaSize)
						{
							continue;
						}

						var titleLabel = CreateTitleLabel(textGame.Area, textGame.Game.Name);
						textureIn.AddChild(titleLabel);
						if (Settings.Instance.ShowHoursOption != 0)
						{
							var hoursLabel = CreateHoursLabel(textGame.Area, textGame.Game.HoursOnRecord);
							textureIn.AddChild(hoursLabel);
						}
					}

					pannoState = PannoState.Drawn;
					break;

				case PannoState.Drawn:
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

				case PannoState.Dirty:
					if (textureOut != null)
					{
						RemoveChild(textureOut);
						textureOut = null;
						textureIn.Texture = null;
						foreach (var pannoControlChild in textureIn.GetChildren())
						{
							if (pannoControlChild is RichTextLabel)
							{
								textureIn.RemoveChild(pannoControlChild);
							}
						}
					}

					pannoState = PannoState.Empty;
					break;

				default:
					break;
			}
		}

		public void Clear()
		{
			pannoState = PannoState.Dirty;
		}

		public async Task LoadAndDraw(PannoGameLayout[] games, PannoLoader loader, PannoDrawer drawer, ICallBack callBack)
		{
			pannoGamesInText = new List<PannoGameLayout>();

			var current = 0;
			foreach (var game in games)
			{
				callBack.ProgressSet(((double)current / games.Length) * 100, $"{game.Game.Name} ({current}/{games.Length})");

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
			pannoState = PannoState.Ready;
		}

		private RichTextLabel CreateTitleLabel(Rect2I area, string text)
		{
			var label = new RichTextLabel();
			label.AddThemeFontOverride("normal_font", ThemeDB.FallbackFont);
			label.AddThemeFontSizeOverride("normal_font_size", Math.Max(area.Size.X / 15, 1));
			label.AddThemeConstantOverride("line_separation", 0);
			label.Text = text;
			label.ClipContents = true;
			label.AutowrapMode = TextServer.AutowrapMode.Word;
			label.HorizontalAlignment = HorizontalAlignment.Center;
			label.VerticalAlignment = VerticalAlignment.Center;
			label.Position = area.Position;
			label.Size = area.Size;

			return label;
		}

		private RichTextLabel CreateHoursLabel(Rect2I area, float hours)
		{
			var label = new RichTextLabel();
			label.AddThemeFontOverride("normal_font", ThemeDB.FallbackFont);
			label.AddThemeFontSizeOverride("normal_font_size", Math.Max(area.Size.X / 25, 1));
			label.AddThemeConstantOverride("line_separation", 0);
			label.AddThemeConstantOverride("text_highlight_h_padding", 4);
			label.AddThemeConstantOverride("text_highlight_v_padding", 0);
			label.AutowrapMode = TextServer.AutowrapMode.Word;
			label.HorizontalAlignment = Settings.Instance.ShowHoursOption switch
			{
				Settings.Dto.ShowHoursOptions.BOTTOM_LEFT => HorizontalAlignment.Left,
				Settings.Dto.ShowHoursOptions.BOTTOM_RIGHT => HorizontalAlignment.Right,
				Settings.Dto.ShowHoursOptions.TOP_LEFT => HorizontalAlignment.Left,
				Settings.Dto.ShowHoursOptions.TOP_RIGHT => HorizontalAlignment.Right,
				_ => HorizontalAlignment.Center,
			};
			label.VerticalAlignment = Settings.Instance.ShowHoursOption switch
			{
				Settings.Dto.ShowHoursOptions.TOP => VerticalAlignment.Top,
				Settings.Dto.ShowHoursOptions.TOP_LEFT => VerticalAlignment.Top,
				Settings.Dto.ShowHoursOptions.TOP_RIGHT => VerticalAlignment.Top,
				_ => VerticalAlignment.Bottom,
			};
			label.ParseBbcode($"[bgcolor=#000000ff]{Math.Round(hours)} h[/bgcolor]");
			label.Position = area.Position;
			label.Size = area.Size;

			return label;
		}
	}
}
