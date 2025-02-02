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
			GetTree().Root.Size = DisplayServer.ScreenGetSize();

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
				var loader = new PannoLoaderCache(new PannoLoaderOnline());
				var pannoSize = DisplayServer.ScreenGetSize();
				var pannoArea = new Rect2I(0, 0, pannoSize.X, pannoSize.Y);
				var drawer = new PannoDrawerResizeProportional()
				{
					Dest = PannoImage.Create(pannoSize.X, pannoSize.Y),
				};
				var generator = new PannoGenerator();
				var builder = new PannoBuilder(loader, drawer);
				
				var games = await loader.GetProfileGames(steamId);
				games = games.OrderByDescending(x => x.HoursOnRecord).Where(x => x.HoursOnRecord >= 1).ToArray();
				var panno = await generator.Generate(games, pannoArea, pannoArea.Size.X > pannoArea.Size.Y);
				await builder.Build(panno);

				pannoImage = drawer.Dest;
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
