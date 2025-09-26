using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SteamPanno.panno.loading;
using SteamPanno.scenes.controls;
using System.Linq;

namespace SteamPanno.scenes
{
	public partial class Config : Control
	{
		private static readonly (string Name, Vector2I Size)[] resolutions = new (string Name, Vector2I Size)[]
		{
			("Full HD", new Vector2I(1920, 1080)),
			("2K QHD", new Vector2I(2560, 1440)),
			("4K UHD", new Vector2I(3840, 2160)),
		};

		private Dictionary<string, Dictionary<string, string>> profileSnapshots;
		private string steamId;
		private string customAccountIdFromClipboard;
		private Dictionary<string, string> selectedBeginingSnapshots;
		private Dictionary<string, string> selectedEndingSnapshots;

		private OptionButton accountIdValue;
		private Control friendAccountId;
		private OptionButton friendAccountIdValue;
		private Control customAccountId;
		private LineEdit customAccountIdValue;
		private ImageButton getProfileIdBtn;
		private Control beginingSnapshot;
		private OptionButton beginingSnapshotValue;
		private Control endingSnapshot;
		private OptionButton endingSnapshotValue;
		private OptionButton pannoResolutionValue;
		private Control customPannoResolution;
		private LineEdit customPannoResolutionValue;
		private OptionButton generationMethodValue;
		private OptionButton outpaintingMethodValue;
		private OptionButton minimalHoursValue;
		private Control customMinimalHours;
		private LineEdit customMinimalHoursValue;
		private OptionButton showHoursValue;

		public Action<bool> OnExit { get; set; }

