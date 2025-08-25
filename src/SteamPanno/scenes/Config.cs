using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteamPanno.panno.loading;
using SteamPanno.scenes.controls;

namespace SteamPanno.scenes
{
	public partial class Config : Control
	{
		private readonly string[] GenerationMethods = new string[]
		{
			"DivideAndConquer",
			"GradualDescent",
		};

		private readonly string[] OutpaintingMethods = new string[]
		{
			"ResizeAndCut",
			"ResizeAndExpand",
			"ResizeAndMirror",
			"ResizeProportional",
			"ResizeUnproportional",
		};

		private Dictionary<string, Dictionary<string, string>> profileSnapshots;
		private string steamId;
		private string customAccountIdFromClipboard;
		private Dictionary<string, string> selectedDiffSnapshots;

		private OptionButton accountIdValue;
		private Control friendAccountId;
		private OptionButton friendAccountIdValue;
		private Control customAccountId;
		private LineEdit customAccountIdValue;
		private ImageButton getProfileIdBtn;
		private Control diffSnapshot;
		private OptionButton diffSnapshotValue;
		private OptionButton pannoResolutionValue;
		private Control customPannoResolution;
		private TextEdit customPannoResolutionValue;
		private OptionButton generationMethodValue;
		private OptionButton outpaintingMethodValue;
		private OptionButton minimalHoursValue;
		private Control customMinimalHours;
		private TextEdit customMinimalHoursValue;
		private OptionButton showHoursValue;

		public Action<bool> OnExit { get; set; }

