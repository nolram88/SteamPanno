using System;
using System.Linq;
using System.Text.RegularExpressions;
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
		private Control gui;
		private Config config;
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
			
			panno = GetNode<Panno>("./Panno");
			gui = GetNode<Control>("./GUI");
			config = GetNode<Config>("./Config");
			config.OnExit = (applied) =>
			{
				ShowConfig(false);
				if (applied)
				{
					Task.Run(async () => await GeneratePanno());
				}
			};
			progressContainer = GetNode<VBoxContainer>("./GUI/Center/Progress");
			pannoProgressBar = GetNode<ProgressBar>("./GUI/Center/Progress/Bar");
			pannoProgressLabel = GetNode<Label>("./GUI/Center/Progress/Text");

			var configButton = GetNode<ImageButton>("./GUI/ConfigButton");
			configButton.OnClick = () => ShowConfig(true);
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

			Task.Run(async () => await GeneratePanno());
		}

		public override void _Process(double delta)
		{
			#if STEAM
			Steam.Update();
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
				Steam.Shutdown();
				#endif
			}
			if (what == NotificationWMCloseRequest)
			{
				GD.Print("ALT+F4 !");
				Quit();
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

		protected async Task GeneratePanno()
		{
			try
			{
				var steamId = GetSteamId();
				if (string.IsNullOrEmpty(steamId))
				{
					return;
				}

				var pannoSize = GetPannoSize();
				if (pannoSize == Vector2I.Zero)
				{
					return;
				}

				var loader = new PannoLoaderCache(new PannoLoaderOnline());
				PannoDrawer drawer = Settings.Instance.TileExpansionMethodOption switch
				{
					0 => CreateDrawer<PannoDrawerResizeAndCut>(pannoSize),
					1 => CreateDrawer<PannoDrawerResizeAndExpand>(pannoSize),
					2 => CreateDrawer<PannoDrawerResizeAndMirror>(pannoSize),
					3 => CreateDrawer<PannoDrawerResizeProportional>(pannoSize),
					4 => CreateDrawer<PannoDrawerResizeUnproportional>(pannoSize),
					_ => CreateDrawer<PannoDrawerResizeAndCut>(pannoSize),
				};
				PannoGameLayoutGenerator generator = (Settings.Instance.GenerationMethodOption) switch
				{
					0 => new PannoGameLayoutGeneratorDivideAndConquer(),
					1 => new PannoGameLayoutGeneratorGradualDescent(),
					_ => new PannoGameLayoutGeneratorDivideAndConquer(),
				};
				
				ProgressSet(0, "Profile loading...");
				var games = await loader.GetProfileGames(steamId);

				ProgressSet(0, "Panno layout generation...");
				var minimalHours = GetMinimalHours();
				games = games
					.Where(x => x.HoursOnRecord >= minimalHours)
					.OrderByDescending(x => x.HoursOnRecord)
					.ToArray();
				
				var pannoStructure = await generator.Generate(
					games.ToArray(),
					new Rect2I(0, 0, pannoSize.X, pannoSize.Y));

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

		protected string GetSteamId()
		{
			#if STEAM
			return (Settings.Instance.AccountIdOption) switch
			{
				1 => ParseSteamId(Settings.Instance.FriendAccountId),
				2 => ParseSteamId(Settings.Instance.CustomAccountId),
				_ => Steam.SteamId,
			};
			#else
			return ParseSteamId(Settings.Instance.CustomAccountId);
			#endif
		}

		protected string ParseSteamId(string input)
		{
			if (string.IsNullOrEmpty(input))
			{
				return null;
			}

			Regex steamIdRegex = new Regex(@"(?:7656119\d{10}|STEAM_[01]:[01]:\d+|\[?[A-Z]:[01]:\d+\]?|U:1:\d+)");
			var match = steamIdRegex.Match(input);
			
			return match.Success ? match.Groups[1].Value : null;
		}

		protected Vector2I GetPannoSize()
		{
			if (Settings.Instance.UseNativeResolution)
			{
				return DisplayServer.ScreenGetSize();
			}

			Regex resRegex = new Regex(@"\b(\d{2,5})\w+(\d{2,5})\b");
			var match = resRegex.Match(Settings.Instance.CustomResolution);
			if (match.Success)
			{
				var x = int.TryParse(match.Groups[1].Value, out var xv) ? xv : 0;
				var y = int.TryParse(match.Groups[2].Value, out var yv) ? yv : 0;

				return new Vector2I(x, y);
			}

			return Vector2I.Zero;
		}

		protected PannoDrawer CreateDrawer<T>(Vector2I pannoSize) where T : PannoDrawer, new()
		{
			return new T()
			{
				Dest = PannoImage.Create(pannoSize.X, pannoSize.Y),
				Builder = PannoImage.Create,
			};
		}

		protected float GetMinimalHours()
		{
			return (Settings.Instance.MinimalHoursOption) switch
			{
				0 => 1,
				1 => 10,
				2 => 100,
				_ => float.TryParse(Settings.Instance.CustomMinimalHours, out var hours) ? hours : 0,
			};
		}

		protected void ShowConfig(bool show)
		{
			panno.Visible = !show;
			gui.Visible = !show;
			config.Visible = show;
		}

		protected void Quit()
		{
			GetTree().Quit();
		}
	}
}
