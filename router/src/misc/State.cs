using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using gdt.router.pages.bank;
using gdt.shared;
using Godot;

namespace gdt.router.misc;

public static partial class state {
	public static GetSet<Page> current_page = new(pagesData.main);
	public static GetSet<DateTime> date = new(new DateTime(2000, 1, 1, new GregorianCalendar()));
	public static GetSet<int> money = new(4_000_000);
	public static GetSet<IList<Loan>> loanList = new([]);
	public static GetSet<Vector2I> viewportVec2 = new(new Vector2I((int)ProjectSettings.Singleton.GetSetting("display/window/size/viewport_width"), (int)ProjectSettings.Singleton.GetSetting("display/window/size/viewport_height")));

	private static System.Globalization.GregorianCalendar calendar = new();

	public static void nextDay(int days = 1) {
		for (int i = 0; i < days; i++) {
			date.value = calendar.AddDays(date.value, 1);
		}
	}

	static state() {
		date.onChange += (old, val) => {
			if (val.Date.Day == 1) {
				loanList.value = loanList.value.Select(l => {
						money -= l.montlyPayback;
						return l with { left = l.left - l.montlyPayback, };
					})
					.Where(l => l.left > 0)
					.ToList();
			}

			if (val.Date.Day == 13 && val.Date.DayOfWeek == DayOfWeek.Friday) {
				GD.PrintS("friday 13", val.Date.Month);
				state.money.value -= 131_313;
			}

			if (val.Date.Day == 24 && val.Date.Month == 12) {
				GD.PrintS("christmas 24", val.Date.Month);
				state.money.value += 242_424;
			}
		};
	}

	public static System.Threading.Timer timer = new(_ => { nextDay(29); }, null, 0, 1_000);
}

public static partial class state {
	public static GetSet<List<building>> buildingList = new([
		buildingsData.house(),
		buildingsData.warehouse(),
	]);
}
