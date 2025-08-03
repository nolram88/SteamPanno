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

		private Settings.Dto settings;
		private OptionButton accountIdValue;
		private OptionButton friendAccountIdValue;
		private TextEdit customAccountIdValue;
		private OptionButton pannoResolutionValue;
		private OptionButton minimalHoursValue;
		private OptionButton generationMethodValue;
		private OptionButton tileExpansionMethod;

		public Action OnExit { get; set; }

		public override void _Ready()
		{
			settings = Settings.Instance;
			accountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/AccountId/AccountIdValue");
			friendAccountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/AccountId/FriendAccountIdValue");
			customAccountIdValue = GetNode<TextEdit>("./VBoxContainer/Content/AccountId/CustomAccountIdValue");
			pannoResolutionValue = GetNode<OptionButton>("./VBoxContainer/Content/PannoResolution/PannoResolutionValue");
			minimalHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/MinimalHours/MinimalHoursValue");
			generationMethodValue = GetNode<OptionButton>("./VBoxContainer/Content/GenerationMethod/GenerationMethodValue");
			tileExpansionMethod = GetNode<OptionButton>("./VBoxContainer/Content/TileExpansionMethod/TileExpansionMethodValue");
			
			accountIdValue.AddItem("My Account");
			accountIdValue.AddItem("My Friend's Account");
			accountIdValue.AddItem("Custom Account");
			accountIdValue.ItemSelected += (index) =>
			{
				friendAccountIdValue.Visible = index == 1;
				customAccountIdValue.Visible = index == 2;
			};
			#if STEAM
			accountIdValue.Select(Math.Clamp(settings.AccountIdOption, 0, accountIdValue.ItemCount));
			#else
			accountIdValue.Select(2));
			accountIdValue.AllowReselect = false;
			#endif

			#if STEAM
			var friends = Steam.GetFriends();
			if (friends.Length > 0)
			{
				foreach (var friend in friends)
				{
					friendAccountIdValue.AddItem($"{friend.name} ({friend.id})");
					if (friend.id == settings.FriendAccountId)
					{
						friendAccountIdValue.Select(friendAccountIdValue.ItemCount - 1);
					}
				}
			}
			#endif
			
			customAccountIdValue.Text = settings.CustomAccountId;


			pannoResolutionValue = GetNode<OptionButton>("./VBoxContainer/Content/PannoResolution/PannoResolutionValue");
			pannoResolutionValue.AddItem("Native (1920x1080)");
			pannoResolutionValue.AddItem("another");
			pannoResolutionValue.Select(0);

			minimalHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/MinimalHours/MinimalHoursValue");
			minimalHoursValue.AddItem("1");
			minimalHoursValue.AddItem("10");
			minimalHoursValue.AddItem("100");
			minimalHoursValue.AddItem("another");
			minimalHoursValue.Select(0);

			generationMethodValue = GetNode<OptionButton>("./VBoxContainer/Content/GenerationMethod/GenerationMethodValue");
			foreach (var method in generationMethods)
			{
				generationMethodValue.AddItem(method);
			};
			var selectedGenerationMethod = Math.Max(Math.Min(Settings.Instance.GenerationMethod, 0), generationMethods.Length - 1);
			generationMethodValue.Select(selectedGenerationMethod);

			tileExpansionMethod = GetNode<OptionButton>("./VBoxContainer/Content/TileExpansionMethod/TileExpansionMethodValue");
			foreach (var method in tileExpansionMethods)
			{
				tileExpansionMethod.AddItem(method);
			};
			var selectedTileExpansionMethod = Math.Max(Math.Min(Settings.Instance.GenerationMethod, 0), generationMethods.Length - 1);
			generationMethodValue.Select(selectedTileExpansionMethod);
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

		private void OnBackBtnPressed()
		{
			OnExit?.Invoke();
		}

		private void OnRestoreBtnPressed()
		{
			
		}

		private void OnApplyBtnPressed()
		{
			Settings.Instance.AccountIdOption = accountIdValue.Selected;
			Settings.Instance.FriendAccountId = friendAccountIdValue.Text;
			Settings.Instance.CustomAccountId = customAccountIdValue.Text;
			Settings.Instance.Resolution = (100, 100);
			Settings.Instance.MinimalHours = int.TryParse(minimalHoursValue.Text, out var minimalHours) ? minimalHours : 0;
			Settings.Instance.GenerationMethod = generationMethodValue.Selected;
			Settings.Instance.TileExpansionMethod = tileExpansionMethod.Selected;
			Settings.Save();

			OnExit?.Invoke();
		}
	}
}
