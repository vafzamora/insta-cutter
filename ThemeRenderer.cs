using System.Drawing;
using System.Windows.Forms;

namespace insta_cutter;

public class DarkMenuRenderer : ToolStripProfessionalRenderer
{
    public DarkMenuRenderer() : base(new DarkColorTable()) { }

    protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
    {
        e.TextColor = Color.White;
        base.OnRenderItemText(e);
    }
}

public class DarkColorTable : ProfessionalColorTable
{
    public override Color MenuItemSelected => Color.FromArgb(70, 70, 74); // Lighter gray for selection
    public override Color MenuItemSelectedGradientBegin => Color.FromArgb(70, 70, 74);
    public override Color MenuItemSelectedGradientEnd => Color.FromArgb(70, 70, 74);
    public override Color MenuItemPressedGradientBegin => Color.FromArgb(60, 60, 64);
    public override Color MenuItemPressedGradientEnd => Color.FromArgb(60, 60, 64);
    public override Color MenuItemBorder => Color.FromArgb(80, 80, 84);
    public override Color MenuBorder => Color.FromArgb(60, 60, 64);
    public override Color ToolStripDropDownBackground => Color.FromArgb(45, 45, 48);
    public override Color ImageMarginGradientBegin => Color.FromArgb(45, 45, 48);
    public override Color ImageMarginGradientMiddle => Color.FromArgb(45, 45, 48);
    public override Color ImageMarginGradientEnd => Color.FromArgb(45, 45, 48);
    public override Color SeparatorDark => Color.FromArgb(60, 60, 64);
    public override Color SeparatorLight => Color.FromArgb(70, 70, 74);
}