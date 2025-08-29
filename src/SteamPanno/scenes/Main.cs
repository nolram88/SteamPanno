using Godot;
using SteamPanno.panno;
using SteamPanno.panno.drawing;
using SteamPanno.panno.generation;
using SteamPanno.panno.loading;
using SteamPanno.scenes.controls;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace SteamPanno.scenes
{
	public partial class Main : Node, IPannoImageProcessor, IPannoObserver
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

		private string pannoSteamId;
		private double pannoProgressValueToSet;
		private string pannoProgressTextToSet;
		private bool saveButtonVisible;
		private bool warningButtonVisible;
		private Channel<string> reportBuffer = Channel.CreateUnbounded<string>();

		[Export]
		public PackedScene ConfigScene { get; set; }

		public override void _Ready()
		{
			var screenResolution = DisplayServer.ScreenGetSize();
			var windowResolution = GetTree().Root.Size;
			
			GD.Print(screenResolution);
			GD.Print(windowResolution);
			
			GetTree().Root.ContentScaleSize = windowResolution;
			
			panno = GetNode<Panno>("./Panno");
			gui = GetNode<Control>("./GUI");
			PrepareConfig();
			report = GetNode<TextEdit>("./GUI/Report");
			progressContainer = GetNode<VBoxContainer>("./GUI/Center/Progress");
			pannoProgressBar = GetNode<ProgressBar>("./GUI/Center/Progress/Bar");
			pannoProgressLabel = GetNode<Label>("./GUI/Center/Progress/Text");

			var configButton = GetNode<ImageButton>("./GUI/Top/ConfigButton");
			configButton.OnClick = () => ShowConfig(true);
			var languageMenu = GetNode<PopupMenu>("./GUI/LanguageMenu");
			var localizations = Localization.GetLocalizations();
			var localization = Localization.GetLocalization();
			for (int i = 0; i < localizations.Length; i++)
			{
				languageMenu.AddCheckItem(localizations[i]);
				languageMenu.SetItemChecked(i, localizations[i] == localization);
			}
			languageMenu.IndexPressed += (index) =>
			{
				if (!languageMenu.IsItemChecked((int)index))
				{
					for (int i = 0; i < languageMenu.ItemCount; i++)
					{
						if (languageMenu.IsItemChecked(i))
						{
							languageMenu.SetItemChecked(i, false);
							break;
						}
					}
					languageMenu.SetItemChecked((int)index, true);
					Localization.SetLocalization((int)index);
					PrepareConfig();
				}
			};
			var languageButton = GetNode<ImageButton>("./GUI/Top/LanguageButton");
			languageButton.OnClick = () => languageMenu.Popup(new Rect2I()
			{
				Position = new Vector2I((int)languageButton.Position.X, (int)languageButton.Position.Y + (int)languageButton.Size.Y),
			});
			var exitButton = GetNode<ImageButton>("./GUI/Top/ExitButton");
			exitButton.OnClick = Quit;
			saveButton = GetNode<ImageButton>("./GUI/Bottom/SaveButton");
			saveButton.OnClick = SavePannoToFile;
			warningButton = GetNode<ImageButton>("./GUI/Bottom/WarningButton");
			warningButton.OnClick = () =>
			{
				report.Visible = !report.Visible;
			};

			if (Settings.Instance.ShowConfigOnStart)
			{
				ShowConfig(true);
			}
			else
			{
				Task.Run(async () => await GeneratePannoBackThread());
			}
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
				Quit();
			}
		}

		public PannoImage Create(int x, int y)
		{
			return PannoImage.Create(x, y);
		}

		public PannoImage Blur(PannoImage src)
		{
			return null;
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
			Report($"{e.GetType().Name}{nl}{e.Message}{nl}{e.StackTrace}");
		}

		public void Report(string text)
		{
			var nl = System.Environment.NewLine;
			reportBuffer.Writer.TryWrite(
				$"{DateTime.Now.ToString()}: {text}{nl}");
			warningButtonVisible = true;
		}

		protected void PrepareConfig()
		{
			if (config != null)
			{
				RemoveChild(config);
				config.QueueFree();
			}

			config = ConfigScene.Instantiate<Config>();
			config.Name = nameof(Config);
			config.Visible = false;
			config.OnExit = (applied) =>
			{
				ShowConfig(false);
				if (applied)
				{
					Task.Run(async () => await GeneratePannoBackThread());
				}
			};
			AddChild(config);
		}

		protected async Task GeneratePannoBackThread()
		{
			Stopwatch time = Stopwatch.StartNew();

			try
			{
				pannoSteamId = null;
				saveButtonVisible = false;
				warningButtonVisible = false;
				panno.Clear();

				pannoSteamId = Settings.GetSteamId();
				if (string.IsNullOrEmpty(pannoSteamId))
				{
					Report(Localization.Localize("CouldNotGetSteamId"));
					return;
				}

				var pannoSize = GetPannoSize();
				
				var loader = new PannoLoaderCache(new PannoLoaderOnline());
				PannoDrawer drawer = Settings.Instance.OutpaintingMethodOption switch
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
				
				this.ProgressSet(0, Localization.Localize("ProfileLoading"));
				var games = await loader.GetProfileGames(pannoSteamId);
				if (Settings.Instance.SelectedDiffSnapshots.TryGetValue(pannoSteamId, out var snapshot))
				{
					var snapshotData = FileExtensions.GetProfileSnapshot(snapshot);
					if (snapshotData != null)
					{
						var gamesBefore = JsonSerializer.Deserialize<PannoGame[]>(snapshotData);
						for (int i = 0; i < games.Length; i++)
						{
							var game = games[i];
							var gameBefore = gamesBefore
								.Where(x => x.Id == game.Id)
								.SingleOrDefault();

							if (gameBefore != null)
							{
								games[i] = new PannoGame()
								{
									Id = game.Id,
									Name = game.Name,
									HoursOnRecord = game.HoursOnRecord - gameBefore.HoursOnRecord,
								};
							}
						}
					}
				}

				var minimalHours = GetMinimalHours();
				games = games
					.Where(x => x.HoursOnRecord >= minimalHours)
					.OrderByDescending(x => x.HoursOnRecord)
					.ToArray();
				if (games.Length == 0)
				{
					Report(Localization.Localize("NoGamesMeetTheGivenCriteria"));
					return;
				}

				ProgressSet(0, Localization.Localize("PannoLayoutGeneration"));
				var pannoStructure = await generator.Generate(
					games.ToArray(),
					new Rect2I(0, 0, pannoSize.X, pannoSize.Y));

				await panno.LoadAndDraw(pannoStructure, loader, drawer, this);

				saveButtonVisible = true;
				GD.Print($"Generation time: {TimeSpan.FromMilliseconds(time.ElapsedMilliseconds).ToString()}");
			}
			catch (Exception e)
			{
				Report(e);
			}
			finally
			{
				ProgressStop();
				time.Stop();
			}
		}

		protected void SavePannoToFile()
		{
			try
			{
				var dateText = DateTime.Today.ToString("yyyy-MM-dd");
				var savePath = Path.Combine(FileExtensions.GetGenerationPath(), $"panno_{pannoSteamId}_{dateText}.png");
				if (File.Exists(savePath))
				{
					File.Delete(savePath);
				}

				panno.Save(savePath);
			}
			catch (Exception e)
			{
				Report(e);
			}
			finally
			{
				saveButtonVisible = false;
			}
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

		protected T CreateDrawer<T>(Vector2I pannoSize) where T : PannoDrawer, new()
		{
			return new T()
			{
				Dest = PannoImage.Create(pannoSize.X, pannoSize.Y),
				Processor = this,
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