		public override void _Ready()
		{
			profileSnapshots = FileExtensions.GetProfileSnapshots();
			selectedDiffSnapshots = new Dictionary<string, string>();

			var generationSettingsLbl = GetNode<Label>("./VBoxContainer/Title/GenerationSettingsLbl");
			generationSettingsLbl.Text = Localization.Localize($"{nameof(Config)}/{generationSettingsLbl.Name}");

			var accountIdLbl = GetNode<Label>("./VBoxContainer/Content/AccountId/AccountIdLbl");
			accountIdLbl.Text = Localization.Localize($"{nameof(Config)}/{accountIdLbl.Name}");
			accountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/AccountId/AccountIdValue");
			accountIdValue.ItemSelected += AccountOptionSelected;
			friendAccountId = GetNode<Control>("./VBoxContainer/Content/FriendAccountId");
			friendAccountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/FriendAccountId/FriendAccountIdValue");
			friendAccountIdValue.ItemSelected += FriendOptionSelected;
			customAccountId = GetNode<Control>("./VBoxContainer/Content/CustomAccountId");
			customAccountIdValue = GetNode<LineEdit>("./VBoxContainer/Content/CustomAccountId/CustomAccountIdRight/CustomAccountIdValue");
			customAccountIdValue.TextChanged += CustomAccountOptionChanged;
			getProfileIdBtn = GetNode<ImageButton>("./VBoxContainer/Content/CustomAccountId/CustomAccountIdRight/GetProfileIdBtn");
			getProfileIdBtn.OnClick = () => Task.Run(async () => await GetSteamIdBackThread());

			var diffSnapshotLbl = GetNode<Label>("./VBoxContainer/Content/DiffSnapshot/DiffSnapshotLbl");
			diffSnapshotLbl.Text = Localization.Localize($"{nameof(Config)}/{diffSnapshotLbl.Name}");
			diffSnapshot = GetNode<Control>("./VBoxContainer/Content/DiffSnapshot");
			diffSnapshotValue = GetNode<OptionButton>("./VBoxContainer/Content/DiffSnapshot/DiffSnapshotValue");
			diffSnapshotValue.ItemSelected += DiffDateOptionSelected;

			var pannoResolutionLbl = GetNode<Label>("./VBoxContainer/Content/PannoResolution/PannoResolutionLbl");
			pannoResolutionLbl.Text = Localization.Localize($"{nameof(Config)}/{pannoResolutionLbl.Name}");
			pannoResolutionValue = GetNode<OptionButton>("./VBoxContainer/Content/PannoResolution/PannoResolutionValue");
			customPannoResolution = GetNode<Control>("./VBoxContainer/Content/CustomPannoResolution");
			customPannoResolutionValue = GetNode<TextEdit>("./VBoxContainer/Content/CustomPannoResolution/CustomPannoResolutionValue");

			var generationMethodLbl = GetNode<Label>("./VBoxContainer/Content/GenerationMethod/GenerationMethodLbl");
			generationMethodLbl.Text = Localization.Localize($"{nameof(Config)}/{generationMethodLbl.Name}");
			generationMethodValue = GetNode<OptionButton>("./VBoxContainer/Content/GenerationMethod/GenerationMethodValue");

			var outpaintingMethodLbl = GetNode<Label>("./VBoxContainer/Content/OutpaintingMethod/OutpaintingMethodLbl");
			outpaintingMethodLbl.Text = Localization.Localize($"{nameof(Config)}/{outpaintingMethodLbl.Name}");
			outpaintingMethodValue = GetNode<OptionButton>("./VBoxContainer/Content/OutpaintingMethod/OutpaintingMethodValue");

			var minimalHoursLbl = GetNode<Label>("./VBoxContainer/Content/MinimalHours/MinimalHoursLbl");
			minimalHoursLbl.Text = Localization.Localize($"{nameof(Config)}/{minimalHoursLbl.Name}");
			minimalHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/MinimalHours/MinimalHoursValue");
			customMinimalHours = GetNode<Control>("./VBoxContainer/Content/CustomMinimalHours");
			customMinimalHoursValue = GetNode<TextEdit>("./VBoxContainer/Content/CustomMinimalHours/CustomMinimalHoursValue");

			var showHoursLbl = GetNode<Label>("./VBoxContainer/Content/ShowHours/ShowHoursLbl");
			showHoursLbl.Text = Localization.Localize($"{nameof(Config)}/{showHoursLbl.Name}");
			showHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/ShowHours/ShowHoursValue");

			#if STEAM
				var friends = Steam.GetFriends();
				if (friends.Length > 0)
				{
					foreach (var friend in friends)
					{
						var itemName = $"{friend.name} ({friend.id})";
						friendAccountIdValue.AddItem(itemName);
						if (friend.id == Settings.Instance.FriendAccountId)
						{
							friendAccountIdValue.Select(friendAccountIdValue.ItemCount - 1);
						}
					}
				}
			#endif
			customAccountIdValue.Text = Settings.Instance.CustomAccountId;
			for (int i = 0; i <= 2; i++)
			{
				var text = Localization.Localize($"{nameof(Config)}/AccountIdOption{i}");
				accountIdValue.AddItem(text);
			}
			var accountOptionIndex = Math.Clamp(Settings.Instance.AccountIdOption, 0, accountIdValue.ItemCount);
			#if STEAM
				accountIdValue.Select(accountOptionIndex);
				AccountOptionSelected(accountOptionIndex);
			#else
				accountIdValue.Select(2);
				AccountOptionSelected(2);
				accountIdValue.Disabled = true;
			#endif

			var screenResolution = DisplayServer.ScreenGetSize();
			for (int i = 0; i <= 1; i++)
			{
				var text = Localization.Localize($"{nameof(Config)}/PannoResolutionOption{i}");
				if (i == 0)
				{
					text += $" ({screenResolution.X}x{screenResolution.Y})";
				}
				pannoResolutionValue.AddItem(text);
			}
			pannoResolutionValue.ItemSelected += ResolutionOptionSelected;
			var resolutionOptionIndex = Settings.Instance.UseCustomResolution ? 1 : 0;
			pannoResolutionValue.Select(resolutionOptionIndex);
			ResolutionOptionSelected(resolutionOptionIndex);
			customPannoResolutionValue.Text = Settings.Instance.CustomResolution;

			foreach (var method in GenerationMethods)
			{
				var text = Localization.Localize($"{nameof(Config)}/{nameof(GenerationMethods)}/{method}");
				generationMethodValue.AddItem(text);
			};
			var selectedGenerationMethod = Math.Min(Math.Max(Settings.Instance.GenerationMethodOption, 0), GenerationMethods.Length - 1);
			generationMethodValue.Select(selectedGenerationMethod);

			foreach (var method in OutpaintingMethods)
			{
				var text = Localization.Localize($"{nameof(Config)}/{nameof(OutpaintingMethods)}/{method}");
				outpaintingMethodValue.AddItem(text);
			};
			var selectedOutpaintingMethod = Math.Min(Math.Max(Settings.Instance.OutpaintingMethodOption, 0), OutpaintingMethods.Length - 1);
			outpaintingMethodValue.Select(selectedOutpaintingMethod);

			minimalHoursValue.AddItem("1");
			minimalHoursValue.AddItem("10");
			minimalHoursValue.AddItem("100");
			minimalHoursValue.AddItem(Localization.Localize($"{nameof(Config)}/MinimalHoursOptionCustom"));
			minimalHoursValue.ItemSelected += HoursOptionSelected;
			var minimalHoursOptionIndex = Math.Clamp(Settings.Instance.MinimalHoursOption, 0, minimalHoursValue.ItemCount);
			minimalHoursValue.Select(minimalHoursOptionIndex);
			HoursOptionSelected(minimalHoursOptionIndex);
			customMinimalHoursValue.Text = Settings.Instance.CustomMinimalHours;

			for (int i = 0; i <= 6; i++)
			{
				var text = Localization.Localize($"{nameof(Config)}/ShowHoursOption{i}");
				showHoursValue.AddItem(text);
			}
			var showHoursOptionIndex = Math.Clamp((int)Settings.Instance.ShowHoursOption, 0, showHoursValue.ItemCount - 1);
			showHoursValue.Select(showHoursOptionIndex);
		}