		public override void _Ready()
		{
			profileSnapshots = FileExtensions.GetProfileSnapshots();
			selectedBeginingSnapshots = new Dictionary<string, string>();
			selectedEndingSnapshots = new Dictionary<string, string>();

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

			var beginingSnapshotLbl = GetNode<Label>("./VBoxContainer/Content/BeginingSnapshot/BeginingSnapshotLbl");
			beginingSnapshotLbl.Text = Localization.Localize($"{nameof(Config)}/{beginingSnapshotLbl.Name}");
			beginingSnapshot = GetNode<Control>("./VBoxContainer/Content/BeginingSnapshot");
			beginingSnapshotValue = GetNode<OptionButton>("./VBoxContainer/Content/BeginingSnapshot/BeginingSnapshotValue");
			beginingSnapshotValue.ItemSelected += BeginingSnapshotSelected;

			var endingSnapshotLbl = GetNode<Label>("./VBoxContainer/Content/EndingSnapshot/EndingSnapshotLbl");
			endingSnapshotLbl.Text = Localization.Localize($"{nameof(Config)}/{endingSnapshotLbl.Name}");
			endingSnapshot = GetNode<Control>("./VBoxContainer/Content/EndingSnapshot");
			endingSnapshotValue = GetNode<OptionButton>("./VBoxContainer/Content/EndingSnapshot/EndingSnapshotValue");
			endingSnapshotValue.ItemSelected += EndingSnapshotSelected;

			var pannoResolutionLbl = GetNode<Label>("./VBoxContainer/Content/PannoResolution/PannoResolutionLbl");
			pannoResolutionLbl.Text = Localization.Localize($"{nameof(Config)}/{pannoResolutionLbl.Name}");
			pannoResolutionValue = GetNode<OptionButton>("./VBoxContainer/Content/PannoResolution/PannoResolutionValue");
			customPannoResolution = GetNode<Control>("./VBoxContainer/Content/CustomPannoResolution");
			customPannoResolutionValue = GetNode<LineEdit>("./VBoxContainer/Content/CustomPannoResolution/CustomPannoResolutionValue");

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
			customMinimalHoursValue = GetNode<LineEdit>("./VBoxContainer/Content/CustomMinimalHours/CustomMinimalHoursValue");

			var showHoursLbl = GetNode<Label>("./VBoxContainer/Content/ShowHours/ShowHoursLbl");
			showHoursLbl.Text = Localization.Localize($"{nameof(Config)}/{showHoursLbl.Name}");
			showHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/ShowHours/ShowHoursValue");

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
			
			customAccountIdValue.Text = Settings.Instance.CustomAccountId;
			for (int i = 0; i <= 2; i++)
			{
				var text = Localization.Localize($"{nameof(Config)}/AccountIdOption{i}");
				accountIdValue.AddItem(text);
			}
			var accountOptionIndex = Math.Clamp(Settings.Instance.AccountIdOption, 0, accountIdValue.ItemCount);
			if (Steam.GetSteamId() != null)
			{
				accountIdValue.SetItemDisabled(1, friends.Length == 0);
				accountIdValue.Select(accountOptionIndex);
				AccountOptionSelected(accountOptionIndex);
			}
			else
			{
				accountIdValue.SetItemDisabled(0, true);
				accountIdValue.SetItemDisabled(1, true);
				accountIdValue.Select(2);
				AccountOptionSelected(2);
			}

			var screenResolution = DisplayServer.ScreenGetSize();
			pannoResolutionValue.AddItem(
				screenResolution.FormatResolution(
					Localization.Localize($"{nameof(Config)}/PannoResolutionOptionNative")));
			var resolutionOptionIndex = 0;
			Settings.Instance.SelectedResolution.TryParseResolution(out var selectedResolution);
			foreach (var r in resolutions)
			{
				if (r.Size.X == screenResolution.X && r.Size.Y == screenResolution.Y)
				{
					continue;
				}

				pannoResolutionValue.AddItem(r.Size.FormatResolution(r.Name));
				if (!Settings.Instance.UseNativeResolution &&
					r.Size.X == selectedResolution.X &&
					r.Size.Y == selectedResolution.Y)
				{
					resolutionOptionIndex = pannoResolutionValue.ItemCount - 1;
				}
			}
			var customResolutionText = Localization.Localize($"{nameof(Config)}/PannoResolutionOptionCustom");
			pannoResolutionValue.AddItem(customResolutionText);
			if (resolutionOptionIndex == 0 && !Settings.Instance.UseNativeResolution)
			{
				resolutionOptionIndex = pannoResolutionValue.ItemCount - 1;
			}
			pannoResolutionValue.ItemSelected += ResolutionOptionSelected;
			pannoResolutionValue.Select(resolutionOptionIndex);
			ResolutionOptionSelected(resolutionOptionIndex);
			customPannoResolutionValue.Text = Settings.Instance.CustomResolution;

			foreach (var method in MetaData.GenerationTypes.Keys)
			{
				var text = Localization.Localize($"{nameof(Config)}/{nameof(MetaData.GenerationTypes)}/{method}");
				generationMethodValue.AddItem(text);
			};
			var selectedGenerationMethod = Math.Min(Math.Max(Settings.Instance.GenerationMethodOption, 0), MetaData.GenerationTypes.Count - 1);
			generationMethodValue.Select(selectedGenerationMethod);

			foreach (var method in MetaData.OutpaintingTypes.Keys)
			{
				var text = Localization.Localize($"{nameof(Config)}/{nameof(MetaData.OutpaintingTypes)}/{method}");
				outpaintingMethodValue.AddItem(text);
			};
			var selectedOutpaintingMethod = Math.Min(Math.Max(Settings.Instance.OutpaintingMethodOption, 0), MetaData.OutpaintingTypes.Count - 1);
			outpaintingMethodValue.Select(selectedOutpaintingMethod);

			minimalHoursValue.AddItem("0");
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

			var okBtn = GetNode<ImageButton>("./VBoxContainer/Buttons/OkButton");
			okBtn.OnClick = OnOkPressed;
			var cancelBtn = GetNode<ImageButton>("./VBoxContainer/Buttons/CancelButton");
			cancelBtn.OnClick = OnCancelPressed;
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
						OnCancelPressed();
						GetTree().Root.SetInputAsHandled();
						break;
				}
			}
		}

		private string GetSteamId()
		{
			var id = Steam.GetSteamId();

			if (id != null)
			{
				if (accountIdValue.Selected == 0)
				{
					return id;
				}
				else if (accountIdValue.Selected == 1)
				{
					return friendAccountIdValue.Text.TryParseSteamId(out var friendSteamId)
						? friendSteamId : null;
				}
			}
			
			return customAccountIdValue.Text.TryParseSteamId(out var customSteamId)
				? customSteamId : null;
		}

		private void AccountOptionSelected(long index)
		{
			friendAccountId.Visible = index == 1;
			customAccountId.Visible = index == 2;

			steamId = GetSteamId();
			UpdateAvailableBeginingSnapshots();
		}

		private void FriendOptionSelected(long index)
		{
			if (friendAccountIdValue.Text.TryParseSteamId(out var friendSteamId))
			{
				steamId = friendSteamId;
				UpdateAvailableBeginingSnapshots();
			}
		}

		private void CustomAccountOptionChanged(string newText)
		{
			steamId = GetSteamId();
			UpdateAvailableBeginingSnapshots();
		}

		private void UpdateAvailableBeginingSnapshots()
		{
			beginingSnapshotValue.Clear();
			beginingSnapshotValue.AddItem(Localization.Localize($"{nameof(Config)}/SnapshotOptionOff"));

			if (!string.IsNullOrEmpty(steamId) &&
				profileSnapshots.TryGetValue(steamId, out var snapshots))
			{
				if (!selectedBeginingSnapshots.TryGetValue(steamId, out var selectedSnapshot) &&
					Settings.Instance.SelectedBeginingSnapshots != null)
				{
					Settings.Instance.SelectedBeginingSnapshots.TryGetValue(steamId, out selectedSnapshot);
				}

				foreach (var snapshot in snapshots)
				{
					beginingSnapshotValue.AddItem(snapshot.Key);
					if (snapshot.Value == selectedSnapshot)
					{
						beginingSnapshotValue.Select(beginingSnapshotValue.ItemCount - 1);
					}
				}
				beginingSnapshot.Visible = true;
			}
			else
			{
				beginingSnapshot.Visible = false;
			}

			UpdateAvailableEndingSnapshots();
		}

		private void UpdateAvailableEndingSnapshots()
		{
			endingSnapshotValue.Clear();
			endingSnapshotValue.AddItem(Localization.Localize($"{nameof(Config)}/SnapshotOptionOff"));

			if (!string.IsNullOrEmpty(steamId) &&
				profileSnapshots.TryGetValue(steamId, out var snapshots) &&
				beginingSnapshotValue.Selected > 0)
			{
				if (!selectedEndingSnapshots.TryGetValue(steamId, out var selectedSnapshot) &&
					Settings.Instance.SelectedEndingSnapshots != null)
				{
					Settings.Instance.SelectedEndingSnapshots.TryGetValue(steamId, out selectedSnapshot);
				}

				foreach (var snapshot in snapshots.Skip(beginingSnapshotValue.Selected))
				{
					endingSnapshotValue.AddItem(snapshot.Key);
					if (snapshot.Value == selectedSnapshot)
					{
						endingSnapshotValue.Select(endingSnapshotValue.ItemCount - 1);
					}
				}
				endingSnapshot.Visible = endingSnapshotValue.ItemCount > 1;
			}
			else
			{
				endingSnapshot.Visible = false;
			}

			if (endingSnapshotValue.Selected >= 0)
			{
				EndingSnapshotSelected(endingSnapshotValue.Selected);
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
			catch
			{
			}
		}

		private void BeginingSnapshotSelected(long index)
		{
			if (index == 0)
			{
				selectedBeginingSnapshots[steamId] = null;
			}
			else
			{
				var selectedDate = beginingSnapshotValue.GetItemText(beginingSnapshotValue.Selected);
				if (profileSnapshots.TryGetValue(steamId, out var snapshots) &&
					snapshots.TryGetValue(selectedDate, out var fileName))
				{
					selectedBeginingSnapshots[steamId] = fileName;
				}
			}

			UpdateAvailableEndingSnapshots();
		}

		private void EndingSnapshotSelected(long index)
		{
			if (index == 0)
			{
				selectedEndingSnapshots[steamId] = null;
			}
			else
			{
				var selectedDate = endingSnapshotValue.GetItemText(endingSnapshotValue.Selected);
				if (profileSnapshots.TryGetValue(steamId, out var snapshots) &&
					snapshots.TryGetValue(selectedDate, out var fileName))
				{
					selectedEndingSnapshots[steamId] = fileName;
				}
			}
		}

		private void ResolutionOptionSelected(long index)
		{
			customPannoResolution.Visible = index == pannoResolutionValue.ItemCount - 1;
		}

		private void HoursOptionSelected(long index)
		{
			customMinimalHours.Visible = index == 4;
		}

		private void OnCancelPressed()
		{
			OnExit?.Invoke(false);
		}

		private void OnOkPressed()
		{
			var maxTextureSize = RenderingServer.GetRenderingDevice().LimitGet(RenderingDevice.Limit.MaxTextureSize2D);

			Settings.Instance.AccountIdOption = accountIdValue.Selected;
			if (friendAccountIdValue.Text.TryParseSteamId(out var friendSteamId))
			{
				Settings.Instance.FriendAccountId = friendSteamId;
			}
			Settings.Instance.CustomAccountId = customAccountIdValue.Text;
			if (selectedBeginingSnapshots.Count > 0)
			{
				if (Settings.Instance.SelectedBeginingSnapshots == null)
				{
					Settings.Instance.SelectedBeginingSnapshots = new Dictionary<string, string>();
				}
				foreach (var selectedBeginingSnapshot in selectedBeginingSnapshots)
				{
					if (selectedBeginingSnapshot.Value != null)
					{
						Settings.Instance.SelectedBeginingSnapshots[selectedBeginingSnapshot.Key] = selectedBeginingSnapshot.Value;
					}
					else if (Settings.Instance.SelectedBeginingSnapshots.ContainsKey(selectedBeginingSnapshot.Key))
					{
						Settings.Instance.SelectedBeginingSnapshots.Remove(selectedBeginingSnapshot.Key);
					}
				}
			}
			if (selectedEndingSnapshots.Count > 0)
			{
				if (Settings.Instance.SelectedEndingSnapshots == null)
				{
					Settings.Instance.SelectedEndingSnapshots = new Dictionary<string, string>();
				}
				foreach (var selectedEndingSnapshot in selectedEndingSnapshots)
				{
					if (selectedEndingSnapshot.Value != null)
					{
						Settings.Instance.SelectedEndingSnapshots[selectedEndingSnapshot.Key] = selectedEndingSnapshot.Value;
					}
					else if (Settings.Instance.SelectedEndingSnapshots.ContainsKey(selectedEndingSnapshot.Key))
					{
						Settings.Instance.SelectedEndingSnapshots.Remove(selectedEndingSnapshot.Key);
					}
				}
			}
			Settings.Instance.UseNativeResolution = pannoResolutionValue.Selected == 0;
			Settings.Instance.SelectedResolution =
				pannoResolutionValue.Selected == 0 ||
				pannoResolutionValue.Selected == pannoResolutionValue.ItemCount - 1
					? null : pannoResolutionValue.Text;
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
