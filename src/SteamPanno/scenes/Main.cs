using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SteamPanno.panno;

namespace SteamPanno.scenes
{
	public partial class Main : Node
	{
		private Panno panno;

		public override void _Ready()
		{
			var screenResolution = DisplayServer.ScreenGetSize();
			var windowResolution = GetTree().Root.Size;
			
			GD.Print(screenResolution);
			GD.Print(windowResolution);
			
			GetTree().Root.ContentScaleSize = windowResolution;
			
			panno = GetNode<Panno>("./GUI/Center/Panno");
			
			#if STEAM
			SteamPanno.global.Steam.Init();
			#endif

			Task.Run(async () => await LoadPanno());
		}

		public override void _Process(double delta)
		{
			#if STEAM
			SteamPanno.global.Steam.Update();
			#endif
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
				#if STEAM
				SteamPanno.global.Steam.Shutdown();
				#endif
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
				#if STEAM
				var steamId = SteamPanno.global.Steam.SteamId;
				#else
				var steamId = "76561198053918407";
				#endif

				var loader = new PannoLoaderCache(new PannoLoaderOnline());
				var pannoSize = DisplayServer.ScreenGetSize();
				//var pannoSize = new Vector2I(1600, 400);
				var pannoArea = new Rect2I(0, 0, pannoSize.X, pannoSize.Y);
				var drawer = new PannoDrawerResizeAndCut()
				{
					Dest = PannoImage.Create(pannoSize.X, pannoSize.Y),
					Builder = PannoImage.Create,
				};
				var generator = new PannoGeneratorDivideAndConquer();
				
				var games = await loader.GetProfileGames(steamId);
				games = games.OrderByDescending(x => x.HoursOnRecord).Where(x => x.HoursOnRecord >= 1).ToArray();
				var pannoStructure = await generator.Generate(games, pannoArea);
				await panno.Build(pannoStructure, loader, drawer);
				
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
