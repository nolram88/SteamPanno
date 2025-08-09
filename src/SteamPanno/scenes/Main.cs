using Godot;
using SteamPanno.panno;
using SteamPanno.panno.drawing;
using SteamPanno.panno.generation;
using SteamPanno.panno.loading;
using SteamPanno.scenes.controls;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class Main : Node, ICallBack
	{
		private Panno panno;
		private Control gui;
		private Config config;
		private TextEdit report;
		private Control progressContainer;
		private ProgressBar pannoProgressBar;
		private Label pannoProgressLabel;
		private ImageButton saveButton;
		private ImageButton warningButton;

		private double pannoProgressValueToSet;
		private string pannoProgressTextToSet;
		private bool saveButtonVisible;
		private bool warningButtonVisible;
		private Channel<string> reportBuffer = Channel.CreateUnbounded<string>();
		
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
					Task.Run(async () => await GeneratePannoBackThread());
				}
			};
			report = GetNode<TextEdit>("./GUI/Report");
			report.TextChanged += () =>
			{
				warningButton.Visible = report.Text.Length > 0;
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
			warningButton = GetNode<ImageButton>("./GUI/WarningButton");
			warningButton.OnClick = () =>
			{
				report.Visible = !report.Visible;
			};

			Task.Run(async () => await GeneratePannoBackThread());
		}

		public override void _Process(double delta)
		{
			#if STEAM
			Steam.Update();
			#endif

			pannoProgressBar.Value = pannoProgressValueToSet;
			pannoProgressLabel.Text = pannoProgressTextToSet;
			progressContainer.Visible = pannoProgressTextToSet != null;
			if (saveButton.Visible != saveButtonVisible)
			{
				saveButton.Visible = saveButtonVisible;
				if (saveButtonVisible)
				{
					saveButton.Blink(true);
				}
			}
			if (warningButton.Visible != warningButtonVisible)
			{
				warningButton.Visible = warningButtonVisible;
				if (warningButtonVisible)
				{
					warningButton.Blink(true);
				}
			}
			while (reportBuffer.Reader.TryRead(out var reportText))
			{
				report.Text += reportText;
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

		public void Report(Exception e)
		{
			var nl = System.Environment.NewLine;
			reportBuffer.Writer.TryWrite(
				$"{e.GetType().Name}{nl}{e.Message}{nl}{e.StackTrace}{nl}");
		}

		public void Report(string text)
		{
			reportBuffer.Writer.TryWrite(
				text + System.Environment.NewLine);
		}

		protected async Task GeneratePannoBackThread()
		{
			try
			{
				saveButtonVisible = false;
				warningButtonVisible = false;
				panno.Clear();

				var steamId = GetSteamId();
				if (string.IsNullOrEmpty(steamId))
				{
					Report("Could not get Steam ID.");
					return;
				}

				var pannoSize = GetPannoSize();
				
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
				
				this.ProgressSet(0, "Profile loading...");
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

				saveButtonVisible = true;
			}
			catch (Exception e)
			{
				Report(e);
				warningButtonVisible = true;
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
				1 => Settings.Instance.FriendAccountId.TryParseSteamId(out var friendSteamId)
					? friendSteamId : null,
				2 => Settings.Instance.CustomAccountId.TryParseSteamId(out var customSteamId)
					? customSteamId : null,
				_ => Steam.SteamId,
			};
			#else
			return Settings.Instance.CustomAccountId.TryParseSteamId(out var customSteamId)
				? customSteamId : null;
			#endif
		}

		protected Vector2I GetPannoSize()
		{
			if (Settings.Instance.UseCustomResolution &&
				Settings.Instance.CustomResolution.TryParseResolution(out var resolution))
			{
				return resolution;
			}

			return DisplayServer.ScreenGetSize();
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
