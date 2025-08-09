using Godot;
using System;

namespace SteamPanno.scenes
{
	public partial class Config : Control
	{
		private string[] generationMethods = new string[]
		{
			"Divide And Conquer",
			"Gradual Descent",
		};

		private string[] tileExpansionMethods = new string[]
		{
			"Resize+Cut",
			"Resize+Expand",
			"Resize+Mirror",
			"Resize Proportional",
			"Resize Unproportional",
		};

		private OptionButton accountIdValue;
		private OptionButton friendAccountIdValue;
		private LineEdit customAccountIdValue;
		private OptionButton pannoResolutionValue;
		private TextEdit customPannoResolutionValue;
		private OptionButton generationMethodValue;
		private OptionButton tileExpansionMethod;
		private OptionButton minimalHoursValue;
		private TextEdit customMinimalHoursValue;
		private OptionButton showHoursValue;

		public Action<bool> OnExit { get; set; }

		public override void _Ready()
		{
			accountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/AccountId/AccountIdValue");
			friendAccountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/AccountId/FriendAccountIdValue");
			friendAccountIdValue.ClipText = true;
			customAccountIdValue = GetNode<LineEdit>("./VBoxContainer/Content/AccountId/CustomAccountIdValue");
			friendAccountIdValue.ClipText = true;
			pannoResolutionValue = GetNode<OptionButton>("./VBoxContainer/Content/PannoResolution/PannoResolutionValue");
			customPannoResolutionValue = GetNode<TextEdit>("./VBoxContainer/Content/PannoResolution/CustomPannoResolutionValue");
			generationMethodValue = GetNode<OptionButton>("./VBoxContainer/Content/GenerationMethod/GenerationMethodValue");
			tileExpansionMethod = GetNode<OptionButton>("./VBoxContainer/Content/TileExpansionMethod/TileExpansionMethodValue");
			minimalHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/MinimalHours/MinimalHoursValue");
			customMinimalHoursValue = GetNode<TextEdit>("./VBoxContainer/Content/MinimalHours/CustomMinimalHoursValue");
			showHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/ShowHours/ShowHoursValue");

			accountIdValue.AddItem("My Account");
			accountIdValue.AddItem("My Friend's Account");
			accountIdValue.AddItem("Custom Account");
			accountIdValue.ItemSelected += AccountOptionSelected;
			var accountOptionIndex = Math.Clamp(Settings.Instance.AccountIdOption, 0, accountIdValue.ItemCount);
			#if STEAM
				accountIdValue.Select(accountOptionIndex);
				AccountOptionSelected(accountOptionIndex);
			#else
				accountIdValue.Select(2);
				AccountOptionSelected(2);
				accountIdValue.Visible = false;
			#endif
			#if STEAM
				var friends = Steam.GetFriends();
				if (friends.Length > 0)
				{
					foreach (var friend in friends)
					{
						var itemName = $"{friend.name} ({friend.id})";
						friendAccountIdValue.AddItem(itemName);
						if (itemName == Settings.Instance.FriendAccountId)
						{
							friendAccountIdValue.Select(friendAccountIdValue.ItemCount - 1);
						}
					}
				}
			#endif
			customAccountIdValue.Text = Settings.Instance.CustomAccountId;

			var screenResolution = DisplayServer.ScreenGetSize();
			pannoResolutionValue.AddItem($"Native ({screenResolution.X}x{screenResolution.Y})");
			pannoResolutionValue.AddItem("Custom");
			pannoResolutionValue.ItemSelected += ResolutionOptionSelected;
			var resolutionOptionIndex = Settings.Instance.UseCustomResolution ? 1 : 0;
			pannoResolutionValue.Select(resolutionOptionIndex);
			ResolutionOptionSelected(resolutionOptionIndex);
			customPannoResolutionValue.Text = Settings.Instance.CustomResolution;

			foreach (var method in generationMethods)
			{
				generationMethodValue.AddItem(method);
			};
			var selectedGenerationMethod = Math.Min(Math.Max(Settings.Instance.GenerationMethodOption, 0), generationMethods.Length - 1);
			generationMethodValue.Select(selectedGenerationMethod);

			foreach (var method in tileExpansionMethods)
			{
				tileExpansionMethod.AddItem(method);
			};
			var selectedTileExpansionMethod = Math.Min(Math.Max(Settings.Instance.TileExpansionMethodOption, 0), tileExpansionMethods.Length - 1);
			generationMethodValue.Select(selectedTileExpansionMethod);

			minimalHoursValue.AddItem("1");
			minimalHoursValue.AddItem("10");
			minimalHoursValue.AddItem("100");
			minimalHoursValue.AddItem("Custom");
			minimalHoursValue.ItemSelected += HoursOptionSelected;
			var minimalHoursOptionIndex = Math.Clamp(Settings.Instance.MinimalHoursOption, 0, minimalHoursValue.ItemCount);
			minimalHoursValue.Select(minimalHoursOptionIndex);
			HoursOptionSelected(minimalHoursOptionIndex);
			customMinimalHoursValue.Text = Settings.Instance.CustomMinimalHours;
			showHoursValue.AddItem("Off");
			showHoursValue.AddItem("Bottom");
			showHoursValue.AddItem("Bottom Left");
			showHoursValue.AddItem("Bottom Right");
			showHoursValue.AddItem("Top");
			showHoursValue.AddItem("Top Left");
			showHoursValue.AddItem("Top Right");
			var showHoursOptionIndex = Math.Clamp((int)Settings.Instance.ShowHoursOption, 0, showHoursValue.ItemCount - 1);
			showHoursValue.Select(showHoursOptionIndex);
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

		private void AccountOptionSelected(long index)
		{
			friendAccountIdValue.Visible = index == 1;
			customAccountIdValue.Visible = index == 2;
		}

		private void ResolutionOptionSelected(long index)
		{
			customPannoResolutionValue.Visible = index == 1;
		}

		private void HoursOptionSelected(long index)
		{
			customMinimalHoursValue.Visible = index == 3;
		}

		private void OnBackBtnPressed()
		{
			OnExit?.Invoke(false);
		}

		private void OnApplyBtnPressed()
		{
			var maxTextureSize = RenderingServer.GetRenderingDevice().LimitGet(RenderingDevice.Limit.MaxTextureSize2D);

			Settings.Instance.AccountIdOption = accountIdValue.Selected;
			Settings.Instance.FriendAccountId = friendAccountIdValue.Text;
			Settings.Instance.CustomAccountId = customAccountIdValue.Text;
			Settings.Instance.UseCustomResolution = pannoResolutionValue.Selected == 1;
			Settings.Instance.CustomResolution = customPannoResolutionValue.Text;
			Settings.Instance.GenerationMethodOption = generationMethodValue.Selected;
			Settings.Instance.TileExpansionMethodOption = tileExpansionMethod.Selected;
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
