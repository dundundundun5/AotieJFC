using System.IO;
using System.Text;

namespace SegmentationTestTool;
using TextBox = System.Windows.Controls.TextBox;
class Console2Textbox : TextWriter {
    public TextBox box;
    public Console2Textbox(TextBox textBox) {
        Console.SetOut(this);
        box = textBox;
    }
    public override Encoding Encoding => Encoding.UTF8;
    public override void Write(string value) {
        if (value != null)
            box.AppendText(value);
        else
            box.AppendText("");

    }
}