		public override void _Process(double delta)
		{
			if (customAccountIdFromClipboard != null)
			{
				customAccountIdValue.Text = customAccountIdFromClipboard;
				customAccountIdFromClipboard = null;
				CustomAccountOptionChanged(customAccountIdValue.Text);
			}
		}

		public override void _UnhandledInput(InputEvent @event)
		{
			if (!Visible) return;

			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				switch (keyEvent.PhysicalKeycode)
				{
					case Key.Escape:
						OnBackBtnPressed();
						GetTree().Root.SetInputAsHandled();
						break;
				}
			}
		}

		private string GetSteamId()
		{
			#if STEAM
				return (accountIdValue.Selected) switch
				{
					1 => friendAccountIdValue.Text.TryParseSteamId(out var friendSteamId)
						? friendSteamId : null,
					2 => customAccountIdValue.Text.TryParseSteamId(out var customSteamId)
						? customSteamId : null,
					_ => Steam.SteamId,
				};
			#else
				return customAccountIdValue.Text.TryParseSteamId(out var customSteamId)
					? customSteamId : null;
			#endif
		}

		private void AccountOptionSelected(long index)
		{
			friendAccountId.Visible = index == 1;
			customAccountId.Visible = index == 2;

			steamId = GetSteamId();
			UpdateAvailableSnapshots();
		}

		private void FriendOptionSelected(long index)
		{
			if (friendAccountIdValue.Text.TryParseSteamId(out var friendSteamId))
			{
				steamId = friendSteamId;
				UpdateAvailableSnapshots();
			}
		}

		private void CustomAccountOptionChanged(string newText)
		{
			steamId = GetSteamId();
			UpdateAvailableSnapshots();
		}

		private void UpdateAvailableSnapshots()
		{
			diffSnapshotValue.Clear();

			if (!string.IsNullOrEmpty(steamId) &&
				profileSnapshots.TryGetValue(steamId, out var snapshots))
			{
				string selectedSnapshot = null;
				if (!selectedDiffSnapshots.TryGetValue(steamId, out selectedSnapshot) &&
					Settings.Instance.SelectedDiffSnapshots != null)
				{
					Settings.Instance.SelectedDiffSnapshots.TryGetValue(steamId, out selectedSnapshot);
				}

				diffSnapshotValue.AddItem(Localization.Localize($"{nameof(Config)}/DiffSnapshotOptionOff"));
				foreach (var snapshot in snapshots)
				{
					diffSnapshotValue.AddItem(snapshot.Key);
					if (snapshot.Value == selectedSnapshot)
					{
						diffSnapshotValue.Select(diffSnapshotValue.ItemCount - 1);
					}
				}
				diffSnapshot.Visible = true;
			}
			else
			{
				diffSnapshot.Visible = false;
			}
		}

