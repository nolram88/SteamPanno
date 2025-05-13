using System;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using SteamPanno.panno;
using SteamPanno.panno.drawing;
using SteamPanno.panno.generation;
using SteamPanno.panno.loading;
using SteamPanno.scenes.controls;

namespace SteamPanno.scenes
{
	public partial class Main : Node, IPannoProgress
	{
		private Panno panno;
		private Control progressContainer;
		private ProgressBar pannoProgressBar;
		private Label pannoProgressLabel;
		private double pannoProgressValueToSet;
		private string pannoProgressTextToSet;
		private ImageButton saveButton;
		private ImageButton wirningButton;

		public override void _Ready()
		{
			var screenResolution = DisplayServer.ScreenGetSize();
			var windowResolution = GetTree().Root.Size;
			
			GD.Print(screenResolution);
			GD.Print(windowResolution);
			
			GetTree().Root.ContentScaleSize = windowResolution;
			
			panno = GetNode<Panno>("./GUI/Center/Panno");
			progressContainer = GetNode<VBoxContainer>("./GUI/Center/Progress");
			pannoProgressBar = GetNode<ProgressBar>("./GUI/Center/Progress/Bar");
			pannoProgressLabel = GetNode<Label>("./GUI/Center/Progress/Text");

			var configButton = GetNode<ImageButton>("./GUI/ConfigButton");
			configButton.OnClick = () => GD.Print("config");
			var exitButton = GetNode<ImageButton>("./GUI/ExitButton");
			exitButton.OnClick = Quit;
			saveButton = GetNode<ImageButton>("./GUI/SaveButton");
			saveButton.OnClick = () =>
			{
				if (panno.Save())
				{
					saveButton.Visible = false;
				}
			};
			wirningButton = GetNode<ImageButton>("./GUI/WarningButton");
			wirningButton.OnClick = () => GD.Print("wirning");
			
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

			pannoProgressBar.Value = pannoProgressValueToSet;
			pannoProgressLabel.Text = pannoProgressTextToSet;
			progressContainer.Visible = pannoProgressTextToSet != null;
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
				//var generator = new PannoGameLayoutGeneratorGradualDescent();
				var generator = new PannoGameLayoutGeneratorDivideAndConquer();

				ProgressSet(0, "Profile loading...");
				var games = await loader.GetProfileGames(steamId);

				ProgressSet(0, "Panno layout generation...");
				games = games.OrderByDescending(x => x.HoursOnRecord).Where(x => x.HoursOnRecord >= 1).ToArray();
				/*
				var big = 10;
				var medium = 10;
				var small = 10;
				var games = new List<PannoGame>();
				for (int i = 0; i < big; i++)
				{
					games.Add(new PannoGame() { Name = "100", HoursOnRecord = 100 });
				}
				for (int i = 0; i < medium; i++)
				{
					games.Add(new PannoGame() { Name = "50", HoursOnRecord = 50 });
				}
				for (int i = 0; i < small; i++)
				{
					games.Add(new PannoGame() { Name = "20", HoursOnRecord = 20 });
				}*/
				
				var pannoStructure = await generator.Generate(games.ToArray(), pannoArea);

				await panno.LoadAndDraw(pannoStructure, loader, drawer, this);
			}
			catch (Exception e)
			{
				GD.Print($"{e.Message}{System.Environment.NewLine}{e.StackTrace}");
			}
			finally
			{
				ProgressStop();
			}
		}

		public void ProgressSet(double value, string text = null)
		{
			pannoProgressValueToSet = value;
			if (text != null)
			{
				pannoProgressTextToSet = text;
			}
		}

		public void ProgressStop()
		{
			pannoProgressTextToSet = null;
			pannoProgressValueToSet = 0;
		}

		protected void Quit()
		{
			GetTree().Quit();
		}
	}
}
