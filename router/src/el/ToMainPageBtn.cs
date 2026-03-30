using gdt.router.misc;

namespace gdt.router.el;

public partial class ToMainPageBtn : NavButton {
	public ToMainPageBtn() {
		page = pagesData.main with { label = "Back", };
		var _page = pagesData.main with { label = "Back", };

		page = _page;
		Text = page.label;
		Name = page.gdName + "Btn";
		onClick = btn => state.current_page.value = btn.page;
	}
}