		private async Task GetSteamIdBackThread()
		{
			try
			{
				var text = DisplayServer.ClipboardHas()
				? DisplayServer.ClipboardGet()
				: null;

				if (text != null && text.Length < 1000)
				{
					if (text.StartsWith("https://steamcommunity.com/profiles/"))
					{
						if (text.TryParseSteamId(out var steamIdParsed))
						{
							customAccountIdFromClipboard = steamIdParsed;
						}
					}
					else if (text.StartsWith("https://steamcommunity.com/id/"))
					{
						var name = text.Replace("https://steamcommunity.com/id/", "").TrimEnd('/');
						var loader = new PannoLoaderOnline();
						var steamIdLoaded = await loader.GetProfileSteamId(name);
						customAccountIdFromClipboard = steamIdLoaded ?? string.Empty;
					}
				}
			}
			catch (Exception e)
			{
				// report
			}
		}

		private void DiffDateOptionSelected(long index)
		{
			if (index == 0)
			{
				selectedDiffSnapshots[steamId] = null;
			}
			else
			{
				var selectedDate = diffSnapshotValue.GetItemText(diffSnapshotValue.Selected);
				if (profileSnapshots.TryGetValue(steamId, out var snapshots) &&
					snapshots.TryGetValue(selectedDate, out var fileName))
				{
					selectedDiffSnapshots[steamId] = fileName;
				}
			}
		}

		private void ResolutionOptionSelected(long index)
		{
			customPannoResolution.Visible = index == 1;
		}

		private void HoursOptionSelected(long index)
		{
			customMinimalHours.Visible = index == 3;
		}

		private void OnBackBtnPressed()
		{
			OnExit?.Invoke(false);
		}

		private void OnApplyBtnPressed()
		{
			var maxTextureSize = RenderingServer.GetRenderingDevice().LimitGet(RenderingDevice.Limit.MaxTextureSize2D);

			Settings.Instance.AccountIdOption = accountIdValue.Selected;
			if (friendAccountIdValue.Text.TryParseSteamId(out var friendSteamId))
			{
				Settings.Instance.FriendAccountId = friendSteamId;
			}
			Settings.Instance.CustomAccountId = customAccountIdValue.Text;
			if (selectedDiffSnapshots.Count > 0)
			{
				if (Settings.Instance.SelectedDiffSnapshots == null)
				{
					Settings.Instance.SelectedDiffSnapshots = new Dictionary<string, string>();
				}
				foreach (var selectedDiffSnapshot in selectedDiffSnapshots)
				{
					if (selectedDiffSnapshot.Value != null)
					{
						Settings.Instance.SelectedDiffSnapshots[selectedDiffSnapshot.Key] = selectedDiffSnapshot.Value;
					}
					else if (Settings.Instance.SelectedDiffSnapshots.ContainsKey(selectedDiffSnapshot.Key))
					{
						Settings.Instance.SelectedDiffSnapshots.Remove(selectedDiffSnapshot.Key);
					}
				}
			}
			Settings.Instance.UseCustomResolution = pannoResolutionValue.Selected == 1;
			Settings.Instance.CustomResolution = customPannoResolutionValue.Text;
			Settings.Instance.GenerationMethodOption = generationMethodValue.Selected;
			Settings.Instance.OutpaintingMethodOption = outpaintingMethodValue.Selected;
			Settings.Instance.MinimalHoursOption = minimalHoursValue.Selected;
			Settings.Instance.CustomMinimalHours = decimal.TryParse(customMinimalHoursValue.Text, out _)
				? customMinimalHoursValue.Text
				: "0";
			Settings.Instance.ShowHoursOption = (Settings.Dto.ShowHoursOptions)showHoursValue.Selected;
			Settings.Save();

			OnExit?.Invoke(true);
		}
	}
}
