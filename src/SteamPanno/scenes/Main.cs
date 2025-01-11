using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SteamPanno.global;
using SteamPanno.panno;

namespace SteamPanno.scenes
{
	public partial class Main : Node
	{
		private TextureRect panno;
		private Image pannoImage;

		public override void _Ready()
		{
			Steam.Init();

			panno = GetNode<TextureRect>("./GUI/Panno");

			Task.Run(async () => await LoadPanno());
		}

		public override void _Process(double delta)
		{
			Steam.Update();

			if (panno.Texture == null && pannoImage != null)
			{
				panno.Texture = ImageTexture.CreateFromImage(pannoImage);
			}
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				switch (keyEvent.PhysicalKeycode)
				{
					case Key.Escape:
						Quit();
						break;
				}
			}
		}

		public override void _Notification(int what)
		{
			if (what == NotificationPredelete)
			{
				Steam.Shutdown();
			}
			if (what == NotificationWMCloseRequest)
			{
				GD.Print("ALT+F4 !");
				Quit();
			}
		}

		protected async Task LoadPanno()
		{
			try
			{
				var steamId = Steam.SteamId;
				var loader = new PannoLoaderOnline();
				var generator = new PannoGenerator(loader);

				var games = await loader.GetProfileGames(steamId);
				games = games.OrderByDescending(x => x.HoursOnRecord).Take(5).ToArray();
				foreach (var game in games)
				{
					await game.LoadLogoH(loader);
				}
				games = games.Where(x => x.LogoH != null).ToArray();

				var pannoSize = DisplayServer.ScreenGetSize();
				var pannoArea = new Rect2I(0, 0, pannoSize.X, pannoSize.Y);
				var pannoImageNew = Image.CreateEmpty(pannoSize.X, pannoSize.Y, false, Image.Format.Rgb8);
				var panno = await generator.Generate(games, true);

				panno.Draw(pannoImageNew, pannoArea, true);
				pannoImage = pannoImageNew;
			}
			catch (Exception e)
			{
				GD.Print($"{e.Message}{System.Environment.NewLine}{e.StackTrace}");
			}
		}

		protected void Quit()
		{
			GetTree().Quit();
		}
	}
}
