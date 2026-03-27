using System;
using System.Collections.Generic;
using gdt.router.pages.bank;
using Godot;

namespace gdt.router.misc;

public class state {
	public static GetSet<Page> current_page = new(pagesData.main);
	public static GetSet<DateTime> date = new(new DateTime(2000, 1, 1));
	public static GetSet<int> money = new(4_000_000);

	//bank
	public static GetSet<List<Loan>> loanList = new([]);

	public static GetSet<Vector2I> viewportVec2 = new(new Vector2I((int)ProjectSettings.Singleton.GetSetting("display/window/size/viewport_width"), (int)ProjectSettings.Singleton.GetSetting("display/window/size/viewport_height")));

	public void nextDay() {
		date.value = date.value.AddDays(1);
	}
}
