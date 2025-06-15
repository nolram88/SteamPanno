using Godot;

namespace SteamPanno.scenes
{
	public partial class Config : Control
	{
		private OptionButton accountIdValue;
		private OptionButton pannoResolutionValue;
		private OptionButton minimalHoursValue;
		private OptionButton generationMethodValue;
		private OptionButton tileExpansionMethod;

		public override void _Ready()
		{
			accountIdValue = GetNode<OptionButton>("./VBoxContainer/Content/AccountId/AccountIdValue");
			accountIdValue.AddItem("this account");
			accountIdValue.AddItem("friend");
			accountIdValue.Select(0);

			pannoResolutionValue = GetNode<OptionButton>("./VBoxContainer/Content/PannoResolution/PannoResolutionValue");
			pannoResolutionValue.AddItem("Native (1920x1080)");
			pannoResolutionValue.AddItem("Custom");
			pannoResolutionValue.Select(0);

			minimalHoursValue = GetNode<OptionButton>("./VBoxContainer/Content/MinimalHours/MinimalHoursValue");
			minimalHoursValue.AddItem("1");
			minimalHoursValue.AddItem("10");
			minimalHoursValue.AddItem("100");
			minimalHoursValue.AddItem("custom");
			minimalHoursValue.Select(0);

			generationMethodValue = GetNode<OptionButton>("./VBoxContainer/Content/GenerationMethod/GenerationMethodValue");
			generationMethodValue.AddItem("Amanda");
			generationMethodValue.AddItem("Britney");
			generationMethodValue.Select(0);

			tileExpansionMethod = GetNode<OptionButton>("./VBoxContainer/Content/TileExpansionMethod/TileExpansionMethodValue");
			tileExpansionMethod.AddItem("Resize+Cut");
			tileExpansionMethod.AddItem("Resize+Expand");
			tileExpansionMethod.Select(0);
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
			this.Visible = false;
			//GetNode<OptionsMenu>("../OptionsMenu").Visible = true;
		}

		private void OnRestoreBtnPressed()
		{
			
		}

		private void OnApplyBtnPressed()
		{
			
		}
	}
}
